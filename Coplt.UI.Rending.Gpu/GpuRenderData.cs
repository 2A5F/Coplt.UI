using System.Diagnostics.CodeAnalysis;
using Coplt.UI.Styles;

namespace Coplt.UI.Rending.Gpu;

public struct GpuRd()
{
    internal bool m_initialized;
    internal GpuStyle m_gpu_style = new();

    [UnscopedRef]
    public readonly ref readonly GpuStyle GpuStyle => ref m_gpu_style;
}
