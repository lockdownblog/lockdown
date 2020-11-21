namespace Lockdown.Commands
{
    using System.Collections.Generic;
    using global::Lockdown.Build;
    using global::Lockdown.Run;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.AspNetCore.Hosting;

    internal class RunCommand : CommandBase
    {
        [Option("-o")]
        [LegalFilePath]
        public string Output { get; set; }

        [Option("--port")]
        public int Port { get; set; } = 5000;

        private Lockdown Parent { get; set; }

        public override List<string> CreateArgs()
        {
            var args = this.Parent.CreateArgs();
            args.Add("build");

            if (this.Path != null)
            {
                args.Add("-p");
                args.Add(this.Path);
            }

            return args;
        }

        protected override int OnExecute(CommandLineApplication app)
        {
            var builder = new SiteBuilder(this.Path, this.Output ?? "./_site");
            builder.Build();

            var host = new WebHostBuilder()
                .UseStartup<Startup>()
                .UseKestrel()
                .UseUrls($"http://*:{this.Port}")
                .Build();

            host.Run();

            return 1;
        }
    }
}
