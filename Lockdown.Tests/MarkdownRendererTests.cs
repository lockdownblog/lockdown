namespace Lockdown.Tests
{
    using Lockdown.Build.Markdown;
    using Shouldly;
    using Xunit;

    public class MarkdownRendererTests
    {
        private readonly MarkdownRenderer markdownRenderer;

        public MarkdownRendererTests()
        {
            this.markdownRenderer = new MarkdownRenderer();
        }

        [Fact]
        public void TestRender()
        {
            var md = @"# Hello world";

            var html = this.markdownRenderer.RenderMarkdown(md);

            html.ShouldBe("<h1 id=\"hello-world\">Hello world</h1>\n");
        }
    }
}
