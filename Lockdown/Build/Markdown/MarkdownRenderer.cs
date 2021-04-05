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
            this.pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UsePipeTables()
                .UseMathematics()
                .Build();
        }

        public string RenderMarkdown(string text)
        {
            return Markdig.Markdown.ToHtml(text, this.pipeline);
        }
    }
}