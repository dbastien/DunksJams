using System;
using System.Collections;
using System.Collections.Generic;

public sealed class PriorityQueue<T> : IEnumerable<T>
{
	private readonly List<T> heap;
	private readonly IComparer<T> comparer;

	public PriorityQueue(int capacity = 0, IComparer<T> comparer = null)
	{
		heap = capacity > 0 ? new List<T>(capacity) : new List<T>();
		this.comparer = comparer ?? Comparer<T>.Default;
	}

	public int Count => heap.Count;
	public bool IsEmpty => heap.Count == 0;

	public void Clear() => heap.Clear();

	public void EnsureCapacity(int capacity)
	{
		if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
		heap.Capacity = Math.Max(heap.Capacity, capacity);
	}

	public void Enqueue(T item)
	{
		heap.Add(item);
		HeapifyUp(heap.Count - 1);
	}

	public void EnqueueRange(IEnumerable<T> items)
	{
		if (items == null) throw new ArgumentNullException(nameof(items));

		// Fast path if it's a collection
		if (items is ICollection<T> col && col.Count > 0)
		{
			int start = heap.Count;
			EnsureCapacity(heap.Count + col.Count);

			foreach (var item in items) heap.Add(item);

			// heapify the whole thing (bottom-up)
			HeapifyAll();
			return;
		}

		foreach (var item in items) Enqueue(item);
	}

	public T Dequeue()
	{
		if (heap.Count == 0) throw new InvalidOperationException("Queue is empty.");

		T root = heap[0];
		int lastIndex = heap.Count - 1;

		if (lastIndex == 0)
		{
			heap.RemoveAt(0);
			return root;
		}

		heap[0] = heap[lastIndex];
		heap.RemoveAt(lastIndex);
		HeapifyDown(0);
		return root;
	}

	public bool TryDequeue(out T result)
	{
		if (heap.Count == 0)
		{
			result = default;
			return false;
		}

		result = Dequeue();
		return true;
	}

	public T Peek()
	{
		if (heap.Count == 0) throw new InvalidOperationException("Queue is empty.");
		return heap[0];
	}

	public bool TryPeek(out T value)
	{
		if (heap.Count == 0)
		{
			value = default;
			return false;
		}

		value = heap[0];
		return true;
	}

	public bool Contains(T item) => heap.Contains(item);

	private void HeapifyUp(int i)
	{
		T item = heap[i];
		while (i > 0)
		{
			int parent = (i - 1) >> 1;
			if (comparer.Compare(item, heap[parent]) >= 0) break;

			heap[i] = heap[parent];
			i = parent;
		}

		heap[i] = item;
	}

	private void HeapifyDown(int i)
	{
		T item = heap[i];
		int count = heap.Count;

		while (true)
		{
			int left = (i << 1) + 1;
			if (left >= count) break;

			int right = left + 1;
			int bestChild = (right < count && comparer.Compare(heap[right], heap[left]) < 0) ? right : left;

			if (comparer.Compare(item, heap[bestChild]) <= 0) break;

			heap[i] = heap[bestChild];
			i = bestChild;
		}

		heap[i] = item;
	}

	private void HeapifyAll()
	{
		// bottom-up heapify: O(n)
		for (int i = (heap.Count >> 1) - 1; i >= 0; --i)
			HeapifyDown(i);
	}

	// Note: enumeration is in heap order, not sorted order.
	public IEnumerator<T> GetEnumerator() => heap.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}