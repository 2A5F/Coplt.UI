using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Coplt.UI.Rending.Gpu.Utilities;

[InlineArray(3)]
public struct FixedArray3<T>
{
    private T _;

    [UnscopedRef]
    public Span<T> AsSpan => this;
}

public static class FixedArrays
{
    public static void Dispose<T>(this FixedArray3<T> array) where T : IDisposable
    {
        foreach (var item in array)
        {
            item.Dispose();
        }
    }
}
