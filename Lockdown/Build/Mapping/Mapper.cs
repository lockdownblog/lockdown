namespace Lockdown.Build.Mapping
{
    using System;
    using AutoMapper;

    public static class Mapper
    {
        public static IMapper GetMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<InputConfiguration.PostConfiguration, OutputConfiguration.Post>()
                    .ForMember(dest => dest.Context, opt => opt.Ignore())
                    .ForMember(dest => dest.YoutubeId, opt => opt.MapFrom(
                        orig => orig.YouTubeID))
                    .ForMember(
                        dest => dest.DateTime,
                        opt => opt.MapFrom(
                            orig => orig.DateTime.GetValueOrDefault(orig.Date.GetValueOrDefault(DateTime.Now))));

                cfg.CreateMap<InputConfiguration.PostConfiguration, OutputConfiguration.Page>()
                    .ForMember(dest => dest.Context, opt => opt.Ignore())
                    .ForMember(
                        dest => dest.DateTime,
                        opt => opt.MapFrom(
                            orig => orig.DateTime.GetValueOrDefault(orig.Date.GetValueOrDefault(DateTime.Now))));

                cfg.CreateMap<InputConfiguration.SocialLink, OutputConfiguration.SocialLink>()
                    .ForMember(dest => dest.Context, opt => opt.Ignore());

                cfg.CreateMap<InputConfiguration.SiteConfiguration, OutputConfiguration.Site>()
                    .ForMember(dest => dest.Context, opt => opt.Ignore());
            });

            configuration.AssertConfigurationIsValid();

            return configuration.CreateMapper();
        }
    }
}
