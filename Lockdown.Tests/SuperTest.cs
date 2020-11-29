using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Lockdown.Build;
using Xunit;

namespace Lockdown.Tests
{
    /// <summary>
    /// This test is massive and should be broken down into much smaller bits!
    /// </summary>
    public class SuperTest : IDisposable
    {
        readonly string RootDirectory;
        readonly string ExitDirectory;

        public SuperTest()
        {
            var workspace = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE");
            RootDirectory = Path.Combine(workspace, "docs");
            ExitDirectory = Path.Combine(RootDirectory, "_docs");

            if (Directory.Exists(ExitDirectory))
            {
                Directory.Delete(ExitDirectory, recursive: true);
            }
        }

        private async Task<IDocument> OpenDocument(string path)
        {
            var context = BrowsingContext.New(Configuration.Default);
            return await context.OpenAsync(req => req.Content(File.ReadAllText(path)));
        }

        public void Dispose()
        {
            Directory.Delete(ExitDirectory, recursive: true);
        }

        [Fact]
        public async Task TestBuild()
        {
            var lockdown = new Lockdown();
            Assert.False(File.Exists(Path.Combine(ExitDirectory, "index.html")));

            var siteBuilder = new SiteBuilder(RootDirectory, ExitDirectory, lockdown.Mapper);

            siteBuilder.Build();

            var indexFile = Path.Combine(ExitDirectory, "index.html");
            Assert.True(File.Exists(indexFile));
            var document = await OpenDocument(indexFile);

            var lis = document.All.Where(node => node.LocalName == "li");
            Assert.True(lis.Count() == 2);

            var titleContainer = document.All.Where(
                node => node.LocalName == "h1" && node.ClassName == "brand-title").First();
            Assert.True(titleContainer.TextContent == "Lockdown");

            var subTitleContainer = document.All.Where(
                node => node.LocalName == "h2" && node.ClassName == "brand-tagline").First();
            Assert.True(subTitleContainer.TextContent == "A static website generator");
        }
    }
}
