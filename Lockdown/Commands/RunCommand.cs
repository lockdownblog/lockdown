namespace Lockdown.Commands
{
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using Lockdown.Build;
    using Lockdown.Run;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

    public class RunCommand
    {
        private readonly IConsole console;
        private readonly ISiteBuilder siteBuilder;
        private readonly IFileSystem fileSystem;

        public RunCommand(IConsole console, ISiteBuilder siteBuilder, IFileSystem fileSystem)
        {
            this.console = console;
            this.siteBuilder = siteBuilder;
            this.fileSystem = fileSystem;
        }

        [Option("-r|--root")]
        public string InputPath { get; set; } = "./";

        [Option("-o|--output")]
        public string OutputPath { get; set; } = "./_site";

        [Option("-p|--port")]
        public int Port { get; set; } = 5000;

        public int OnExecute()
        {
            // Construye el sitio web en OutputPath
            this.siteBuilder.Build(this.InputPath, this.OutputPath);

            // Monta el sitio web
            var raizWeb = this.fileSystem.Path.Combine(this.fileSystem.Directory.GetCurrentDirectory(), this.OutputPath);

            var settings = new Dictionary<string, string>
            {
                { "webRoot", raizWeb },
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

            return 0;
        }
    }
}
