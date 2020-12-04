using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Lockdown.Build;
using Xunit;
using Shouldly;

namespace Lockdown.Tests
{
    /// <summary>
    /// This test is massive and should be broken down into much smaller bits!
    /// </summary>
    public class SuperTest
    {
        readonly string RootDirectory;
        readonly string ExitDirectory;
        readonly MockFileSystem MockFileSystem;

        public SuperTest()
        {
            var workspace = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../../../"));

            RootDirectory = Path.Combine(workspace, "Lockdown", "BlankTemplate");
            ExitDirectory = Path.Combine(workspace, "_docs");
            var dictionary = new Dictionary<string, MockFileData>();
            foreach (var path in TestingUtils.GetFilesIncludingSubfolders(RootDirectory))
            {
                dictionary.Add(path, new MockFileData(File.ReadAllBytes(path)));
            }

            MockFileSystem = new MockFileSystem(dictionary);
        }

        private async Task<IDocument> OpenDocument(string path)
        {
            var context = BrowsingContext.New(Configuration.Default);
            return await context.OpenAsync(req => req.Content(MockFileSystem.File.ReadAllText(path)));
        }

        [Fact]
        public async Task TestBuild()
        {
            MockFileSystem.File.Exists(Path.Combine(ExitDirectory, "index.html")).ShouldBeFalse();

            var siteBuilder = new SiteBuilder(RootDirectory, ExitDirectory, MockFileSystem);
            siteBuilder.Build();

            var indexFile = Path.Combine(ExitDirectory, "index.html");
            MockFileSystem.File.Exists(indexFile).ShouldBeTrue();
            var indexDocument = await OpenDocument(indexFile);

            var sections = indexDocument.All.Where(node => node.LocalName == "section");
            sections.Count().ShouldBe(2);

            var titleContainer = indexDocument.All.First(node => node.LocalName == "div" && node.ClassName == "main");
            titleContainer.TextContent.Trim().ShouldBe("Your new blog!");

            var subTitleContainer = indexDocument.All.First(node => node.LocalName == "div" && node.ClassName == "site-description");
            subTitleContainer.TextContent.Trim().ShouldBe("Edit this subtitle in the site.yml file");
            
            var secondPostFile = Path.Combine(ExitDirectory, "posts", "second-post.html");
            MockFileSystem.File.Exists(secondPostFile).ShouldBeTrue();
            var secondPost = await OpenDocument(secondPostFile);
            var title = secondPost.All.First(node => node.LocalName == "h1");
            title.TextContent.ShouldBe("My second post!");

            var pageFile = Path.Combine(ExitDirectory, "pages", "about.html");
            MockFileSystem.File.Exists(pageFile).ShouldBeTrue();
            var page = await OpenDocument(pageFile);
            var pageTitle = page.All.First(node => node.LocalName == "h1");
            pageTitle.TextContent.ShouldBe("About");
        }
    }
}
