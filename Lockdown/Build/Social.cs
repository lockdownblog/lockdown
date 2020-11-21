namespace Lockdown.Build
{
    using System.Collections.Generic;
    using YamlDotNet.Serialization;

    public class Social
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "link")]
        public string Link { get; set; }
    }
}
