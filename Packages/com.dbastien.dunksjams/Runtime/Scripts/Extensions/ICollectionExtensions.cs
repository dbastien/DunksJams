using System;
using System.Collections.Generic;

public static class ICollectionExtensions
{
    public static bool IsNullOrEmpty<T>(this ICollection<T> c) => c == null || c.Count == 0;

    public static void AddRange<T>(this ICollection<T> c, IEnumerable<T> items)
    {
        foreach (var item in items) c.Add(item);
    }

    public static void RemoveRange<T>(this ICollection<T> c, IEnumerable<T> items)
    {
        foreach (var item in items) c.Remove(item);
    }

    public static void RemoveMatching<T>(this ICollection<T> c, Predicate<T> match)
    {
        var itemsToRemove = new List<T>();
        foreach (var item in c)
        {
            if (match(item))
                itemsToRemove.Add(item);
        }

        foreach (var item in itemsToRemove) c.Remove(item);
    }

    public static bool TryAdd<T>(this ICollection<T> c, T item)
    {
        if (c.Contains(item)) return false;
        c.Add(item);
        return true;
    }

    public static T GetOrAdd<T>(this ICollection<T> c, Func<T, bool> predicate, Func<T> newItem)
    {
        foreach (var item in c)
        {
            if (predicate(item))
                return item;
        }

        var newItemInstance = newItem();
        c.Add(newItemInstance);
        return newItemInstance;
    }

    public static bool TogglePresence<T>(this ICollection<T> c, T newItem)
    {
        if (c.Contains(newItem))
        {
            c.Remove(newItem);
            return false;
        }

        c.Add(newItem);
        return true;
    }
}