namespace Lockdown.Commands
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using McMaster.Extensions.CommandLineUtils;

    internal class NewCommand : CommandBase
    {
        private string fullOutputPath;

        [Argument(0)]
        [Required]
        public string BlogDir { get; }

        private Lockdown Parent { get; set; }

        protected override int OnExecute(CommandLineApplication app)
        {
            this.fullOutputPath = Path.GetFullPath(this.BlogDir);
            Console.WriteLine(this.fullOutputPath);
            return 1;
        }
    }
}
