namespace Lockdown.Build.InputConfiguration
{
    using System;
    using System.Linq;
    using YamlDotNet.Serialization;

    public class PostConfiguration
    {
        [YamlMember(Alias = "tags")]
        public string Tags { get; set; }

        [YamlMember(Alias = "layout")]
        public string Layout { get; set; }

        [YamlMember(Alias = "youtube_id")]
        public string YouTubeID { get; set; }

        [YamlMember(Alias = "datetime")]
        public DateTime? DateTime { get; set; }

        [YamlMember(Alias = "date")]
        public DateTime? Date { get; set; }

        [YamlMember(Alias = "title")]
        public string Title { get; set; }

        [YamlMember(Alias = "summary")]
        public string Summary { get; set; }

        [YamlMember(Alias = "author")]
        public string Author { get; set; }

        [YamlMember(Alias = "image")]
        public string Image { get; set; }

        [YamlMember(Alias = "image_credit_name")]
        public string ImageCreditName { get; set; }

        [YamlMember(Alias = "image_credit_url")]
        public string ImageCreditUrl { get; set; }

        [YamlMember(Alias = "image_alt")]
        public string ImageAlt { get; set; }

        [YamlMember(Alias = "redirect_from")]
        public string[] RedirectFrom { get; set; }

        [YamlIgnore]
        public string Url { get; set; }

        [YamlIgnore]
        public string[] GetTags => this.Tags?
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToArray();
    }
}
