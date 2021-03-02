namespace Lockdown.Build.Entities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DotLiquid;

    public class TagCollection : Drop, IDictionary<string, TagGroup>
    {
        private readonly Dictionary<string, TagGroup> innerDict;

        public TagCollection()
        {
            this.innerDict = new Dictionary<string, TagGroup>();
        }

        public ICollection<string> Keys => this.Keys;

        public ICollection<TagGroup> Values => this.innerDict.Values;

        public int Count => this.innerDict.Count;

        public bool IsReadOnly => false;

        public TagGroup this[string key]
        {
            get => this.innerDict[key];
            set => this.innerDict[key] = value;
        }

        public void Add(string key, TagGroup value)
        {
            this.innerDict.Add(key, value);
        }

        public void Add(KeyValuePair<string, TagGroup> item)
        {
            this.innerDict.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this.innerDict.Clear();
        }

        public bool Contains(KeyValuePair<string, TagGroup> item)
        {
            return this.innerDict.ContainsKey(item.Key);
        }

        public bool ContainsKey(string key)
        {
            return this.innerDict.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, TagGroup>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, TagGroup>> GetEnumerator()
        {
            return this.innerDict.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return this.innerDict.Remove(key);
        }

        public bool Remove(KeyValuePair<string, TagGroup> item)
        {
            return this.innerDict.Remove(item.Key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TagGroup value)
        {
            return this.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}