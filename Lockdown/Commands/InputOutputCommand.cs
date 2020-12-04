namespace Lockdown.Commands
{
    using McMaster.Extensions.CommandLineUtils;

    [HelpOption("-h|--help")]
    public abstract class InputOutputCommand : CommandBase
    {
        [Option("-r|--root", Description = "The root path of your website")]
        [LegalFilePath]
        public string InputPath { get; set; } = "./";

        [Option("-o|--out", Description = "The output directory where your built website will be stored")]
        [LegalFilePath]
        public string OutputPath { get; set; } = "./_site";

        protected override int OnExecute(CommandLineApplication app) => 0;
    }
}
