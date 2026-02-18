using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public static class H
{
    public static void Seq<T>(IEnumerable<T> actual, params T[] expected) =>
        CollectionAssert.AreEqual(expected, actual.ToArray());

    public static void Contains<T>(IEnumerable<T> c, T item) =>
        Assert.IsTrue(c.Contains(item), $"Collection does not contain {item}");

    public static void Count<T>(ICollection<T> c, int n) =>
        Assert.AreEqual(n, c.Count, $"Expected count {n}, got {c.Count}");

    public static void Empty<T>(ICollection<T> c) =>
        Assert.AreEqual(0, c.Count, $"Expected empty, got {c.Count}");

    public static void AllInRange(IEnumerable<float> vals, float min, float max)
    {
        foreach (float v in vals)
            Assert.IsTrue(v >= min && v <= max, $"{v} not in [{min},{max}]");
    }

    public static void AllUnique<T>(IEnumerable<T> items)
    {
        List<T> list = items.ToList();
        Assert.AreEqual(list.Count, list.Distinct().Count(), "Items are not unique");
    }
}