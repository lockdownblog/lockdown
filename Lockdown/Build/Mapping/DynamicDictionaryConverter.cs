namespace Lockdown.Build.Mapping
{
    using System.Collections.Generic;
    using AutoMapper;
    using Raw = Lockdown.Build.RawEntities;

    internal class DynamicDictionaryConverter : IValueConverter<Raw.DynamicDictionary, Dictionary<string, string>>
    {
        public Dictionary<string, string> Convert(Raw.DynamicDictionary sourceMember, ResolutionContext context)
        {
            return sourceMember.Dictionary;
        }
    }
}