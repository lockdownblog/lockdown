namespace Lockdown.Build.Entities
{
    using System;
    using DotLiquid;

    public class Link : Drop, IEquatable<Link>
    {
        public string Text { get; set; }

        public string Url { get; set; }

        public bool Equals(Link other)
        {
            return this.Text == other.Text &&
                this.Url == other.Url;
        }
    }
}
