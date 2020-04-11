using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

public unsafe struct UnsafeArray
{
    [NativeDisableUnsafePtrRestriction]
    public void* Ptr;
    public int Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T AsRef<T>(int i) where T : unmanaged => ref ((T*)Ptr)[i];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* AsPointer<T>(int i) where T : unmanaged => (T*)((byte*)Ptr + UnsafeUtility.SizeOf<T>() * i);
}

public unsafe struct UnsafeArray<T> where T : unmanaged
{
    [NativeDisableUnsafePtrRestriction]
    public T* Ptr;
    public int Length;

    public ref T this[int i] => ref Ptr[i];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator UnsafeArray(UnsafeArray<T> a)
        => UnsafeUtilityEx.AsRef<UnsafeArray>(UnsafeUtility.AddressOf(ref a));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator UnsafeArray<T>(UnsafeArray a)
        => UnsafeUtilityEx.AsRef<UnsafeArray<T>>(UnsafeUtility.AddressOf(ref a));
}