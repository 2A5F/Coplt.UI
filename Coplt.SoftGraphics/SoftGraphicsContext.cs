using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using Coplt.Mathematics;
using Coplt.Mathematics.Simt;

namespace Coplt.SoftGraphics;

/// <summary>
/// An immediate context, operations are synchronously blocked
/// </summary>
public sealed class SoftGraphicsContext(AJobScheduler? scheduler = null)
{
    #region Fields

    private AJobScheduler m_job_scheduler = scheduler ?? ParallelJobScheduler.Instance;

    private SoftTexture? m_rt_color;
    private SoftTexture? m_rt_depth_stencil;

    private SoftViewport m_viewport;
    private SoftRect m_scissor_rect;

    #endregion

    #region Reset

    public void Reset()
    {
        m_rt_color = null;
        m_rt_depth_stencil = null;
        m_viewport = default;
        m_scissor_rect = default;
    }

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
        float4_mt color = Color;
        var w = (rt.Width + 3) / 4;
        var h = (rt.Height + 3) / 4;
        m_job_scheduler.Dispatch(w, h, (rt, color),
            [MethodImpl(512)] static (ctx, x, y) => ctx.rt.QuadQuadStore(x, y, ctx.color));
    }

    #endregion

    #region SetViewport

    public void SetViewport(SoftViewport viewport)
    {
        m_viewport = viewport;
    }

    #endregion

    #region SetScissorRect

    public void SetScissorRect(SoftRect rect)
    {
        m_scissor_rect = rect;
    }

    #endregion

    #region Draw

    /// <summary>
    /// Draw a mesh
    /// <b>Not an async operation, will block until complete</b>
    /// </summary>
    [MethodImpl(512)]
    public unsafe void Draw<TMesh, TPipeline>(
        TMesh Mesh, TPipeline Pipeline
    )
        where TMesh : ISoftMeshData, allows ref struct
        where TPipeline : ISoftGraphicPipelineState, ISoftPixelShader<TMesh>, allows ref struct
    {
        if (
            m_viewport.Width < float.Epsilon || m_viewport.Height < float.Epsilon ||
            m_viewport.MaxDepth - m_viewport.MinDepth < float.Epsilon
            || m_scissor_rect.Width == 0 || m_scissor_rect.Height == 0
        ) return;

        RasterizerContext rc = new()
        {
            viewport = ref m_viewport,
            state = ref Pipeline.State,
        };

        var triangles = PooledArray<TriangleContext>.Rent((int)(Mesh.MaxPrimitives + 15) / 16);
        var triangle_count = SetupTriangles(Mesh, rc, triangles.Span);
        if (triangle_count == 0) return;

        DispatchTileContext<TMesh, TPipeline> dtc = new(
            Mesh, Pipeline, m_rt_color, m_rt_depth_stencil, triangles.Span[..triangle_count]
        );
        Rasterizer<TMesh, TPipeline>.DispatchTile(m_job_scheduler, &rc, &dtc);
    }

    [MethodImpl(512)]
    private static int SetupTriangles<TMesh>(
        in TMesh Mesh, in RasterizerContext rc, Span<TriangleContext> triangles
    )
        where TMesh : ISoftMeshData, allows ref struct
    {
        var triangle_count = 0;

        var num_clusters = Mesh.NumClusters;
        for (var cluster = 0u; cluster < num_clusters; cluster++)
        {
            var num_primitives = Mesh.NumPrimitives(cluster);
            var num_primitives_mt = (num_primitives + 15) / 16;
            for (var primitive = 0u; primitive < num_primitives_mt; primitive++)
            {
                Mesh.Load(cluster, primitive * 16, out var cs_a, out var cs_b, out var cs_c, out var index, out var active_lanes);

                TriangleContext tc = new(cluster, index, active_lanes, cs_a, cs_b, cs_c);
                if (tc.Setup(in rc)) continue;
                triangles[triangle_count++] = tc;
            }
        }

        return triangle_count;
    }

    #endregion
}
