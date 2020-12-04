namespace Lockdown.Build.InputConfiguration
{
    using YamlDotNet.Serialization;

    public class SocialLink
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "link")]
        public string Link { get; set; }
    }
}
