namespace Lockdown.Tests
{
    using System.Collections.Generic;
    using Lockdown.Build.Entities;
    using Lockdown.Build.Utils;
    using Shouldly;
    using Xunit;
    using Raw = Lockdown.Build.RawEntities;

    public class YamlConfigurationReaderTests
    {
        private readonly YamlConfigurationReader configurationReader;

        public YamlConfigurationReaderTests()
        {
            this.configurationReader = new YamlConfigurationReader(new YamlParser(), Build.Mapping.Mapper.GetMapper());
        }

        public static IEnumerable<object[]> GetData()
        {
            var title = "Lockdown";
            var subtitle = "Static website generator";
            var description = "Some description with weird cháractèrs";
            var defaultAuthor = "Lockdonw Blogmaster";
            var siteUrl = "https://github.com/lockdownblog/lockdown";
            var gitHub = "https://github.com/lockdownblog/lockdown";

            // First example
            var yamlString1 = @$"title: ""{title}""  
subtitle: {subtitle}  
description: {description}  
default-author: {defaultAuthor}
site-url: {siteUrl}
social:
  - text: GitHub
    url: {gitHub}
#routes:
#    post-routes:
#        - /post/{{}}/index.html
#        - /post/{{}}.html
#    tag-page-route: tag/{{}}/index.html
#    tag-index-route: tag/index.html
";

            Raw.SiteConfiguration rawConfiguration = new Raw.SiteConfiguration
            {
                Title = title,
                Subtitle = subtitle,
                Description = description,
                DefaultAuthor = defaultAuthor,
                SiteUrl = siteUrl,
                Social = new List<Raw.Link>
                {
                    new Raw.Link { Text = "GitHub", Url = gitHub },
                },
                RouteConfiguration = new Raw.RouteConfiguration
                {
                    TagPageRoute = "tag/{}/index.html",
                    TagIndexRoute = "tag/index.html",
                    PostRoutes = new List<string> { "/post/{}/index.html" },
                },
                TemplateConfiguration = new Raw.TemplateConfiguration
                {
                    IndexTemplate = "index.liquid",
                    TagPageTemplate = "tag_page.liquid",
                    TagIndexTemplate = "tag_index.liquid",
                    PostTemplate = "post.liquid",
                },
            };

            SiteConfiguration configuration = new SiteConfiguration
            {
                Title = title,
                Subtitle = subtitle,
                Description = description,
                DefaultAuthor = defaultAuthor,
                SiteUrl = siteUrl,
                Social = new List<Link>
                {
                    new Link { Text = "GitHub", Url = gitHub },
                },
            };

            yield return new object[] { yamlString1, rawConfiguration, configuration };

            // Second example
            var yamlString2 = @$"title: ""{title}""  
subtitle: {subtitle}  
description: {description}  
default-author: {defaultAuthor}
site-url: {siteUrl}
social:
  - text: GitHub
    url: {gitHub}
routes:
    post-routes:
        - /some/route/{{}}/index.html
#        - /post/{{}}.html
    tag-page-route: tags/{{}}/index.html
    tag-index-route: tags/index.html
";

            rawConfiguration = new Raw.SiteConfiguration
            {
                Title = title,
                Subtitle = subtitle,
                Description = description,
                DefaultAuthor = defaultAuthor,
                SiteUrl = siteUrl,
                Social = new List<Raw.Link>
                {
                    new Raw.Link { Text = "GitHub", Url = gitHub },
                },
                RouteConfiguration = new Raw.RouteConfiguration
                {
                    TagPageRoute = "tags/{}/index.html",
                    TagIndexRoute = "tags/index.html",
                    PostRoutes = new List<string> { "/some/route/{}/index.html" },
                },
                TemplateConfiguration = new Raw.TemplateConfiguration
                {
                    IndexTemplate = "index.liquid",
                    TagPageTemplate = "tag_page.liquid",
                    TagIndexTemplate = "tag_index.liquid",
                    PostTemplate = "post.liquid",
                },
            };

            yield return new object[] { yamlString2, rawConfiguration, configuration };

            // Third example
            var yamlString3 = @$"title: ""{title}""  
subtitle: {subtitle}  
description: {description}  
default-author: {defaultAuthor}
site-url: {siteUrl}
social:
  - text: GitHub
    url: {gitHub}
routes:
    post-routes:
        - /some/route/{{}}/index.html
    tag-page-route: tags/{{}}/index.html
    tag-index-route: tags/index.html
templates:
    post: default-post-template.html
    
";

            rawConfiguration = new Raw.SiteConfiguration
            {
                Title = title,
                Subtitle = subtitle,
                Description = description,
                DefaultAuthor = defaultAuthor,
                SiteUrl = siteUrl,
                Social = new List<Raw.Link>
                {
                    new Raw.Link { Text = "GitHub", Url = gitHub },
                },
                RouteConfiguration = new Raw.RouteConfiguration
                {
                    TagPageRoute = "tags/{}/index.html",
                    TagIndexRoute = "tags/index.html",
                    PostRoutes = new List<string> { "/some/route/{}/index.html" },
                },
                TemplateConfiguration = new Raw.TemplateConfiguration
                {
                    IndexTemplate = "index.liquid",
                    TagPageTemplate = "tag_page.liquid",
                    TagIndexTemplate = "tag_index.liquid",
                    PostTemplate = "default-post-template.html",
                },
            };

            yield return new object[] { yamlString3, rawConfiguration, configuration };
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void TestLoadSiteConfiguration(string inputConfiguration, Raw.SiteConfiguration expectedRaw, SiteConfiguration expected)
        {
            var (actualRaw, actual) = this.configurationReader.LoadSiteConfiguration(inputConfiguration);

            Assert.True(actualRaw.Equals(expectedRaw));
            Assert.True(actual.Equals(expected));
        }
    }
}
