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

    internal float_mt den;

    #endregion

    #region Init

    [MethodImpl(256 | 512)]
    internal void Init(float_mt w0, float_mt w1, float_mt w2)
    {
        r0 = w0.rcp();
        r1 = w1.rcp();
        r2 = w2.rcp();

        // l0 * r0 + l1 * r1 + l2 * r2
        den = math_mt.fam(math_mt.fam(l0 * r0, l1, r1), l2, r2);
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
        return num / den;
    }

    [MethodImpl(256 | 512)]
    public readonly float2_mt PerspectiveInterpolate(float2_mt f0, float2_mt f1, float2_mt f2)
    {
        // l0 * r0 * f0 + l1 * r1 * f1 + l2 * r2 * f2;
        var num = math_mt.fam(math_mt.fam(l0 * r0 * f0, l1 * r1, f1), l2 * r2, f2);
        return num / den;
    }

    [MethodImpl(256 | 512)]
    public readonly float3_mt PerspectiveInterpolate(float3_mt f0, float3_mt f1, float3_mt f2)
    {
        // l0 * r0 * f0 + l1 * r1 * f1 + l2 * r2 * f2;
        var num = math_mt.fam(math_mt.fam(l0 * r0 * f0, l1 * r1, f1), l2 * r2, f2);
        return num / den;
    }

    [MethodImpl(256 | 512)]
    public readonly float4_mt PerspectiveInterpolate(float4_mt f0, float4_mt f1, float4_mt f2)
    {
        // l0 * r0 * f0 + l1 * r1 * f1 + l2 * r2 * f2;
        var num = math_mt.fam(math_mt.fam(l0 * r0 * f0, l1 * r1, f1), l2 * r2, f2);
        return num / den;
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

public record struct PixelBasicData(
    uint_mt Index,
    b32_mt ActiveLanes,
    float4_mt cs_a,
    float4_mt cs_b,
    float4_mt cs_c,
    float3_mt ss_a,
    float3_mt ss_b,
    float3_mt ss_c)
{
    public uint_mt Index = Index;
    public b32_mt ActiveLanes = ActiveLanes;

    public float4_mt cs_a = cs_a;
    public float4_mt cs_b = cs_b;
    public float4_mt cs_c = cs_c;

    public float3_mt ss_a = ss_a;
    public float3_mt ss_b = ss_b;
    public float3_mt ss_c = ss_c;
}
