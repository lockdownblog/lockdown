namespace Lockdown
{
    using System.Reflection;
    using AutoMapper;
    using global::Lockdown.Commands;
    using McMaster.Extensions.CommandLineUtils;

    [Command("lockdown")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(BuildCommand))]
    [Subcommand(typeof(RunCommand))]
    [Subcommand(typeof(NewCommand))]
    public class Lockdown : CommandBase
    {
        public Lockdown()
        {
        }

        public IMapper Mapper { get; private set; }

        public static void Main(string[] args) => CommandLineApplication.Execute<Lockdown>(args);

        protected override int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }

        private static string GetVersion()
            => typeof(Lockdown).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}
