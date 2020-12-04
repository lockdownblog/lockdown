namespace Lockdown.Commands
{
    using System.Collections.Generic;
    using global::Lockdown.Build;
    using McMaster.Extensions.CommandLineUtils;

    internal class BuildCommand : InputOutputCommand
    {
        private Lockdown Parent { get; set; }

        protected override int OnExecute(CommandLineApplication app)
        {
            var builder = new SiteBuilder(this.InputPath, this.OutputPath);
            builder.Build();
            return 1;
        }
    }
}
