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
        private const string TagsPath = "tags";
        private const string StaticPath = "static";

        private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        private readonly List<Tuple<Post, string, string>> posts;

        private readonly List<Tuple<Page, string, string>> pages;

        private readonly Dictionary<string, List<Content>> tags;

        private readonly Dictionary<string, Tuple<string, string>> tagLinks;

        private readonly IMapper mapper;

        private readonly IFileSystem fileSystem;

        private Site siteConfig;

        private string rootPath;

        private string outputPath;

        private string postsInputPath;

        private string pagesInputPath;

        private string staticInputPath;

        private string postsOutputPath;

        private string pagesOutputPath;

        private string tagsOutputPath;

        public SiteBuilder(IFileSystem fileSystem, IMapper mapper)
        {
            this.fileSystem = fileSystem;
            this.posts = new List<Tuple<Post, string, string>>();
            this.pages = new List<Tuple<Page, string, string>>();
            this.tags = new Dictionary<string, List<Content>>();
            this.tagLinks = new Dictionary<string, Tuple<string, string>>();
            this.mapper = mapper;
        }

        public void Build(string rootPath, string outPath)
        {
            this.SetPaths(rootPath, outPath);
            this.CleanOutput();
            this.siteConfig = this.GetConfig();

            var pagesDirectory = this.GetFilesIncludingSubfolders(this.pagesInputPath);
            foreach (var filePath in pagesDirectory)
            {
                var (page, textContent) = this.GenerateContent<Page>(filePath, "page_content");
                var (fileToWriteTo, url) = this.CalculatePageRoutes(filePath);
                page.Url = url;
                if (this.siteConfig.PagesInTags)
                {
                    foreach (var tag in page.TagsAsStrings)
                    {
                        this.tags.AddDefault(tag, page);
                    }
                }

                this.pages.Add(Tuple.Create(page, fileToWriteTo, textContent));
            }

            var postDirectory = this.GetFilesIncludingSubfolders(this.postsInputPath);
            foreach (var filePath in postDirectory)
            {
                var (post, textContent) = this.GenerateContent<Post>(filePath, "post_content");
                var (fileToWriteTo, url) = this.CalculatePostRoutes(filePath);
                post.Url = url;
                foreach (var tag in post.TagsAsStrings)
                {
                    this.tags.AddDefault(tag, post);
                }

                this.posts.Add(Tuple.Create(post, fileToWriteTo, textContent));
            }

            foreach (var tag in this.tags.Keys)
            {
                this.tagLinks.Add(tag, this.CalculateTagRoutes(tag));
            }

            // Fill tags in posts
            foreach (var (post, fileToWriteTo, textContent) in this.posts)
            {
                post.Tags = post.TagsAsStrings.Select(tag => new Link { Text = tag, Url = this.tagLinks[tag].Item2 });
                this.WriteContent(fileToWriteTo, textContent, post);
            }

            foreach (var (page, fileToWriteTo, textContent) in this.pages)
            {
                page.Tags = page.TagsAsStrings.Select(tag => new Link { Text = tag, Url = this.tagLinks[tag].Item2 });
                this.WriteContent(fileToWriteTo, textContent, page);
            }

            this.MoveStaticFiles();
            this.WriteTags();
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
            if (this.fileSystem.Directory.Exists(this.outputPath))
            {
                this.fileSystem.Directory.Delete(this.outputPath, recursive: true);
            }

            this.fileSystem.Directory.CreateDirectory(this.outputPath);
            this.fileSystem.Directory.CreateDirectory(this.tagsOutputPath);
            this.fileSystem.Directory.CreateDirectory(this.postsOutputPath);
            this.fileSystem.Directory.CreateDirectory(this.pagesOutputPath);
        }

        private void WriteIndex()
        {
            var orderedPosts = this.posts.Select(element => element.Item1).OrderBy(post => post.DateTime).Reverse().ToList();
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

                var stream = this.fileSystem.File.OpenWrite(Path.Combine(this.outputPath, index));
                using var file = new StreamWriter(stream);
                var fileText = this.fileSystem.File.ReadAllText(Path.Combine(this.rootPath, "templates", "_index.liquid"));
                var template = Template.Parse(fileText);

                var renderVars = Hash.FromAnonymousObject(new
                {
                    site = this.siteConfig,
                    paginator = paginator,
                    posts = orderedPosts,
                    pages = this.pages.Select(element => element.Item1),
                });

                var rendered = template.Render(renderVars);
                file.Write(rendered);
            }
        }

        private void WriteTags()
        {
            var tagIndexFile = Path.Combine(this.rootPath, "templates", "_tag_index.liquid");
            var tagPageFile = Path.Combine(this.rootPath, "templates", "_tag_page.liquid");

            if (!this.fileSystem.File.Exists(tagIndexFile) && !this.fileSystem.File.Exists(tagPageFile))
            {
                return;
            }

            var fileTagsIndex = this.fileSystem.File.ReadAllText(tagIndexFile);
            var fielTagsPage = this.fileSystem.File.ReadAllText(tagPageFile);
            var tagsIndexTemplate = Template.Parse(fileTagsIndex);
            var tagsPageTemplate = Template.Parse(fielTagsPage);

            foreach (var thing in this.tags)
            {
                var (tagPage, tagUrl) = this.tagLinks[thing.Key];
                using var specificTagFileStream = this.fileSystem.File.OpenWrite(tagPage);
                using var tagPageFileStreamWriter = new StreamWriter(specificTagFileStream);
                var tagPageRendered = tagsPageTemplate.Render(Hash.FromAnonymousObject(new
                {
                    site = this.siteConfig,
                    tag_name = thing.Key,
                    articles = thing.Value,
                }));
                tagPageFileStreamWriter.Write(tagPageRendered);
            }

            var renderVars = Hash.FromAnonymousObject(new
            {
                site = this.siteConfig,
                tags = this.tags.Keys.Select(tag => new Link { Text = tag, Url = this.tagLinks[tag].Item2 }),
            });

            var stream = this.fileSystem.File.OpenWrite(Path.Combine(this.outputPath, "tags.html"));
            using var file = new StreamWriter(stream);
            var rendered = tagsIndexTemplate.Render(renderVars);
            file.Write(rendered);
        }

        private void MoveStaticFiles()
        {
            foreach (string dirPath in Directory.GetDirectories(this.staticInputPath, "*", SearchOption.AllDirectories))
            {
                this.fileSystem.Directory.CreateDirectory(dirPath.Replace(this.staticInputPath, this.outputPath));
            }

            foreach (string newPath in Directory.GetFiles(this.staticInputPath, "*.*", SearchOption.AllDirectories))
            {
                this.fileSystem.File.Copy(newPath, newPath.Replace(this.staticInputPath, this.outputPath), true);
            }
        }

        private Site GetConfig()
        {
            var siteConfig = this.fileSystem.File.ReadAllText(Path.Combine(this.rootPath, "site.yml"));
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
            this.outputPath = outPath;
            this.rootPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), rootPath));
            this.postsInputPath = Path.Combine(this.rootPath, "content", PostsPath);
            this.pagesInputPath = Path.Combine(this.rootPath, "content", PagesPath);
            this.staticInputPath = Path.Combine(this.rootPath, StaticPath);
            this.postsOutputPath = Path.Combine(this.outputPath, PostsPath);
            this.pagesOutputPath = Path.Combine(this.outputPath, PagesPath);
            this.tagsOutputPath = Path.Combine(this.outputPath, TagsPath);
            Template.FileSystem = new LockdownFileSystem(Path.Combine(this.rootPath, "templates"));
        }

        private Tuple<T, string> GenerateContent<T>(string filePath, string contentBlockName)
            where T : Content
        {
            var fileText = this.fileSystem.File.ReadAllText(filePath);
            var document = fileText.ParseMarkdown();
            var frontMatter = document.GetFrontMatter<PostConfiguration>();
            var pageContent = this.mapper.Map<T>(frontMatter);
            using var writer = new StringWriter();
            var renderer = new HtmlRenderer(writer);

            writer.Write($"{{% extends '{frontMatter.Layout}' %}}\n\n");

            writer.Write($"{{% block {contentBlockName} %}}\n");

            foreach (var documentPart in document.Skip(1))
            {
                renderer.Write(documentPart);
            }

            writer.Write("{% endblock %}\n");
            writer.Flush();

            return Tuple.Create(pageContent, writer.ToString());
        }

        private void WriteContent<T>(string fileToWriteTo, string stringTemplate, T content)
            where T : Content
        {
            var siteVars = new
            {
                site = this.siteConfig,
                post = content,
                page = content,
                pages = this.pages.Select(element => element.Item1),
            };

            var template = Template.Parse(stringTemplate);
            var rendered = template.Render(Hash.FromAnonymousObject(siteVars));

            using var stream = this.fileSystem.File.OpenWrite(fileToWriteTo);
            using var file = new StreamWriter(stream);
            file.Write(rendered);
        }

        private Tuple<string, string> CalculatePageRoutes(string filePath)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            var parts = filePath[this.postsInputPath.Length..].TrimStart('/', ' ')
                .Split('/', StringSplitOptions.RemoveEmptyEntries).SkipLast(1);

            var outFileName = $"{fileNameWithoutExtension}.html";
            var fileToWriteTo = Path.Combine(this.pagesOutputPath, outFileName);
            var url = "/" + PagesPath + "/" + outFileName;

            return Tuple.Create(fileToWriteTo, url);
        }

        private Tuple<string, string> CalculatePostRoutes(string filePath)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            var parts = filePath[this.postsInputPath.Length..].TrimStart('/', ' ')
                .Split('/', StringSplitOptions.RemoveEmptyEntries).SkipLast(1);
            string fileToWriteTo;
            string url;
            var finalFilename = $"{fileNameWithoutExtension}.html";
            if (parts.Any())
            {
                var newFileDirectory = Path.Combine(parts.ToArray());
                var path = Path.Combine(this.outputPath, newFileDirectory);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                fileToWriteTo = Path.Combine(this.outputPath, finalFilename);
                url = finalFilename;
            }
            else
            {
                fileToWriteTo = Path.Combine(this.postsOutputPath, finalFilename);
                url = "/" + PostsPath + "/" + finalFilename;
            }

            return Tuple.Create(fileToWriteTo, url);
        }

        private Tuple<string, string> CalculateTagRoutes(string tagName)
        {
            var tagUrl = Path.Combine("tags", $"{tagName}.html");
            var tagFile = Path.Combine(this.outputPath, tagUrl);

            return Tuple.Create(tagFile, '/' + tagUrl.Replace('\\', '/'));
        }
    }
}
