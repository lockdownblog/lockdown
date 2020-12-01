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

            RootDirectory = Path.Combine(workspace, "docs");
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
            var lockdown = new Lockdown();
            MockFileSystem.File.Exists(Path.Combine(ExitDirectory, "index.html")).ShouldBeFalse();

            var siteBuilder = new SiteBuilder(RootDirectory, ExitDirectory, MockFileSystem);

            siteBuilder.Build();

            var indexFile = Path.Combine(ExitDirectory, "index.html");
            MockFileSystem.File.Exists(indexFile).ShouldBeTrue();
            var document = await OpenDocument(indexFile);

            var lis = document.All.Where(node => node.LocalName == "li");
            lis.Count().ShouldBe(2);

            var titleContainer = document.All.Where(
                node => node.LocalName == "h1" && node.ClassName == "brand-title").First();
            titleContainer.TextContent.ShouldBe("Lockdown");

            var subTitleContainer = document.All.Where(
                node => node.LocalName == "h2" && node.ClassName == "brand-tagline").First();
            subTitleContainer.TextContent.ShouldBe("A static website generator");
        }
    }
}
