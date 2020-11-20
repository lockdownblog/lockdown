namespace Lockdown.LiquidEntities
{
    using DotLiquid;
    using System.Collections.Generic;

    public class Social : Drop
    {
        public string Name { get; set; }

        public string Link { get; set; }
    }

    public class Site : Drop
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public List<Social> Social {get; set;}
    }
}
