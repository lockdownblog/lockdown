namespace Lockdown.Build.InputConfiguration
{
    using System.Collections.Generic;
    using YamlDotNet.Serialization;

    public class SiteConfiguration
    {
        [YamlMember(Alias = "title")]
        public string Title { get; set; }

        [YamlMember(Alias = "subtitle")]
        public string Subtitle { get; set; }

        [YamlMember(Alias = "default-author")]
        public string DefaultAuthor { get; set; }

        [YamlMember(Alias = "pages-in-tags")]
        public bool PagesInTags { get; set; }

        [YamlMember(Alias = "social")]
        public List<SocialLink> Social { get; set; }
    }
}
