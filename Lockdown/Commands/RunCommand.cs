using System;
using McMaster.Extensions.CommandLineUtils;
using Lockdown.Run;
using Lockdown.Build;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;

namespace Lockdown.Commands
{
    class RunCommand : CommandBase
    {

        [Option("-o")]
        [LegalFilePath]
        public string Output { get; set; }



        [Option("--port")]
        public int Port { get; set; } = 5000;

        private Lockdown Parent { get; set; }

        public override List<string> CreateArgs()
        {
            var args = Parent.CreateArgs();
            args.Add("build");

            if (Path != null)
            {
                args.Add("-p");
                args.Add(Path);
            }

            return args;
        }

        protected override int OnExecute(CommandLineApplication app)
        {
            var builder = new SiteBuilder(Path, Output ?? "./_site");
            builder.Build();

            var host = new WebHostBuilder()
                .UseStartup<Startup>()
                .UseKestrel()
                .UseUrls($"http://127.0.0.1:{Port}")
                .Build();

            host.Run();

            return 1;
        }
    }
}
