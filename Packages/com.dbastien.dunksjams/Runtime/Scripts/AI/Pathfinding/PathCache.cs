using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LRU cache for path results. Same start+goal on the same frame = skip the search.
/// Automatically invalidates when the graph is marked dirty.
/// </summary>
public class PathCache
{
    readonly LRUCache<long, List<Vector2Int>> _cache;
    int _version;

    public PathCache(int capacity = 64) => _cache = new LRUCache<long, List<Vector2Int>>(capacity);

    /// <summary>Call when the graph changes (obstacle added/removed, costs changed).</summary>
    public void Invalidate()
    {
        _version++;
        _cache.Clear();
    }

    public bool TryGet(Vector2Int start, Vector2Int goal, out List<Vector2Int> path) =>
        _cache.TryGetValue(MakeKey(start, goal), out path);

    public void Store(Vector2Int start, Vector2Int goal, List<Vector2Int> path)
    {
        if (path == null) return;
        // Store a copy so the caller can't mutate the cached version
        _cache.Set(MakeKey(start, goal), new List<Vector2Int>(path));
    }

    public void Clear() => _cache.Clear();

    static long MakeKey(Vector2Int start, Vector2Int goal)
    {
        // Pack 4 shorts into a long: start.x, start.y, goal.x, goal.y
        long key = (long)(start.x & 0xFFFF) << 48
                 | (long)(start.y & 0xFFFF) << 32
                 | (long)(goal.x & 0xFFFF) << 16
                 | (long)(goal.y & 0xFFFF);
        return key;
    }
}
