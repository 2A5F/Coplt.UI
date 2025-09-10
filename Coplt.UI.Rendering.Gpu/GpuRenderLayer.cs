using Coplt.Mathematics;
using Coplt.UI.Collections;
using Coplt.UI.Rendering.Gpu.Graphics;

namespace Coplt.UI.Rendering.Gpu;

internal record GpuRenderLayer
{
    public GpuRenderLayerType Type;
    // l t w h; only use when Clip = true
    public uint4 ScissorRect;
    public bool Clip;

    internal EmbedList<BoxDataHandleData> m_items;

    public void AddItem(BoxDataHandleData item)
    {
        m_items.Add(item);
    }

    public void Reset()
    {
        m_items.Clear();
    }
}

internal enum GpuRenderLayerType : uint
{
    Opaque,
    Alpha,
    Shadow,
}
