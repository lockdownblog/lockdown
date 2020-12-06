namespace Lockdown.Build.Utils
{
    using System.Collections.Generic;

    public static class DictionaryExtensions
    {
        public static void AddDefault<T>(this Dictionary<string, List<T>> content, string key, T element)
        {
            if (content.TryGetValue(key, out var collection))
            {
                collection.Add(element);
            }
            else
            {
                var newCollection = new List<T>
                {
                    element,
                };
                content.Add(key, newCollection);
            }
        }
    }
}