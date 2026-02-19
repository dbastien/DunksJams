using System;
using System.Runtime.CompilerServices;

public struct RingCore<T>
{
	public T[] Buffer;
	public int Head; // oldest
	public int Tail; // next write position
	public int Size;

	public int Capacity => Buffer?.Length ?? 0;
	public bool IsEmpty => Size == 0;
	public bool IsFull => Size == Capacity;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RingCore(int capacity)
	{
		if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be > 0.");
		Buffer = new T[capacity];
		Head = 0;
		Tail = 0;
		Size = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int PhysicalIndex(int logicalIndex) => (Head + logicalIndex) % Buffer.Length;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Inc(int i) => (i + 1) == Buffer.Length ? 0 : i + 1;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int Dec(int i) => i == 0 ? Buffer.Length - 1 : i - 1;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int PhysicalIndexBack() => Dec(Tail);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref T ElementAtUnchecked(int logicalIndex) => ref Buffer[PhysicalIndex(logicalIndex)];

	public void Clear(bool clearData)
	{
		if (clearData && Buffer.Length > 0)
			Array.Clear(Buffer, 0, Buffer.Length);

		Head = 0;
		Tail = 0;
		Size = 0;
	}

	public void CopyOrderedTo(T[] destination, int destinationIndex)
	{
		if (destination == null) throw new ArgumentNullException(nameof(destination));
		if ((uint)destinationIndex > (uint)destination.Length) throw new ArgumentOutOfRangeException(nameof(destinationIndex));
		if (destination.Length - destinationIndex < Size) throw new ArgumentException("Destination array is too small.");

		if (Size == 0) return;

		int firstSegment = Math.Min(Size, Buffer.Length - Head);
		Array.Copy(Buffer, Head, destination, destinationIndex, firstSegment);

		int remaining = Size - firstSegment;
		if (remaining > 0)
			Array.Copy(Buffer, 0, destination, destinationIndex + firstSegment, remaining);
	}

	public void GrowToAtLeast(int minCapacity)
	{
		if (minCapacity <= Buffer.Length) return;

		int newCapacity = Buffer.Length == 0 ? 4 : Buffer.Length;
		while (newCapacity < minCapacity)
			newCapacity = Math.Max(newCapacity + 1, newCapacity * 2);

		var newBuffer = new T[newCapacity];
		if (Size > 0)
		{
			int firstSegment = Math.Min(Size, Buffer.Length - Head);
			Array.Copy(Buffer, Head, newBuffer, 0, firstSegment);

			int remaining = Size - firstSegment;
			if (remaining > 0)
				Array.Copy(Buffer, 0, newBuffer, firstSegment, remaining);
		}

		Buffer = newBuffer;
		Head = 0;
		Tail = Size;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void DropOldestOne(bool clearSlot = true)
	{
		if (Size == 0) return;
		if (clearSlot) Buffer[Head] = default;
		Head = Inc(Head);
		Size--;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void DropOldest(int count, bool clearSlots = true)
	{
		if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
		if (count > Size) throw new ArgumentOutOfRangeException(nameof(count), "Cannot drop more than Size.");
		if (count == 0) return;

		if (clearSlots)
		{
			if (Head + count <= Buffer.Length)
			{
				Array.Clear(Buffer, Head, count);
			}
			else
			{
				int rightLen = Buffer.Length - Head;
				Array.Clear(Buffer, Head, rightLen);
				Array.Clear(Buffer, 0, count - rightLen);
			}
		}

		Head = (Head + count) % Buffer.Length;
		Size -= count;
		if (Size == 0)
		{
			// Keep invariants nice for callers who assume empty => Head==Tail
			Tail = Head;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushBackAssumeNotFull(T item)
	{
		Buffer[Tail] = item;
		Tail = Inc(Tail);
		Size++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushFrontAssumeNotFull(T item)
	{
		Head = Dec(Head);
		Buffer[Head] = item;
		Size++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryPushBackNoOverwrite(T item)
	{
		if (IsFull) return false;
		PushBackAssumeNotFull(item);
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryPushFrontNoOverwrite(T item)
	{
		if (IsFull) return false;
		PushFrontAssumeNotFull(item);
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushBackOverwrite(T item, bool clearDroppedSlot = false)
	{
		if (IsFull)
			DropOldestOne(clearDroppedSlot);

		PushBackAssumeNotFull(item);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryPopFront(out T item, bool clearSlot = true)
	{
		if (Size == 0)
		{
			item = default;
			return false;
		}

		item = Buffer[Head];
		if (clearSlot) Buffer[Head] = default;
		Head = Inc(Head);
		Size--;
		if (Size == 0) Tail = Head;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryPopBack(out T item, bool clearSlot = true)
	{
		if (Size == 0)
		{
			item = default;
			return false;
		}

		Tail = Dec(Tail);
		item = Buffer[Tail];
		if (clearSlot) Buffer[Tail] = default;
		Size--;
		if (Size == 0) Head = Tail;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryPeekFront(out T item)
	{
		if (Size == 0)
		{
			item = default;
			return false;
		}

		item = Buffer[Head];
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryPeekBack(out T item)
	{
		if (Size == 0)
		{
			item = default;
			return false;
		}

		item = Buffer[PhysicalIndexBack()];
		return true;
	}
}