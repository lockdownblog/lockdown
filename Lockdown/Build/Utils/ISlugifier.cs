namespace Lockdown.Build.Utils
{
    public interface ISlugifier
    {
        bool VerifySlug(string slug);

        string Slugify(string text);
    }
}
