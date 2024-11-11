using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> l) => Rand.Shuffle(l);

    public static T PopFirst<T>(this IList<T> l)
    {
        if (l.Count == 0) throw new InvalidOperationException("Cannot pop from empty list.");
        var item = l[0];
        l.RemoveAt(0);
        return item;
    }

    public static T PopLast<T>(this IList<T> l)
    {
        if (l.Count == 0) throw new InvalidOperationException("Cannot pop from empty list.");
        var item = l[^1];
        l.RemoveAt(l.Count - 1);
        return item;
    }

    public static void ClearTo<T>(this List<T> l, int cap)
    {
        l.Clear();
        l.Capacity = cap;
    }

    public static void ClearAndEnsureCapacity<T>(this List<T> l, int cap)
    {
        l.Clear();
        if (l.Capacity < cap) l.Capacity = cap;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Swap<T>(this IList<T> l, int idxA, int idxB) =>
        (l[idxA], l[idxB]) = (l[idxB], l[idxA]);

    public static void RemoveAllByPredicate<T>(this IList<T> l, Predicate<T> match)
    {
        for (int i = l.Count - 1; i >= 0; --i)
            if (match(l[i])) l.RemoveAt(i);
    }
}