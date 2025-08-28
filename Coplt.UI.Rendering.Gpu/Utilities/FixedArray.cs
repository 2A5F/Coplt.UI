using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Coplt.UI.Rendering.Gpu.Utilities;

[InlineArray(3)]
public struct FixedArray3<T>
{
    private T _;

    [UnscopedRef]
    public Span<T> AsSpan => this;
}

public static class FixedArrays
{
    extension<T>(FixedArray3<T> array) where T : IDisposable
    {
        public void Dispose()
        {
            foreach (var item in array)
            {
                item.Dispose();
            }
        }
    }
}
