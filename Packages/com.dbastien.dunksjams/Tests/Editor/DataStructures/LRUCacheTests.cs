using NUnit.Framework;

public class LRUCacheTests : TestBase
{
    [Test]
    public void Set_Get_ReturnsValue()
    {
        var cache = new LRUCache<string, int>(3);
        cache.Set("a", 1);
        cache.Set("b", 2);
        Eq(1, cache.Get("a"));
        Eq(2, cache.Get("b"));
    }

    [Test]
    public void Evicts_LeastRecent()
    {
        var cache = new LRUCache<string, int>(2);
        cache.Set("a", 1);
        cache.Set("b", 2);
        cache.Set("c", 3); // "a" should be evicted
        Eq(default, cache.Get("a")); // not found
        Eq(2, cache.Get("b"));
        Eq(3, cache.Get("c"));
    }

    [Test]
    public void Get_UpdatesRecency()
    {
        var cache = new LRUCache<string, int>(2);
        cache.Set("a", 1);
        cache.Set("b", 2);
        cache.Get("a"); // "a" is now most recent
        cache.Set("c", 3); // "b" should be evicted
        Eq(1, cache.Get("a"));
        Eq(default, cache.Get("b")); // evicted
        Eq(3, cache.Get("c"));
    }

    [Test]
    public void TryGetValue_ReturnsFalseForMissing()
    {
        var cache = new LRUCache<string, int>(2);
        False(cache.TryGetValue("missing", out _));
    }

    [Test]
    public void Set_OverwritesExisting()
    {
        var cache = new LRUCache<string, int>(3);
        cache.Set("a", 1);
        cache.Set("a", 99);
        Eq(99, cache.Get("a"));
    }

    [Test]
    public void Clear_EmptiesCache()
    {
        var cache = new LRUCache<string, int>(3);
        cache.Set("a", 1);
        cache.Clear();
        Eq(default, cache.Get("a"));
    }
}