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

    private AJobScheduler m_job_scheduler = scheduler ?? new ParallelJobScheduler();

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
        float4_mt16 color = Color;
        var w = (rt.Width + 3) / 4;
        var h = (rt.Height + 3) / 4;
        m_job_scheduler.Dispatch(w, h, (rt, color), static (ctx, x, y) => ctx.rt.QuadQuadStore(x, y, ctx.color));
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
    /// Dispatching pixel shader<br/>
    /// <b>Not an async operation, will block until complete</b>
    /// </summary>
    public unsafe void Draw<VertexData, PixelData>(
        uint NumIndices,
        ReadOnlySpan<uint_mt16> IndicesA,
        ReadOnlySpan<uint_mt16> IndicesB,
        ReadOnlySpan<uint_mt16> IndicesC,
        VertexData Vertices,
        SoftPixelShader<VertexData, PixelData> PixelShader
    )
        where VertexData : IVertexData, allows ref struct
        where PixelData : IPixelData, allows ref struct
    {
        if (NumIndices % 3 != 0) throw new InvalidOperationException("Indices must be a multiple of 3");
        if (IndicesA.Length != IndicesB.Length || IndicesA.Length != IndicesC.Length)
            throw new InvalidOperationException("Indices length not same");
        if (IndicesA.Length < (NumIndices + 15) / 16)
            throw new InvalidOperationException("Indices length is less then NumIndices");
        if (
            m_viewport.Width < float.Epsilon || m_viewport.Height < float.Epsilon ||
            m_viewport.MaxDepth - m_viewport.MinDepth < float.Epsilon
            || m_scissor_rect.Width == 0 || m_scissor_rect.Height == 0
        ) return;

        fixed (uint_mt16* p_indices_a = IndicesA)
        fixed (uint_mt16* p_indices_b = IndicesB)
        fixed (uint_mt16* p_indices_c = IndicesC)
        {
            NSpan<uint_mt16> n_indices_a = new(p_indices_a, IndicesA.Length);
            NSpan<uint_mt16> n_indices_b = new(p_indices_b, IndicesB.Length);
            NSpan<uint_mt16> n_indices_c = new(p_indices_c, IndicesC.Length);
            var num_triangles = NumIndices / 3;
            var num_triangles_mt = (num_triangles + 15) / 16;

            #region Calc AABB

            using var triangles_aabb_min_x = PooledArray.Rent<float_mt16>((int)num_triangles_mt);
            using var triangles_aabb_min_y = PooledArray.Rent<float_mt16>((int)num_triangles_mt);
            using var triangles_aabb_max_x = PooledArray.Rent<float_mt16>((int)num_triangles_mt);
            using var triangles_aabb_max_y = PooledArray.Rent<float_mt16>((int)num_triangles_mt);

            m_job_scheduler.Dispatch(
                num_triangles_mt,
                (
                    num_triangles, n_indices_a, n_indices_b, n_indices_c, p_vertices: (nuint)(&Vertices),
                    triangles_aabb_min_x, triangles_aabb_min_y, triangles_aabb_max_x, triangles_aabb_max_y
                ),
                static (ctx, i, _) =>
                {
                    var num_triangles = ctx.num_triangles;
                    var indices_a = ctx.n_indices_a.Span;
                    var indices_b = ctx.n_indices_b.Span;
                    var indices_c = ctx.n_indices_c.Span;
                    var vertices = *(VertexData*)ctx.p_vertices;
                    var triangles_aabb_min_x = ctx.triangles_aabb_min_x.Span;
                    var triangles_aabb_min_y = ctx.triangles_aabb_min_y.Span;
                    var triangles_aabb_max_x = ctx.triangles_aabb_max_x.Span;
                    var triangles_aabb_max_y = ctx.triangles_aabb_max_y.Span;

                    var index = i * 16 + SoftGraphicsUtils.IncMt16;
                    var active_lanes = index < num_triangles;

                    var index_a = indices_a[(int)i];
                    var index_b = indices_b[(int)i];
                    var index_c = indices_c[(int)i];

                    var pos_a = vertices.Gather_Position_ClipSpace_XY_Only(index_a.asi(), active_lanes);
                    var pos_b = vertices.Gather_Position_ClipSpace_XY_Only(index_b.asi(), active_lanes);
                    var pos_c = vertices.Gather_Position_ClipSpace_XY_Only(index_c.asi(), active_lanes);

                    var min = pos_a.min(pos_b).min(pos_c);
                    var max = pos_a.max(pos_b).max(pos_c);

                    triangles_aabb_min_x[(int)i] = min.x;
                    triangles_aabb_min_y[(int)i] = min.y;
                    triangles_aabb_max_x[(int)i] = max.x;
                    triangles_aabb_max_y[(int)i] = max.y;
                }
            );

            #endregion

            // todo
        }
    }

    #endregion
}
