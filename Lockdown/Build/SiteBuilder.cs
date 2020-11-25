namespace Lockdown.Build
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AutoMapper;
    using DotLiquid;
    using global::Lockdown.LiquidEntities;
    using Markdig;
    using Markdig.Renderers;
    using YamlDotNet.Serialization;
    using LiquidSocial = global::Lockdown.LiquidEntities.Social;

    public class SiteBuilder
    {
        private const string PostsPath = "posts";
        private const string StaticPath = "static";

        private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        private Site siteConfig;

        private List<IndexPost> posts;

        private IMapper mapper;

        public SiteBuilder(string rootPath, string outPath, IMapper mapper)
        {
            this.RootPath = rootPath;
            this.OutputPath = outPath;
            this.PostsInputPath = Path.Combine(this.RootPath, "content", PostsPath);
            this.StaticInputPath = Path.Combine(this.RootPath, StaticPath);
            this.PostsOutputPath = Path.Combine(this.OutputPath, PostsPath);
            this.posts = new List<IndexPost>();
            this.mapper = mapper;
            Template.FileSystem = new LockdownFileSystem(Path.Combine(rootPath, "templates"));
        }

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

        public void CleanOutput()
        {
            if (Directory.Exists(this.OutputPath))
            {
                Directory.Delete(this.OutputPath, recursive: true);
            }

            Directory.CreateDirectory(this.OutputPath);
            Directory.CreateDirectory(this.PostsOutputPath);
        }

        public void WriteIndex()
        {
            using (var file = new System.IO.StreamWriter(Path.Combine(this.OutputPath, "index.html")))
            {
                var file_text = File.ReadAllText(Path.Combine(this.RootPath, "content", "index.html"));
                var template = Template.Parse(file_text);

                var orderedPosts = this.posts.OrderBy(post => post.DateTime).Reverse();

                var renderVars = Hash.FromAnonymousObject(new
                {
                    site = this.siteConfig,
                    posts = orderedPosts,
                });

                var rendered = template.Render(renderVars);
                file.Write(rendered);
            }
        }

        public void MoveStaticFiles()
        {
            foreach (string dirPath in Directory.GetDirectories(this.StaticInputPath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(this.StaticInputPath, this.OutputPath));
            }

            foreach (string newPath in Directory.GetFiles(this.StaticInputPath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(this.StaticInputPath, this.OutputPath), true);
            }
        }

        public Site GetConfig()
        {
            var siteConfig = File.ReadAllText(Path.Combine(this.RootPath, "site.yml"));
            var config = YamlDeserializer.Deserialize<SiteConfig>(siteConfig);

            return this.mapper.Map<Site>(config);
        }

        public IEnumerable<string> GetFilesIncludingSubfolders(string path)
        {
            var paths = new List<string>();
            var directories = Directory.GetDirectories(path);

            foreach (var directory in directories)
            {
                paths.AddRange(this.GetFilesIncludingSubfolders(directory));
            }

            paths.AddRange(Directory.GetFiles(path).ToList());
            return paths;
        }

        public int Build()
        {
            this.CleanOutput();
            this.siteConfig = this.GetConfig();

            var postDirectory = this.GetFilesIncludingSubfolders(this.PostsInputPath);

            foreach (var file_path in postDirectory)
            {
                var file_text = File.ReadAllText(file_path);
                var file_name = Path.GetFileNameWithoutExtension(file_path);
                var fullFileName = file_path.Substring(this.PostsInputPath.Length).TrimStart('/', ' ');
                var parts = fullFileName.Split('/', StringSplitOptions.RemoveEmptyEntries).SkipLast(1);
                string newFileDirectory = Path.Combine(parts.ToArray());
                string fileToWriteTo = null;
                var outFileName = string.IsNullOrWhiteSpace(newFileDirectory) ? $"{file_name}.html" : Path.Combine(newFileDirectory, $"{file_name}.html");
                string url = null;

                if (parts.Count() > 0)
                {
                    var path = Path.Combine(this.OutputPath, newFileDirectory);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    fileToWriteTo = Path.Combine(this.OutputPath, outFileName);
                    url = outFileName;
                }
                else
                {
                    fileToWriteTo = Path.Combine(this.PostsOutputPath, outFileName);
                    url = PostsPath + "/" + outFileName;
                }

                var document = Markdown.Parse(file_text, MarkdownExtensions.Pipeline);

                var frontMatter = document.GetFrontMatter<PostFrontMatter>();

                var writer = new StringWriter();
                var renderer = new HtmlRenderer(writer);

                writer.Write($"{{% extends 'post' %}}\n\n");

                writer.Write("{% block post_content %}\n");

                foreach (var documentPart in document.Skip(1))
                {
                    renderer.Write(documentPart);
                }

                writer.Write("{% endblock %}\n");
                writer.Flush();

                frontMatter.Url = url;
                var indexPost = this.mapper.Map<IndexPost>(frontMatter);

                this.posts.Add(indexPost);

                var siteVars = new { site = this.siteConfig, post = indexPost };

                var template = Template.Parse(writer.ToString());
                var rendered = template.Render(Hash.FromAnonymousObject(siteVars));

                using var file = new System.IO.StreamWriter(fileToWriteTo);
                file.Write(rendered);
            }

            this.MoveStaticFiles();
            this.WriteIndex();

            return 1;
        }
    }
}
