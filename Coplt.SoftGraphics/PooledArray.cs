using System.Buffers;

namespace Coplt.SoftGraphics;

internal readonly struct PooledArray<T> : IDisposable
{
    public readonly T[] Array;
    public readonly int Size;

    public Span<T> Span => Array.AsSpan(0, Size);

    internal PooledArray(T[] array, int size)
    {
        Array = array;
        Size = size;
    }

    public void Dispose() => ArrayPool<T>.Shared.Return(Array);

    public static PooledArray<T> Rent(int size) => new(ArrayPool<T>.Shared.Rent(size), size);
}

internal static class PooledArray
{
    public static PooledArray<T> Rent<T>(int size) => new(ArrayPool<T>.Shared.Rent(size), size);
}
