namespace Lockdown.Build.RawEntities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using YamlDotNet.Serialization;

    public class SiteConfiguration : IEquatable<SiteConfiguration>
    {
        [YamlMember(Alias = "title")]
        public string Title { get; set; }

        [YamlMember(Alias = "subtitle")]
        public string Subtitle { get; set; }

        [YamlMember(Alias = "description")]
        public string Description { get; set; }

        [YamlMember(Alias = "site-url")]
        public string SiteUrl { get; set; }

        [YamlMember(Alias = "default-author")]
        public string DefaultAuthor { get; set; }

        [YamlMember(Alias = "pages-in-tags")]
        public bool PagesInTags { get; set; }

        [YamlMember(Alias = "social")]
        public List<Link> Social { get; set; }

        [YamlMember(Alias = "templates")]
        public TemplateConfiguration TemplateConfiguration { get; set; }

        [YamlMember(Alias = "routes")]
        public RouteConfiguration RouteConfiguration { get; set; }

        public bool Equals(SiteConfiguration other)
        {
            var sameTemplateConfiguration = (other.TemplateConfiguration is null &&
                this.TemplateConfiguration is null) ||
                (other.TemplateConfiguration?.Equals(this.TemplateConfiguration) ?? false);

            var sameRouteConfiguration = (other.RouteConfiguration is null &&
                this.RouteConfiguration is null) ||
                (other.RouteConfiguration?.Equals(this.RouteConfiguration) ?? false);

            var sameSocial = Enumerable.SequenceEqual(this.Social, other.Social);

            return other.Title == this.Title &&
                other.Subtitle == this.Subtitle &&
                other.Description == this.Description &&
                other.SiteUrl == this.SiteUrl &&
                other.DefaultAuthor == this.DefaultAuthor &&
                other.PagesInTags == this.PagesInTags &&
                sameSocial &&
                sameTemplateConfiguration &&
                sameRouteConfiguration
                ;
        }
    }
}
