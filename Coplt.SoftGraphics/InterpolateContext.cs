using System.Runtime.CompilerServices;
using Coplt.Mathematics.Simt;

namespace Coplt.SoftGraphics;

public record struct InterpolateContext
{
    #region Fields

    internal float_mt l0;
    internal float_mt l1;
    internal float_mt l2;

    internal float_mt r0;
    internal float_mt r1;
    internal float_mt r2;

    internal float_mt i_den;

    #endregion

    #region Init

    [MethodImpl(256 | 512)]
    internal void Init(in TriangleSlice ts)
    {
        r0 = ts.cs_a.w.rcp();
        r1 = ts.cs_b.w.rcp();
        r2 = ts.cs_c.w.rcp();

        // l0 * r0 + l1 * r1 + l2 * r2
        i_den = 1 / math_mt.fam(math_mt.fam(l0 * r0, l1, r1), l2, r2);
    }

    #endregion

    #region InterpolateClipSpace

    [MethodImpl(256 | 512)]
    public readonly float4_mt InterpolateClipSpace(float4_mt f0, float4_mt f1, float4_mt f2)
    {
        return new(
            ScreenSpaceInterpolate(f0.xy, f1.xy, f2.xy),
            ScreenSpaceInterpolate(f0.zw, f1.zw, f2.zw)
        );
    }

    #endregion

    #region PerspectiveInterpolate

    [MethodImpl(256 | 512)]
    public readonly float_mt PerspectiveInterpolate(float_mt f0, float_mt f1, float_mt f2)
    {
        // l0 * r0 * f0 + l1 * r1 * f1 + l2 * r2 * f2;
        var num = math_mt.fam(math_mt.fam(l0 * r0 * f0, l1 * r1, f1), l2 * r2, f2);
        return num * i_den;
    }

    [MethodImpl(256 | 512)]
    public readonly float2_mt PerspectiveInterpolate(float2_mt f0, float2_mt f1, float2_mt f2)
    {
        // l0 * r0 * f0 + l1 * r1 * f1 + l2 * r2 * f2;
        var num = math_mt.fam(math_mt.fam(l0 * r0 * f0, l1 * r1, f1), l2 * r2, f2);
        return num * i_den;
    }

    [MethodImpl(256 | 512)]
    public readonly float3_mt PerspectiveInterpolate(float3_mt f0, float3_mt f1, float3_mt f2)
    {
        // l0 * r0 * f0 + l1 * r1 * f1 + l2 * r2 * f2;
        var num = math_mt.fam(math_mt.fam(l0 * r0 * f0, l1 * r1, f1), l2 * r2, f2);
        return num * i_den;
    }

    [MethodImpl(256 | 512)]
    public readonly float4_mt PerspectiveInterpolate(float4_mt f0, float4_mt f1, float4_mt f2)
    {
        // l0 * r0 * f0 + l1 * r1 * f1 + l2 * r2 * f2;
        var num = math_mt.fam(math_mt.fam(l0 * r0 * f0, l1 * r1, f1), l2 * r2, f2);
        return num * i_den;
    }

    #endregion

    #region ScreenSpaceInterpolate

    [MethodImpl(256 | 512)]
    public readonly float_mt ScreenSpaceInterpolate(float_mt f0, float_mt f1, float_mt f2)
    {
        // f0 * l0 + f1 * l1 + f2 * l2
        return math_mt.fam(math_mt.fam(f0 * l0, f1, l1), f2, l2);
    }

    [MethodImpl(256 | 512)]
    public readonly float2_mt ScreenSpaceInterpolate(float2_mt f0, float2_mt f1, float2_mt f2)
    {
        // f0 * l0 + f1 * l1 + f2 * l2
        return math_mt.fam(math_mt.fam(f0 * l0, f1, l1), f2, l2);
    }

    [MethodImpl(256 | 512)]
    public readonly float3_mt ScreenSpaceInterpolate(float3_mt f0, float3_mt f1, float3_mt f2)
    {
        // f0 * l0 + f1 * l1 + f2 * l2
        return math_mt.fam(math_mt.fam(f0 * l0, f1, l1), f2, l2);
    }

    [MethodImpl(256 | 512)]
    public readonly float4_mt ScreenSpaceInterpolate(float4_mt f0, float4_mt f1, float4_mt f2)
    {
        // f0 * l0 + f1 * l1 + f2 * l2
        return math_mt.fam(math_mt.fam(f0 * l0, f1, l1), f2, l2);
    }

    #endregion
}

public ref struct PixelBasicData
{
    internal ref readonly Rasterizer.PixelTaskContext ptc;
    internal ref readonly TriangleSlice ts;

    public ref readonly uint_mt Index => ref ptc.zi;
    public ref readonly b32_mt ActiveLanes => ref ptc.active_lanes;

    public ref readonly float4_mt cs_a => ref ts.cs_a;
    public ref readonly float4_mt cs_b => ref ts.cs_b;
    public ref readonly float4_mt cs_c => ref ts.cs_c;

    public ref readonly float3_mt ss_a => ref ts.ss_a;
    public ref readonly float3_mt ss_b => ref ts.ss_b;
    public ref readonly float3_mt ss_c => ref ts.ss_c;
}
