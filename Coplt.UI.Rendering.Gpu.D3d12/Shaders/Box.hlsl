#include "./Sdf.hlsl"

float4 ApplyGamma(float4 color)
{
    const float inv_gamma = 1.0 / 2.2;
    float3 c = pow(color.rgb, inv_gamma);
    return float4(c, color.a);
}

float2 ScreenToClip(float2 pos, float4 size)
{
    float2 output = float2((pos.x * size.z) * 2.0 - 1.0, ((size.y - pos.y) * size.w) * 2.0 - 1.0);
    return output;
}

enum class SamplerType : uint
{
    LinearClamp,
    LinearWrap,
    PointClamp,
    PointWrap,
};

enum class RenderFlags : uint
{
    None = 0,
    ContentBox = 1 << 0,
};

struct BoxData
{
    float4x4 TransformMatrix;
    float4 LeftTopWidthHeight;
    float4 BorderSize_TopRightBottomLeft;
    // br, tr, bl, tl
    float4 BorderRound;
    float4 BackgroundColor;
    float4 BackgroundImageTint;
    float4 BorderColor_Top;
    float4 BorderColor_Right;
    float4 BorderColor_Bottom;
    float4 BorderColor_Left;
    float Opaque;
    float Z;
    RenderFlags Flags;
    SamplerType BackgroundImageSampler;
    SdRoundBoxType BorderRadiusMode;
    uint BackgroundImage;
};

cbuffer ViewData : register(b0, space0)
{
    // (w, h, 1 / w, 1 / h)
    float4 ViewSize;
    uint Count;
}

StructuredBuffer<BoxData> BoxDatas : register(t0, space0);
Texture2D Images[] : register(t0, space1);
SamplerState Samplers[] : register(s0, space2);

struct Varying
{
    float4 PositionCS : SV_Position;
    float2 PositionScreenSpace : PositionScreenSpace;
    float2 PositionLocal : PositionLocal;
    float2 PositionCenterLocal : PositionCenterLocal;
    float2 UV: UV;
    nointerpolation uint iid: iid;
};

struct Attrs
{
    uint vid: SV_VertexID;
    uint iid: SV_InstanceID;
};

[Shader("vertex")]
Varying Vertex(Attrs input)
{
    BoxData data = BoxDatas.Load(input.iid);

    const float2 uvs[4] =
    {
        float2(0, 0),
        float2(1, 0),
        float2(1, 1),
        float2(0, 1),
    };

    float2 uv = uvs[input.vid];
    float2 position_local = data.LeftTopWidthHeight.zw * uv;
    float2 screen_space_position = position_local + data.LeftTopWidthHeight.xy;
    screen_space_position = mul(data.TransformMatrix, float4(screen_space_position, 0, 1)).xy;
    float2 clip_space_position = ScreenToClip(screen_space_position, ViewSize);

    Varying output;
    output.iid = input.iid;
    output.PositionCS = float4(clip_space_position, data.Z, 1);
    output.PositionScreenSpace = screen_space_position;
    output.PositionLocal = position_local;
    output.PositionCenterLocal = position_local - data.LeftTopWidthHeight.zw * 0.5f;
    output.UV = uv;

    return output;
}

float2 Rotate45(float2 p)
{
    const float sincos45 = 0.707106781186548;
    float2x2 mat = float2x2(sincos45, -sincos45, sincos45, sincos45);
    return mul(mat, p);
}

float DDAA(float d, float m = 1.5)
{
    return saturate(d / (m * length(float2(ddx(d), ddy(d)))) + 0.5);
}

float color_min(float a, float b, float4 ca, float4 cb, out float4 co)
{
    co = lerp(ca, cb, b / (a + b));
    return min(a, b);
}

float4 CalcBorderColor(
    float4 col, in AA_t aa, in BoxData data,
    float2 pos, float2 size,
    float t, float r, float b, float l,
    float2 blt, float2 brt, float2 blb, float2 brb,
    float2 lt, float2 rt, float2 lb, float2 rb,
    float2 ncl, float2 nct, float2 nblt, float2 nbrt, float2 nblb, float2 nbrb
)
{
    float2 np = aa.Apply(pos);

    float dcv = SdInfLineV(np, ncl, float2(1, 0));
    float dch = SdInfLineV(np, nct, float2(0, 1));

    float dlt = SdInfLineV(np, nblt, normalize(lt));
    float drt = SdInfLineV(np, nbrt, normalize(rt - brt));
    float dlb = SdInfLineV(np, nblb, normalize(lb - blb));
    float drb = SdInfLineV(np, nbrb, normalize(rb - brb));

    float dt = max(max(dlt, -drt), dcv);
    float dr = max(max(-drb, drt), dch);
    float db = max(max(-dlb, drb), -dcv);
    float dl = max(max(-dlt, dlb), -dch);

    // if (t > 0) col = dt <= 0 ? data.border_top_color : col;
    // if (r > 0) col = dr <= 0 ? data.border_right_color : col;
    // if (b > 0) col = db <= 0 ? data.border_bottom_color : col;
    // if (l > 0) col = dl <= 0 ? data.border_left_color : col;

    if (dt < aa.scale())
    {
        if (dr < aa.scale())
        {
            return lerp(data.BorderColor_Right, data.BorderColor_Top, SdAA2(dt, size.y));
        }
        else if (dl < aa.scale())
        {
            return lerp(data.BorderColor_Left, data.BorderColor_Top, SdAA2(dt, size.y));
        }
        else
        {
            return data.BorderColor_Top;
        }
    }
    else if (db < aa.scale())
    {
        if (dr < aa.scale())
        {
            return lerp(data.BorderColor_Right, data.BorderColor_Bottom, SdAA2(db, size.y));
        }
        else if (dl < aa.scale())
        {
            return lerp(data.BorderColor_Left, data.BorderColor_Bottom, SdAA2(db, size.y));
        }
        else
        {
            return data.BorderColor_Bottom;
        }
    }
    else if (dl < aa.scale())
    {
        if (dt < aa.scale())
        {
            return lerp(data.BorderColor_Top, data.BorderColor_Left, SdAA2(dl, size.y));
        }
        else if (db < aa.scale())
        {
            return lerp(data.BorderColor_Bottom, data.BorderColor_Left, SdAA2(dl, size.y));
        }
        else
        {
            return data.BorderColor_Left;
        }
    }
    else if (dr < aa.scale())
    {
        if (dt < aa.scale())
        {
            return lerp(data.BorderColor_Top, data.BorderColor_Right, SdAA2(dr, size.y));
        }
        else if (db < aa.scale())
        {
            return lerp(data.BorderColor_Bottom, data.BorderColor_Right, SdAA2(dr, size.y));
        }
        else
        {
            return data.BorderColor_Right;
        }
    }

    return col;
}

[Shader("pixel")]
float4 Pixel(Varying input) : SV_Target
{
    BoxData data = BoxDatas.Load(input.iid);
    float2 uv = input.UV;
    float2 pos = input.PositionLocal;
    float4 color = data.BackgroundColor;
    float2 size = data.LeftTopWidthHeight.zw;
    AA_t aa = BuildAA(size);
    float4 border_round = min(data.BorderRound, min(size.x, size.y));

    if (any(data.BorderSize_TopRightBottomLeft > 0))
    {
        float t = data.BorderSize_TopRightBottomLeft.x;
        float r = data.BorderSize_TopRightBottomLeft.y;
        float b = data.BorderSize_TopRightBottomLeft.z;
        float l = data.BorderSize_TopRightBottomLeft.w;

        float2 blt = 0;
        float2 brt = float2(size.x, 0);
        float2 blb = float2(0, size.y);
        float2 brb = float2(size.x, size.y);

        float2 lt = float2(l, t);
        float2 rt = float2(size.x - r, t);
        float2 lb = float2(l, size.y - b);
        float2 rb = float2(size.x - r, size.y - b);

        float2 b_uv = pos - float2(l, t);
        float2 b_size = size - float2(r, b) - float2(l, t);
        AA_t iaa = BuildAA(b_size);

        float4 inner_border_round = border_round;
        inner_border_round.x = max(0, inner_border_round.x - distance(brb, rb));
        inner_border_round.y = max(0, inner_border_round.y - distance(brt, rt));
        inner_border_round.z = max(0, inner_border_round.z - distance(blb, lb));
        inner_border_round.w = max(0, inner_border_round.w - distance(blt, lt));
        inner_border_round = min(inner_border_round, min(b_size.x, b_size.y));

        float ti = TRoundBoxAA(b_uv, inner_border_round, data.BorderRadiusMode, iaa);

        if (ti < 1)
        {
            float4 col;

            if (
                all(
                    data.BorderColor_Top == data.BorderColor_Right &
                    data.BorderColor_Top == data.BorderColor_Bottom &
                    data.BorderColor_Top == data.BorderColor_Left
                )
            )
            {
                col = data.BorderColor_Top;
            }
            else
            {
                float2 ncl = aa.Apply(float2(0, size.y * 0.5));
                float2 nct = aa.Apply(float2(size.x * 0.5, 0));
                float2 nblt = aa.Apply(blt);
                float2 nbrt = aa.Apply(brt);
                float2 nblb = aa.Apply(blb);
                float2 nbrb = aa.Apply(brb);

                col = CalcBorderColor(
                    color, aa, data,
                    pos, size,
                    t, r, b, l,
                    blt, brt, blb, brb,
                    lt, rt, lb, rb,
                    ncl, nct, nblt, nbrt, nblb, nbrb
                );
            }

            color = lerp(col, color, ti);
        }
    }
    float t = TRoundBoxAA(pos, border_round, data.BorderRadiusMode, aa);
    color.a *= t;

    return color;
}

// sdf debug
// float d = min(min(dt, dr), min(db, dl));
// col = (d > 0.0) ? float3(0.9, 0.6, 0.3) : float3(0.65, 0.85, 1.0);
// col *= 1.0 - exp(-6.0 * abs(d));
// col *= 0.8 + 0.2 * cos(150.0 * d);
// col = lerp(col, 1, 1.0 - smoothstep(0.0, 0.01, abs(d)));
