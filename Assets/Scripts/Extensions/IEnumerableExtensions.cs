using System;
using System.Collections.Generic;
using System.Linq;

public static class IEnumerableExtensions
{
    public static T MaxBy<T, TKey>(this IEnumerable<T> e, Func<T, TKey> key) where TKey : IComparable<TKey> =>
        e.Aggregate((a, b) => key(a).CompareTo(key(b)) > 0 ? a : b);

    public static T MinBy<T, TKey>(this IEnumerable<T> e, Func<T, TKey> key) where TKey : IComparable<TKey> =>
        e.Aggregate((a, b) => key(a).CompareTo(key(b)) < 0 ? a : b);

    public static void Shuffle<T>(this IEnumerable<T> e) => 
        Rand.Shuffle(e as List<T> ?? e.ToList());

    public static bool None<T>(this IEnumerable<T> e) => !e.Any();

    public static T FirstOr<T>(this IEnumerable<T> e, T fallback) =>
        e.DefaultIfEmpty(fallback).First();

    public static T LastOr<T>(this IEnumerable<T> e, T fallback) =>
        e.DefaultIfEmpty(fallback).Last();

    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> e, Func<T, TKey> key) =>
        e.GroupBy(key).Select(g => g.First());

    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> e, int size)
    {
        if (size <= 0) throw new ArgumentException("Chunk size must be greater than 0.", nameof(size));

        var chunk = new List<T>(size);
        foreach (T item in e)
        {
            chunk.Add(item);
            if (chunk.Count != size) continue;
            yield return new List<T>(chunk);
            chunk.Clear();
        }
        if (chunk.Any()) yield return chunk;
    }

    public static T RandomElement<T>(this IEnumerable<T> e)
    {
        var list = e as List<T> ?? e.ToList();
        return list.Count == 0 ? default : Rand.Element(list);
    }

    public static int IndexOf<T>(this IEnumerable<T> e, T element)
    {
        int index = 0;
        foreach (T item in e)
        {
            if (EqualityComparer<T>.Default.Equals(item, element)) return index;
            ++index;
        }
        return -1;
    }

    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> e) =>
        e.SelectMany(s => s);

    public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> e, int count)
    {
        if (count <= 0) return Enumerable.Empty<T>();

        var result = new Queue<T>(count);
        foreach (T item in e)
        {
            if (result.Count == count) result.Dequeue();
            result.Enqueue(item);
        }
        return result;
    }

    public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> e, int count)
    {
        if (count <= 0) 
        {
            foreach (T item in e) yield return item;
            yield break;
        }

        var buffer = new Queue<T>(count);
        foreach (T item in e)
        {
            if (buffer.Count == count) yield return buffer.Dequeue();
            buffer.Enqueue(item);
        }
    }

    public static (IEnumerable<T> Matches, IEnumerable<T> NonMatches) Partition<T>(
        this IEnumerable<T> e, Func<T, bool> predicate)
    {
        var matches = new List<T>();
        var nonMatches = new List<T>();
        foreach (T item in e)
        {
            if (predicate(item)) matches.Add(item);
            else nonMatches.Add(item);
        }
        return (matches, nonMatches);
    }

    public static bool IsSingle<T>(this IEnumerable<T> e)
    {
        using var enumerator = e.GetEnumerator();
        return enumerator.MoveNext() && !enumerator.MoveNext();
    }

    public static bool SequenceEqualUnordered<T>(this IEnumerable<T> e, IEnumerable<T> second)
    {
        var firstCounts = e.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

        foreach (T item in second)
        {
            if (!firstCounts.TryGetValue(item, out var count) || count == 0) return false;
            --firstCounts[item];
        }

        return firstCounts.Values.All(count => count == 0);
    }

    public static IEnumerable<T> IntersectBy<T, TKey>(this IEnumerable<T> e, IEnumerable<T> other, Func<T, TKey> key) =>
        e.Join(other, key, key, (x, _) => x).Distinct();

    public static T MostFrequent<T>(this IEnumerable<T> e) =>
        e.GroupBy(x => x).MaxBy(g => g.Count()).Key;

    public static T LeastFrequent<T>(this IEnumerable<T> e) =>
        e.GroupBy(x => x).MinBy(g => g.Count()).Key;
    
    public static IEnumerable<T> ExceptBy<T, TKey>(this IEnumerable<T> e, IEnumerable<T> other, Func<T, TKey> key) =>
        e.Where(item => !other.Select(key).Contains(key(item)));

    public static IDictionary<TKey, int> CountBy<T, TKey>(this IEnumerable<T> e, Func<T, TKey> key) =>
        e.GroupBy(key).ToDictionary(g => g.Key, g => g.Count());
}