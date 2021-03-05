namespace Lockdown.Build.Utils
{
    using Lockdown.Build.Entities;
    using Raw = Lockdown.Build.RawEntities;

    public interface IYamlConfigurationReader
    {
        (Raw.SiteConfiguration, SiteConfiguration) LoadSiteConfiguration(string path);

        (Raw.PostMetadata, PostMetadata) LoadPostMetadata(string metadata);
    }
}
