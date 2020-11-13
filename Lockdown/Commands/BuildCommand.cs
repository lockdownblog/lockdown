using System;
using System.Collections.Generic;
using McMaster.Extensions.CommandLineUtils;

namespace Lockdown.Commands
{
    class BuildCommand : CommandBase
    {

        [Option("-p")]
        [LegalFilePath]
        public string Path { get; set; }

        private LockdownCommand Parent { get; set; }

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
    }
}
