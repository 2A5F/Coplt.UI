using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Coplt.Dropping;
using Coplt.UI.Document.Interfaces;
using Coplt.UI.Rendering.Gpu;
using Coplt.UI.Rendering.Gpu.Graphics;
using Coplt.UI.Styles;

namespace Coplt.UI.Rendering;

[Dropping]
[StructLayout(LayoutKind.Auto)]
public partial record struct GpuRd() : IRenderData
{
    #region Fields

    internal GpuStyle m_gpu_style = new();

    internal BoxDataHandle m_box_data;

    internal ulong m_last_version;

    #endregion

    #region Props

    [UnscopedRef]
    public readonly ref readonly GpuStyle GpuStyle => ref m_gpu_style;

    #endregion

    #region IRenderData

    public void OnRemoveFromTree()
    {
        ReturnBoxData();
    }

    #endregion

    #region BoxData

    internal void MakeBoxData(BoxDataSource BoxDataSource)
    {
        m_box_data = BoxDataSource.RentBoxData();
    }

    [Drop]
    private void ReturnBoxData()
    {
        if (m_box_data.IsNull) return;
        m_box_data.Source?.ReturnBoxData(m_box_data);
        m_box_data = default;
    }

    #endregion
}
