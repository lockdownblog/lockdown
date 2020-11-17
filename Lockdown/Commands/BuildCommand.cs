using System;
using System.Collections.Generic;
using Lockdown.Build;
using McMaster.Extensions.CommandLineUtils;

namespace Lockdown.Commands
{
    class BuildCommand : CommandBase
    {

        [Option("-o")]
        [LegalFilePath]
        public string Output { get; set; }

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
            return 1;
        }
    }
}
