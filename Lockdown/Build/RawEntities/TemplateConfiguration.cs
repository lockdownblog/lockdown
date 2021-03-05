namespace Lockdown.Build.RawEntities
{
    using System;
    using YamlDotNet.Serialization;

    public class TemplateConfiguration : IEquatable<TemplateConfiguration>
    {
        [YamlMember(Alias = "tag-page")]
        public string TagPageTemplate { get; set; }

        [YamlMember(Alias = "tag-index")]
        public string TagIndexTemplate { get; set; }

        [YamlMember(Alias = "post")]
        public string PostTemplate { get; set; }

        [YamlMember(Alias = "index")]
        public string IndexTemplate { get; set; }

        public bool Equals(TemplateConfiguration other)
        {
            return this.TagPageTemplate == other.TagPageTemplate &&
                this.TagIndexTemplate == other.TagIndexTemplate &&
                this.IndexTemplate == other.IndexTemplate &&
                this.PostTemplate == other.PostTemplate;
        }
    }
}
