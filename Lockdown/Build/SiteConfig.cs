namespace Lockdown.Build
{
    using YamlDotNet.Serialization;
    using System.Collections.Generic;

    public class Social
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "link")]
        public string Link { get; set; }
    }

    public class SiteConfig
    {
        [YamlMember(Alias = "title")]
        public string Title { get; set; }

        [YamlMember(Alias = "subtitle")]
        public string Subtitle { get; set; }

        [YamlMember(Alias = "social")]
        public List<Social> Social { get; set; }
    }
}
