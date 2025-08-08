namespace Coplt.SoftGraphics;

internal readonly unsafe struct NSpan<T>(T* ptr, int length)
    where T : unmanaged
{
    public readonly T* Ptr = ptr;
    public readonly int Length = length;

    public Span<T> Span => new(Ptr, Length);
}
