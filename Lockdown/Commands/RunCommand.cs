namespace Lockdown.Commands
{
    using System.Collections.Generic;
    using System.IO;
    using global::Lockdown.Build;
    using global::Lockdown.Run;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

    internal class RunCommand : CommandBase
    {
        [Option("--port")]
        public int Port { get; set; } = 5000;

        private Lockdown Parent { get; set; }

        protected override int OnExecute(CommandLineApplication app)
        {
            var builder = new SiteBuilder(this.InputPath, this.OutputPath, this.Parent.Mapper);
            builder.Build();

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
    }
}
