using System;
using UnityEngine;

public sealed class ArrayPool<T>
{
    const int MaxArraysPerBucket = 64;
    const int MaxBuckets = 16;
    static readonly T[] _emptyArray = Array.Empty<T>();
    public static readonly ArrayPool<T> Shared = new();

    readonly MinimumQueue<T[]>[] _buckets = new MinimumQueue<T[]>[MaxBuckets];

    ArrayPool()
    {
        for (int i = 0; i < _buckets.Length; ++i)
            _buckets[i] = new MinimumQueue<T[]>(4);
    }

    /// <summary> Rent cleared array w/ at least the specified length </summary>
    public T[] RentCleared(int minLen)
    {
        T[] rentedArray = Rent(minLen);
        Array.Clear(rentedArray, 0, rentedArray.Length);
        return rentedArray;
    }

    /// <summary> Rent an array w/ at least the specified length. </summary>
    public T[] Rent(int minLen)
    {
        Debug.Assert(minLen >= 0, "Array length must be non-negative.");

        if (minLen == 0) return _emptyArray;

        int size = minLen.NextPowerOfTwoAtLeast();
        int index = GetQueueIndex(size);

        if (index < 0 || index >= MaxBuckets) return new T[size];

        var bucket = _buckets[index];
        return bucket.Count > 0 ? bucket.Dequeue() : new T[size];
    }

    /// <summary> Returns an array to the pool </summary>
    public void Return(T[] array)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (array.Length == 0) throw new ArgumentException("Array must have positive length.", nameof(array));

        int index = GetQueueIndex(array.Length);
        Debug.Assert(index >= 0, $"Invalid bucket index: {index}");

        var bucket = _buckets[index];
        if (bucket.Count >= MaxArraysPerBucket) bucket.Dequeue(); // LRU-style eviction
        bucket.Enqueue(array);
    }

    /// <summary> Map array size to bucket index based on power-of-2 increments </summary>
    static int GetQueueIndex(int size) => size switch
    {
        8 => 0, 16 => 1, 32 => 2, 64 => 3, 128 => 4, 256 => 5,
        512 => 6, 1024 => 7, 2048 => 8, 4096 => 9, 8192 => 10,
        16384 => 11, 32768 => 12, 65536 => 13, 131072 => 14,
        262144 => 15, _ => -1
    };
}