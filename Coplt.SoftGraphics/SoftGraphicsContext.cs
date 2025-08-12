using System.Runtime.CompilerServices;
using Coplt.Mathematics;
using Coplt.Mathematics.Simt;
using Coplt.SoftGraphics.Utilities;

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

            #region Calc AABB then Cull then Gen Task

            using var tasks = new Collector<PixelTask>();

            m_job_scheduler.Dispatch(
                num_triangles_mt,
                (
                    num_triangles, n_indices_a, n_indices_b, n_indices_c, p_vertices: (nuint)(&Vertices), m_viewport,
                    p_tasks: (nuint)(&tasks)
                ),
                static ctx =>
                {
                    ref var tasks = ref *(Collector<PixelTask>*)ctx.p_tasks;
                    return tasks.Alloc();
                },
                static (ctx, local, i, _) =>
                {
                    ref var tasks = ref *(Collector<PixelTask>*)ctx.p_tasks;
                    ref var local_tasks = ref tasks[local];
                    var num_triangles = ctx.num_triangles;
                    var viewport = ctx.m_viewport;
                    var indices_a = ctx.n_indices_a.Span;
                    var indices_b = ctx.n_indices_b.Span;
                    var indices_c = ctx.n_indices_c.Span;
                    var vertices = *(VertexData*)ctx.p_vertices;

                    var index = i * 16 + SoftGraphicsUtils.IncMt16;
                    var active_lanes = index < num_triangles;

                    var index_a = indices_a[(int)i];
                    var index_b = indices_b[(int)i];
                    var index_c = indices_c[(int)i];

                    var pos_a = vertices.Gather_Position_ClipSpace(index_a.asi(), active_lanes);
                    var pos_b = vertices.Gather_Position_ClipSpace(index_b.asi(), active_lanes);
                    var pos_c = vertices.Gather_Position_ClipSpace(index_c.asi(), active_lanes);

                    var visible_a = Visible(pos_a);
                    var visible_b = Visible(pos_a);
                    var visible_c = Visible(pos_a);

                    var visible_any = active_lanes & (visible_a | visible_b | visible_c);

                    var ndc_a = ToNdc(pos_a);
                    var ndc_b = ToNdc(pos_b);
                    var ndc_c = ToNdc(pos_c);

                    var ss_a = ToScreenSpace(ndc_a, viewport);
                    var ss_b = ToScreenSpace(ndc_b, viewport);
                    var ss_c = ToScreenSpace(ndc_c, viewport);

                    var clamp_min = new float3_mt16(0, 0, viewport.MinDepth);
                    var clamp_max = new float3_mt16(viewport.Width, viewport.Height, viewport.MaxDepth);

                    var css_a = ss_a.clamp(clamp_min, clamp_max);
                    var css_b = ss_b.clamp(clamp_min, clamp_max);
                    var css_c = ss_c.clamp(clamp_min, clamp_max);

                    var min = (int2_mt16)math_mt.floor(css_a.xy.min(css_b.xy).min(css_c.xy) * (1f / 4f));
                    var max = (int2_mt16)math_mt.ceil(css_a.xy.max(css_b.xy).max(css_c.xy) * (1f / 4f));

                    var visibles = new Span<b32>(&visible_any, 16);
                    var indexes = new Span<uint>(&index, 16);

                    var min_xs = new Span<uint>(&min.x, 16);
                    var min_ys = new Span<uint>(&min.y, 16);

                    var max_xs = new Span<uint>(&max.x, 16);
                    var max_ys = new Span<uint>(&max.y, 16);

                    var cs_a_x = new Span<float>(&pos_a.x, 16);
                    var cs_a_y = new Span<float>(&pos_a.y, 16);
                    var cs_a_z = new Span<float>(&pos_a.z, 16);
                    var cs_a_w = new Span<float>(&pos_a.w, 16);

                    var cs_b_x = new Span<float>(&pos_b.x, 16);
                    var cs_b_y = new Span<float>(&pos_b.y, 16);
                    var cs_b_z = new Span<float>(&pos_b.z, 16);
                    var cs_b_w = new Span<float>(&pos_b.w, 16);

                    var cs_c_x = new Span<float>(&pos_c.x, 16);
                    var cs_c_y = new Span<float>(&pos_c.y, 16);
                    var cs_c_z = new Span<float>(&pos_c.z, 16);
                    var cs_c_w = new Span<float>(&pos_c.w, 16);

                    var ss_a_x = new Span<float>(&ss_a.x, 16);
                    var ss_a_y = new Span<float>(&ss_a.y, 16);
                    var ss_a_z = new Span<float>(&ss_a.z, 16);

                    var ss_b_x = new Span<float>(&ss_b.x, 16);
                    var ss_b_y = new Span<float>(&ss_b.y, 16);
                    var ss_b_z = new Span<float>(&ss_b.z, 16);

                    var ss_c_x = new Span<float>(&ss_c.x, 16);
                    var ss_c_y = new Span<float>(&ss_c.y, 16);
                    var ss_c_z = new Span<float>(&ss_c.z, 16);

                    for (var c = 0; c < 16; c++)
                    {
                        if (!visibles[c]) continue;
                        ref var slot = ref local_tasks.Add();
                        slot.Index = indexes[c];

                        slot.MinX = min_xs[c];
                        slot.MinY = min_ys[c];

                        slot.MaxX = max_xs[c];
                        slot.MaxY = max_ys[c];

                        slot.PosA_CS_X = cs_a_x[c];
                        slot.PosA_CS_Y = cs_a_y[c];
                        slot.PosA_CS_Z = cs_a_z[c];
                        slot.PosA_CS_W = cs_a_w[c];

                        slot.PosB_CS_X = cs_b_x[c];
                        slot.PosB_CS_Y = cs_b_y[c];
                        slot.PosB_CS_Z = cs_b_z[c];
                        slot.PosB_CS_W = cs_b_w[c];

                        slot.PosC_CS_X = cs_c_x[c];
                        slot.PosC_CS_Y = cs_c_y[c];
                        slot.PosC_CS_Z = cs_c_z[c];
                        slot.PosC_CS_W = cs_c_w[c];

                        slot.PosA_SS_X = ss_a_x[c];
                        slot.PosA_SS_Y = ss_a_y[c];
                        slot.PosA_SS_Z = ss_a_z[c];

                        slot.PosB_SS_X = ss_b_x[c];
                        slot.PosB_SS_Y = ss_b_y[c];
                        slot.PosB_SS_Z = ss_b_z[c];

                        slot.PosC_SS_X = ss_c_x[c];
                        slot.PosC_SS_Y = ss_c_y[c];
                        slot.PosC_SS_Z = ss_c_z[c];
                    }

                    return;

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    static b32_mt16 Visible(float4_mt16 pos)
                    {
                        var nw = -pos.w;
                        var pw = pos.w;
                        return pos.x >= nw & pos.x <= pw & pos.y >= nw & pos.y <= pw & pos.z >= 0 & pos.z <= pw;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    static float3_mt16 ToNdc(float4_mt16 pos) => new(pos.x / pos.w, pos.y / pos.w, pos.z / pos.w);

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    static float3_mt16 ToScreenSpace(float3_mt16 pos, SoftViewport viewport)
                    {
                        var x = math_mt.fam(viewport.TopLeftX, (pos.x + 1) * 0.5f, viewport.Width);
                        var y = math_mt.fam(viewport.TopLeftX, (1 - pos.y) * 0.5f, viewport.Height);
                        var z = viewport.MinDepth + pos.z * (viewport.MaxDepth - viewport.MinDepth);
                        return new(x, y, z);
                    }
                }
            );

            #endregion

            #region Dispatch pixel

            using var collected_tasks = tasks.ToCollected();
            foreach (ref var pt in collected_tasks)
            {
                m_job_scheduler.Dispatch(
                    pt.MaxX - pt.MinX, pt.MaxY - pt.MinX,
                    (pt, 0),
                    static (ctx, x, y) =>
                    {
                        var pt = ctx.pt;

                        // todo calc z index

                        var ss_a = new float3_mt16(pt.PosA_SS_X, pt.PosA_SS_Y, pt.PosA_SS_Z);
                        var ss_b = new float3_mt16(pt.PosB_SS_X, pt.PosB_SS_Y, pt.PosB_SS_Z);
                        var ss_c = new float3_mt16(pt.PosC_SS_X, pt.PosC_SS_Y, pt.PosC_SS_Z);
                    }
                );
            }

            #endregion
        }
    }

    private record struct PixelTask
    {
        public uint Index;

        public uint MinX;
        public uint MinY;

        public uint MaxX;
        public uint MaxY;

        public float PosA_CS_X;
        public float PosA_CS_Y;
        public float PosA_CS_Z;
        public float PosA_CS_W;

        public float PosB_CS_X;
        public float PosB_CS_Y;
        public float PosB_CS_Z;
        public float PosB_CS_W;

        public float PosC_CS_X;
        public float PosC_CS_Y;
        public float PosC_CS_Z;
        public float PosC_CS_W;

        public float PosA_SS_X;
        public float PosA_SS_Y;
        public float PosA_SS_Z;

        public float PosB_SS_X;
        public float PosB_SS_Y;
        public float PosB_SS_Z;

        public float PosC_SS_X;
        public float PosC_SS_Y;
        public float PosC_SS_Z;
    }

    #endregion
}
