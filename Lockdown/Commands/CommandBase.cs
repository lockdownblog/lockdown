using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;

namespace Lockdown.Commands
{
    [HelpOption("--help")]
    abstract class CommandBase
    {

        [Option("-p")]
        [LegalFilePath]
        public string Path { get; set; } = "./";

        public abstract List<string> CreateArgs();

        protected virtual int OnExecute(CommandLineApplication app)
        {
            var args = CreateArgs();

            Console.WriteLine("Called " + ArgumentEscaper.EscapeAndConcatenate(args));
            return 0;
        }
    }
}
