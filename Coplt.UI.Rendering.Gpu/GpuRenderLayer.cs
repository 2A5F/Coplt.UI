using System.Diagnostics.CodeAnalysis;
using Coplt.Dropping;
using Coplt.Mathematics;
using Coplt.UI.Collections;
using Coplt.UI.Rendering.Gpu.Graphics;

namespace Coplt.UI.Rendering.Gpu;

[Dropping(Unmanaged = true)]
public abstract unsafe partial class GpuRenderLayerManager(GpuRendererBackend Backend)
{
    #region Fields

    public GpuRendererBackend Backend { get; } = Backend;

    #endregion

    public abstract void Reset(uint width, uint height, float MaxZ);

    public abstract GpuRenderLayer RentLayer();

    public abstract void ReturnLayer(ref GpuRenderLayer? layer);

    public abstract void Record(ReadOnlySpan<GpuRenderLayer> layers);

    public abstract void Render(ReadOnlySpan<GpuRenderLayer> layers);
}

public abstract unsafe class GpuRenderLayer
{
    public GpuRenderLayerData Data;

    public abstract void AddItem(in BoxDataHandle handle);
}

public record struct GpuRenderLayerData
{
    public GpuRenderLayerType Type;
    // l t w h; only use when Clip = true
    public uint4 ScissorRect;
    public bool Clip;
}

public enum GpuRenderLayerType : uint
{
    Opaque,
    Alpha,
}
