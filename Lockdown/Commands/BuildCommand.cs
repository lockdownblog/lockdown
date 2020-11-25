namespace Lockdown.Commands
{
    using System.Collections.Generic;
    using global::Lockdown.Build;
    using McMaster.Extensions.CommandLineUtils;

    internal class BuildCommand : CommandBase
    {
        private Lockdown Parent { get; set; }

        protected override int OnExecute(CommandLineApplication app)
        {
            var builder = new SiteBuilder(this.InputPath, this.OutputPath, this.Parent.Mapper);
            builder.Build();
            return 1;
        }
    }
}
