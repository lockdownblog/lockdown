namespace Lockdown.Build.Entities
{
    using System.Collections.Generic;
    using DotLiquid;

    public class EntityExtras : Drop
    {
        public Dictionary<string, string> Extras { get; set; }
    }
}
