using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public enum QueueFullMode
{
	ThrowOnFull = 0,
	Grow = 1
}

public sealed class ArrayQueue<T> : IEnumerable<T>
{
	private const int DefaultCapacity = 16;
	private const int GrowthFactor = 2;

	private RingCore<T> core;
	private int version;
	private readonly QueueFullMode fullMode;

	public ArrayQueue(int capacity = DefaultCapacity, QueueFullMode fullMode = QueueFullMode.Grow)
	{
		core = new RingCore<T>(Math.Max(1, capacity));
		version = 0;
		this.fullMode = fullMode;
	}

	public int Count => core.Size;
	public int Capacity => core.Capacity;
	public bool IsEmpty => core.IsEmpty;
	public bool IsFull => core.IsFull;

	public void EnsureCapacity(int minCapacity)
	{
		if (minCapacity < 0) throw new ArgumentOutOfRangeException(nameof(minCapacity));
		if (minCapacity <= core.Capacity) return;
		core.GrowToAtLeast(minCapacity);
		version++;
	}

	public void Clear(bool clearData = false)
	{
		core.Clear(clearData);
		version++;
	}

	public void Enqueue(T item)
	{
		EnsureSpaceForOneThrowing();
		core.PushBackAssumeNotFull(item);
		version++;
	}

	public bool TryEnqueue(T item)
	{
		if (core.IsFull)
		{
			switch (fullMode)
			{
				case QueueFullMode.ThrowOnFull:
					return false;

				case QueueFullMode.Grow:
					core.GrowToAtLeast(Math.Max(1, core.Capacity * GrowthFactor));
					break;
			}
		}

		core.PushBackAssumeNotFull(item);
		version++;
		return true;
	}

	public T Dequeue()
	{
		if (!core.TryPopFront(out var value, clearSlot: true))
			throw new InvalidOperationException("Queue is empty.");
		version++;
		return value;
	}

	public bool TryDequeue(out T value)
	{
		if (!core.TryPopFront(out value, clearSlot: true))
			return false;
		version++;
		return true;
	}

	public T Peek()
	{
		if (!core.TryPeekFront(out var value))
			throw new InvalidOperationException("Queue is empty.");
		return value;
	}

	public bool TryPeek(out T value) => core.TryPeekFront(out value);

	public void CopyTo(T[] destination, int destinationIndex)
	{
		core.CopyOrderedTo(destination, destinationIndex);
	}

	public T[] ToArray()
	{
		if (core.Size == 0) return Array.Empty<T>();
		var array = new T[core.Size];
		core.CopyOrderedTo(array, 0);
		return array;
	}

	public Enumerator GetEnumerator() => new Enumerator(this);
	IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public struct Enumerator : IEnumerator<T>
	{
		private readonly ArrayQueue<T> queue;
		private readonly int version;
		private RingSnapshotEnumerator<T> inner;

		public T Current => inner.Current;
		object IEnumerator.Current => Current;

		internal Enumerator(ArrayQueue<T> queue)
		{
			this.queue = queue;
			version = queue.version;
			inner = new RingSnapshotEnumerator<T>(queue.core.Buffer, queue.core.Head, queue.core.Size);
		}

		public bool MoveNext()
		{
			if (version != queue.version)
				throw new InvalidOperationException("Collection was modified during enumeration.");
			return inner.MoveNext();
		}

		public void Reset()
		{
			if (version != queue.version)
				throw new InvalidOperationException("Collection was modified during enumeration.");
			inner.Reset();
		}

		public void Dispose() => inner.Dispose();
	}

	private void EnsureSpaceForOneThrowing()
	{
		if (!core.IsFull) return;

		switch (fullMode)
		{
			case QueueFullMode.ThrowOnFull:
				throw new InvalidOperationException("Queue is full.");

			case QueueFullMode.Grow:
				core.GrowToAtLeast(Math.Max(1, core.Capacity * GrowthFactor));
				return;
		}
	}
}