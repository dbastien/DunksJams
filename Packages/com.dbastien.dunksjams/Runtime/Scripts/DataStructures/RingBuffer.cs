using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public enum RingBufferFullMode
{
	OverwriteOldest = 0,
	ThrowOnFull = 1,
	Grow = 2
}

public sealed class RingBuffer<T> : IEnumerable<T>
{
	private const int DefaultGrowthFactor = 2;

	private RingCore<T> core;
	private int version;
	private readonly RingBufferFullMode fullMode;

	public RingBuffer(int capacity, RingBufferFullMode fullMode = RingBufferFullMode.OverwriteOldest)
	{
		core = new RingCore<T>(capacity);
		version = 0;
		this.fullMode = fullMode;
	}

	public int Capacity => core.Capacity;
	public int Count => core.Size;
	public bool IsEmpty => core.IsEmpty;
	public bool IsFull => core.IsFull;

	public T this[int index]
	{
		get
		{
			if ((uint)index >= (uint)core.Size) throw new ArgumentOutOfRangeException(nameof(index));
			return core.Buffer[core.PhysicalIndex(index)];
		}
		set
		{
			if ((uint)index >= (uint)core.Size) throw new ArgumentOutOfRangeException(nameof(index));
			core.Buffer[core.PhysicalIndex(index)] = value;
			version++;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(T item)
	{
		EnsureSpaceForOneThrowing();
		core.PushBackAssumeNotFull(item);
		version++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryAdd(T item)
	{
		if (core.IsFull)
		{
			switch (fullMode)
			{
				case RingBufferFullMode.ThrowOnFull:
					return false;

				case RingBufferFullMode.OverwriteOldest:
					core.PushBackOverwrite(item, clearDroppedSlot: true);
					version++;
					return true;

				case RingBufferFullMode.Grow:
					core.GrowToAtLeast(Math.Max(1, core.Capacity * DefaultGrowthFactor));
					break;
			}
		}

		core.PushBackAssumeNotFull(item);
		version++;
		return true;
	}

	public void AddRange(IEnumerable<T> items)
	{
		if (items == null) throw new ArgumentNullException(nameof(items));
		foreach (var item in items) Add(item);
	}

	public T First()
	{
		if (core.Size == 0) throw new InvalidOperationException("RingBuffer is empty.");
		return core.Buffer[core.Head];
	}

	public T Last()
	{
		if (core.Size == 0) throw new InvalidOperationException("RingBuffer is empty.");
		return core.Buffer[core.PhysicalIndexBack()];
	}

	public T GetRecent(int index)
	{
		if ((uint)index >= (uint)core.Size) throw new ArgumentOutOfRangeException(nameof(index));
		return this[core.Size - 1 - index];
	}

	public bool TryGetOldest(out T value) => core.TryPeekFront(out value);
	public bool TryGetNewest(out T value) => core.TryPeekBack(out value);

	public void Skip(int count = 1)
	{
		if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
		if (count > core.Size) throw new ArgumentOutOfRangeException(nameof(count));

		core.DropOldest(count, clearSlots: true);
		version++;
	}

	public void Clear(bool clearData = false)
	{
		core.Clear(clearData);
		version++;
	}

	public void CopyTo(T[] destination, int destinationIndex)
	{
		core.CopyOrderedTo(destination, destinationIndex);
	}

	public T[] ToArrayOrdered()
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
		private readonly RingBuffer<T> rb;
		private readonly int version;
		private RingSnapshotEnumerator<T> inner;

		public T Current => inner.Current;
		object IEnumerator.Current => Current;

		internal Enumerator(RingBuffer<T> rb)
		{
			this.rb = rb;
			version = rb.version;
			inner = new RingSnapshotEnumerator<T>(rb.core.Buffer, rb.core.Head, rb.core.Size);
		}

		public bool MoveNext()
		{
			if (version != rb.version)
				throw new InvalidOperationException("Collection was modified during enumeration.");

			return inner.MoveNext();
		}

		public void Reset()
		{
			if (version != rb.version)
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
			case RingBufferFullMode.OverwriteOldest:
				core.DropOldestOne(clearSlot: true);
				return;

			case RingBufferFullMode.ThrowOnFull:
				throw new InvalidOperationException("RingBuffer is full.");

			case RingBufferFullMode.Grow:
				core.GrowToAtLeast(Math.Max(1, core.Capacity * DefaultGrowthFactor));
				return;
		}
	}
}