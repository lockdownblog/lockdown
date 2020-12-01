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

    public class SiteBuilder
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

        private IFileSystem fileSystem;

        private Site siteConfig;

        public SiteBuilder(string rootPath, string outPath)
            : this(rootPath, outPath, new FileSystem())
        {
        }

        public SiteBuilder(string rootPath, string outPath, IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
            this.OutputPath = outPath;
            this.RootPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), rootPath));
            this.PostsInputPath = Path.Combine(this.RootPath, "content", PostsPath);
            this.PagesInputPath = Path.Combine(this.RootPath, "content", PagesPath);
            this.StaticInputPath = Path.Combine(this.RootPath, StaticPath);
            this.PostsOutputPath = Path.Combine(this.OutputPath, PostsPath);
            this.PagesOutputPath = Path.Combine(this.OutputPath, PagesPath);
            this.posts = new List<Post>();
            this.pages = new List<Post>();
            this.mapper = Mapping.Mapper.GetMapper();
            Template.FileSystem = new LockdownFileSystem(Path.Combine(this.RootPath, "templates"));
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

        public static List<List<T>> SplitList<T>(List<T> values, int size = 30)
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

        public void CleanOutput()
        {
            if (this.fileSystem.Directory.Exists(this.OutputPath))
            {
                this.fileSystem.Directory.Delete(this.OutputPath, recursive: true);
            }

            this.fileSystem.Directory.CreateDirectory(this.OutputPath);
            this.fileSystem.Directory.CreateDirectory(this.PostsOutputPath);
            this.fileSystem.Directory.CreateDirectory(this.PagesOutputPath);
        }

        public void WriteIndex()
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
                var file_text = this.fileSystem.File.ReadAllText(Path.Combine(this.RootPath, "templates", "_index.liquid"));
                var template = Template.Parse(file_text);

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

        public void MoveStaticFiles()
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

        public Site GetConfig()
        {
            var siteConfig = this.fileSystem.File.ReadAllText(Path.Combine(this.RootPath, "site.yml"));
            var config = YamlDeserializer.Deserialize<SiteConfiguration>(siteConfig);

            return this.mapper.Map<Site>(config);
        }

        public IEnumerable<string> GetFilesIncludingSubfolders(string path)
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

        public int Build()
        {
            this.CleanOutput();
            this.siteConfig = this.GetConfig();

            var pagesDirectory = this.GetFilesIncludingSubfolders(this.PagesInputPath);
            foreach (var file_path in pagesDirectory)
            {
                var file_text = this.fileSystem.File.ReadAllText(file_path);
                var file_name = Path.GetFileNameWithoutExtension(file_path);
                var fullFileName = file_path.Substring(this.PostsInputPath.Length).TrimStart('/', ' ');
                string fileToWriteTo = null;
                var outFileName = $"{file_name}.html";
                string url = null;

                fileToWriteTo = Path.Combine(this.PagesOutputPath, outFileName);
                url = PagesPath + "/" + outFileName;

                var document = file_text.ParseMarkdown();

                var frontMatter = document.GetFrontMatter<PostConfiguration>();
                frontMatter.Url = url;

                var writer = new StringWriter();
                var renderer = new HtmlRenderer(writer);

                writer.Write($"{{% extends '{frontMatter.Layout}' %}}\n\n");

                writer.Write("{% block page_content %}\n");

                foreach (var documentPart in document.Skip(1))
                {
                    renderer.Write(documentPart);
                }

                writer.Write("{% endblock %}\n");
                writer.Flush();

                var indexPost = this.mapper.Map<Post>(frontMatter);

                this.pages.Add(indexPost);

                var siteVars = new { site = this.siteConfig, page = indexPost };

                var template = Template.Parse(writer.ToString());
                var rendered = template.Render(Hash.FromAnonymousObject(siteVars));

                var stream = this.fileSystem.File.OpenWrite(fileToWriteTo);
                using var file = new StreamWriter(stream);
                file.Write(rendered);
            }

            var postDirectory = this.GetFilesIncludingSubfolders(this.PostsInputPath);
            foreach (var file_path in postDirectory)
            {
                var file_text = this.fileSystem.File.ReadAllText(file_path);
                var file_name = Path.GetFileNameWithoutExtension(file_path);
                var fullFileName = file_path.Substring(this.PostsInputPath.Length).TrimStart('/', ' ');
                var parts = fullFileName.Split('/', StringSplitOptions.RemoveEmptyEntries).SkipLast(1);
                string newFileDirectory = Path.Combine(parts.ToArray());
                string fileToWriteTo = null;
                var outFileName = string.IsNullOrWhiteSpace(newFileDirectory) ? $"{file_name}.html" : Path.Combine(newFileDirectory, $"{file_name}.html");
                string url = null;

                if (parts.Any())
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

                var document = file_text.ParseMarkdown();

                var frontMatter = document.GetFrontMatter<PostConfiguration>();
                frontMatter.Url = url;

                var writer = new StringWriter();
                var renderer = new HtmlRenderer(writer);

                writer.Write($"{{% extends '{frontMatter.Layout}' %}}\n\n");

                writer.Write("{% block post_content %}\n");

                foreach (var documentPart in document.Skip(1))
                {
                    renderer.Write(documentPart);
                }

                writer.Write("{% endblock %}\n");
                writer.Flush();

                var indexPost = this.mapper.Map<Post>(frontMatter);

                this.posts.Add(indexPost);

                var siteVars = new { site = this.siteConfig, post = indexPost };

                var template = Template.Parse(writer.ToString());
                var rendered = template.Render(Hash.FromAnonymousObject(siteVars));

                var stream = this.fileSystem.File.OpenWrite(fileToWriteTo);
                using var file = new StreamWriter(stream);
                file.Write(rendered);
            }

            this.MoveStaticFiles();
            this.WriteIndex();

            return 1;
        }
    }
}
