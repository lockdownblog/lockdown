namespace Lockdown.LiquidEntities
{
    using System.Collections.Generic;
    using DotLiquid;

    public class Site : Drop
    {
        public string Title { get; set; }

        public string Subtitle { get; set; }

        public List<Social> Social { get; set; }
    }
}
