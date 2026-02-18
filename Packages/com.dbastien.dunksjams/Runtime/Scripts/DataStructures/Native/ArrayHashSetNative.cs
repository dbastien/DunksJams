using System;
using System.Collections;
using System.Collections.Generic;

public sealed class ArrayHashSetNative<T>
{
    public T[] buffer;
    public int[] hashes;
    public int[] buckets;
    public int[] next;
    public int size;
    public int freeList;
    public int lastIndex;
    public IEqualityComparer<T> comparer;

    public int Count => size;
    public bool IsEmpty => size == 0;

    public ArrayHashSetNative() : this(11) { }

    public ArrayHashSetNative(int capacity) : this(capacity, null) { }

    public ArrayHashSetNative(int capacity, IEqualityComparer<T> comp)
    {
        buffer = new T[capacity];
        hashes = new int[capacity];
        buckets = new int[capacity];
        next = new int[capacity];
        size = 0;
        freeList = -1;
        lastIndex = 0;
        comparer = comp ?? EqualityComparer<T>.Default;
    }

    public bool Add(T value)
    {
        int hash = comparer.GetHashCode(value) & 0x7FFFFFFF;
        int index = hash % buckets.Length;

        for (int i = buckets[index] - 1; i >= 0; i = next[i])
            if (hashes[i] == hash && comparer.Equals(buffer[i], value))
                return false;

        int freeIndex;
        if (freeList >= 0)
        {
            freeIndex = freeList;
            freeList = next[freeIndex];
        }
        else
        {
            if (lastIndex == buffer.Length)
            {
                Resize(buffer.Length << 1);
                index = hash % buffer.Length;
            }

            freeIndex = lastIndex++;
        }

        buffer[freeIndex] = value;
        hashes[freeIndex] = hash;
        next[freeIndex] = buckets[index] - 1;
        buckets[index] = freeIndex + 1;
        ++size;
        return true;
    }

    public void Clear()
    {
        Array.Clear(buffer, 0, lastIndex);
        Array.Clear(hashes, 0, lastIndex);
        Array.Clear(buckets, 0, lastIndex);
        Array.Clear(next, 0, lastIndex);
        size = 0;
        lastIndex = 0;
        freeList = -1;
    }

    public bool Contains(T value)
    {
        if (size == 0) return false;

        int hash = comparer.GetHashCode(value) & 0x7FFFFFFF;
        for (int i = buckets[hash % buckets.Length] - 1; i >= 0; i = next[i])
            if (hashes[i] == hash && comparer.Equals(buffer[i], value))
                return true;

        return false;
    }

    public void CopyTo(T[] array) => CopyTo(array, 0, size);
    public void CopyTo(T[] array, int arrayIndex) => CopyTo(array, arrayIndex, size);

    public void CopyTo(T[] array, int arrayIndex, int length)
    {
        var count = 0;
        for (var i = 0; i < lastIndex && count < length; ++i)
            if (hashes[i] >= 0)
                array[arrayIndex + count++] = buffer[i];
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        if (size > 0)
            foreach (T item in other)
                Remove(item);
    }

    public void ExceptWith(T[] values)
    {
        for (var i = 0; i < values.Length; ++i)
            Remove(values[i]);
    }

    public void ExceptWith(List<T> values)
    {
        for (var i = 0; i < values.Count; ++i)
            Remove(values[i]);
    }

    public Enumerator GetEnumerator() => new(this);

    public bool IsSupersetOf(IEnumerable<T> other)
    {
        if (other is ICollection<T> collection && collection.Count == 0)
            return true;

        foreach (T item in other)
            if (!Contains(item))
                return false;

        return true;
    }

    public bool Overlaps(IEnumerable<T> other)
    {
        if (size == 0) return false;

        foreach (T item in other)
            if (Contains(item))
                return true;

        return false;
    }

    public bool Remove(T value)
    {
        if (size == 0) return false;

        int hash = comparer.GetHashCode(value) & 0x7FFFFFFF;
        int index = hash % buckets.Length;
        int removeIndex = -1;

        for (int i = buckets[index] - 1; i >= 0; i = next[i])
        {
            if (hashes[i] == hash && comparer.Equals(buffer[i], value))
            {
                if (removeIndex < 0)
                    buckets[index] = next[i] + 1;
                else
                    next[removeIndex] = next[i];

                buffer[i] = default;
                hashes[i] = -1;
                next[i] = freeList;
                freeList = i;
                --size;
                return true;
            }

            removeIndex = i;
        }

        return false;
    }

    public int RemoveWhere(Predicate<T> match)
    {
        var removed = 0;
        for (var i = 0; i < lastIndex; ++i)
            if (hashes[i] >= 0)
            {
                T value = buffer[i];
                if (match(value) && Remove(value))
                    ++removed;
            }

        return removed;
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        if (size == 0)
            UnionWith(other);
        else
            foreach (T item in other)
                if (!Remove(item))
                    Add(item);
    }

    public T[] ToArray()
    {
        var arr = new T[size];
        CopyTo(arr, 0, size);
        return arr;
    }

    public void TrimExcess()
    {
        if (size < buffer.Length)
            Resize(size);
    }

    public void UnionWith(IEnumerable<T> other)
    {
        foreach (T item in other)
            Add(item);
    }

    public void UnionWith(T[] values)
    {
        for (var i = 0; i < values.Length; ++i)
            Add(values[i]);
    }

    public void UnionWith(List<T> values)
    {
        for (var i = 0; i < values.Count; ++i)
            Add(values[i]);
    }

    public struct Enumerator : IEnumerator<T>
    {
        private int index;
        private ArrayHashSetNative<T> hashSet;
        private T current;

        public T Current => current;
        object IEnumerator.Current => current;

        internal Enumerator(ArrayHashSetNative<T> hashSet)
        {
            index = 0;
            this.hashSet = hashSet;
            current = default;
        }

        public void Dispose()
        {
            hashSet = null;
            current = default;
        }

        public bool MoveNext()
        {
            while (index < hashSet.lastIndex)
            {
                if (hashSet.hashes[index] >= 0)
                {
                    current = hashSet.buffer[index++];
                    return true;
                }

                ++index;
            }

            index = hashSet.lastIndex + 1;
            current = default;
            return false;
        }

        public void Reset()
        {
            index = 0;
            current = default;
        }
    }

    private void Resize(int capacity)
    {
        var newBuffer = new T[capacity];
        var newHashes = new int[capacity];
        var newBuckets = new int[capacity];
        var newNext = new int[capacity];
        int newLastIndex = lastIndex;
        int newFreeList = freeList;

        if (capacity > buffer.Length)
        {
            Array.Copy(buffer, 0, newBuffer, 0, lastIndex);
            Array.Copy(hashes, 0, newHashes, 0, lastIndex);

            for (var i = 0; i < lastIndex; ++i)
            {
                int index = newHashes[i] % capacity;
                newNext[i] = newBuckets[index] - 1;
                newBuckets[index] = i + 1;
            }
        }
        else if (capacity < buffer.Length)
        {
            newFreeList = -1;
            newLastIndex = 0;
            for (var i = 0; i < lastIndex; ++i)
            {
                int hashCode = hashes[i];
                if (hashCode >= 0)
                {
                    newBuffer[newLastIndex] = buffer[i];
                    newHashes[newLastIndex] = hashCode;

                    int index = hashCode % capacity;
                    newNext[newLastIndex] = newBuckets[index] - 1;
                    newBuckets[index] = newLastIndex + 1;
                    ++newLastIndex;
                }
            }
        }

        buffer = newBuffer;
        hashes = newHashes;
        buckets = newBuckets;
        next = newNext;
        freeList = newFreeList;
        lastIndex = newLastIndex;
    }
}