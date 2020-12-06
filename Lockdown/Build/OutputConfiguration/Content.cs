namespace Lockdown.Build.OutputConfiguration
{
    using System;
    using DotLiquid;

    public class Content : Drop
    {
        public string Title { get; set; }

        public DateTime DateTime { get; set; }

        public string Summary { get; set; }

        public string Author { get; set; }

        public string ImageCreditName { get; set; }

        public string ImageCreditUrl { get; set; }

        public string ImageAlt { get; set; }

        public string[] Tags { get; set; }
    }
}