namespace Lockdown.Build.Utils
{
    using System.Linq;
    using Markdig;
    using Markdig.Extensions.Yaml;
    using Markdig.Syntax;
    using YamlDotNet.Serialization;

    public static class MarkdownExtensions
    {
        public static readonly MarkdownPipeline Pipeline
            = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .UseAdvancedExtensions()
            .Build();

        private static readonly IDeserializer YamlDeserializer =
            new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        public static MarkdownDocument ParseMarkdown(this string text)
        {
            return Markdown.Parse(text, Pipeline);
        }

        public static T GetFrontMatter<T>(this MarkdownDocument document)
        {
            var block = document
                .Descendants<YamlFrontMatterBlock>()
                .FirstOrDefault();

            if (block == null)
            {
                return default;
            }

            var yaml =
                block
                .Lines
                .Lines
                .OrderByDescending(x => x.Line)
                .Select(x => $"{x}\n")
                .ToList()
                .Select(x => x.Replace("---", string.Empty))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Aggregate((s, agg) => agg + s);

            return YamlDeserializer.Deserialize<T>(yaml);
        }
    }
}
