using System;
using System.Collections.Generic;

public sealed class MinQueue<T>
{
	private readonly Deque<T> values;
	private readonly Deque<Entry> mins;
	private readonly IComparer<T> comparer;

	private struct Entry
	{
		public T Value;
		public int Count;
	}

	public MinQueue(int capacity = 16, IComparer<T> comparer = null)
	{
		if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
		this.comparer = comparer ?? Comparer<T>.Default;

		values = new Deque<T>(Math.Max(1, capacity));
		mins = new Deque<Entry>(Math.Max(1, capacity));
	}

	public int Count => values.Count;
	public bool IsEmpty => values.Count == 0;

	public void Clear(bool clearData = false)
	{
		values.Clear(clearData);
		mins.Clear(clearData);
	}

	public void Enqueue(T item)
	{
		values.PushBack(item);

		int poppedEqualCount = 0;

		// Maintain monotonic non-decreasing mins deque:
		// Remove all candidates > item. Combine counts for equals to compress duplicates.
		while (!mins.IsEmpty)
		{
			var back = mins.PeekBack();
			int cmp = comparer.Compare(back.Value, item);

			if (cmp < 0)
				break; // back.Value < item => keep it

			mins.PopBack();

			if (cmp == 0)
				poppedEqualCount += back.Count;
			// if cmp > 0: popped because it's worse (larger)
		}

		var entry = new Entry { Value = item, Count = poppedEqualCount + 1 };
		mins.PushBack(entry);
	}

	public T Dequeue()
	{
		if (values.IsEmpty) throw new InvalidOperationException("Queue is empty.");

		var item = values.PopFront();

		// If the dequeued item matches the current minimum candidate,
		// decrement its multiplicity.
		var front = mins.PeekFront();
		if (comparer.Compare(front.Value, item) == 0)
		{
			front.Count--;
			if (front.Count == 0)
				mins.PopFront();
			else
			{
				// Update front in place: pop then push updated front to front.
				// (Keeps Deque minimal API; if you add a "ReplaceFront" later, use that.)
				mins.PopFront();
				mins.PushFront(front);
			}
		}

		return item;
	}

	public T Peek()
	{
		if (values.IsEmpty) throw new InvalidOperationException("Queue is empty.");
		return values.PeekFront();
	}

	public T Min()
	{
		if (mins.IsEmpty) throw new InvalidOperationException("Queue is empty.");
		return mins.PeekFront().Value;
	}

	public bool TryMin(out T value)
	{
		if (mins.IsEmpty)
		{
			value = default;
			return false;
		}

		value = mins.PeekFront().Value;
		return true;
	}
}