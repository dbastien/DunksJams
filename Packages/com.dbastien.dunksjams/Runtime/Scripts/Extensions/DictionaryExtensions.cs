using System;
using System.Collections.Generic;
using System.Linq;

public static class DictionaryExtensions
{
    public static TValue GetOrAdd<TKey, TValue>
    (
        this Dictionary<TKey, TValue> dict, TKey key,
        Func<TKey, TValue> valueFactory
    ) =>
        dict.TryGetValue(key, out TValue value) ? value : dict[key] = valueFactory(key);

    public static void ClearAndEnsureCapacity<TKey, TValue>(this Dictionary<TKey, TValue> d, int cap)
    {
        d.Clear();
        d.EnsureCapacity(cap);
    }

    // Tabify compatibility extension
    public static void RemoveValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TValue value)
    {
        if (dict.FirstOrDefault(kvp => EqualityComparer<TValue>.Default.Equals(kvp.Value, value)) is var pair)
            dict.Remove(pair);
    }
}