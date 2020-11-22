namespace Lockdown.Commands
{
    using System.Collections.Generic;
    using global::Lockdown.Build;
    using McMaster.Extensions.CommandLineUtils;

    internal class BuildCommand : CommandBase
    {
        [Option("-o")]
        [LegalFilePath]
        public string Output { get; set; }

        private Lockdown Parent { get; set; }

        public override List<string> CreateArgs()
        {
            var args = this.Parent.CreateArgs();
            args.Add("build");

            if (this.InputPath != null)
            {
                args.Add("-p");
                args.Add(this.InputPath);
            }

            return args;
        }

        protected override int OnExecute(CommandLineApplication app)
        {
            var builder = new SiteBuilder(this.InputPath, this.Output ?? "./_site");
            builder.Build();
            return 1;
        }
    }
}
