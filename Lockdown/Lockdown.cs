using System;
using System.Collections.Generic;
using System.Reflection;
using DotLiquid;
using DotLiquid.FileSystems;
using DotLiquid.NamingConventions;
using Lockdown.Build;
using Lockdown.Commands;
using McMaster.Extensions.CommandLineUtils;

namespace Lockdown
{
    [Command("lockdown")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        typeof(BuildCommand)
    )]
    [Subcommand(
        typeof(RunCommand)
    )]
    class Lockdown : CommandBase
    {
        public static void Main(string[] args) => CommandLineApplication.Execute<Lockdown>(args);

        protected override int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }

        private static string GetVersion()
            => typeof(Lockdown).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        public override List<string> CreateArgs()
        {
            return new List<string>();
        }
    }
}
