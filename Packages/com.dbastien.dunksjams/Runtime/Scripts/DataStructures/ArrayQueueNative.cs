using System;
using System.Collections;
using System.Collections.Generic;

public sealed class ArrayQueueNative<T>
{
    public T[] buffer;
    public int head;
    public int tail;
    public int size;

    public int Count => size;
    public bool IsEmpty => size == 0;
    public int Capacity
    {
        get => buffer.Length;
        set { if (buffer.Length < value) Expand(value); }
    }

    public T Head => buffer[head];
    public T Tail => buffer[tail == 0 ? buffer.Length - 1 : tail - 1];
    public T this[int index] => buffer[(head + index) % buffer.Length];

    public ArrayQueueNative() : this(16) { }

    public ArrayQueueNative(int capacity)
    {
        buffer = new T[capacity];
        head = 0;
        tail = 0;
        size = 0;
    }

    public void Clear()
    {
        if (head < tail)
            Array.Clear(buffer, head, size);
        else
        {
            Array.Clear(buffer, head, buffer.Length - head);
            Array.Clear(buffer, 0, tail);
        }
        head = 0;
        tail = 0;
        size = 0;
    }

    public bool Contains(T value) => IndexOf(value) >= 0;

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (head < tail)
            Array.Copy(buffer, head, array, arrayIndex, size);
        else
        {
            Array.Copy(buffer, head, array, arrayIndex, buffer.Length - head);
            Array.Copy(buffer, 0, array, arrayIndex + buffer.Length - head, tail);
        }
    }

    public T Dequeue()
    {
        T v = buffer[head];
        buffer[head] = default;
        head = (head + 1) % buffer.Length;
        --size;
        return v;
    }

    public void Enqueue(T value)
    {
        if (size == buffer.Length)
            Expand(buffer.Length << 1);

        buffer[tail] = value;
        tail = (tail + 1) % buffer.Length;
        ++size;
    }

    public Enumerator GetEnumerator() => new(this);

    public T Peek() => buffer[head];

    public T[] ToArray()
    {
        if (size == 0) return Array.Empty<T>();

        T[] array = new T[size];
        if (head < tail)
            Array.Copy(buffer, head, array, 0, size);
        else
        {
            Array.Copy(buffer, head, array, 0, buffer.Length - head);
            Array.Copy(buffer, 0, array, buffer.Length - head, tail);
        }
        return array;
    }

    public void TrimExcess()
    {
        if (size < buffer.Length)
            Expand(size);
    }

    public void Foreach(Action<T> func)
    {
        if (size == 0) return;

        if (head < tail)
        {
            for (int i = head; i < tail; ++i)
                func(buffer[i]);
        }
        else
        {
            for (int i = head; i < buffer.Length; ++i)
                func(buffer[i]);
            for (int i = 0; i < tail; ++i)
                func(buffer[i]);
        }
    }

    public void ReverseForeach(Action<T> func)
    {
        if (size == 0) return;

        if (head < tail)
        {
            for (int i = tail - 1; i >= head; --i)
                func(buffer[i]);
        }
        else
        {
            for (int i = tail - 1; i >= 0; --i)
                func(buffer[i]);
            for (int i = buffer.Length - 1; i >= head; --i)
                func(buffer[i]);
        }
    }

    public int IndexOf(T value, IEqualityComparer<T> comparer = null)
    {
        if (size == 0) return -1;

        comparer ??= EqualityComparer<T>.Default;

        if (head < tail)
        {
            for (int i = head; i < tail; ++i)
            {
                if (comparer.Equals(value, buffer[i]))
                    return i;
            }
        }
        else
        {
            for (int i = head; i < buffer.Length; ++i)
            {
                if (comparer.Equals(value, buffer[i]))
                    return i;
            }
            for (int i = 0; i < tail; ++i)
            {
                if (comparer.Equals(value, buffer[i]))
                    return i;
            }
        }
        return -1;
    }

    public int LastIndexOf(T value, IEqualityComparer<T> comparer = null)
    {
        if (size == 0) return -1;

        comparer ??= EqualityComparer<T>.Default;

        if (head < tail)
        {
            for (int i = tail - 1; i >= head; --i)
            {
                if (comparer.Equals(value, buffer[i]))
                    return i;
            }
        }
        else
        {
            for (int i = tail - 1; i >= 0; --i)
            {
                if (comparer.Equals(value, buffer[i]))
                    return i;
            }
            for (int i = buffer.Length - 1; i >= head; --i)
            {
                if (comparer.Equals(value, buffer[i]))
                    return i;
            }
        }
        return -1;
    }

    public int FindIndex(Predicate<T> find)
    {
        if (size == 0) return -1;

        if (head < tail)
        {
            for (int i = head; i < tail; ++i)
            {
                if (find(buffer[i]))
                    return i;
            }
        }
        else
        {
            for (int i = head; i < buffer.Length; ++i)
            {
                if (find(buffer[i]))
                    return i;
            }
            for (int i = 0; i < tail; ++i)
            {
                if (find(buffer[i]))
                    return i;
            }
        }
        return -1;
    }

    public int FindLastIndex(Predicate<T> find)
    {
        if (size == 0) return -1;

        if (head < tail)
        {
            for (int i = tail - 1; i >= head; --i)
            {
                if (find(buffer[i]))
                    return i;
            }
        }
        else
        {
            for (int i = tail - 1; i >= 0; --i)
            {
                if (find(buffer[i]))
                    return i;
            }
            for (int i = buffer.Length - 1; i >= head; --i)
            {
                if (find(buffer[i]))
                    return i;
            }
        }
        return -1;
    }

    public bool Exists(Predicate<T> test) => FindIndex(test) >= 0;

    public struct Enumerator : IEnumerator<T>
    {
        int index;
        ArrayQueueNative<T> queue;
        T current;

        public T Current => current;
        object IEnumerator.Current => Current;

        internal Enumerator(ArrayQueueNative<T> queue)
        {
            this.queue = queue;
            index = 0;
            current = default;
        }

        public void Dispose()
        {
            queue = null;
            current = default;
        }

        public bool MoveNext()
        {
            if (index < queue.size)
            {
                current = queue[index++];
                return true;
            }
            index = queue.size + 1;
            current = default;
            return false;
        }

        public void Reset()
        {
            index = 0;
            current = default;
        }
    }

    void Expand(int capacity)
    {
        T[] newBuffer = new T[capacity];

        if (head < tail)
            Array.Copy(buffer, head, newBuffer, 0, size);
        else
        {
            Array.Copy(buffer, head, newBuffer, 0, buffer.Length - head);
            Array.Copy(buffer, 0, newBuffer, buffer.Length - head, tail);
        }

        head = 0;
        tail = size;
        buffer = newBuffer;
    }
}
