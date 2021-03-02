namespace Lockdown.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using System.Linq;
    using System.Threading.Tasks;
    using AngleSharp;
    using AngleSharp.Dom;
    using AutoMapper;
    using Lockdown.Build;
    using Lockdown.Build.Entities;
    using Lockdown.Build.Markdown;
    using Lockdown.Build.Utils;
    using Lockdown.Tests.Data;
    using Moq;
    using Shouldly;
    using Xunit;

    public class SiteBuilderTests
    {
        private const string InputPath = "./input";
        private const string Output = "./output";

        private readonly IFileSystem fakeFileSystem;
        private readonly ISlugifier slugifier;
        private readonly IMapper mapper;
        private readonly Mock<IYamlParser> moqYamlParser;
        private readonly Mock<IMarkdownRenderer> moqMarkdownRenderer;
        private readonly Mock<ILiquidRenderer> moqLiquidRenderer;
        private readonly SiteBuilder genericSiteBuilder;

        public SiteBuilderTests()
        {
            this.fakeFileSystem = new MockFileSystem();
            this.moqYamlParser = new Mock<IYamlParser>();
            this.moqMarkdownRenderer = new Mock<IMarkdownRenderer>();
            this.moqLiquidRenderer = new Mock<ILiquidRenderer>();
            this.slugifier = new Slugifier();
            this.mapper = Build.Mapping.Mapper.GetMapper();
            this.genericSiteBuilder = new SiteBuilder(
                this.fakeFileSystem,
                this.moqYamlParser.Object,
                this.moqMarkdownRenderer.Object,
                this.moqLiquidRenderer.Object,
                this.slugifier,
                this.mapper);
        }

        [Fact]
        public void TestOutputFolderExist()
        {
            // Setup
            var fakeFilePath = this.fakeFileSystem.Path.Combine(Output, "archivo.txt");
            this.fakeFileSystem.Directory.CreateDirectory(Output);
            this.fakeFileSystem.File.WriteAllText(fakeFilePath, "hola mundo");

            // Act
            this.genericSiteBuilder.CleanFolder(Output);

            // Asserts
            this.AssertDirectoryIsEmpty(Output);
        }

        [Fact]
        public void TestOutputFolderDoesNotExist()
        {
            // Act
            this.genericSiteBuilder.CleanFolder(Output);

            // Asserts
            this.AssertDirectoryIsEmpty(Output);
        }

        [Theory]
        [ClassData(typeof(SplitPostTestData))]
        public void CanAddTheoryClassData(string post, string metadata, string content)
        {
            var (actualMetadata, actualContent) = this.genericSiteBuilder.SplitPost(post);

            actualMetadata.ShouldBe(metadata);
            actualContent.ShouldBe(content);
        }

        [Fact]
        public void TestCopyFiles()
        {
            // Setup
            var stylesFile = this.fakeFileSystem.Path.Combine(InputPath, "style.css");
            var someOtherFile = this.fakeFileSystem.Path.Combine(InputPath, "subfolder", "style.css");

            var contents = new Dictionary<string, MockFileData>
            {
                { stylesFile, new MockFileData("body { color: #fff; }") },
                { someOtherFile, new MockFileData("more data") },
            };

            var fakeFileSystem = new MockFileSystem(contents);
            fakeFileSystem.Directory.CreateDirectory(Output);
            var siteBuilder = new SiteBuilder(
                fakeFileSystem,
                this.moqYamlParser.Object,
                this.moqMarkdownRenderer.Object,
                this.moqLiquidRenderer.Object,
                this.slugifier,
                this.mapper);

            // Act
            siteBuilder.CopyFiles(InputPath, Output);

            // Assert
            fakeFileSystem.Directory.EnumerateFiles(Output, "*.*", SearchOption.AllDirectories).Count().ShouldBe(2);
        }

        [Fact]
        public void TestWriteFile()
        {
            var destination = this.fakeFileSystem.Path.Combine(InputPath, "some", "folder", "file.txt");
            var content = "Hello world!";

            // Act
            this.genericSiteBuilder.WriteFile(destination, content);

            // Assert
            this.fakeFileSystem.File.ReadAllText(destination).ShouldBe(content);
        }

        [Fact]
        public void TestConvertMetadata()
        {
            var metadata = @"layout: post
title: ""My first post!""
summary: ""This is my first Lockdown post""
datetime: 2020-12-29 12:00
tags: lockdown, Data Science, Some Other tag
";
            var expectedMetadata = new PostMetadata
            {
                Title = "My first post!",
                Slug = "my-first-post",
                TagArray = new string[] { "lockdown", "data-science", "some-other-tag" },
            };

            var siteBuilder = new SiteBuilder(
                this.fakeFileSystem,
                new YamlParser(),
                this.moqMarkdownRenderer.Object,
                this.moqLiquidRenderer.Object,
                this.slugifier,
                this.mapper);

            // Act
            var actualMetadata = siteBuilder.ConvertMetadata(metadata);

            // Assert
            actualMetadata.Title.ShouldBe(expectedMetadata.Title);
            actualMetadata.Slug.ShouldBe(expectedMetadata.Slug);
            actualMetadata.TagArray.ShouldBe(expectedMetadata.TagArray);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void TestGetPostsWithSinglePost(int files)
        {
            var postsPath = this.fakeFileSystem.Path.Combine(InputPath, "content", "posts");
            this.fakeFileSystem.Directory.CreateDirectory(postsPath);
            var fileContents = new List<string>();
            for (var i = 0; i < files; i++)
            {
                var postPath = this.fakeFileSystem.Path.Combine(postsPath, $"file_{i}.txt");
                var content = "# Hola Mundo!\n\n**Prueba {i}**";
                this.fakeFileSystem.File.WriteAllText(postPath, content);
                fileContents.Add(content);
            }

            var posts = this.genericSiteBuilder.GetPosts(InputPath);

            posts.OrderBy(content => content).ShouldBe(fileContents);
        }

        [Theory]
        [InlineData("/{}.html", "hello-world.html", "/hello-world.html")]
        [InlineData("{}.html", "hello-world.html", "/hello-world.html")]
        [InlineData("/{}", "hello-world/index.html", "/hello-world")]
        [InlineData("/post/{}", "post/hello-world/index.html", "/post/hello-world")]
        [InlineData("/post/{}/index.html", "post/hello-world/index.html", "/post/hello-world")]
        public void TestGetRoutes(string template, string fileExpected, string canonicalExpected)
        {
            var metadata = new PostMetadata { Title = "Hello World", Slug="hello-world" };

            var (filePath, canonicalPath) = this.genericSiteBuilder.GetPostPaths(template, metadata);

            filePath.ShouldBe(fileExpected);
            canonicalPath.ShouldBe(canonicalExpected);
        }

        [Fact]
        public async Task TestRenderContent()
        {
            // Setup: Prepare our test by copying our `demo` folder into our "fake" file system.
            var workspace = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../../../"));
            var templatePath = Path.Combine(workspace, "Lockdown", "BlankTemplate", "templates");
            var dictionary = new Dictionary<string, MockFileData>();
            foreach (var path in Directory.EnumerateFiles(templatePath))
            {
                var fakePath = path.Replace(templatePath, Path.Combine(InputPath, "templates"));
                dictionary.Add(fakePath, new MockFileData(File.ReadAllBytes(path)));
            }

            var fakeFileSystem = new MockFileSystem(dictionary);

            var metadata = new PostMetadata { Title = "Test post", Date = new DateTime(2000, 1, 1) };
            var postContent = "# Content #";
            var dotLiquidRenderer = new DotLiquidRenderer(fakeFileSystem);
            dotLiquidRenderer.SetRoot(InputPath);
            this.moqMarkdownRenderer.Setup(moq => moq.RenderMarkdown("# Content #")).Returns(() => "<b>Content</b>");
            var siteBuilder = new SiteBuilder(
                fakeFileSystem,
                this.moqYamlParser.Object,
                this.moqMarkdownRenderer.Object,
                dotLiquidRenderer,
                this.slugifier,
                this.mapper);

            // Act
            var convertedPost = siteBuilder.RenderContent(metadata, postContent, InputPath);

            // Assert
            var html = await this.ParseHtml(convertedPost);

            var heading1 = html.All.First(node => node.LocalName == "h1");
            heading1.TextContent.ShouldBe("Test post");

            var bold = html.All.First(node => node.LocalName == "b");
            bold.TextContent.ShouldBe("Content");
        }

        [Theory]
        [InlineData(0, 1, null, "index.html", null)]
        [InlineData(0, 2, null, "index.html", "index-1.html")]
        [InlineData(1, 2, "index.html", "index-1.html", null)]
        [InlineData(1, 3, "index.html", "index-1.html", "index-2.html")]
        public void TestGenerateIndexNames(int currentPage, int pageCount, string previous, string current, string next)
        {
            var (actualPrevious, actualCurrent, actualNext) = this.genericSiteBuilder.GenerateIndexNames(currentPage, pageCount);

            Assert.Equal(previous, actualPrevious);
            Assert.Equal(next, actualNext);
            actualCurrent.ShouldBe(current);
        }

        [Theory]
        [InlineData(10, 2, 2)]
        [InlineData(9, 2, 1)]
        [InlineData(1, 2, 1)]
        [InlineData(1, 1, 1)]
        public void SplitChunks(int totalSize, int chunkSize, int lastSize)
        {
            var collection = Enumerable.Range(0, totalSize).ToList();

            var chunks = this.genericSiteBuilder.SplitChunks(collection, chunkSize);

            chunks.Last().Count().ShouldBe(lastSize);
            foreach (var chunk in chunks.SkipLast(1))
            {
                chunk.Count().ShouldBe(chunkSize);
            }
        }

        private void AssertDirectoryIsEmpty(string output)
        {
            this.fakeFileSystem.Directory.Exists(output).ShouldBeTrue();
            this.fakeFileSystem.Directory.EnumerateFiles(output).Any().ShouldBeFalse();
            this.fakeFileSystem.Directory.EnumerateDirectories(output).Any().ShouldBeFalse();
        }

        private async Task<IDocument> ParseHtml(string document)
        {
            var context = BrowsingContext.New(Configuration.Default);
            return await context.OpenAsync(req => req.Content(document));
        }
    }
}
