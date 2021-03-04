namespace Lockdown.Build
{
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using DotLiquid;

    public class HelperFileSystem : DotLiquid.FileSystems.IFileSystem
    {
        private static readonly string[] ValidExtensions = new string[] { "liquid", "html" };
        private readonly IFileSystem fileSystem;
        private readonly Dictionary<string, string> cache;
        private readonly string rootPath;

        public HelperFileSystem(IFileSystem fileSystem, string rootPath)
        {
            this.fileSystem = fileSystem;
            this.rootPath = rootPath;
            this.cache = new Dictionary<string, string>();
        }

        public string FindFile(string path)
        {
            var cleanTemplateName = path.Trim(' ', '\'', '"');
            var extension = this.fileSystem.Path.GetExtension(cleanTemplateName);
            string finalPath = null;

            if (string.IsNullOrEmpty(extension))
            {
                foreach (var validExtension in ValidExtensions)
                {
                    var templatePath = this.fileSystem.Path.Combine(this.rootPath, "templates", $"{cleanTemplateName}.{validExtension}");
                    if (this.fileSystem.File.Exists(templatePath))
                    {
                        finalPath = templatePath;
                        break;
                    }
                }
            }
            else
            {
                if (this.fileSystem.File.Exists(cleanTemplateName))
                {
                    finalPath = cleanTemplateName;
                }
            }

            return finalPath;
        }

        public string ReadTemplateFile(Context context, string templateName)
        {
            templateName = this.FindFile(templateName);
            if (!this.cache.ContainsKey(templateName))
            {
                this.cache[templateName] = this.fileSystem.File.ReadAllText(templateName);
            }

            return this.cache[templateName];
        }
    }
}
