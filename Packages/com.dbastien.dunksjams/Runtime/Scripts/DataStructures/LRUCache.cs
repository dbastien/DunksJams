using System;
using System.Collections;
using System.Collections.Generic;

public class LRUCache<TK, TV> : IEnumerable<KeyValuePair<TK, TV>>
{
    private readonly int _capacity;
    private readonly Dictionary<TK, LinkedListNode<(TK key, TV val)>> _cache = new();
    private readonly LinkedList<(TK key, TV val)> _order = new();

    public LRUCache(int capacity) =>
        _capacity = capacity > 0 ? capacity : throw new ArgumentOutOfRangeException(nameof(capacity));

    public TV Get
        (TK key) => _cache.TryGetValue(key, out LinkedListNode<(TK key, TV val)> node)
        ? MoveToFrontAndReturn(node)
        : default;

    public bool TryGetValue(TK key, out TV val)
    {
        if (_cache.TryGetValue(key, out LinkedListNode<(TK key, TV val)> node))
        {
            val = MoveToFrontAndReturn(node);
            return true;
        }

        val = default!;
        return false;
    }

    public void Set(TK key, TV val)
    {
        if (_cache.TryGetValue(key, out LinkedListNode<(TK key, TV val)> node))
        {
            node.Value = (key, val);
            if (node != _order.First) MoveToFront(node);
        }
        else
        {
            if (_cache.Count >= _capacity)
            {
                TK lru = _order.Last!.Value.key;
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

    private TV MoveToFrontAndReturn(LinkedListNode<(TK key, TV val)> node)
    {
        MoveToFront(node);
        return node.Value.val;
    }

    private void MoveToFront(LinkedListNode<(TK key, TV val)> node)
    {
        _order.Remove(node);
        _order.AddFirst(node);
    }

    public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
    {
        foreach ((TK key, TV val) in _order)
            yield return new KeyValuePair<TK, TV>(key, val);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}