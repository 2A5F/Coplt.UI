using System.Diagnostics.CodeAnalysis;
using Coplt.UI.Collections;
using Coplt.UI.Rendering.Gpu.Graphics;
using Coplt.UI.Styles;

namespace Coplt.UI.Rendering;

public struct GpuRd()
{
    #region Fields

    internal bool m_initialized;
    internal GpuStyle m_gpu_style = new();

    internal EmbedList<BatchData> m_batch_data;
    internal EmbedList<BoxData> m_box_data;

    #endregion

    #region Props

    [UnscopedRef]
    public readonly ref readonly GpuStyle GpuStyle => ref m_gpu_style;

    #endregion
}
