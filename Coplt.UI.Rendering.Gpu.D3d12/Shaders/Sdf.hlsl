#pragma once
#ifndef __SDF_HLSL__
#define __SDF_HLSL__

const static float2 hammersley_8[8] = {
    float2(-0.5, -0.5),
    float2(-0.375, 0),
    float2(-0.25, -0.25),
    float2(-0.125, 0.25),
    float2(0, -0.375),
    float2(0.125, 0.125),
    float2(0.25, -0.125),
    float2(0.375, 0.375),
};

struct AA_t
{
    float2 size;
    float2 inv_size;

    float2 Apply(float2 pos)
    {
        return (2 * pos - size) * scale();
    }

    float scale()
    {
        return inv_size.y;
    }
};

AA_t BuildAA(float2 size)
{
    AA_t r;
    r.size = size;
    r.inv_size = 1 / size;
    return r;
}

float SdAA2(float d, float size_y)
{
    const float sqrt_e = 1.64872127070013;
    float aa = sqrt_e / size_y;
    return smoothstep(aa, -aa, d);
}

float SdInfLine(float2 p, float2 position, float2 normal)
{
    return dot(p - position, normal);
}

float SdInfLineV(float2 p, float2 position, float2 normal)
{
    return dot(p - position, float2(-normal.y, normal.x));
}

enum class SdRoundBoxType : uint
{
    Circle,
    Parabola,
    Cosine,
    Cubic,
};

float SdCornerCircle(float2 p)
{
    return length(p - float2(0.0, -1.0)) - sqrt(2.0);
}

float SdCornerParabola(float2 p)
{
    // https://www.shadertoy.com/view/ws3GD7
    float y = (0.5 + p.y) * (2.0 / 3.0);
    float h = p.x * p.x + y * y * y;
    float w = pow(p.x + sqrt(abs(h)), 1.0 / 3.0);
    // note I allow a tiny error in the very interior of the shape so that I don't have to branch into the 3 root solution
    float x = w - y / w;
    float2 q = float2(x, 0.5 * (1.0 - x * x));
    return length(p - q) * sign(p.y - q.y);
}

float SdCornerCosine(float2 uv)
{
    const float kT = 6.28318531;
    // https://www.shadertoy.com/view/3t23WG
    uv *= (kT / 4.0);

    float ta = 0.0, tb = kT / 4.0;
    for (int i = 0; i < 8; i++)
    {
        float t = 0.5 * (ta + tb);
        float y = t - uv.x + (uv.y - cos(t)) * sin(t);
        if (y < 0.0) ta = t;
        else tb = t;
    }
    float2 qa = float2(ta, cos(ta)), qb = float2(tb, cos(tb));
    float2 pa = uv - qa, di = qb - qa;
    float h = clamp(dot(pa, di) / dot(di, di), 0.0, 1.0);
    return length(pa - di * h) * sign(pa.y * di.x - pa.x * di.y) * (4.0 / kT);
}

float SdCornerCubic(float2 uv)
{
    float ta = 0.0, tb = 1.0;
    for (int i = 0; i < 12; i++)
    {
        float t = 0.5 * (ta + tb);
        float c = (t * t * (t - 3.0) + 2.0) / 3.0;
        float dc = t * (t - 2.0);
        float y = (uv.x - t) + (uv.y - c) * dc;
        if (y > 0.0) ta = t;
        else tb = t;
    }
    float2 qa = float2(ta, (ta * ta * (ta - 3.0) + 2.0) / 3.0);
    float2 qb = float2(tb, (tb * tb * (tb - 3.0) + 2.0) / 3.0);
    float2 pa = uv - qa, di = qb - qa;
    float h = clamp(dot(pa, di) / dot(di, di), 0.0, 1.0);
    return length(pa - di * h) * sign(pa.y * di.x - pa.x * di.y);
}

float SdBox(float2 p, float2 b)
{
    float2 d = abs(p) - b;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

float SdRoundBox(float2 p, float2 b, float4 r, SdRoundBoxType type)
{
    if (all(r <= 0)) return SdBox(p, b);
    // select corner radius
    r.xy = (p.x > 0.0) ? r.xy : r.zw;
    r.x = (p.y > 0.0) ? r.x : r.y;
    // box coordinates
    float2 q = abs(p) - b + r.x;
    // distance to sides
    if (min(q.x, q.y) < 0.0) return max(q.x, q.y) - r.x;
    // rotate 45 degrees, offset by r and scale by r*sqrt(0.5) to canonical corner coordinates
    float2 uv = float2(abs(q.x - q.y), q.x + q.y - r.x) / r.x;
    // compute distance to corner shape
    float d;
    switch (type)
    {
    case SdRoundBoxType::Cosine:
        d = SdCornerCosine(uv);
        break;
    case SdRoundBoxType::Parabola:
        d = SdCornerParabola(uv);
        break;
    case SdRoundBoxType::Cubic:
        d = SdCornerCubic(uv);
        break;
    default:
        d = SdCornerCircle(uv);
        break;
    }
    // undo scale
    return d * r.x * sqrt(0.5);
}

float TRoundBoxAA(float2 pos, float4 radius, SdRoundBoxType type, AA_t aa)
{
    float2 p = aa.Apply(pos);
    float2 b = aa.size * aa.scale();
    float4 r = radius * aa.scale();
    // select corner radius
    r.xy = (p.x > 0.0) ? r.xy : r.zw;
    r.x = (p.y > 0.0) ? r.x : r.y;
    // box coordinates
    float2 q = abs(p) - b + r.x;
    // distance to sides
    if (min(q.x, q.y) < 0.0) return 1;
    // if (min(q.x, q.y) < 0.0)
    // {
    //     if (max(abs(q.x), abs(q.y)) < 2)
    //     {
    //         float sum = 0;
    //         for (uint i = 0; i < 8; i++)
    //         {
    //             float2 offset = hammersley_8[i] * aa.scale();
    //             float2 lq = q + offset;
    //             sum += max(lq.x, lq.y) - r.x <= 0;
    //         }
    //         return sum / 8;
    //     }
    //     else
    //     {
    //         return max(q.x, q.y) - r.x <= 0;
    //     }
    // }
    // rotate 45 degrees, offset by r and scale by r*sqrt(0.5) to canonical corner coordinates
    float2 uv = float2(abs(q.x - q.y), q.x + q.y - r.x) / r.x;
    // compute distance to corner shape
    float d;
    switch (type)
    {
    case SdRoundBoxType::Cosine:
        d = SdCornerCosine(uv);
        break;
    case SdRoundBoxType::Parabola:
        d = SdCornerParabola(uv);
        break;
    case SdRoundBoxType::Cubic:
        d = SdCornerCubic(uv);
        break;
    default:
        d = SdCornerCircle(uv);
        break;
    }
    // undo scale
    d = d * r.x * sqrt(0.5);
    return SdAA2(d, aa.size.y);
}

#endif
