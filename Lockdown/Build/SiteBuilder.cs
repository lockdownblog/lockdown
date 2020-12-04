namespace Lockdown.Build
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using AutoMapper;
    using DotLiquid;
    using global::Lockdown.Build.InputConfiguration;
    using global::Lockdown.Build.OutputConfiguration;
    using global::Lockdown.Build.Utils;
    using Markdig.Renderers;
    using YamlDotNet.Serialization;

    public class SiteBuilder : ISiteBuilder
    {
        private const string PostsPath = "posts";
        private const string PagesPath = "pages";
        private const string StaticPath = "static";

        private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        private readonly List<Post> posts;

        private readonly List<Post> pages;

        private readonly IMapper mapper;

        private readonly IFileSystem fileSystem;

        private Site siteConfig;

        public SiteBuilder(IFileSystem fileSystem, IMapper mapper)
        {
            this.fileSystem = fileSystem;
            this.posts = new List<Post>();
            this.pages = new List<Post>();
            this.mapper = mapper;
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

        public string PagesInputPath
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

        public string PagesOutputPath
        {
            get;
            private set;
        }

        public void Build(string rootPath, string outPath)
        {
            this.SetPaths(rootPath, outPath);
            this.CleanOutput();
            this.siteConfig = this.GetConfig();

            var pagesDirectory = this.GetFilesIncludingSubfolders(this.PagesInputPath);
            foreach (var filePath in pagesDirectory)
            {
                var page = this.GenerateContent<Post>(filePath, "page_content", this.CalculatePageRoutes);
                this.pages.Add(page);
            }

            var postDirectory = this.GetFilesIncludingSubfolders(this.PostsInputPath);
            foreach (var filePath in postDirectory)
            {
                var post = this.GenerateContent<Post>(filePath, "post_content", this.CalculatePostRoutes);
                this.posts.Add(post);
            }

            this.MoveStaticFiles();
            this.WriteIndex();
        }

        private static List<List<T>> SplitList<T>(List<T> values, int size = 30)
        {
            List<List<T>> list = new List<List<T>>();
            for (int i = 0; i < values.Count; i += size)
            {
                var finalSize = Math.Min(size, values.Count - i);
                var content = values.GetRange(i, finalSize);
                list.Add(content);
            }

            return list;
        }

        private void CleanOutput()
        {
            if (this.fileSystem.Directory.Exists(this.OutputPath))
            {
                this.fileSystem.Directory.Delete(this.OutputPath, recursive: true);
            }

            this.fileSystem.Directory.CreateDirectory(this.OutputPath);
            this.fileSystem.Directory.CreateDirectory(this.PostsOutputPath);
            this.fileSystem.Directory.CreateDirectory(this.PagesOutputPath);
        }

        private void WriteIndex()
        {
            var orderedPosts = this.posts.OrderBy(post => post.DateTime).Reverse().ToList();
            var splits = SplitList(orderedPosts, size: 10);

            for (int i = 0; i < splits.Count; i++)
            {
                var first = i == 0;
                var last = i == splits.Count - 1;
                var currentPage = i + 1;

                var index = first ? "index.html" : $"index-{i}.html";
                var previousIndex = $"index-{i - 1}.html";
                var nextIndex = $"index-{i + 1}.html";
                if (i - 1 == 0)
                {
                    previousIndex = "index.html";
                }

                var paginator = new Paginator()
                {
                    PageCount = splits.Count,
                    CurrentPage = currentPage,
                    HasNextPage = !last,
                    HasPreviousPage = !first,
                    PreviousPage = currentPage - 1,
                    NextPage = currentPage + 1,
                    NextPageUrl = nextIndex,
                    PreviousPageUrl = previousIndex,
                    Posts = splits[i],
                };

                var stream = this.fileSystem.File.OpenWrite(Path.Combine(this.OutputPath, index));
                using var file = new StreamWriter(stream);
                var fileText = this.fileSystem.File.ReadAllText(Path.Combine(this.RootPath, "templates", "_index.liquid"));
                var template = Template.Parse(fileText);

                var renderVars = Hash.FromAnonymousObject(new
                {
                    site = this.siteConfig,
                    paginator = paginator,
                    posts = orderedPosts,
                    pages = this.pages,
                });

                var rendered = template.Render(renderVars);
                file.Write(rendered);
            }
        }

        private void MoveStaticFiles()
        {
            foreach (string dirPath in Directory.GetDirectories(this.StaticInputPath, "*", SearchOption.AllDirectories))
            {
                this.fileSystem.Directory.CreateDirectory(dirPath.Replace(this.StaticInputPath, this.OutputPath));
            }

            foreach (string newPath in Directory.GetFiles(this.StaticInputPath, "*.*", SearchOption.AllDirectories))
            {
                this.fileSystem.File.Copy(newPath, newPath.Replace(this.StaticInputPath, this.OutputPath), true);
            }
        }

        private Site GetConfig()
        {
            var siteConfig = this.fileSystem.File.ReadAllText(Path.Combine(this.RootPath, "site.yml"));
            var config = YamlDeserializer.Deserialize<SiteConfiguration>(siteConfig);

            return this.mapper.Map<Site>(config);
        }

        private IEnumerable<string> GetFilesIncludingSubfolders(string path)
        {
            var paths = new List<string>();

            if (!this.fileSystem.Directory.Exists(path))
            {
                return paths;
            }

            var directories = this.fileSystem.Directory.GetDirectories(path);

            foreach (var directory in directories)
            {
                paths.AddRange(this.GetFilesIncludingSubfolders(directory));
            }

            paths.AddRange(this.fileSystem.Directory.GetFiles(path).ToList());
            return paths;
        }

        private void SetPaths(string rootPath, string outPath)
        {
            this.OutputPath = outPath;
            this.RootPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), rootPath));
            this.PostsInputPath = Path.Combine(this.RootPath, "content", PostsPath);
            this.PagesInputPath = Path.Combine(this.RootPath, "content", PagesPath);
            this.StaticInputPath = Path.Combine(this.RootPath, StaticPath);
            this.PostsOutputPath = Path.Combine(this.OutputPath, PostsPath);
            this.PagesOutputPath = Path.Combine(this.OutputPath, PagesPath);
            Template.FileSystem = new LockdownFileSystem(Path.Combine(this.RootPath, "templates"));
        }

        private T GenerateContent<T>(string filePath, string contentBlockName, Func<string, IEnumerable<string>, Tuple<string, string>> routeCalculator)
            where T : Post
        {
            var fileText = this.fileSystem.File.ReadAllText(filePath);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            var parts = filePath.Substring(this.PostsInputPath.Length).TrimStart('/', ' ')
                .Split('/', StringSplitOptions.RemoveEmptyEntries).SkipLast(1);

            var (fileToWriteTo, url) = routeCalculator(fileNameWithoutExtension, parts);

            var document = fileText.ParseMarkdown();

            var frontMatter = document.GetFrontMatter<PostConfiguration>();
            frontMatter.Url = url;

            var writer = new StringWriter();
            var renderer = new HtmlRenderer(writer);

            writer.Write($"{{% extends '{frontMatter.Layout}' %}}\n\n");

            writer.Write($"{{% block {contentBlockName} %}}\n");

            foreach (var documentPart in document.Skip(1))
            {
                renderer.Write(documentPart);
            }

            writer.Write("{% endblock %}\n");
            writer.Flush();

            var pageContent = this.mapper.Map<T>(frontMatter);

            var siteVars = new { site = this.siteConfig, post = pageContent, page = pageContent };

            var template = Template.Parse(writer.ToString());
            var rendered = template.Render(Hash.FromAnonymousObject(siteVars));

            var stream = this.fileSystem.File.OpenWrite(fileToWriteTo);
            using var file = new StreamWriter(stream);
            file.Write(rendered);

            return pageContent;
        }

        private Tuple<string, string> CalculatePageRoutes(string fileNameWithoutExtension, IEnumerable<string> parts = null)
        {
            var outFileName = $"{fileNameWithoutExtension}.html";
            var fileToWriteTo = Path.Combine(this.PagesOutputPath, outFileName);
            var url = PagesPath + "/" + outFileName;

            return Tuple.Create(fileToWriteTo, url);
        }

        private Tuple<string, string> CalculatePostRoutes(string fileNameWithoutExtension, IEnumerable<string> parts)
        {
            string fileToWriteTo;
            string url;
            var finalFilename = $"{fileNameWithoutExtension}.html";
            if (parts.Any())
            {
                var newFileDirectory = Path.Combine(parts.ToArray());
                var path = Path.Combine(this.OutputPath, newFileDirectory);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                fileToWriteTo = Path.Combine(this.OutputPath, finalFilename);
                url = finalFilename;
            }
            else
            {
                fileToWriteTo = Path.Combine(this.PostsOutputPath, finalFilename);
                url = PostsPath + "/" + finalFilename;
            }

            return Tuple.Create(fileToWriteTo, url);
        }
    }
}
