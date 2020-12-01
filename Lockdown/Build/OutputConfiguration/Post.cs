namespace Lockdown.Build.OutputConfiguration
{
    using System;
    using DotLiquid;

    public class Post : Drop
    {
        public DateTime DateTime { get; set; }

        public string Title { get; set; }

        public string Image { get; set; }

        public string Summary { get; set; }

        public string YoutubeId { get; set; }

        public string Author { get; set; }

        public string ImageCreditName { get; set; }

        public string ImageCreditUrl { get; set; }

        public string ImageAlt { get; set; }

        public string[] RedirectFrom { get; set; }

        public string Url { get; set; }

        public string[] Tags { get; set; }
    }
}
