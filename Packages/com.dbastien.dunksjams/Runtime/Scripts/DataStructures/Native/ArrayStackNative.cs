using System;
using System.Collections;
using System.Collections.Generic;

public sealed class ArrayStackNative<T>
{
    public T[] buffer;
    public int size;

    public int Count => size;
    public bool IsEmpty => size == 0;

    public int Capacity
    {
        get => buffer.Length;
        set
        {
            if (buffer.Length < value) Expand(value);
        }
    }

    public T Top => size > 0 ? buffer[size - 1] : default;
    public T Bottom => size > 0 ? buffer[0] : default;
    public T this[int index] => buffer[index];

    public ArrayStackNative() : this(16)
    {
    }

    public ArrayStackNative(int capacity)
    {
        buffer = new T[capacity];
        size = 0;
    }

    public void Clear()
    {
        Array.Clear(buffer, 0, size);
        size = 0;
    }

    public bool Contains(T value) => IndexOf(value) >= 0;

    public void CopyTo(T[] array, int arrayIndex)
    {
        Array.Copy(buffer, 0, array, arrayIndex, size);
        Array.Reverse(array, arrayIndex, size);
    }

    public Enumerator GetEnumerator() => new(this);

    public T Peek() => size > 0 ? buffer[size - 1] : default;

    public T Pop()
    {
        if (size > 0)
        {
            --size;
            var value = buffer[size];
            buffer[size] = default;
            return value;
        }

        return default;
    }

    public void Push(T value)
    {
        if (size == buffer.Length)
            Expand(buffer.Length << 1);

        buffer[size++] = value;
    }

    public T[] ToArray()
    {
        var array = new T[size];
        for (var i = 0; i < size; ++i)
            array[i] = buffer[size - i - 1];
        return array;
    }

    public int IndexOf(T value, IEqualityComparer<T> comparer = null)
    {
        if (size == 0) return -1;

        comparer ??= EqualityComparer<T>.Default;

        for (var i = 0; i < size; ++i)
        {
            if (comparer.Equals(value, buffer[i]))
                return i;
        }

        return -1;
    }

    public int LastIndexOf(T value, IEqualityComparer<T> comparer = null)
    {
        if (size == 0) return -1;

        comparer ??= EqualityComparer<T>.Default;

        for (var i = size - 1; i >= 0; --i)
        {
            if (comparer.Equals(value, buffer[i]))
                return i;
        }

        return -1;
    }

    public int FindIndex(Predicate<T> find)
    {
        if (size == 0) return -1;

        for (var i = size - 1; i >= 0; --i)
        {
            if (find(buffer[i]))
                return i;
        }

        return -1;
    }

    public int FindLastIndex(Predicate<T> find)
    {
        if (size == 0) return -1;

        for (var i = 0; i < size; ++i)
        {
            if (find(buffer[i]))
                return i;
        }

        return -1;
    }

    public void Foreach(Action<T> func)
    {
        if (size == 0) return;

        for (var i = size - 1; i >= 0; --i)
            func(buffer[i]);
    }

    public void ReverseForeach(Action<T> func)
    {
        if (size == 0) return;

        for (var i = 0; i < size; ++i)
            func(buffer[i]);
    }

    public bool Exists(Predicate<T> test) => FindIndex(test) >= 0;

    public void TrimExcess()
    {
        if (size < buffer.Length)
            Expand(size);
    }

    public struct Enumerator : IEnumerator<T>
    {
        int index;
        ArrayStackNative<T> stack;
        T current;

        public T Current => current;
        object IEnumerator.Current => current;

        internal Enumerator(ArrayStackNative<T> stack)
        {
            this.stack = stack;
            index = stack.size;
            current = default;
        }

        public void Dispose()
        {
            current = default;
            index = -1;
            stack = null;
        }

        public bool MoveNext()
        {
            var ok = --index >= 0;
            if (ok)
                current = stack.buffer[index];
            else
                current = default;
            return ok;
        }

        public void Reset()
        {
            index = stack.size;
            current = default;
        }
    }

    void Expand(int capacity)
    {
        var newBuffer = new T[capacity];
        Array.Copy(buffer, 0, newBuffer, 0, size);
        buffer = newBuffer;
    }
}