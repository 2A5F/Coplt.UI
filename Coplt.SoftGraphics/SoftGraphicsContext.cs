using System.Runtime.CompilerServices;
using Coplt.Mathematics;
using Coplt.Mathematics.Simt;

namespace Coplt.SoftGraphics;

/// <summary>
/// An immediate context, operations are synchronously blocked
/// </summary>
public sealed class SoftGraphicsContext
{
    #region Fields

    private AJobScheduler m_job_scheduler = ParallelJobScheduler.Instance;

    private SoftTexture? m_rt_color;
    private SoftTexture? m_rt_depth_stencil;

    #endregion

    #region SetJobScheduler

    public void SetJobScheduler(AJobScheduler scheduler)
    {
        m_job_scheduler = scheduler;
    }

    #endregion

    #region SetRenderTarget

    public void SetRenderTarget(SoftTexture? Color, SoftTexture? DepthStencil = null)
    {
        if (Color is not null && !Color.HasColor)
            throw new InvalidOperationException($"Color texture {Color} dose not have color channel");
        if (DepthStencil is not null && !DepthStencil.HasDepthStencil)
            throw new InvalidOperationException($"Depth Stencil texture {DepthStencil} dose not have depth or stencil channel");
        m_rt_color = Color;
        m_rt_depth_stencil = DepthStencil;
    }

    #endregion

    #region Clear

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearRenderTarget(float4 Color) =>
        ClearRenderTarget(Color, 0, 0, SoftClearFlags.Color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearRenderTarget(float Depth, byte Stencil, SoftClearFlags Flags = SoftClearFlags.DepthStencil) =>
        ClearRenderTarget(default, Depth, Stencil, Flags & ~SoftClearFlags.Color);

    public void ClearRenderTarget(float4 Color, float Depth, byte Stencil, SoftClearFlags Flags)
    {
        if ((Flags & SoftClearFlags.Color) != 0) ClearColor(Color);
        // todo depth stencil
    }

    private void ClearColor(float4 Color)
    {
        var rt = m_rt_color;
        if (rt == null) throw new InvalidOperationException("Color texture not set");
        float4_mt16 color = Color;
        var w = (rt.Width + 3) / 4;
        var h = (rt.Height + 3) / 4;
        m_job_scheduler.Dispatch(w, h, (rt, color), static (ctx, x, y) => ctx.rt.QuadQuadStore(x, y, ctx.color));
    }

    #endregion

    #region Draw

    /// <summary>
    /// Dispatching pixel shader<br/>
    /// <b>Not an async operation, will block until complete</b>
    /// </summary>
    public void Draw<VertexData, PixelData>(
        SoftPrimitiveType Primitive,
        ReadOnlySpan<uint> Indices,
        ReadOnlySpan<VertexData> Vertices,
        SoftPixelShader<VertexData, PixelData> PixelShader
    )
        where VertexData : unmanaged, IVertexData<VertexData>
        where PixelData : unmanaged, IPixelData<PixelData>
    {
        // todo
    }

    #endregion
}

[Flags]
public enum SoftClearFlags
{
    None = 0,
    Color = 1 << 0,
    Depth = 1 << 1,
    Stencil = 1 << 2,
    DepthStencil = Depth | Stencil,
    All = Color | Depth | Stencil,
}
