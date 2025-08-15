using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using Coplt.Mathematics;
using Coplt.Mathematics.Simt;

namespace Coplt.SoftGraphics;

[StructLayout(LayoutKind.Auto)]
internal ref struct RasterizerContext
{
    public required ref readonly SoftViewport viewport;
    public required ref readonly SoftGraphicPipelineState state;
}

[StructLayout(LayoutKind.Auto)]
internal struct TriangleContext(uint cluster, uint_mt index, b32_mt active_lanes, float4_mt cs_a, float4_mt cs_b, float4_mt cs_c)
{
    #region Fields

    public uint cluster = cluster;
    public uint_mt index = index;
    public b32_mt active_lanes = active_lanes;

    public float4_mt cs_a = cs_a;
    public float4_mt cs_b = cs_b;
    public float4_mt cs_c = cs_c;

    public float3_mt ss_a;
    public float3_mt ss_b;
    public float3_mt ss_c;

    public float2_mt min;
    public float2_mt max;

    public float_mt sign;
    public float_mt inv_area;

    public b32_mt tla;
    public b32_mt tlb;
    public b32_mt tlc;

    #endregion

    #region Setup

    /// <returns>All skipped</returns>
    [MethodImpl(512)]
    public bool Setup(in RasterizerContext rc)
    {
        if (CheckVisible()) return true;
        CalcScreenSpace(in rc);
        if (CalcArea(in rc)) return true;
        CalcAABB();
        CalcTopLeft();
        return false;
    }

    #endregion

    #region Visible

    [MethodImpl(512)]
    private bool CheckVisible()
    {
        active_lanes &= Visible(cs_a) | Visible(cs_b) | Visible(cs_c);
        return active_lanes.lane_all_false();
    }

    [MethodImpl(256 | 512)]
    private static b32_mt Visible(in float4_mt pos_cs)
    {
        var nw = -pos_cs.w;
        var pw = pos_cs.w;
        return pos_cs.x >= nw & pos_cs.x <= pw & pos_cs.y >= nw & pos_cs.y <= pw & pos_cs.z >= 0 & pos_cs.z <= pw;
    }

    #endregion

    #region ScreenSpace

    [MethodImpl(512)]
    private void CalcScreenSpace(in RasterizerContext rc)
    {
        ToScreenSpace(out ss_a, in rc.viewport, in cs_a);
        ToScreenSpace(out ss_b, in rc.viewport, in cs_b);
        ToScreenSpace(out ss_c, in rc.viewport, in cs_c);
    }

    [MethodImpl(256 | 512)]
    private static void ToScreenSpace(out float3_mt r, in SoftViewport viewport, in float4_mt pos_cs)
    {
        var ndc = pos_cs.xyz * (1f / pos_cs.w);
        var x = math_mt.fam(viewport.TopLeftX, math_mt.fma(ndc.x, 0.5f, 0.5f), viewport.Width);
        var y = math_mt.fam(viewport.TopLeftY, math_mt.fma(-ndc.y, 0.5f, 0.5f), viewport.Height);
        var z = math_mt.fam(viewport.MinDepth, math_mt.fma(ndc.z, 0.5f, 0.5f), viewport.MaxDepth - viewport.MinDepth);
        r = new(x, y, z);
    }

    #endregion

    #region CheckArea

    [MethodImpl(512)]
    public bool CalcArea(in RasterizerContext rc)
    {
        ref var a = ref Unsafe.As<float3_mt, float2_mt>(ref ss_a);
        ref var b = ref Unsafe.As<float3_mt, float2_mt>(ref ss_b);
        ref var c = ref Unsafe.As<float3_mt, float2_mt>(ref ss_c);

        var area = Rasterizer.Edge(a, b, c);

        active_lanes = area.isFinite() & (area.abs() > float.Epsilon);
        active_lanes &= rc.state.CullMode switch
        {
            SoftCullMode.Back => active_lanes & (rc.state.FrontCounterClockwise ? area > 0 : area < 0),
            SoftCullMode.Front => active_lanes & (rc.state.FrontCounterClockwise ? area < 0 : area > 0),
            _ => active_lanes
        };
        if (active_lanes.lane_all_false()) return true;

        inv_area = 1 / area.abs();
        sign = math_mt.select(area > 0f, 1f, -1f);

        return false;
    }

    #endregion

    #region CalcAABB

    [MethodImpl(512)]
    private void CalcAABB()
    {
        ref var a = ref Unsafe.As<float3_mt, float2_mt>(ref ss_a);
        ref var b = ref Unsafe.As<float3_mt, float2_mt>(ref ss_b);
        ref var c = ref Unsafe.As<float3_mt, float2_mt>(ref ss_c);

        min = a.min(b).min(c);
        max = a.max(b).max(c);
    }

    #endregion

    #region TopLeft

    [MethodImpl(512)]
    private void CalcTopLeft()
    {
        ref var a = ref Unsafe.As<float3_mt, float2_mt>(ref ss_a);
        ref var b = ref Unsafe.As<float3_mt, float2_mt>(ref ss_b);
        ref var c = ref Unsafe.As<float3_mt, float2_mt>(ref ss_c);

        TopLeft(out tla, b, c, sign);
        TopLeft(out tlb, c, a, sign);
        TopLeft(out tlc, a, b, sign);
    }

    [MethodImpl(256 | 512)]
    private static void TopLeft(out b32_mt r, in float2_mt a, in float2_mt b, in float_mt s)
    {
        var e = (b - a).chgsign(s);
        r = (e.y < 0f) | (e.y == 0f & e.x > 0f);
    }

    #endregion

    #region AABBOverlap

    [MethodImpl(256 | 512)]
    public readonly b32_mt AABBOverlap(in float2_mt box_min, in float2_mt box_max) => (max >= box_min).all() & (min <= box_max).all();

    #endregion

    #region Slice

    [MethodImpl(256 | 512)]
    public readonly void LoadSlice(ref TriangleSlice r_, int lane_)
    {
        ref var r = ref r_;
        var lane = lane_;

        r.cs_a.x = cs_a.x[lane];
        r.cs_a.y = cs_a.y[lane];
        r.cs_a.z = cs_a.z[lane];
        r.cs_a.w = cs_a.w[lane];

        r.cs_b.x = cs_b.x[lane];
        r.cs_b.y = cs_b.y[lane];
        r.cs_b.z = cs_b.z[lane];
        r.cs_b.w = cs_b.w[lane];

        r.cs_c.x = cs_c.x[lane];
        r.cs_c.y = cs_c.y[lane];
        r.cs_c.z = cs_c.z[lane];
        r.cs_c.w = cs_c.w[lane];

        r.ss_a.x = ss_a.x[lane];
        r.ss_a.y = ss_a.y[lane];
        r.ss_a.z = ss_a.z[lane];

        r.ss_b.x = ss_b.x[lane];
        r.ss_b.y = ss_b.y[lane];
        r.ss_b.z = ss_b.z[lane];

        r.ss_c.x = ss_c.x[lane];
        r.ss_c.y = ss_c.y[lane];
        r.ss_c.z = ss_c.z[lane];

        r.sign = sign[lane];
        r.inv_area = inv_area[lane];

        r.tla = tla[lane];
        r.tlb = tlb[lane];
        r.tlc = tlc[lane];
    }

    #endregion
}

internal struct TriangleSlice
{
    #region Fields

    public float4_mt cs_a;
    public float4_mt cs_b;
    public float4_mt cs_c;

    public float3_mt ss_a;
    public float3_mt ss_b;
    public float3_mt ss_c;

    public float_mt sign;
    public float_mt inv_area;

    public b32_mt tla;
    public b32_mt tlb;
    public b32_mt tlc;

    #endregion

    #region PointInTriangle

    [MethodImpl(256 | 512)]
    public readonly b32_mt PointInTriangle(
        ref InterpolateContext ic,
        float2_mt p
    )
    {
        var ea = Rasterizer.Edge(ss_b.xy, ss_c.xy, p).chgsign(sign);
        var eb = Rasterizer.Edge(ss_c.xy, ss_a.xy, p).chgsign(sign);
        var ec = Rasterizer.Edge(ss_a.xy, ss_b.xy, p).chgsign(sign);

        ic.l0 = ea * inv_area;
        ic.l1 = eb * inv_area;
        ic.l2 = ec * inv_area;

        var ina = (ea > 0) | (ea.abs() <= 0 & tla);
        var inb = (eb > 0) | (eb.abs() <= 0 & tlb);
        var inc = (ec > 0) | (ec.abs() <= 0 & tlc);

        return ina & inb & inc;
    }

    #endregion
}

[StructLayout(LayoutKind.Auto)]
internal ref struct DispatchTileContext<TMesh, TPipeline>(
    TMesh Mesh,
    TPipeline Pipeline,
    SoftTexture? RtColor,
    SoftTexture? RtDepthStencil,
    ReadOnlySpan<TriangleContext> Triangles
)
    where TMesh : ISoftMeshData, allows ref struct
    where TPipeline : ISoftGraphicPipelineState, ISoftPixelShader<TMesh>, allows ref struct
{
    public TMesh Mesh = Mesh;
    public TPipeline Pipeline = Pipeline;
    public SoftTexture? RtColor = RtColor;
    public SoftTexture? RtDepthStencil = RtDepthStencil;
    public uint Width = RtColor?.Width ?? RtDepthStencil!.Width;
    public uint Height = RtColor?.Height ?? RtDepthStencil!.Height;
    public ReadOnlySpan<TriangleContext> Triangles = Triangles;
}

internal abstract unsafe class Rasterizer<TMesh, TPipeline> : Rasterizer
    where TMesh : ISoftMeshData, allows ref struct
    where TPipeline : ISoftGraphicPipelineState, ISoftPixelShader<TMesh>, allows ref struct
{
    #region TileTask

    [MethodImpl(256 | 512)]
    public static void DispatchTile(
        AJobScheduler scheduler,
        RasterizerContext* rc,
        DispatchTileContext<TMesh, TPipeline>* dtc
    )
    {
        var w = (dtc->Width + (TileSize - 1)) / TileSize;
        var h = (dtc->Height + (TileSize - 1)) / TileSize;
        scheduler.Dispatch(w, h, (rc: (nuint)rc, dtc: (nuint)dtc), TileTask);
    }

    [MethodImpl(512)]
    private static void TileTask((nuint rc, nuint dtc) ctx, uint x, uint y)
    {
        var rc = (RasterizerContext*)ctx.rc;
        var dtc = (DispatchTileContext<TMesh, TPipeline>*)ctx.dtc;
        TileTask(in *rc, ref *dtc, x, y);
    }


    [MethodImpl(512)]
    private static void TileTask(
        in RasterizerContext rc,
        ref DispatchTileContext<TMesh, TPipeline> dtc,
        uint x, uint y
    )
    {
        Unsafe.SkipInit(out TileTaskContext tc);
        tc.Init(x, y, dtc.Width, dtc.Height);

        foreach (ref readonly var triangle in dtc.Triangles)
        {
            if (TileGenActiveLanes(out var active_lanes, in tc, in triangle)) continue;

            for (var c = 0; c < 16; c++)
            {
                if (!CheckLane(active_lanes, c)) continue;
                TriangleSlice ts;
                triangle.LoadSlice(ref *&ts, c);
                for (var zi = 0u; zi < TileSize * TileSize; zi += 16)
                {
                    PixelTask(
                        in rc, ref dtc,
                        tc.pixel_x, tc.pixel_y, tc.tile_zi, zi,
                        in triangle, in ts,
                        tc.size
                    );
                }
            }
        }
    }

    #endregion

    #region PixelTask

    [MethodImpl(512)]
    private static void PixelTask(
        in RasterizerContext rc,
        ref DispatchTileContext<TMesh, TPipeline> dtc,
        uint tx, uint ty, uint tzi, uint lzi,
        in TriangleContext tc, in TriangleSlice ts,
        in float2_mt size
    )
    {
        Unsafe.SkipInit(out PixelTaskContext ptc);
        ptc.Init(tx, ty, tzi, lzi);
        if (ptc.InitActiveLanes(in ts, in size)) return;
        ptc.ic.Init(in ts);

        Unsafe.SkipInit(out PixelBasicData pbd);
        pbd.ptc = ref ptc;
        pbd.ts = ref ts;

        Unsafe.SkipInit(out float4_mt output_color);
        Unsafe.SkipInit(out float_mt output_depth);
        Unsafe.SkipInit(out uint_mt output_stencil);
        InvokePixelShader(
            ref output_color, ref output_depth, ref output_stencil,
            ref dtc, in ptc, in pbd
        );

        // todo depth

        if (dtc.RtColor != null & rc.state.PixelWriteColor) // & is faster then &&
        {
            WriteColor(in rc, in ptc, dtc.RtColor!, in output_color);
        }
    }

    #endregion

    #region InvokePixelShader

    [MethodImpl(256 | 512)]
    private static void InvokePixelShader(
        ref float4_mt output_color, ref float_mt output_depth, ref uint_mt output_stencil,
        ref DispatchTileContext<TMesh, TPipeline> dtc,
        in PixelTaskContext ptc, in PixelBasicData pbd
    )
    {
        var lc = new SoftLaneContext(); // todo
        dtc.Pipeline.Invoke(
            ref dtc.Mesh, in ptc.ic, in lc, in pbd,
            ref output_color, ref output_depth, ref output_stencil
        );
    }

    #endregion
}

internal abstract class Rasterizer
{
    #region Const

    public const uint TileSize = 16;

    #endregion

    #region Utils

    [MethodImpl(256 | 512)]
    public static bool CheckLane(in b32_mt active_lanes, int lane) => active_lanes.vector[lane] != 0;

    #endregion

    #region TileTaskContext

    [StructLayout(LayoutKind.Auto)]
    public struct TileTaskContext
    {
        public uint pixel_x;
        public uint pixel_y;
        public uint tile_zi;

        public float2_mt box_min;
        public float2_mt box_max;

        public float2_mt size;

        [MethodImpl(512)]
        public void Init(uint x, uint y, uint width, uint height)
        {
            pixel_x = x * TileSize;
            pixel_y = y * TileSize;
            var end_x = pixel_x + TileSize + 1;
            var end_y = pixel_y + TileSize + 1;
            tile_zi = SoftGraphicsUtils.EncodeZOrder(pixel_x, pixel_y);

            box_min = new float2_mt(pixel_x, pixel_y);
            box_max = new float2_mt(end_x, end_y);

            size = new(width, height);
        }
    }

    [MethodImpl(512)]
    public static bool TileGenActiveLanes(out b32_mt active_lanes, in TileTaskContext tc, in TriangleContext triangle)
    {
        var active_lanes_ = triangle.active_lanes;
        active_lanes_ &= triangle.AABBOverlap(tc.box_min, tc.box_max);
        active_lanes = active_lanes_;
        return active_lanes_.lane_all_false();
    }

    #endregion

    #region PixelTaskContext

    [StructLayout(LayoutKind.Auto)]
    public struct PixelTaskContext
    {
        public uint zis;
        public uint_mt zi;
        public float2_mt pos;
        public InterpolateContext ic;

        public b32_mt active_lanes;

        [MethodImpl(512)]
        public void Init(uint tx, uint ty, uint tzi, uint lzi)
        {
            var (ox, oy) = SoftGraphicsUtils.DecodeZOrder(lzi);
            zis = tzi + lzi;
            zi = zis + SoftGraphicsUtils.IncMt;
            pos = new float2_mt(tx + ox, ty + oy) + SoftGraphicsUtils.ZOrderOffMt;
        }

        [MethodImpl(512)]
        public bool InitActiveLanes(in TriangleSlice ts, in float2_mt size)
        {
            active_lanes = (pos < size).all();
            if (active_lanes.lane_all_false()) return true;

            active_lanes &= ts.PointInTriangle(ref ic, pos);
            if (active_lanes.lane_all_false()) return true;

            return false;
        }
    }

    #endregion

    #region Edge

    [MethodImpl(256 | 512)]
    public static float_mt Edge(in float2_mt a, in float2_mt b, in float2_mt p)
    {
        var ab = b - a;
        var ap = p - a;
        // ap.x * ab.y - ap.y * ab.x
        return math_mt.fsm(ap.x * ab.y, ap.y, ab.x);
    }

    #endregion

    #region Blend

    [MethodImpl(256 | 512)]
    public static void Blend(
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
    public static float3_mt ApplyBlendColor(float3_mt self, float4_mt src, float4_mt dst, SoftBlend blend) => blend switch
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
    public static float_mt ApplyBlendAlpha(float_mt self, float4_mt src, float4_mt dst, SoftBlend blend) => blend switch
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
    public static float3_mt ApplyBlendOpColor(float3_mt src, float3_mt dst, SoftBlendOp op) => op switch
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
    public static float_mt ApplyBlendOpAlpha(float_mt src, float_mt dst, SoftBlendOp op) => op switch
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

    #region WriteColor

    [MethodImpl(256 | 512)]
    public static void WriteColor(
        in RasterizerContext rc,
        in PixelTaskContext ptc,
        SoftTexture rt_color,
        in float4_mt output_color
    )
    {
        if (rc.state.BlendEnable)
        {
            Blend(rt_color, in rc.state, ptc.zis, output_color, ptc.active_lanes);
        }
        else
        {
            if (ptc.active_lanes.lane_all())
            {
                rt_color.QuadQuadStore(ptc.zis, output_color);
            }
            else
            {
                var dst = rt_color.QuadQuadLoad(ptc.zis);
                var color = math_mt.select(ptc.active_lanes, output_color, dst);
                rt_color.QuadQuadStore(ptc.zis, color);
            }
        }
    }

    #endregion
}
