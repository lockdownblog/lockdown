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

    internal class RunCommand : CommandBase
    {
        private FileSystemWatcher watcher;

        private SiteBuilder siteBuilder;

        [Option("--port")]
        public int Port { get; set; } = 5000;

        [Option]
        public bool Watch { get; set; } = false;

        private Lockdown Parent { get; set; }

        protected override int OnExecute(CommandLineApplication app)
        {
            this.siteBuilder = new SiteBuilder(this.InputPath, this.OutputPath, this.Parent.Mapper);
            this.siteBuilder.Build();

            if (this.Watch)
            {
                var path = Path.Combine(Environment.CurrentDirectory, this.InputPath);
                this.watcher = new FileSystemWatcher
                {
                    Path = path,
                    NotifyFilter =
                    NotifyFilters.LastWrite |
                    NotifyFilters.CreationTime |
                    NotifyFilters.LastAccess,
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true,
                };
                this.watcher.Created += this.FileChanged;
                this.watcher.Deleted += this.FileChanged;
                this.watcher.Changed += this.FileChanged;
                this.watcher.Renamed += this.FileChanged;
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
            System.Console.WriteLine(file.FullPath);
           //this.siteBuilder.Build();
        }
    }
}
