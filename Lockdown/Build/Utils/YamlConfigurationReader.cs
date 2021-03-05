namespace Lockdown.Build.Utils
{
    using System.Collections.Generic;
    using AutoMapper;

    public class YamlConfigurationReader : IYamlConfigurationReader
    {
        private const string IndexFileName = "index.liquid";
        private const string TagIndexFileName = "tag_index.liquid";
        private const string TagPageFileName = "tag_page.liquid";
        private const string DefaultPostLayout = "post.liquid";

        private const string TagPageRoute = "tag/{}/index.html";
        private const string TagIndexRoute = "tag/index.html";
        private const string DefaultPostRoute = "/post/{}/index.html";

        private static readonly RawEntities.RouteConfiguration DefaultRouteConfiguration = new RawEntities.RouteConfiguration
        {
            TagIndexRoute = TagIndexRoute,
            TagPageRoute = TagPageRoute,
            PostRoutes = new List<string> { DefaultPostRoute },
        };

        private static readonly RawEntities.TemplateConfiguration DefaultLayoutConfiguration = new RawEntities.TemplateConfiguration
        {
            TagIndexTemplate = TagIndexFileName,
            TagPageTemplate = TagPageFileName,
            PostTemplate = DefaultPostLayout,
            IndexTemplate = IndexFileName,
        };

        private readonly IYamlParser yamlParser;
        private readonly IMapper autoMapper;

        public YamlConfigurationReader(IYamlParser yamlParser, IMapper autoMapper)
        {
            this.yamlParser = yamlParser;
            this.autoMapper = autoMapper;
        }

        public (RawEntities.PostMetadata, Entities.PostMetadata) LoadPostMetadata(string metadata)
        {
            var rawMetadata = this.yamlParser.ParseExtras<RawEntities.PostMetadata>(metadata);
            var trueMetadata = this.autoMapper.Map<Entities.PostMetadata>(rawMetadata);

            return (rawMetadata, trueMetadata);
        }

        public (RawEntities.SiteConfiguration, Entities.SiteConfiguration) LoadSiteConfiguration(string configuration)
        {
            var rawSiteConf = this.yamlParser.Parse<RawEntities.SiteConfiguration>(configuration);

            var siteConf = this.autoMapper.Map<Entities.SiteConfiguration>(rawSiteConf);

            // Set defaults
            if (rawSiteConf.RouteConfiguration is null)
            {
                rawSiteConf.RouteConfiguration = DefaultRouteConfiguration;
            }
            else
            {
                rawSiteConf.RouteConfiguration.PostRoutes = rawSiteConf.RouteConfiguration.PostRoutes ?? DefaultRouteConfiguration.PostRoutes;
                rawSiteConf.RouteConfiguration.TagIndexRoute = rawSiteConf.RouteConfiguration.TagIndexRoute ?? DefaultRouteConfiguration.TagIndexRoute;
                rawSiteConf.RouteConfiguration.TagPageRoute = rawSiteConf.RouteConfiguration.TagPageRoute ?? DefaultRouteConfiguration.TagPageRoute;
            }

            if (rawSiteConf.TemplateConfiguration is null)
            {
                rawSiteConf.TemplateConfiguration = DefaultLayoutConfiguration;
            }
            else
            {
                rawSiteConf.TemplateConfiguration.TagIndexTemplate = rawSiteConf.TemplateConfiguration.TagIndexTemplate ?? DefaultLayoutConfiguration.TagIndexTemplate;
                rawSiteConf.TemplateConfiguration.TagPageTemplate = rawSiteConf.TemplateConfiguration.TagPageTemplate ?? DefaultLayoutConfiguration.TagPageTemplate;
                rawSiteConf.TemplateConfiguration.PostTemplate = rawSiteConf.TemplateConfiguration.PostTemplate ?? DefaultLayoutConfiguration.PostTemplate;
                rawSiteConf.TemplateConfiguration.IndexTemplate = rawSiteConf.TemplateConfiguration.IndexTemplate ?? DefaultLayoutConfiguration.IndexTemplate;
            }

            return (rawSiteConf, siteConf);
        }
    }
}
