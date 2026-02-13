using System;
using System.Collections.Generic;

public static class DictionaryExtensions
{
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key,
        Func<TKey, TValue> valueFactory) =>
        dict.TryGetValue(key, out var value) ? value : dict[key] = valueFactory(key);

    public static void ClearAndEnsureCapacity<TKey, TValue>(this Dictionary<TKey, TValue> d, int cap)
    {
        d.Clear();
        d.EnsureCapacity(cap);
    }
}