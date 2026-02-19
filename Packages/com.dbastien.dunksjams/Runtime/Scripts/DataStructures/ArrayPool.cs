using System;
using UnityEngine;

public sealed class ArrayPool<T>
{
	private const int MaxArraysPerBucket = 64;
	private const int MaxBuckets = 16;

	private static readonly T[] EmptyArray = Array.Empty<T>();
	public static readonly ArrayPool<T> Shared = new();

	private readonly ArrayQueue<T[]>[] buckets = new ArrayQueue<T[]>[MaxBuckets];

	private ArrayPool()
	{
		for (var i = 0; i < buckets.Length; ++i)
			buckets[i] = new ArrayQueue<T[]>(capacity: 4, fullMode: QueueFullMode.Grow);
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

		if (minLen == 0) return EmptyArray;

		int size = minLen.NextPowerOfTwoAtLeast();
		int index = DataStructureUtils.GetQueueIndex(size);

		if ((uint)index >= (uint)MaxBuckets) return new T[size];

		var bucket = buckets[index];
		return bucket.Count > 0 ? bucket.Dequeue() : new T[size];
	}

	/// <summary> Returns an array to the pool </summary>
	public void Return(T[] array)
	{
		if (array == null) throw new ArgumentNullException(nameof(array));
		if (array.Length == 0) throw new ArgumentException("Array must have positive length.", nameof(array));

		int index = DataStructureUtils.GetQueueIndex(array.Length);
		Debug.Assert((uint)index < (uint)MaxBuckets, $"Invalid bucket index: {index}");

		var bucket = buckets[index];

		if (bucket.Count >= MaxArraysPerBucket)
			bucket.Dequeue(); // drop oldest

		bucket.Enqueue(array);
	}
}