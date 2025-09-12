using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Coplt.UI.Rendering.Gpu.Utilities;

public static class InlineArrayExtensions
{
    extension<T>(ref InlineArray3<T> array) where T : IDisposable
    {
        public Span<T> AsSpan => array;

        public void Dispose()
        {
            foreach (var item in array)
            {
                item.Dispose();
            }
        }
    }

    extension<T>(ref InlineArray4<T> array) where T : IDisposable
    {
        public Span<T> AsSpan => array;

        public void Dispose()
        {
            foreach (var item in array)
            {
                item.Dispose();
            }
        }
    }
}
