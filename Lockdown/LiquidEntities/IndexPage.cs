namespace Lockdown.LiquidEntities
{
    using System.Collections;
    using System.Collections.Generic;
    using DotLiquid;

    public class IndexPage : Drop, IEnumerable<IndexPost>
    {
        public int CurrentPage { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public int NextPage { get; set; }
        public int PreviousPage { get; set; }
        public string NextPageUrl { get; set; }
        public string PreviousPageUrl { get; set; }
        public int PageCount { get; set; }
        public List<IndexPost> Posts { get; set; }

        public IEnumerator<IndexPost> GetEnumerator()
        {
            foreach(var post in this.Posts)
            {
                yield return post;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach(var post in this.Posts)
            {
                yield return post;
            }
        }
    }
}
