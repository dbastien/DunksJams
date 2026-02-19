using System;
using System.Threading;
using UnityEngine;

public sealed class ConcurrentArrayPool<T>
{
	private const int MaxArraysPerBucket = 64;
	private const int MaxBuckets = 16;

	private static readonly T[] EmptyArray = Array.Empty<T>();
	public static readonly ConcurrentArrayPool<T> Shared = new();

	private readonly ArrayQueue<T[]>[] buckets = new ArrayQueue<T[]>[MaxBuckets];
	private readonly SpinLock[] locks = new SpinLock[MaxBuckets];

	private ConcurrentArrayPool()
	{
		for (var i = 0; i < buckets.Length; ++i)
		{
			buckets[i] = new ArrayQueue<T[]>(capacity: 4, fullMode: QueueFullMode.Grow);
			locks[i] = new SpinLock(false); // no thread-owner tracking for lower overhead
		}
	}

	/// <summary> Rent cleared array w/ at least the specified length </summary>
	public T[] RentCleared(int minLen)
	{
		T[] rentedArray = Rent(minLen);
		Array.Clear(rentedArray, 0, rentedArray.Length);
		return rentedArray;
	}

	/// <summary> Rent array w/ at least the specified length </summary>
	public T[] Rent(int minLen)
	{
		Debug.Assert(minLen >= 0, "Array length must be non-negative.");

		if (minLen <= 0) return EmptyArray;

		int size = minLen.NextPowerOfTwoAtLeast();
		int index = DataStructureUtils.GetQueueIndex(size);

		if ((uint)index >= (uint)MaxBuckets)
			return new T[size];

		var bucket = buckets[index];

		// lock-avoid fast path (racy is fine; we re-check under the lock)
		if (bucket.Count == 0) return new T[size];

		ref SpinLock spinLock = ref locks[index];
		var lockTaken = false;
		try
		{
			spinLock.TryEnter(ref lockTaken);
			if (!lockTaken || bucket.Count == 0) return new T[size];
			return bucket.Dequeue();
		}
		finally
		{
			if (lockTaken) spinLock.Exit(false);
		}
	}

	/// <summary> Returns an array to the pool </summary>
	public void Return(T[] array)
	{
		if (array == null) throw new ArgumentNullException(nameof(array));
		if (array.Length == 0) throw new ArgumentException("Array must have positive length.", nameof(array));

		int index = DataStructureUtils.GetQueueIndex(array.Length);
		if ((uint)index >= (uint)MaxBuckets)
			return; // not a supported bucket size; just don't pool it

		var bucket = buckets[index];

		ref SpinLock spinLock = ref locks[index];
		var lockTaken = false;
		try
		{
			spinLock.TryEnter(ref lockTaken);
			if (!lockTaken) return;

			if (bucket.Count >= MaxArraysPerBucket)
				bucket.Dequeue(); // drop oldest

			bucket.Enqueue(array);
		}
		finally
		{
			if (lockTaken) spinLock.Exit(false);
		}
	}
}