namespace Lockdown.Build.Utils
{
    using Slugify;

    public class Slugifier : ISlugifier
    {
        private readonly SlugHelper slugHelper;

        public Slugifier()
        {
            this.slugHelper = new SlugHelper();
        }

        public string Slugify(string text)
        {
            return this.slugHelper.GenerateSlug(text);
        }

        public bool VerifySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return false;
            }

            var trueSlug = this.Slugify(slug);

            return trueSlug.Equals(slug);
        }
    }
}
