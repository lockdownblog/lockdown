using System;
using System.IO;
using System.Text.RegularExpressions;
using DotLiquid;
using DotLiquid.Exceptions;
using DotLiquid.FileSystems;

namespace Lockdown.Build
{
    public class LockdownFileSystem : IFileSystem
    {
        public string Root { get; set; }

        public LockdownFileSystem(string root)
        {
            Root = Path.GetFullPath(root);
        }

        public string ReadTemplateFile(Context context, string templateName)
        {
            string templatePath = (string)context[templateName];
            string fullPath = FullPath(templatePath);
            if (!File.Exists(fullPath))
                throw new FileSystemException("The template file {0} does not exist", templatePath);
            return File.ReadAllText(fullPath);
        }

        public string FullPath(string templatePath)
        {
            if (templatePath == null || !Regex.IsMatch(templatePath, @"^[^.\/][a-zA-Z0-9_\/]+$"))
                throw new FileSystemException("Illegal template path name {0}", templatePath);

            string fullPath = templatePath.Contains("/")
                ? Path.Combine(Path.Combine(Root, Path.GetDirectoryName(templatePath)), string.Format("_{0}.liquid", Path.GetFileName(templatePath)))
                : Path.Combine(Root, string.Format("_{0}.liquid", templatePath));

            string escapedPath = Regex.Escape(Root);
            
            if (!fullPath.StartsWith(escapedPath))
            {
                throw new FileSystemException("The path {0} does not exist in the specified root {1}",fullPath, escapedPath);
            }

            return fullPath;
        }
    }
}
