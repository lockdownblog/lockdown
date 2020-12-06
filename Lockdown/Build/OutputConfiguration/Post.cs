namespace Lockdown.Build.OutputConfiguration
{
    using System;
    using DotLiquid;

    public class Post : Content
    {
        public string Image { get; set; }

        public string YoutubeId { get; set; }

        public string[] RedirectFrom { get; set; }

        public string Url { get; set; }
    }
}
