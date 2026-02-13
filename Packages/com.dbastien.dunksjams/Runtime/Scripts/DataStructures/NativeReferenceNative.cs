using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

[GenerateTestsForBurstCompatibility]
[StructLayout(LayoutKind.Sequential)]
public unsafe struct NativeReferenceNative<T> where T : unmanaged
{
    [NativeDisableUnsafePtrRestriction] internal readonly void* ptr;

    public NativeReferenceNative(ref T value) => ptr = UnsafeUtility.AddressOf(ref value);

    public NativeReferenceNative(void* value) => ptr = value;

    public ref T Value => ref UnsafeUtility.AsRef<T>(ptr);
    public T* Ptr => (T*)ptr;
}