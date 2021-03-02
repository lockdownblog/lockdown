namespace Lockdown.Build.Markdown
{
    using System.IO;
    using Markdig;
    using Markdig.Renderers;

    public class MarkdownRenderer : IMarkdownRenderer
    {
        private readonly MarkdownPipeline pipeline;

        public MarkdownRenderer()
        {
            this.pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        }

        public string RenderMarkdown(string text)
        {
            var document = Markdown.Parse(text, this.pipeline);

            using var writer = new StringWriter();
            var htmlRenderer = new HtmlRenderer(writer);

            foreach (var documentPart in document)
            {
                htmlRenderer.Write(documentPart);
            }

            return writer.ToString();
        }
    }
}