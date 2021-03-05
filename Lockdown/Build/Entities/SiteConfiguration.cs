namespace Lockdown.Build.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DotLiquid;

    public class SiteConfiguration : Drop, IEquatable<SiteConfiguration>
    {
        public string Title { get; set; }

        public string Subtitle { get; set; }

        public string DefaultAuthor { get; set; }

        public bool PagesInTags { get; set; }

        public string Description { get; set; }

        public string SiteUrl { get; set; }

        public List<Link> Social { get; set; }

        public bool Equals(SiteConfiguration other)
        {
            var sameSocial = Enumerable.SequenceEqual(this.Social, other.Social);

            return other.Title == this.Title &&
                other.Subtitle == this.Subtitle &&
                other.Description == this.Description &&
                other.SiteUrl == this.SiteUrl &&
                other.DefaultAuthor == this.DefaultAuthor &&
                other.PagesInTags == this.PagesInTags &&
                sameSocial
                ;
        }
    }
}
