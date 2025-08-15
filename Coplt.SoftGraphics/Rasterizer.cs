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
        active_lanes &= Visible(cs_a) | Visible(cs_b) | Visible(cs_c);
        if (active_lanes.lane_all_false()) return true;

        ScreenSpaceCtx ss_ctx = new(in rc.viewport);
        ss_a = ss_ctx.ToScreenSpace(cs_a);
        ss_b = ss_ctx.ToScreenSpace(cs_b);
        ss_c = ss_ctx.ToScreenSpace(cs_c);

        var a = ss_a.xy;
        var b = ss_b.xy;
        var c = ss_c.xy;

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

        min = a.min(b).min(c);
        max = a.max(b).max(c);

        tla = TopLeft(b, c, sign);
        tlb = TopLeft(c, a, sign);
        tlc = TopLeft(a, b, sign);

        return false;
    }

    #endregion

    #region Visible

    [MethodImpl(256 | 512)]
    private static b32_mt Visible(float4_mt pos_cs)
    {
        var nw = -pos_cs.w;
        var pw = pos_cs.w;
        return pos_cs.x >= nw & pos_cs.x <= pw & pos_cs.y >= nw & pos_cs.y <= pw & pos_cs.z >= 0 & pos_cs.z <= pw;
    }

    #endregion

    #region ScreenSpace

    private readonly struct ScreenSpaceCtx(in SoftViewport viewport)
    {
        private readonly float_mt TopLeftX = viewport.TopLeftX;
        private readonly float_mt TopLeftY = viewport.TopLeftY;
        private readonly float_mt Width = viewport.Width;
        private readonly float_mt Height = viewport.Height;
        private readonly float_mt MinDepth = viewport.MinDepth;
        private readonly float_mt DepthRange = viewport.MaxDepth - viewport.MinDepth;

        [MethodImpl(256 | 512)]
        public float3_mt ToScreenSpace(float4_mt pos_cs)
        {
            var ndc = pos_cs.xyz * (1f / pos_cs.w);
            var x = math_mt.fam(TopLeftX, math_mt.fma(ndc.x, 0.5f, 0.5f), Width);
            var y = math_mt.fam(TopLeftY, math_mt.fma(-ndc.y, 0.5f, 0.5f), Height);
            var z = math_mt.fam(MinDepth, math_mt.fma(ndc.z, 0.5f, 0.5f), DepthRange);
            return new(x, y, z);
        }
    }

    #endregion

    #region TopLeft

    [MethodImpl(256 | 512)]
    private static b32_mt TopLeft(float2_mt a, float2_mt b, float_mt s)
    {
        var e = (b - a).chgsign(s);
        return (e.y < 0f) | (e.y == 0f & e.x > 0f);
    }

    #endregion

    #region AABBOverlap

    [MethodImpl(256 | 512)]
    public b32_mt AABBOverlap(float2_mt box_min, float2_mt box_max) => (max >= box_min).all() & (min <= box_max).all();

    #endregion

    #region Slice

    [MethodImpl(256 | 512)]
    public readonly TriangleSlice At(int lane) => new()
    {
        cs_a = new(
            new(Vector512.Shuffle(cs_a.x.vector, Vector512.Create(lane))),
            new(Vector512.Shuffle(cs_a.y.vector, Vector512.Create(lane))),
            new(Vector512.Shuffle(cs_a.z.vector, Vector512.Create(lane))),
            new(Vector512.Shuffle(cs_a.w.vector, Vector512.Create(lane)))
        ),
        cs_b = new(
            new(Vector512.Shuffle(cs_b.x.vector, Vector512.Create(lane))),
            new(Vector512.Shuffle(cs_b.y.vector, Vector512.Create(lane))),
            new(Vector512.Shuffle(cs_b.z.vector, Vector512.Create(lane))),
            new(Vector512.Shuffle(cs_b.w.vector, Vector512.Create(lane)))
        ),
        cs_c = new(
            new(Vector512.Shuffle(cs_c.x.vector, Vector512.Create(lane))),
            new(Vector512.Shuffle(cs_c.y.vector, Vector512.Create(lane))),
            new(Vector512.Shuffle(cs_c.z.vector, Vector512.Create(lane))),
            new(Vector512.Shuffle(cs_c.w.vector, Vector512.Create(lane)))
        ),

        ss_a = new(
            new(Vector512.Shuffle(ss_a.x.vector, Vector512.Create(lane))),
            new(Vector512.Shuffle(ss_a.y.vector, Vector512.Create(lane))),
            new(Vector512.Shuffle(ss_a.z.vector, Vector512.Create(lane)))
        ),
        ss_b = new(
            new(Vector512.Shuffle(ss_b.x.vector, Vector512.Create(lane))),
            new(Vector512.Shuffle(ss_b.y.vector, Vector512.Create(lane))),
            new(Vector512.Shuffle(ss_b.z.vector, Vector512.Create(lane)))
        ),
        ss_c = new(
            new(Vector512.Shuffle(ss_c.x.vector, Vector512.Create(lane))),
            new(Vector512.Shuffle(ss_c.y.vector, Vector512.Create(lane))),
            new(Vector512.Shuffle(ss_c.z.vector, Vector512.Create(lane)))
        ),

        sign = new(Vector512.Shuffle(sign.vector, Vector512.Create(lane))),
        inv_area = new(Vector512.Shuffle(inv_area.vector, Vector512.Create(lane))),

        tla = new(Vector512.Shuffle(tla.vector, Vector512.Create((uint)lane))),
        tlb = new(Vector512.Shuffle(tlb.vector, Vector512.Create((uint)lane))),
        tlc = new(Vector512.Shuffle(tlc.vector, Vector512.Create((uint)lane))),
    };

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
internal ref struct DispatchTileContext<TMesh, TPipeline, TInput, TOutput>(
    TMesh Mesh,
    TPipeline Pipeline,
    SoftPixelShader<TPipeline, TInput, TOutput> PixelShader,
    SoftTexture? RtColor,
    SoftTexture? RtDepthStencil,
    ReadOnlySpan<TriangleContext> Triangles
)
    where TMesh : ISoftMeshData, allows ref struct
    where TPipeline : ISoftGraphicPipelineState, ISoftPixelShader<TMesh, TInput, TOutput>, allows ref struct
    where TInput : allows ref struct
    where TOutput : allows ref struct
{
    public TMesh Mesh = Mesh;
    public TPipeline Pipeline = Pipeline;
    public SoftPixelShader<TPipeline, TInput, TOutput> PixelShader = PixelShader;
    public SoftTexture? RtColor = RtColor;
    public SoftTexture? RtDepthStencil = RtDepthStencil;
    public uint Width = RtColor?.Width ?? RtDepthStencil!.Width;
    public uint Height = RtColor?.Height ?? RtDepthStencil!.Height;
    public ReadOnlySpan<TriangleContext> Triangles = Triangles;
}

internal abstract unsafe class Rasterizer<TMesh, TPipeline, TInput, TOutput> : Rasterizer
    where TMesh : ISoftMeshData, allows ref struct
    where TPipeline : ISoftGraphicPipelineState, ISoftPixelShader<TMesh, TInput, TOutput>, allows ref struct
    where TInput : allows ref struct
    where TOutput : allows ref struct
{
    [MethodImpl(256 | 512)]
    public static void DispatchTile(
        AJobScheduler scheduler,
        RasterizerContext* rc,
        DispatchTileContext<TMesh, TPipeline, TInput, TOutput>* dtc
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
        var dtc = (DispatchTileContext<TMesh, TPipeline, TInput, TOutput>*)ctx.dtc;
        TileTask(in *rc, ref *dtc, x, y);
    }

    [MethodImpl(256 | 512)]
    private static void TileTask(
        in RasterizerContext rc,
        ref DispatchTileContext<TMesh, TPipeline, TInput, TOutput> dtc,
        uint x, uint y
    )
    {
        var pixel_x = x * TileSize;
        var pixel_y = y * TileSize;
        var end_x = pixel_x + TileSize + 1;
        var end_y = pixel_y + TileSize + 1;
        var tile_zi = SoftGraphicsUtils.EncodeZOrder(pixel_x, pixel_y);

        var box_min = new float2_mt(pixel_x, pixel_y);
        var box_max = new float2_mt(end_x, end_y);

        var size = new float2_mt(dtc.Width, dtc.Height);

        foreach (ref readonly var triangle in dtc.Triangles)
        {
            var active_lanes = triangle.active_lanes;
            active_lanes &= triangle.AABBOverlap(box_min, box_max);
            if (active_lanes.lane_all_false()) continue;

            for (var c = 0; c < 16; c++)
            {
                if (!active_lanes[c]) continue;
                var ts = triangle.At(c);
                for (var zi = 0u; zi < TileSize * TileSize; zi += 16)
                {
                    PixelTask(
                        in rc, ref dtc,
                        pixel_x, pixel_y, tile_zi, zi,
                        c, in triangle, in ts,
                        size
                    );
                }
            }
        }
    }

    [MethodImpl(256 | 512)]
    private static void PixelTask(
        in RasterizerContext rc,
        ref DispatchTileContext<TMesh, TPipeline, TInput, TOutput> dtc,
        uint tx, uint ty, uint tzi, uint lzi,
        int c, in TriangleContext tc, in TriangleSlice ts,
        float2_mt size
    )
    {
        var (ox, oy) = SoftGraphicsUtils.DecodeZOrder(lzi);
        var zis = tzi + lzi;
        var zi = zis + SoftGraphicsUtils.IncMt;
        var pos = new float2_mt(tx + ox, ty + oy) + SoftGraphicsUtils.ZOrderOffMt;
        InterpolateContext ic = new();

        var active_lanes = (pos < size).all();
        if (active_lanes.lane_all_false()) return;

        active_lanes &= ts.PointInTriangle(ref ic, pos);
        if (active_lanes.lane_all_false()) return;

        ic.Init(ts.cs_a.w, ts.cs_b.w, ts.cs_c.w);

        var pbd = new PixelBasicData(
            zi,
            active_lanes,
            ts.cs_a, ts.cs_b, ts.cs_c,
            ts.ss_a, ts.ss_b, ts.ss_c
        );

        var pixel_ctx = new SoftLaneContext();
        var pixel_input = dtc.Pipeline.CreateInput(ref dtc.Mesh, in ic, in pbd);
        var pixel_output = dtc.PixelShader(ref dtc.Pipeline, pixel_ctx, pixel_input);

        var r_color = dtc.Pipeline.GetColor(in pixel_output);
        
        // todo depth

        if (dtc.RtColor != null)
        {
            if (rc.state.BlendEnable)
            {
                Blend(dtc.RtColor, in rc.state, zis, r_color, active_lanes);
            }
            else
            {
                if (active_lanes.lane_all())
                {
                    dtc.RtColor.QuadQuadStore(zis, r_color);
                }
                else
                {
                    var dst = dtc.RtColor.QuadQuadLoad(zis);
                    var color = math_mt.select(active_lanes, r_color, dst);
                    dtc.RtColor.QuadQuadStore(zis, color);
                }
            }
        }
    }
}

internal abstract class Rasterizer
{
    #region Const

    public const uint TileSize = 16;

    #endregion

    #region Edge

    [MethodImpl(256 | 512)]
    public static float_mt Edge(float2_mt a, float2_mt b, float2_mt p)
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
}
