namespace Lockdown.Commands
{
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using McMaster.Extensions.CommandLineUtils;

    internal class NewCommand : CommandBase
    {
        private string fullOutputPath;

        [Argument(0)]
        [Required]
        public string BlogDir { get; }

        private Lockdown Parent { get; set; }

        protected override int OnExecute(CommandLineApplication app)
        {
            this.fullOutputPath = Path.GetFullPath(this.BlogDir);

            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();

            foreach (var resource in resources)
            {
                var parts = resource.Split('.');
                var parent = string.Join("/", parts.Skip(2).SkipLast(2));
                var filename = string.Join('.', parts.TakeLast(2));

                var fullDir = Directory.CreateDirectory(Path.Combine(this.BlogDir, parent));
                var fullPath = Path.Combine(this.BlogDir, parent, filename);

                using var stream = assembly.GetManifestResourceStream(resource);
                using var reader = new StreamReader(stream);
                using var file = new System.IO.StreamWriter(fullPath);
                file.Write(reader.ReadToEnd());
            }

            return 1;
        }
    }
}
