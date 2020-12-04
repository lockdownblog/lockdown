namespace Lockdown.Commands
{
    using global::Lockdown.Build;
    using McMaster.Extensions.CommandLineUtils;

    [Command(Description = "Build your website")]
    internal class BuildCommand : InputOutputCommand
    {
        private readonly ISiteBuilder siteBuilder;

        public BuildCommand(ISiteBuilder siteBuilder)
        {
            this.siteBuilder = siteBuilder;
        }

        private Lockdown Parent { get; set; }

        protected override int OnExecute(CommandLineApplication app)
        {
            this.siteBuilder.Build(this.InputPath, this.OutputPath);
            return 1;
        }
    }
}
