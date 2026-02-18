using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using System.Diagnostics;
using Unity.Collections;

[StructLayout(LayoutKind.Sequential)]
[NativeContainer]
[NativeContainerSupportsDeallocateOnJobCompletion]
[DebuggerDisplay("Value = {Value}")]
public unsafe struct NativeAtomicCounterNative : IDisposable
{
#if ENABLE_UNITY_COLLECTIONS_CHECKS
    internal AtomicSafetyHandle safety;
    [NativeSetClassTypeToNullOnSchedule] internal DisposeSentinel disposeSentinel;
#endif

    [NativeDisableUnsafePtrRestriction] private void* buffer;
    private readonly Allocator allocatorLabel;

    public int Value
    {
        get
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(safety);
#endif
            return *(int*)buffer;
        }
    }

    public bool IsCreated => buffer != null;

    public NativeAtomicCounterNative(Allocator allocator)
    {
        buffer = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<int>(), UnsafeUtility.AlignOf<int>(), allocator);
        allocatorLabel = allocator;
        *(int*)buffer = 0;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        DisposeSentinel.Create(out safety, out disposeSentinel, 1, allocatorLabel);
#endif
    }

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
        else { throw new Exception("NativeAtomicCounterNative has yet to be allocated or has been deallocated!"); }
#endif
    }

    [WriteAccessRequired]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Increment()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(safety);
#endif
        return System.Threading.Interlocked.Increment(ref *(int*)buffer);
    }

    [WriteAccessRequired]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckWriteAndBumpSecondaryVersion(safety);
#endif
        System.Threading.Interlocked.Exchange(ref *(int*)buffer, 0);
    }
}