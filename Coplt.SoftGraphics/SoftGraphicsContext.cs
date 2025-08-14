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
    public unsafe void Draw<TMesh, TPipeline, TInput, TOutput>(
        TMesh Mesh, TPipeline Pipeline, SoftPixelShader<TPipeline, TInput, TOutput> PixelShader
    )
        where TMesh : ISoftMeshData, allows ref struct
        where TPipeline : ISoftGraphicPipelineState, ISoftPixelShader<TMesh, TInput, TOutput>, allows ref struct
        where TInput : allows ref struct
        where TOutput : allows ref struct
    {
        if (
            m_viewport.Width < float.Epsilon || m_viewport.Height < float.Epsilon ||
            m_viewport.MaxDepth - m_viewport.MinDepth < float.Epsilon
            || m_scissor_rect.Width == 0 || m_scissor_rect.Height == 0
        ) return;

        var num_clusters = Mesh.NumClusters;
        for (var cluster = 0u; cluster < num_clusters; cluster++)
        {
            var num_primitives = Mesh.NumPrimitives(cluster);
            var num_primitives_mt = (num_primitives + 15) / 16;
            for (var primitive = 0u; primitive < num_primitives_mt; primitive++)
            {
                Mesh.Load(cluster, primitive * 16, out var cs_a, out var cs_b, out var cs_c, out var index, out var active_lanes);

                var visible_a = Visible(cs_a);
                var visible_b = Visible(cs_b);
                var visible_c = Visible(cs_c);

                var visible_any = active_lanes & (visible_a | visible_b | visible_c);

                var ndc_a = ToNdc(cs_a);
                var ndc_b = ToNdc(cs_b);
                var ndc_c = ToNdc(cs_c);

                var ss_a = ToScreenSpace(ndc_a, m_viewport);
                var ss_b = ToScreenSpace(ndc_b, m_viewport);
                var ss_c = ToScreenSpace(ndc_c, m_viewport);

                var clamp_min = new float3_mt(0, 0, m_viewport.MinDepth);
                var clamp_max = new float3_mt(m_viewport.Width, m_viewport.Height, m_viewport.MaxDepth);

                var css_a = ss_a.clamp(clamp_min, clamp_max);
                var css_b = ss_b.clamp(clamp_min, clamp_max);
                var css_c = ss_c.clamp(clamp_min, clamp_max);

                var min = (int2_mt)math_mt.floor(css_a.xy.min(css_b.xy).min(css_c.xy) * (1f / 4f));
                var max = (int2_mt)math_mt.ceil(css_a.xy.max(css_b.xy).max(css_c.xy) * (1f / 4f));

                for (var c = 0; c < 16; c++)
                {
                    if (!visible_any[c]) continue;
                    var task = new PixelTask
                    {
                        Index = index[c],

                        MinX = (uint)min.x[c],
                        MinY = (uint)min.y[c],

                        MaxX = (uint)max.x[c],
                        MaxY = (uint)max.y[c],

                        PosA_CS_X = cs_a.x[c],
                        PosA_CS_Y = cs_a.y[c],
                        PosA_CS_Z = cs_a.z[c],
                        PosA_CS_W = cs_a.w[c],

                        PosB_CS_X = cs_b.x[c],
                        PosB_CS_Y = cs_b.y[c],
                        PosB_CS_Z = cs_b.z[c],
                        PosB_CS_W = cs_b.w[c],

                        PosC_CS_X = cs_c.x[c],
                        PosC_CS_Y = cs_c.y[c],
                        PosC_CS_Z = cs_c.z[c],
                        PosC_CS_W = cs_c.w[c],

                        PosA_SS_X = ss_a.x[c],
                        PosA_SS_Y = ss_a.y[c],
                        PosA_SS_Z = ss_a.z[c],

                        PosB_SS_X = ss_b.x[c],
                        PosB_SS_Y = ss_b.y[c],
                        PosB_SS_Z = ss_b.z[c],

                        PosC_SS_X = ss_c.x[c],
                        PosC_SS_Y = ss_c.y[c],
                        PosC_SS_Z = ss_c.z[c],
                    };

                    DispatchPixel(&Mesh, &task, &Pipeline, PixelShader);
                }
            }
        }

        return;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static b32_mt Visible(float4_mt pos)
        {
            var nw = -pos.w;
            var pw = pos.w;
            return pos.x >= nw & pos.x <= pw & pos.y >= nw & pos.y <= pw & pos.z >= 0 & pos.z <= pw;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float3_mt ToNdc(float4_mt pos) => new(pos.x / pos.w, pos.y / pos.w, pos.z / pos.w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float3_mt ToScreenSpace(float3_mt pos, SoftViewport viewport)
        {
            var x = math_mt.fam(viewport.TopLeftX, (pos.x + 1) * 0.5f, viewport.Width);
            var y = math_mt.fam(viewport.TopLeftX, (1 - pos.y) * 0.5f, viewport.Height);
            var z = viewport.MinDepth + pos.z * (viewport.MaxDepth - viewport.MinDepth);
            return new(x, y, z);
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

        public float _pad;
    }

    [MethodImpl(512)]
    private unsafe void DispatchPixel<TMesh, TPipeline, TInput, TOutput>(
        TMesh* mesh, PixelTask* pt, TPipeline* pipeline, SoftPixelShader<TPipeline, TInput, TOutput> PixelShader
    )
        where TMesh : ISoftMeshData, allows ref struct
        where TPipeline : ISoftGraphicPipelineState, ISoftPixelShader<TMesh, TInput, TOutput>, allows ref struct
        where TInput : allows ref struct
        where TOutput : allows ref struct
    {
        var width = m_rt_color?.Width ?? m_rt_depth_stencil!.Width;
        var height = m_rt_color?.Height ?? m_rt_depth_stencil!.Height;

        m_job_scheduler.Dispatch(
            pt->MaxX - pt->MinX, pt->MaxY - pt->MinX,
            (PixelShader, mesh: (nuint)mesh, p_pt: (nuint)pt, pipeline: (nuint)pipeline, m_rt_color, m_rt_depth_stencil, w: width + 0.5f, h: height + 0.5f),
            [MethodImpl(512)] static (ctx, x, y) =>
            {
                var PixelShader = ctx.PixelShader;
                var mesh = (TMesh*)ctx.mesh;
                var pt = (PixelTask*)ctx.p_pt;
                var pipeline = (TPipeline*)ctx.pipeline;
                ref readonly var state = ref pipeline->State;
                var rt_color = ctx.m_rt_color;
                var rt_depth_stencil = ctx.m_rt_depth_stencil;

                var qq_x = pt->MinX + x;
                var qq_y = pt->MinY + y;
                var start_x = qq_x * 4;
                var start_y = qq_y * 4;

                var s_pos = new float2_mt(start_x, start_y) + SoftGraphicsUtils.ZOrderOffMt;
                var ss_a = new float3_mt(pt->PosA_SS_X, pt->PosA_SS_Y, pt->PosA_SS_Z);
                var ss_b = new float3_mt(pt->PosB_SS_X, pt->PosB_SS_Y, pt->PosB_SS_Z);
                var ss_c = new float3_mt(pt->PosC_SS_X, pt->PosC_SS_Y, pt->PosC_SS_Z);

                var ic = new InterpolateContext();
                var active_lanes = (s_pos < new float2_mt(ctx.w, ctx.h)).all();
                active_lanes &= PointInTriangle(s_pos, ss_a.xy, ss_b.xy, ss_c.xy, ref ic);
                if (!active_lanes.lane_any()) return;

                var zz_index_first = SoftGraphicsUtils.EncodeZOrder(start_x, start_y);
                var zz_index = new uint_mt(zz_index_first) + SoftGraphicsUtils.IncMt;

                var cs_a = new float4_mt(pt->PosA_CS_X, pt->PosA_CS_Y, pt->PosA_CS_Z, pt->PosA_CS_W);
                var cs_b = new float4_mt(pt->PosB_CS_X, pt->PosB_CS_Y, pt->PosB_CS_Z, pt->PosB_CS_W);
                var cs_c = new float4_mt(pt->PosC_CS_X, pt->PosC_CS_Y, pt->PosC_CS_Z, pt->PosC_CS_W);

                ic.Init(cs_a.w, cs_b.w, cs_c.w);

                var pbd = new PixelBasicData(
                    zz_index,
                    active_lanes,
                    cs_a, cs_b, cs_c,
                    ss_a, ss_b, ss_c
                );

                var pixel_ctx = new SoftLaneContext();
                var pixel_input = pipeline->CreateInput(ref *mesh, in ic, in pbd);
                var pixel_output = PixelShader(ref *pipeline, pixel_ctx, pixel_input);

                var r_color = pipeline->GetColor(in pixel_output);

                // todo depth

                if (rt_color != null)
                {
                    if (state.BlendEnable)
                    {
                        Blend(rt_color, in state, zz_index_first, r_color, active_lanes);
                    }
                    else
                    {
                        if (active_lanes.lane_all())
                        {
                            rt_color.QuadQuadStore(zz_index_first, r_color);
                        }
                        else
                        {
                            var c = rt_color.QuadQuadLoad(zz_index_first);
                            var color = math_mt.select(active_lanes, r_color, c);
                            rt_color.QuadQuadStore(zz_index_first, color);
                        }
                    }
                }

                return;

                #region Rasterization

                [MethodImpl(256 | 512)]
                static float_mt Edge(float2_mt a, float2_mt b, float2_mt p)
                {
                    var ab = b - a;
                    var ap = p - a;
                    // ap.x * ab.y - ap.y * ab.x
                    return math_mt.fsm(ap.x * ab.y, ap.y, ab.x);
                }

                [MethodImpl(256 | 512)]
                static b32_mt IsTopLeft(float2_mt a, float2_mt b, float_mt s)
                {
                    var e = (b - a) * s;
                    return (e.y < 0f) | (e.y == 0f & e.x > 0f);
                }

                [MethodImpl(256 | 512)]
                static b32_mt PointInTriangle(
                    float2_mt p,
                    float2_mt a, float2_mt b, float2_mt c,
                    ref InterpolateContext ic,
                    float epsilon = float.Epsilon
                )
                {
                    float_mt ep = epsilon;
                    var area = Edge(a, b, c);
                    if (Vector512.EqualsAny(area.vector, default)) return default;

                    var s = math_mt.select(area > 0f, 1f, -1f);

                    var e0 = Edge(b, c, p) * s;
                    var e1 = Edge(c, a, p) * s;
                    var e2 = Edge(a, b, p) * s;

                    var tl0 = IsTopLeft(b, c, s);
                    var tl1 = IsTopLeft(c, a, s);
                    var tl2 = IsTopLeft(a, b, s);

                    var in0 = (e0 > ep) | (e0.abs() <= ep & tl0);
                    var in1 = (e1 > ep) | (e1.abs() <= ep & tl1);
                    var in2 = (e2 > ep) | (e2.abs() <= ep & tl2);

                    var is_in = in0 & in1 & in2;

                    var inv_area = 1f / area.abs();
                    ic.l0 = (e0 * inv_area) & is_in.asf();
                    ic.l1 = (e1 * inv_area) & is_in.asf();
                    ic.l2 = (e2 * inv_area) & is_in.asf();

                    return is_in;
                }

                #endregion

                #region Blend

                [MethodImpl(256 | 512)]
                static void Blend(
                    SoftTexture rt,
                    in SoftGraphicPipelineState state,
                    uint index,
                    float4_mt src,
                    b32_mt active_lanes
                )
                {
                    var all_active = active_lanes.lane_all();

                    var need_dst_color = !all_active || state.SrcBlend.NeedFetchDst(false, src) || state.DstBlend.NeedFetchDst(true, src);
                    var need_dst_alpha = !all_active || state.SrcAlphaBlend.NeedFetchDst(false, src) || state.DstAlphaBlend.NeedFetchDst(true, src);
                    var dst_color = need_dst_color ? rt.QuadQuadLoadRGB(index) : default;
                    var dst_alpha = need_dst_alpha ? rt.QuadQuadLoad(index, SoftColorChannel.A) : default;
                    var dst = new float4_mt(dst_color, dst_alpha);

                    var rsc = ApplyBlendColor(src.rgb, src, dst, state.SrcBlend);
                    var rdc = ApplyBlendColor(dst.rgb, src, dst, state.DstBlend);
                    var rsa = ApplyBlendAlpha(src.a, src, dst, state.SrcAlphaBlend);
                    var rda = ApplyBlendAlpha(dst.a, src, dst, state.DstAlphaBlend);

                    var rc = ApplyBlendOpColor(rsc, rdc, state.BlendOp);
                    var ra = ApplyBlendOpAlpha(rsa, rda, state.AlphaBlendOp);

                    var r = new float4_mt(rc, ra);

                    if ((state.ColorBlendWriteMask & SoftColorWriteMask.RGBA) == SoftColorWriteMask.RGBA)
                    {
                        r = active_lanes.select(r, dst);
                        rt.QuadQuadStore(index, r);
                    }
                    else if ((state.ColorBlendWriteMask & SoftColorWriteMask.RGB) == SoftColorWriteMask.RGB)
                    {
                        r.rgb = active_lanes.select(r.rgb, dst.rgb);
                        rt.QuadQuadStoreRGB(index, r.rgb);
                    }
                    else if ((state.ColorBlendWriteMask & SoftColorWriteMask.R) != 0)
                    {
                        r.r = active_lanes.select(r.r, dst.r);
                        rt.QuadQuadStore(index, r.r, SoftColorChannel.R);
                    }
                    else if ((state.ColorBlendWriteMask & SoftColorWriteMask.G) != 0)
                    {
                        r.g = active_lanes.select(r.g, dst.g);
                        rt.QuadQuadStore(index, r.g, SoftColorChannel.G);
                    }
                    else if ((state.ColorBlendWriteMask & SoftColorWriteMask.B) != 0)
                    {
                        r.b = active_lanes.select(r.b, dst.b);
                        rt.QuadQuadStore(index, r.b, SoftColorChannel.B);
                    }
                    else if ((state.ColorBlendWriteMask & SoftColorWriteMask.A) != 0)
                    {
                        r.a = active_lanes.select(r.a, dst.a);
                        rt.QuadQuadStore(index, r.a, SoftColorChannel.A);
                    }
                }

                #region ApplyBlend

                [MethodImpl(256 | 512)]
                static float3_mt ApplyBlendColor(float3_mt self, float4_mt src, float4_mt dst, SoftBlend blend) => blend switch
                {
                    SoftBlend.None => self,
                    SoftBlend.Zero => default,
                    SoftBlend.One => self,
                    SoftBlend.SrcColor => self * src.rgb,
                    SoftBlend.InvSrcColor => self * (float3_mt.One - src.rgb),
                    SoftBlend.SrcAlpha => self * src.a,
                    SoftBlend.InvSrcAlpha => self * (1 - src.a),
                    SoftBlend.DstAlpha => self * dst.a,
                    SoftBlend.InvDstAlpha => self * (1 - dst.a),
                    SoftBlend.DstColor => self * dst.rgb,
                    SoftBlend.InvDstColor => self * (float3_mt.One - dst.rgb),
                    _ => throw new ArgumentOutOfRangeException(nameof(blend), blend, null)
                };

                [MethodImpl(256 | 512)]
                static float_mt ApplyBlendAlpha(float_mt self, float4_mt src, float4_mt dst, SoftBlend blend) => blend switch
                {
                    SoftBlend.None => self,
                    SoftBlend.Zero => default,
                    SoftBlend.One => self,
                    SoftBlend.SrcColor => self * src.r,
                    SoftBlend.InvSrcColor => self * (1 - src.r),
                    SoftBlend.SrcAlpha => self * src.a,
                    SoftBlend.InvSrcAlpha => self * (1 - src.a),
                    SoftBlend.DstAlpha => self * dst.a,
                    SoftBlend.InvDstAlpha => self * (1 - dst.a),
                    SoftBlend.DstColor => self * dst.r,
                    SoftBlend.InvDstColor => self * (1 - dst.r),
                    _ => throw new ArgumentOutOfRangeException(nameof(blend), blend, null)
                };

                #endregion

                #region ApplyBlendOp

                [MethodImpl(256 | 512)]
                static float3_mt ApplyBlendOpColor(float3_mt src, float3_mt dst, SoftBlendOp op) => op switch
                {
                    SoftBlendOp.None => src,
                    SoftBlendOp.Add => src + dst,
                    SoftBlendOp.Sub => src - dst,
                    SoftBlendOp.RevSub => dst - src,
                    SoftBlendOp.Min => math_mt.min(src, dst),
                    SoftBlendOp.Max => math_mt.max(src, dst),
                    _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
                };

                [MethodImpl(256 | 512)]
                static float_mt ApplyBlendOpAlpha(float_mt src, float_mt dst, SoftBlendOp op) => op switch
                {
                    SoftBlendOp.None => src,
                    SoftBlendOp.Add => src + dst,
                    SoftBlendOp.Sub => src - dst,
                    SoftBlendOp.RevSub => dst - src,
                    SoftBlendOp.Min => math_mt.min(src, dst),
                    SoftBlendOp.Max => math_mt.max(src, dst),
                    _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
                };

                #endregion

                #endregion
            }
        );
    }

    #endregion
}
