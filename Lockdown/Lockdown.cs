namespace Lockdown
{
    using System.Collections.Generic;
    using System.Reflection;
    using global::Lockdown.Commands;
    using McMaster.Extensions.CommandLineUtils;

    [Command("lockdown")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(BuildCommand))]
    [Subcommand(typeof(RunCommand))]
    internal class Lockdown : CommandBase
    {
        public static void Main(string[] args) => CommandLineApplication.Execute<Lockdown>(args);

        public override List<string> CreateArgs()
        {
            return new List<string>();
        }

        protected override int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }

        private static string GetVersion()
            => typeof(Lockdown).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}
