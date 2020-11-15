using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotLiquid;
using Markdig;
using Markdig.Renderers;

namespace Lockdown.Build
{

    public class SiteBuilder
    {
        public string RootPath
        {
            get;
            private set;
        }

        public string OutputPath
        {
            get;
            private set;
        }

        public string PostsInputPath
        {
            get;
            private set;
        }

        public string PostsOutputPath
        {
            get;
            private set;
        }

        List<PostFrontMatter> Posts;

        public SiteBuilder(string rootPath, string outPath)
        {
            RootPath = rootPath;
            OutputPath = outPath;
            PostsInputPath = Path.Combine(RootPath, "content", "posts");
            PostsOutputPath = Path.Combine(OutputPath, "posts");
            Posts = new List<PostFrontMatter>();
            Template.FileSystem = new LockdownFileSystem(Path.Combine(rootPath, "templates"));
        }

        public void CleanOutput()
        {
            if (Directory.Exists(OutputPath))
            {
                Directory.Delete(OutputPath, recursive: true);
            }
            Directory.CreateDirectory(OutputPath);
            Directory.CreateDirectory(PostsOutputPath);
        }

        public void WriteIndex()
        {
            using (var file = new System.IO.StreamWriter(Path.Combine(OutputPath, "index.html")))
            {
                var file_text = File.ReadAllText(Path.Combine(RootPath, "content", "index.html"));
                var template = Template.Parse(file_text);

                var orderedPosts = Posts.OrderBy(post => post.DateTime).Reverse();

                var rendered = template.Render(Hash.FromAnonymousObject(new { posts = orderedPosts }));
                file.Write(rendered);
            }

        }

        public int Build()
        {
            CleanOutput();

            var postDirectory = Directory.GetFiles(PostsInputPath);

            foreach(var file_path in postDirectory)
            {
                var file_text = File.ReadAllText(file_path);
                var file_name = Path.GetFileNameWithoutExtension(file_path);
                var document = Markdown.Parse(file_text, MarkdownExtensions.Pipeline);
                var frontMatter = document.GetFrontMatter<PostFrontMatter>();

                var writer = new StringWriter();
                var renderer = new HtmlRenderer(writer);

                writer.Write($"{{% extends '{frontMatter.Layout}' %}}\n");
                writer.Write("{% block post_content %}\n");
                foreach (var documentPart in document.Skip(1))
                {
                    renderer.Write(documentPart);
                }
                writer.Write("{% endblock %}\n");
                writer.Flush();


                Template template = Template.Parse(writer.ToString());

                Posts.Add(frontMatter);

                var rendered = template.Render(Hash.FromAnonymousObject(new { title = frontMatter.Title }));

                using (var file = new System.IO.StreamWriter(Path.Combine(PostsOutputPath, $"{file_name}.html")))
                {
                    file.Write(rendered);
                }
            }


            WriteIndex();


            return 1;
        }
    }
}
