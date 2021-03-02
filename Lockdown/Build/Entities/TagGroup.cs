namespace Lockdown.Build.Entities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using DotLiquid;

    public class TagGroup : Drop, IList<PostMetadata>
    {
        private readonly List<PostMetadata> postMetadatas;

        public TagGroup(Link link, string slug)
        {
            this.Link = link;
            this.Slug = slug;
            this.postMetadatas = new List<PostMetadata>();
        }

        public int Count => this.postMetadatas.Count;

        public bool IsReadOnly => false;

        public string Slug { get; private set; }

        public Link Link { get; private set; }

        public PostMetadata this[int index]
        {
            get => this.postMetadatas[index];
            set => this.postMetadatas[index] = value;
        }

        public void Add(PostMetadata item)
        {
            this.postMetadatas.Add(item);
        }

        public void Clear()
        {
            this.postMetadatas.Clear();
        }

        public bool Contains(PostMetadata item)
        {
            return this.postMetadatas.Contains(item);
        }

        public void CopyTo(PostMetadata[] array, int arrayIndex)
        {
            this.postMetadatas.CopyTo(array, arrayIndex);
        }

        public IEnumerator<PostMetadata> GetEnumerator()
        {
            return this.postMetadatas.GetEnumerator();
        }

        public int IndexOf(PostMetadata item)
        {
            return this.postMetadatas.IndexOf(item);
        }

        public void Insert(int index, PostMetadata item)
        {
            this.postMetadatas.Insert(index, item);
        }

        public bool Remove(PostMetadata item)
        {
            return this.postMetadatas.Remove(item);
        }

        public void RemoveAt(int index)
        {
            this.postMetadatas.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.postMetadatas.GetEnumerator();
        }
    }
}
