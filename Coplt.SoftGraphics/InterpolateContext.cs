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

    #region PerspectiveInterpolate

    [MethodImpl(256 | 512)]
    public float_mt PerspectiveInterpolate(float_mt f0, float_mt f1, float_mt f2)
    {
        // l0 * r0 * f0 + l1 * r1 * f1 + l2 * r2 * f2;
        var num = math_mt.fam(math_mt.fam(l0 * r0 * f0, l1 * r1, f1), l2 * r2, f2);
        return num / den;
    }

    [MethodImpl(256 | 512)]
    public float2_mt PerspectiveInterpolate(float2_mt f0, float2_mt f1, float2_mt f2)
    {
        // l0 * r0 * f0 + l1 * r1 * f1 + l2 * r2 * f2;
        var num = math_mt.fam(math_mt.fam(l0 * r0 * f0, l1 * r1, f1), l2 * r2, f2);
        return num / den;
    }

    [MethodImpl(256 | 512)]
    public float3_mt PerspectiveInterpolate(float3_mt f0, float3_mt f1, float3_mt f2)
    {
        // l0 * r0 * f0 + l1 * r1 * f1 + l2 * r2 * f2;
        var num = math_mt.fam(math_mt.fam(l0 * r0 * f0, l1 * r1, f1), l2 * r2, f2);
        return num / den;
    }

    [MethodImpl(256 | 512)]
    public float4_mt PerspectiveInterpolate(float4_mt f0, float4_mt f1, float4_mt f2)
    {
        // l0 * r0 * f0 + l1 * r1 * f1 + l2 * r2 * f2;
        var num = math_mt.fam(math_mt.fam(l0 * r0 * f0, l1 * r1, f1), l2 * r2, f2);
        return num / den;
    }

    #endregion

    #region ScreenSpaceInterpolate

    [MethodImpl(256 | 512)]
    public float_mt ScreenSpaceInterpolate(float_mt f0, float_mt f1, float_mt f2)
    {
        // f0 * l0 + f1 * l1 + f2 * l2
        return math_mt.fam(math_mt.fam(f0 * l0, f1, l1), f2, l2);
    }

    [MethodImpl(256 | 512)]
    public float2_mt ScreenSpaceInterpolate(float2_mt f0, float2_mt f1, float2_mt f2)
    {
        // f0 * l0 + f1 * l1 + f2 * l2
        return math_mt.fam(math_mt.fam(f0 * l0, f1, l1), f2, l2);
    }

    [MethodImpl(256 | 512)]
    public float3_mt ScreenSpaceInterpolate(float3_mt f0, float3_mt f1, float3_mt f2)
    {
        // f0 * l0 + f1 * l1 + f2 * l2
        return math_mt.fam(math_mt.fam(f0 * l0, f1, l1), f2, l2);
    }

    [MethodImpl(256 | 512)]
    public float4_mt ScreenSpaceInterpolate(float4_mt f0, float4_mt f1, float4_mt f2)
    {
        // f0 * l0 + f1 * l1 + f2 * l2
        return math_mt.fam(math_mt.fam(f0 * l0, f1, l1), f2, l2);
    }

    #endregion
}
