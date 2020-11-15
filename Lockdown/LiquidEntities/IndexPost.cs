using System;
using DotLiquid;

namespace Lockdown.LiquidEntities
{
    public class IndexPost : Drop
    {
        public string DateTime { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string ImageCreditName { get; set; }
        public string ImageCreditUrl { get; set; }
        public string ImageAlt { get; set; }
        public string[] RedirectFrom { get; set; }
        public string Url { get; set; }
        public string[] Tags { get; set; }
        public DateTime Dt { get; set; }
    }
}
