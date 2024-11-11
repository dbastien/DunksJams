using System;
using System.Threading;
using UnityEngine;

public sealed class ConcurrentArrayPool<T>
{
    const int MaxArraysPerBucket = 64;
    const int MaxBuckets = 18;
    static readonly T[] _emptyArray = Array.Empty<T>();
    public static readonly ConcurrentArrayPool<T> Shared = new();

    readonly MinimumQueue<T[]>[] _buckets = new MinimumQueue<T[]>[MaxBuckets];
    readonly SpinLock[] _locks = new SpinLock[MaxBuckets];

    ConcurrentArrayPool()
    {
        for (int i = 0; i < _buckets.Length; ++i)
        {
            _buckets[i] = new MinimumQueue<T[]>(4);  // small initial size
            _locks[i] = new SpinLock(false);  // no thread-owner tracking for lower overhead
        }
    }

    /// <summary> Rents a cleared array of at least the specified length. </summary>
    public T[] RentCleared(int minLen)
    {
        T[] rentedArray = Rent(minLen);
        Array.Clear(rentedArray, 0, rentedArray.Length);
        return rentedArray;
    }

    /// <summary> Rents an array of at least the specified length. </summary>
    public T[] Rent(int minLen)
    {
        Debug.Assert(minLen >= 0, "Array length must be non-negative.");

        if (minLen == 0) return _emptyArray;

        int size = CalculateSize(minLen);
        int index = GetQueueIndex(size);

        if (index < 0 || index >= MaxBuckets) return new T[size];  // Fallback if index is invalid

        // Avoid lock if bucket is empty
        if (_buckets[index].Count == 0) return new T[size];  

        ref var spinLock = ref _locks[index];
        bool lockTaken = false;
        try
        {
            spinLock.TryEnter(ref lockTaken);
            return lockTaken && _buckets[index].Count > 0 ? _buckets[index].Dequeue() : new T[size];
        }
        finally
        {
            if (lockTaken) spinLock.Exit(false);
        }
    }

    /// <summary> Returns an array to the pool for reuse. </summary>
    public void Return(T[] array)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (array.Length == 0) throw new ArgumentException("Array must have positive length.", nameof(array));

        int index = GetQueueIndex(array.Length);
        Debug.Assert(index >= 0, $"Invalid bucket index: {index}");

        ref var spinLock = ref _locks[index];
        bool lockTaken = false;
        try
        {
            spinLock.TryEnter(ref lockTaken);
            if (lockTaken)
            {
                var bucket = _buckets[index];
                if (bucket.Count >= MaxArraysPerBucket) bucket.Dequeue(); // LRU-style eviction
                bucket.Enqueue(array);
            }
        }
        finally
        {
            if (lockTaken) spinLock.Exit(false);
        }
    }

    /// <summary> Calculates the smallest power of 2 >= size. </summary>
    static int CalculateSize(int size) => 1 << (31 - (size - 1 | 7).LeadingZeroCount());

    /// <summary> Maps array size to bucket index based on power-of-2 increments. </summary>
    static int GetQueueIndex(int size) => size switch
    {
        8 => 0, 16 => 1, 32 => 2, 64 => 3, 128 => 4, 256 => 5,
        512 => 6, 1024 => 7, 2048 => 8, 4096 => 9, 8192 => 10,
        16384 => 11, 32768 => 12, 65536 => 13, 131072 => 14,
        262144 => 15, 524288 => 16, 1048576 => 17, _ => -1
    };
}
