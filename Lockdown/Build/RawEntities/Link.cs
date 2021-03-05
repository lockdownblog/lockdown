namespace Lockdown.Build.RawEntities
{
    using System;
    using YamlDotNet.Serialization;

    public class Link : IEquatable<Link>
    {
        [YamlMember(Alias = "text")]
        public string Text { get; set; }

        [YamlMember(Alias = "url")]
        public string Url { get; set; }

        public bool Equals(Link other)
        {
            return this.Text == other.Text && this.Url == other.Url;
        }
    }
}
