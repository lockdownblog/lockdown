namespace Lockdown
{
    using System.IO.Abstractions;
    using System.Reflection;
    using AutoMapper;
    using global::Lockdown.Build;
    using global::Lockdown.Commands;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Extensions.DependencyInjection;

    [Command("lockdown", FullName = "Lockdown", Description = "A static website generator")]
    [Subcommand(typeof(BuildCommand))]
    [Subcommand(typeof(RunCommand))]
    [Subcommand(typeof(NewCommand))]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    public class Lockdown : CommandBase
    {
        private readonly IConsole console;

        public Lockdown(IConsole console)
        {
            this.console = console;
        }

        public IMapper Mapper { get; private set; }

        public static int Main(string[] args)
        {
            var mapper = Build.Mapping.Mapper.GetMapper();
            var services = new ServiceCollection()
                .AddSingleton<ISiteBuilder, SiteBuilder>()
                .AddSingleton<IFileSystem, FileSystem>()
                .AddSingleton(mapper)
                .BuildServiceProvider();

            var app = new CommandLineApplication<Lockdown>();

            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);

            return app.Execute(args);
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
