using System;
using System.Collections;
using System.Collections.Generic;

public sealed class LRUCache<TK, TV> : IEnumerable<KeyValuePair<TK, TV>>
{
	private readonly int capacity;
	private readonly Dictionary<TK, LinkedListNode<Entry>> cache;
	private readonly LinkedList<Entry> order;

	private struct Entry
	{
		public TK Key;
		public TV Value;

		public Entry(TK key, TV value)
		{
			Key = key;
			Value = value;
		}
	}

	/// <summary>
	/// Called when an entry is evicted (LRU removed). Useful for Dispose/Return-to-pool/etc.
	/// </summary>
	public Action<TK, TV> OnEvicted { get; set; }

	public int Capacity => capacity;
	public int Count => cache.Count;

	public LRUCache(int capacity, IEqualityComparer<TK> keyComparer = null)
	{
		if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
		this.capacity = capacity;

		cache = new Dictionary<TK, LinkedListNode<Entry>>(capacity, keyComparer);
		order = new LinkedList<Entry>();
	}

	/// <summary> True if the key exists (does not update recency). </summary>
	public bool ContainsKey(TK key) => cache.ContainsKey(key);

	/// <summary> Try get a value; updates recency if found. </summary>
	public bool TryGetValue(TK key, out TV value)
	{
		if (cache.TryGetValue(key, out var node))
		{
			MoveToFront(node);
			value = node.Value.Value;
			return true;
		}

		value = default;
		return false;
	}

	/// <summary> Get value or throw; updates recency. </summary>
	public TV GetOrThrow(TK key)
	{
		if (!TryGetValue(key, out var value))
			throw new KeyNotFoundException($"Key not found: {key}");
		return value;
	}

	/// <summary> Get value or return fallback; updates recency if found. </summary>
	public TV GetOrDefault(TK key, TV fallback = default)
	{
		return TryGetValue(key, out var value) ? value : fallback;
	}

	/// <summary> Add or update; updates recency. </summary>
	public void Set(TK key, TV value)
	{
		if (cache.TryGetValue(key, out var node))
		{
			node.Value = new Entry(key, value);
			MoveToFront(node);
			return;
		}

		if (cache.Count >= capacity)
			EvictOne();

		cache[key] = order.AddFirst(new Entry(key, value));
	}

	/// <summary> Remove key if present; returns true if removed. </summary>
	public bool Remove(TK key)
	{
		if (!cache.TryGetValue(key, out var node))
			return false;

		cache.Remove(key);
		order.Remove(node);
		return true;
	}

	/// <summary> Clears cache (does not call OnEvicted). </summary>
	public void Clear()
	{
		cache.Clear();
		order.Clear();
	}

	/// <summary> Remove least-recently-used entry; returns true if an entry was evicted. </summary>
	public bool EvictLeastRecent()
	{
		if (cache.Count == 0) return false;
		EvictOne();
		return true;
	}

	private void EvictOne()
	{
		var lruNode = order.Last;
		var entry = lruNode.Value;

		order.RemoveLast();
		cache.Remove(entry.Key);

		OnEvicted?.Invoke(entry.Key, entry.Value);
	}

	private void MoveToFront(LinkedListNode<Entry> node)
	{
		if (node == order.First) return;
		order.Remove(node);
		order.AddFirst(node);
	}

	/// <summary> Enumerates from most-recently-used (MRU) to least-recently-used (LRU). </summary>
	public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
	{
		for (var node = order.First; node != null; node = node.Next)
			yield return new KeyValuePair<TK, TV>(node.Value.Key, node.Value.Value);
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}