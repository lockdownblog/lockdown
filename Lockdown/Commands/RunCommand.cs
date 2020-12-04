namespace Lockdown.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using global::Lockdown.Build;
    using global::Lockdown.Run;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

    [Command(Description = "Launch a web server running your web site")]
    internal class RunCommand : InputOutputCommand, IDisposable
    {
        private static readonly string[] WatchablePaths = new string[]
        {
            "content",
            "static",
            "site.yml",
        };

        private readonly ISiteBuilder siteBuilder;

        private FileSystemWatcher fileWatcher;

        public RunCommand(ISiteBuilder siteBuilder)
        {
            this.siteBuilder = siteBuilder;
        }

        [Option("-p|--port", Description = "Specify a port where your app should run, the default is 5000")]
        public int Port { get; set; } = 5000;

        [Option(Description = "Set this flag if you want the changes to your source files to be incorporated in your running app")]
        public bool Watch { get; set; } = false;

        private Lockdown Parent { get; set; }

        public void Dispose()
        {
            if (this.fileWatcher != null)
            {
                this.fileWatcher.Dispose();
            }
        }

        protected override int OnExecute(CommandLineApplication app)
        {
            this.siteBuilder.Build(this.InputPath, this.OutputPath);

            if (this.Watch)
            {
                var path = Path.Combine(Environment.CurrentDirectory, this.InputPath);
                this.fileWatcher = new FileSystemWatcher
                {
                    Path = path,
                    NotifyFilter = NotifyFilters.LastWrite,
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true,
                };
                this.fileWatcher.Created += this.FileChanged;
                this.fileWatcher.Deleted += this.FileChanged;
                this.fileWatcher.Changed += this.FileChanged;
                this.fileWatcher.Renamed += this.FileChanged;
            }

            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), this.OutputPath);

            var settings = new Dictionary<string, string>
            {
                { "webRoot", webRoot },
            };
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(settings);

            var host = new WebHostBuilder()
                .UseConfiguration(configBuilder.Build())
                .UseStartup<Startup>()
                .UseKestrel()
                .UseUrls($"http://*:{this.Port}")
                .Build();

            host.Run();

            return 1;
        }

        private void FileChanged(object sender, FileSystemEventArgs file)
        {
            // TODO: This fires twice, fix that!
            var ok = false;
            foreach (var watchablePath in WatchablePaths)
            {
                var startsWith = Path.GetFullPath(Path.Combine(this.InputPath, watchablePath));
                ok = Path.GetFullPath(file.FullPath).StartsWith(startsWith);
                if (ok)
                {
                    break;
                }
            }

            if (ok)
            {
                this.siteBuilder.Build(this.InputPath, this.OutputPath);
            }
        }
    }
}
