namespace Lockdown.Build.RawEntities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using YamlDotNet.Serialization;

    public class RouteConfiguration : IEquatable<RouteConfiguration>
    {
        [YamlMember(Alias = "post-routes")]
        public List<string> PostRoutes { get; set; }

        [YamlMember(Alias = "tag-index-route")]
        public string TagIndexRoute { get; set; }

        [YamlMember(Alias = "tag-page-route")]
        public string TagPageRoute { get; set; }

        public bool Equals(RouteConfiguration other)
        {
            return Enumerable.SequenceEqual(this.PostRoutes, other.PostRoutes) &&
                this.TagIndexRoute == other.TagIndexRoute &&
                this.TagPageRoute == other.TagPageRoute;
        }
    }
}
