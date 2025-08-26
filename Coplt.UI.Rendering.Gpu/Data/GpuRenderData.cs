using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Coplt.UI.Rendering.Gpu.Graphics;
using Coplt.UI.Styles;

namespace Coplt.UI.Rendering;

[StructLayout(LayoutKind.Auto)]
public record struct GpuRd()
{
    #region Fields

    internal GpuStyle m_gpu_style = new();

    internal BoxData m_box_data;

    internal ulong m_last_version;

    #endregion

    #region Props

    [UnscopedRef]
    public readonly ref readonly GpuStyle GpuStyle => ref m_gpu_style;

    #endregion
}
