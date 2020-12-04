namespace Lockdown.Commands
{
    using McMaster.Extensions.CommandLineUtils;

    [HelpOption("--help")]
    public abstract class InputOutputCommand : CommandBase
    {
        [Option("-p")]
        [LegalFilePath]
        public string InputPath { get; set; } = "./";

        [Option("-o")]
        [LegalFilePath]
        public string OutputPath { get; set; } = "./_site";

        protected virtual int OnExecute(CommandLineApplication app)
        {
            return 0;
        }
    }
}
