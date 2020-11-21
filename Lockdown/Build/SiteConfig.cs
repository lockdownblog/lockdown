namespace Lockdown.Build
{
    using System.Collections.Generic;
    using YamlDotNet.Serialization;

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
