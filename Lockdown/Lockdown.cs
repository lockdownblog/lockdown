namespace Lockdown
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AutoMapper;
    using global::Lockdown.Build;
    using global::Lockdown.Commands;
    using global::Lockdown.LiquidEntities;
    using McMaster.Extensions.CommandLineUtils;

    [Command("lockdown")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(BuildCommand))]
    [Subcommand(typeof(RunCommand))]
    internal class Lockdown : CommandBase
    {
        public Lockdown()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PostFrontMatter, IndexPost>()
                    .ForMember(dest => dest.Context, opt => opt.Ignore())
                    .ForMember(
                        dest => dest.DateTime,
                        opt => opt.MapFrom(
                            orig => orig.DateTime.GetValueOrDefault(orig.Date.GetValueOrDefault(DateTime.Now))));
            });

            configuration.AssertConfigurationIsValid();

            this.Mapper = configuration.CreateMapper();
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
