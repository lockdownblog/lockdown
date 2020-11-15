using System;
using System.Linq;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using YamlDotNet.Serialization;

namespace Lockdown.Build
{
    public static class MarkdownExtensions
    {
        private static readonly IDeserializer YamlDeserializer =
            new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        public static readonly MarkdownPipeline Pipeline
            = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .UseAdvancedExtensions()
            .Build();



        public static T GetFrontMatter<T>(this MarkdownDocument document)
        {
            var block = document
                .Descendants<YamlFrontMatterBlock>()
                .FirstOrDefault();

            if (block == null)
                return default;

            var yaml =
                block
                // this is not a mistake
                // we have to call .Lines 2x
                .Lines // StringLineGroup[]
                .Lines // StringLine[]
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
