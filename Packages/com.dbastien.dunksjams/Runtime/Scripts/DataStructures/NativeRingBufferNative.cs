using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using System.Diagnostics;

public enum NativeRingBufferOptions
{
    UninitializedMemory = 0,
    ClearMemory = 1
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeRingBufferData
{
    public int head;
    public int tail;
}

[GenerateTestsForBurstCompatibility]
[StructLayout(LayoutKind.Sequential)]
[NativeContainer]
[DebuggerDisplay("Count = {Count}")]
[DebuggerDisplay("Capacity = {Capacity}")]
[NativeContainerSupportsDeallocateOnJobCompletion]
public unsafe struct NativeRingBufferNative<T> : IDisposable where T : struct
{
    public static readonly int SizeOfT = UnsafeUtility.SizeOf<T>();

#if ENABLE_UNITY_COLLECTIONS_CHECKS
    internal AtomicSafetyHandle safety;
    [NativeSetClassTypeToNullOnSchedule]
    internal DisposeSentinel disposeSentinel;
#endif
    [NativeDisableUnsafePtrRestriction]
    internal void* buffer;

    internal readonly Allocator allocatorLabel;
    internal readonly int capacity;

    public bool IsCreated => buffer != null;
    public int Capacity => capacity;

    public int Count
    {
        get
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(safety);
#endif
            NativeRingBufferData* data = (NativeRingBufferData*)buffer;
            int count = data->head - data->tail;
            return count < 0 ? count + capacity : count;
        }
    }

    public long Size => Count * SizeOfT;
    public long TotalSize => capacity * SizeOfT;

    public bool IsEmpty
    {
        get
        {
            NativeRingBufferData* data = (NativeRingBufferData*)buffer;
            return data->head == data->tail;
        }
    }

    public NativeRingBufferNative(int capacity, Allocator allocator, NativeRingBufferOptions options = NativeRingBufferOptions.ClearMemory) : this(capacity, allocator, options, 2) { }

    NativeRingBufferNative(int capacity, Allocator allocator, NativeRingBufferOptions options, int stackDepth)
    {
        UnityEngine.Debug.Assert(capacity > 0, "NativeRingBufferNative cannot be created with 0 or negative capacity");

        long totalSize = (capacity * SizeOfT) + UnsafeUtility.SizeOf<NativeRingBufferData>();

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (allocator <= Allocator.None)
            throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", nameof(allocator));

        if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Count must be >= 0");

        IsBlittableAndThrow();

        if (totalSize > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(capacity), $"Capacity * sizeof(T) cannot exceed {int.MaxValue} bytes");
#endif
        allocatorLabel = allocator;
        this.capacity = capacity;

        buffer = UnsafeUtility.Malloc(totalSize, UnsafeUtility.AlignOf<T>(), allocatorLabel);

        if (options == NativeRingBufferOptions.ClearMemory)
        {
            UnsafeUtility.MemClear(buffer, totalSize);
        }
        else
        {
            NativeRingBufferData* data = (NativeRingBufferData*)buffer;
            data->head = 0;
            data->tail = 0;
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        DisposeSentinel.Create(out safety, out disposeSentinel, stackDepth, allocatorLabel);
#endif
    }

    [BurstDiscard]
    internal static void IsBlittableAndThrow()
    {
        if (!UnsafeUtility.IsBlittable<T>())
            throw new ArgumentException($"{typeof(T)} used in NativeRingBufferNative<{typeof(T)}> must be blittable");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetCount()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckReadAndThrow(safety);
#endif
        NativeRingBufferData* data = (NativeRingBufferData*)buffer;
        int count = data->head - data->tail;
        return count < 0 ? count + capacity : count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long GetSize() => GetCount() * SizeOfT;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long GetTotalSize() => capacity * SizeOfT;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetIsEmpty()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckReadAndThrow(safety);
#endif
        NativeRingBufferData* data = (NativeRingBufferData*)buffer;
        return data->head == data->tail;
    }

    [WriteAccessRequired]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Push(T value)
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(safety);
#endif
        NativeRingBufferData* data = (NativeRingBufferData*)buffer;

        int next = data->head + 1;
        next = next == capacity ? 0 : next;
        if (next == data->tail) return false;

        UnsafeUtility.ArrayElementAsRef<T>(data + 1, data->head) = value;
        data->head = next;
        return true;
    }

    [WriteAccessRequired]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Push(ref T value)
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(safety);
#endif
        NativeRingBufferData* data = (NativeRingBufferData*)buffer;

        int next = data->head + 1;
        next = next == capacity ? 0 : next;
        if (next == data->tail) return false;

        UnsafeUtility.ArrayElementAsRef<T>(data + 1, data->head) = value;
        data->head = next;
        return true;
    }

    [WriteAccessRequired]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Pop()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(safety);
#endif
        NativeRingBufferData* data = (NativeRingBufferData*)buffer;

        if (data->head == data->tail) return false;

        int next = data->tail + 1;
        next = next == capacity ? 0 : next;
        data->tail = next;
        return true;
    }

    [WriteAccessRequired]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPop(out T value)
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(safety);
#endif
        value = default;

        NativeRingBufferData* data = (NativeRingBufferData*)buffer;

        if (data->head == data->tail) return false;

        int next = data->tail + 1;
        next = next == capacity ? 0 : next;

        value = UnsafeUtility.ArrayElementAsRef<T>(data + 1, data->tail);
        data->tail = next;
        return true;
    }

    [WriteAccessRequired]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(safety);
#endif
        long totalSize = (capacity * SizeOfT) + UnsafeUtility.SizeOf<NativeRingBufferData>();
        UnsafeUtility.MemClear(buffer, totalSize);
    }

    [WriteAccessRequired]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FastClear()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(safety);
#endif
        NativeRingBufferData* data = (NativeRingBufferData*)buffer;
        data->tail = data->head;
    }

    [WriteAccessRequired]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FastClearZero()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(safety);
#endif
        NativeRingBufferData* data = (NativeRingBufferData*)buffer;
        data->head = 0;
        data->tail = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        DisposeSentinel.Dispose(ref safety, ref disposeSentinel);
#endif
        if (buffer != null)
        {
            UnsafeUtility.Free(buffer, allocatorLabel);
            buffer = null;
        }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        else
            throw new Exception("NativeRingBufferNative has yet to be allocated or has been de-allocated!");
#endif
    }
}
