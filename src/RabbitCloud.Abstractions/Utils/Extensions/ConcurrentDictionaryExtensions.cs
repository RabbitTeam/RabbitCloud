using System.Collections.Concurrent;

namespace RabbitCloud.Abstractions.Utils.Extensions
{
    public static class ConcurrentDictionaryExtensions
    {
        public static TValue Get<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            return value;
        }

        public static TValue Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryRemove(key, out value);
            return value;
        }
    }
}