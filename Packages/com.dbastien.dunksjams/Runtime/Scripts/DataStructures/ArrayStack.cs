using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public sealed class ArrayStack<T> : IEnumerable<T>
{
	private const int DefaultCapacity = 16;

	private T[] buffer;
	private int size;
	private int version;

	public int Count => size;
	public bool IsEmpty => size == 0;
	public int Capacity => buffer.Length;

	public ArrayStack(int capacity = DefaultCapacity)
	{
		if (capacity < 1) capacity = DefaultCapacity;
		buffer = new T[capacity];
		size = 0;
		version = 0;
	}

	public T Top
	{
		get
		{
			if (size == 0) throw new InvalidOperationException("Stack is empty.");
			return buffer[size - 1];
		}
	}

	public T Bottom
	{
		get
		{
			if (size == 0) throw new InvalidOperationException("Stack is empty.");
			return buffer[0];
		}
	}

	// Index from bottom: [0..Count-1]
	public T this[int index]
	{
		get
		{
			if ((uint)index >= (uint)size) throw new ArgumentOutOfRangeException(nameof(index));
			return buffer[index];
		}
	}

	public void EnsureCapacity(int minCapacity)
	{
		if (minCapacity < 0) throw new ArgumentOutOfRangeException(nameof(minCapacity));
		if (minCapacity <= buffer.Length) return;
		ResizeBuffer(minCapacity);
	}

	public void Clear(bool clearData = true)
	{
		if (clearData && size > 0)
			Array.Clear(buffer, 0, size);

		size = 0;
		version++;
	}

	public bool Contains(T value, IEqualityComparer<T> comparer = null) => IndexOf(value, comparer) >= 0;

	public void CopyTo(T[] array, int arrayIndex)
	{
		if (array == null) throw new ArgumentNullException(nameof(array));
		if ((uint)arrayIndex > (uint)array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
		if (array.Length - arrayIndex < size) throw new ArgumentException("Destination array is too small.");

		// Copy in stack order: top-to-bottom
		for (int i = 0; i < size; ++i)
			array[arrayIndex + i] = buffer[size - 1 - i];
	}

	public bool TryPeek(out T value)
	{
		if (size == 0)
		{
			value = default;
			return false;
		}

		value = buffer[size - 1];
		return true;
	}

	public T Peek()
	{
		if (!TryPeek(out var value))
			throw new InvalidOperationException("Stack is empty.");
		return value;
	}

	public bool TryPop(out T value, bool clearSlot = true)
	{
		if (size == 0)
		{
			value = default;
			return false;
		}

		int newSize = size - 1;
		value = buffer[newSize];
		if (clearSlot) buffer[newSize] = default;
		size = newSize;

		version++;
		return true;
	}

	public T Pop()
	{
		if (!TryPop(out var value))
			throw new InvalidOperationException("Stack is empty.");
		return value;
	}

	public void Push(T value)
	{
		if (size == buffer.Length)
		{
			int newCap = buffer.Length == 0 ? DefaultCapacity : buffer.Length << 1;
			ResizeBuffer(newCap);
		}

		buffer[size++] = value;
		version++;
	}

	public T[] ToArray()
	{
		if (size == 0) return Array.Empty<T>();

		var array = new T[size];
		for (int i = 0; i < size; ++i)
			array[i] = buffer[size - 1 - i];

		return array;
	}

	// Index from bottom: [0..Count-1]
	public int IndexOf(T value, IEqualityComparer<T> comparer = null)
	{
		if (size == 0) return -1;
		comparer ??= EqualityComparer<T>.Default;

		for (int i = 0; i < size; ++i)
			if (comparer.Equals(value, buffer[i]))
				return i;

		return -1;
	}

	// Index from bottom: [0..Count-1]
	public int LastIndexOf(T value, IEqualityComparer<T> comparer = null)
	{
		if (size == 0) return -1;
		comparer ??= EqualityComparer<T>.Default;

		for (int i = size - 1; i >= 0; --i)
			if (comparer.Equals(value, buffer[i]))
				return i;

		return -1;
	}

	public int FindIndex(Predicate<T> find)
	{
		if (find == null) throw new ArgumentNullException(nameof(find));
		for (int i = size - 1; i >= 0; --i)
			if (find(buffer[i]))
				return i;
		return -1;
	}

	public int FindLastIndex(Predicate<T> find)
	{
		if (find == null) throw new ArgumentNullException(nameof(find));
		for (int i = 0; i < size; ++i)
			if (find(buffer[i]))
				return i;
		return -1;
	}

	public bool Exists(Predicate<T> test) => FindIndex(test) >= 0;

	public void Foreach(Action<T> func)
	{
		if (func == null) throw new ArgumentNullException(nameof(func));
		for (int i = size - 1; i >= 0; --i)
			func(buffer[i]);
	}

	public void ReverseForeach(Action<T> func)
	{
		if (func == null) throw new ArgumentNullException(nameof(func));
		for (int i = 0; i < size; ++i)
			func(buffer[i]);
	}

	public void TrimExcess(int minimumCapacity = DefaultCapacity)
	{
		if (minimumCapacity < 1) minimumCapacity = 1;

		int target = Math.Max(size, minimumCapacity);
		if (target == buffer.Length) return;

		if (target < buffer.Length)
			ResizeBuffer(target);
	}

	public Enumerator GetEnumerator() => new Enumerator(this);
	IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public struct Enumerator : IEnumerator<T>
	{
		private readonly ArrayStack<T> stack;
		private readonly int version;
		private int index;
		private T current;

		public T Current => current;
		object IEnumerator.Current => current;

		internal Enumerator(ArrayStack<T> stack)
		{
			this.stack = stack;
			version = stack.version;
			index = stack.size;
			current = default;
		}

		public bool MoveNext()
		{
			if (version != stack.version)
				throw new InvalidOperationException("Collection was modified during enumeration.");

			if (--index >= 0)
			{
				current = stack.buffer[index];
				return true;
			}

			current = default;
			return false;
		}

		public void Reset()
		{
			if (version != stack.version)
				throw new InvalidOperationException("Collection was modified during enumeration.");

			index = stack.size;
			current = default;
		}

		public void Dispose() { }
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ResizeBuffer(int capacity)
	{
		if (capacity < 1) capacity = 1;
		if (capacity == buffer.Length) return;

		var newBuffer = new T[capacity];
		int copyCount = Math.Min(size, capacity);

		if (copyCount > 0)
			Array.Copy(buffer, 0, newBuffer, 0, copyCount);

		buffer = newBuffer;
		if (size > capacity) size = capacity;

		version++;
	}
}