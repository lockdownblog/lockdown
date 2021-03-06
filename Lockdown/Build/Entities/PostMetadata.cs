namespace Lockdown.Build.Entities
{
    using System;
    using System.Collections.Generic;

    public class PostMetadata : EntityExtras
    {
        public string Title { get; set; }

        public string Slug { get; set; }

        public string Layout { get; set; }

        public DateTime? Date { get; set; }

        public string Summary { get; set; }

        public string Author { get; set; }

        public DateTime? DateTime { get; set; }

        public string Image { get; set; }

        public Link[] Tags { get; set; }

        public List<string> PostRoutes { get; set; }

        public string[] TagArray { get; set; }

        public string CanonicalUrl { get; set; }
    }
}