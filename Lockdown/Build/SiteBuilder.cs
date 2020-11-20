using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotLiquid;
using Lockdown.LiquidEntities;
using Markdig;
using Markdig.Renderers;
using YamlDotNet.Serialization;
using LiquidSocial = Lockdown.LiquidEntities.Social;

namespace Lockdown.Build
{

    public class SiteBuilder
    {
        const string PostsPath = "posts";
        const string StaticPath = "static";

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

        public string StaticInputPath
        {
            get;
            private set;
        }

        public string PostsOutputPath
        {
            get;
            private set;
        }

        private Site SiteConfig;

        List<IndexPost> Posts;

        public SiteBuilder(string rootPath, string outPath)
        {
            RootPath = rootPath;
            OutputPath = outPath;
            PostsInputPath = Path.Combine(RootPath, "content", PostsPath);
            StaticInputPath = Path.Combine(RootPath, StaticPath);
            PostsOutputPath = Path.Combine(OutputPath, PostsPath);
            Posts = new List<IndexPost>();
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

        public IndexPost MapToIndexPost(PostFrontMatter post, string filename)
        {
            var indexPost = new IndexPost();
            indexPost.Title = post.Title;
            indexPost.Dt = post.DateTime;
            indexPost.DateTime = post.DateTime.ToString();
            indexPost.Url = PostsPath + "/" +  filename;
            return indexPost;
        }

        public void WriteIndex()
        {
            using (var file = new System.IO.StreamWriter(Path.Combine(OutputPath, "index.html")))
            {
                var file_text = File.ReadAllText(Path.Combine(RootPath, "content", "index.html"));
                var template = Template.Parse(file_text);

                var orderedPosts = Posts.OrderBy(post => post.Dt).Reverse();

                var renderVars = Hash.FromAnonymousObject(new { 
                    site = SiteConfig,
                    posts = orderedPosts
                });

                var rendered = template.Render(renderVars);
                file.Write(rendered);
            }

        }

        public void MoveStaticFiles()
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(StaticInputPath, "*", 
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(StaticInputPath, OutputPath));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(StaticInputPath, "*.*", 
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(StaticInputPath, OutputPath), true);
        }

        private static readonly IDeserializer YamlDeserializer =
            new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        public Site GetConfig()
        {
            var siteConfig = File.ReadAllText(Path.Combine(RootPath, "site.yml"));
            var config =  YamlDeserializer.Deserialize<SiteConfig>(siteConfig);

            return new Site 
            {
                Subtitle = config.Subtitle,
                Title = config.Title,
                Social = config.Social.Select(
                    social => new LiquidSocial { Link = social.Link, Name = social.Name }
                ).ToList()
            };
        }

        public int Build()
        {
            CleanOutput();
            SiteConfig = GetConfig();

            var postDirectory = Directory.GetFiles(PostsInputPath);

            foreach(var file_path in postDirectory)
            {
                var file_text = File.ReadAllText(file_path);
                var file_name = Path.GetFileNameWithoutExtension(file_path);
                var outFileName = $"{file_name}.html";
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

                Posts.Add(MapToIndexPost(frontMatter, outFileName));

                var siteVars = new { site = SiteConfig, post = frontMatter };

                var rendered = template.Render(Hash.FromAnonymousObject(siteVars));

                using (var file = new System.IO.StreamWriter(Path.Combine(PostsOutputPath, outFileName)))
                {
                    file.Write(rendered);
                }
            }

            MoveStaticFiles();
            WriteIndex();


            return 1;
        }
    }
}
