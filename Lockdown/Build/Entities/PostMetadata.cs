namespace Lockdown.Build.Entities
{
    using System;
    using DotLiquid;

    public class PostMetadata : Drop
    {
        public string Title { get; set; }

        public DateTime? Date { get; set; }

        public string Summary { get; set; }

        public string Author { get; set; }

        public DateTime? DateTime { get; set; }

        public string Image { get; set; }

        public Link[] Tags { get; set; }

        public string[] TagArray { get; set; }

        public string CanonicalUrl { get; set; }
    }
}