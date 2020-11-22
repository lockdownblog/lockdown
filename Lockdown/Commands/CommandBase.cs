namespace Lockdown.Commands
{
    using System;
    using System.Collections.Generic;
    using McMaster.Extensions.CommandLineUtils;

    [HelpOption("--help")]
    internal abstract class CommandBase
    {
        [Option("-p")]
        [LegalFilePath]
        public string InputPath { get; set; } = "./";

        public abstract List<string> CreateArgs();

        protected virtual int OnExecute(CommandLineApplication app)
        {
            var args = this.CreateArgs();

            Console.WriteLine("Called " + ArgumentEscaper.EscapeAndConcatenate(args));
            return 0;
        }
    }
}
