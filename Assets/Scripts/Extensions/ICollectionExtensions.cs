using System;
using System.Collections.Generic;
using System.Linq;

public static class ICollectionExtensions
{
    public static bool IsNullOrEmpty<T>(this ICollection<T> c) => c == null || c.Count == 0;
    
    public static void AddRange<T>(this ICollection<T> c, IEnumerable<T> items)
    {
        foreach (T item in items) c.Add(item);
    }
    
    public static void RemoveRange<T>(this ICollection<T> c, IEnumerable<T> items)
    {
        foreach (T item in items) c.Remove(item);
    }
    
    public static void RemoveMatching<T>(this ICollection<T> c, Predicate<T> match)
    {
        List<T> itemsToRemove = c.Where(i => match(i)).ToList();
        foreach (T item in itemsToRemove) c.Remove(item);
    }
    
    public static bool TryAdd<T>(this ICollection<T> c, T item)
    {
        if (c.Contains(item)) return false;
        c.Add(item);
        return true;
    }

    public static T GetOrAdd<T>(this ICollection<T> c, Func<T, bool> predicate, Func<T> newItem)
    {
        var item = c.FirstOrDefault(predicate);
        if (item != null) return item;
        item = newItem();
        c.Add(item);
        return item;
    }
}