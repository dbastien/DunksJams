using System;
using System.Collections;
using System.Collections.Generic;

public class LRUCache<TK, TV> : IEnumerable<KeyValuePair<TK, TV>>
{
    readonly int _capacity;
    readonly Dictionary<TK, LinkedListNode<(TK key, TV val)>> _cache = new();
    readonly LinkedList<(TK key, TV val)> _order = new();

    public LRUCache(int capacity) => 
        _capacity = capacity > 0 ? capacity : throw new ArgumentOutOfRangeException(nameof(capacity));

    public TV Get(TK key) => _cache.TryGetValue(key, out var node) ? MoveToFrontAndReturn(node) : default;

    public bool TryGetValue(TK key, out TV val)
    {
        if (_cache.TryGetValue(key, out var node))
        {
            val = MoveToFrontAndReturn(node);
            return true;
        }
        val = default!;
        return false;
    }

    public void Set(TK key, TV val)
    {
        if (_cache.TryGetValue(key, out var node))
        {
            node.Value = (key, val);
            if (node != _order.First) MoveToFront(node);
        }
        else
        {
            if (_cache.Count >= _capacity)
            {
                var lru = _order.Last!.Value.key;
                _order.RemoveLast();
                _cache.Remove(lru);
            }
            _cache[key] = _order.AddFirst((key, val));
        }
    }
    
    public void Clear()
    {
        _cache.Clear();
        _order.Clear();
    }

    TV MoveToFrontAndReturn(LinkedListNode<(TK key, TV val)> node)
    {
        MoveToFront(node);
        return node.Value.val;
    }

    void MoveToFront(LinkedListNode<(TK key, TV val)> node)
    {
        _order.Remove(node);
        _order.AddFirst(node);
    }
    
    public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
    {
        foreach (var (key, val) in _order)
            yield return new KeyValuePair<TK, TV>(key, val);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}