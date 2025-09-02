#include "./Common.hlsl"
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
    float4x4 VP;
}

// StructuredBuffer<BoxData> BoxDatas : register(t0, space0);

enum class Box_Varying_Flags : uint
{
    None,
    HasAnyBorder,
};

struct Box_Varying
{
    float4 PositionCS : SV_Position;
    float2 UV: UV;
    float2 LocalPosition: LocalPosition;
    nointerpolation uint BorderIndex: BorderIndex;
    nointerpolation Box_Varying_Flags Flags: Flags;
    nointerpolation uint iid: iid;
    nointerpolation uint BatchBuffer: BatchBuffer;
    nointerpolation uint BatchIndex: BatchIndex;
};

struct Box_Attrs
{
    uint iid: SV_InstanceID;
    uint vid: SV_VertexID;
};

struct Vertex
{
    float2 pos;
};

struct BatchData
{
    uint Buffer;
    uint Index;
};

StructuredBuffer<BatchData> Batches : register(t0, space10);

StructuredBuffer<BoxData> BoxDataBuffers[] : register(t0, space1);

[Shader("vertex")]
Box_Varying Box_Vertex(Box_Attrs input)
{
    BatchData batch = Batches.Load(input.iid);
    StructuredBuffer<BoxData> BoxDatas = BoxDataBuffers[NonUniformResourceIndex(batch.Buffer)];
    BoxData data = BoxDatas[batch.Index];

    Box_Varying output = (Box_Varying)0;
    output.iid = input.iid;
    output.BatchBuffer = batch.Buffer;
    output.BatchIndex = batch.Index;

    float4 LeftTopWidthHeight = abs(data.LeftTopWidthHeight);
    float2 offset = LeftTopWidthHeight.xy;
    float2 size = LeftTopWidthHeight.zw;
    float2 half_size = size * 0.5f;
    float4 border_size = abs(data.BorderSize_TopRightBottomLeft);
    float2 inner_size = abs(size - (border_size.yx + border_size.wz));

    float4 bc_t = data.BorderColor_Top;
    float4 bc_r = data.BorderColor_Right;
    float4 bc_b = data.BorderColor_Bottom;
    float4 bc_l = data.BorderColor_Left;

    Box_Varying_Flags flags = Box_Varying_Flags::None;
    bool has_any_border = any(border_size > 0) && any(float4(bc_t.a, bc_r.a, bc_b.a, bc_l.a) != 0);

    float2 pos;
    uint border_index;

    if (has_any_border)
    {
        AddFlag(flags, Box_Varying_Flags::HasAnyBorder);
        float b_t = border_size.x;
        float b_r = border_size.y;
        float b_b = border_size.z;
        float b_l = border_size.w;

        float2 bp_lt = float2(b_l, b_t);
        float2 bp_rt = float2(size.x - b_r, b_t);
        float2 bp_lb = float2(b_l, size.y - b_b);
        float2 bp_rb = size - float2(b_r, b_b);

        {
            bool same_dir = size.y > size.x == inner_size.y > size.x;
            float2 size_base = size.y == size.x ? inner_size : size;
            bool dir_v = same_dir ? size_base.y > size_base.x : size_base.x >= size_base.y;

            // calc border line
            Line2d border_line;
            {
                Ray2d ray_b, ray_c;
                Ray2d ray_a = ray2d_to(0, bp_lt);
                Ray2d ray_d = ray2d_to(size, bp_rb);
                {
                    Ray2d ray_t0 = ray2d_to(float2(size.x, 0), bp_rt);
                    Ray2d ray_t1 = ray2d_to(float2(0, size.y), bp_lb);
                    if (dir_v)
                    {
                        ray_b = ray_t0;
                        ray_c = ray_t1;
                    }
                    else
                    {
                        ray_b = ray_t1;
                        ray_c = ray_t0;
                    }
                }
                intersect(ray_a, ray_b, border_line.start);
                intersect(ray_c, ray_d, border_line.end);
            }

            if (dir_v)
            {
                float2 pos_arr[] =
                {
                    // t
                    border_line.start,
                    float2(0, 0),
                    float2(size.x, 0),

                    // r
                    float2(size.x, 0),
                    size,
                    border_line.start,

                    border_line.start,
                    size,
                    border_line.end,

                    // b
                    border_line.end,
                    size,
                    float2(0, size.y),

                    // l
                    float2(0, size.y),
                    float2(0, 0),
                    border_line.end,

                    border_line.end,
                    float2(0, 0),
                    border_line.start,
                };
                uint border_index_arr[] =
                {
                    // t
                    0, 0, 0,
                    // r
                    1, 1, 1, 1, 1, 1,
                    // b,
                    2, 2, 2,
                    // l
                    3, 3, 3, 3, 3, 3,
                };
                pos = pos_arr[input.vid];
                border_index = border_index_arr[input.vid];
            }
            else
            {
                float2 pos_arr[] =
                {
                    // t
                    float2(0, 0),
                    float2(size.x, 0),
                    border_line.start,

                    border_line.start,
                    float2(size.x, 0),
                    border_line.end,

                    // r
                    border_line.end,
                    float2(size.x, 0),
                    size,

                    // b
                    size,
                    float2(0, size.y),
                    border_line.end,

                    border_line.end,
                    float2(0, size.y),
                    border_line.start,

                    // l
                    border_line.start,
                    float2(0, size.y),
                    float2(0, 0),
                };
                uint border_index_arr[] =
                {
                    // t
                    0, 0, 0, 0, 0, 0,
                    // r
                    1, 1, 1,
                    // b,
                    2, 2, 2, 2, 2, 2,
                    // l
                    3, 3, 3,
                };
                pos = pos_arr[input.vid];
                border_index = border_index_arr[input.vid];
            }
        }
    }
    else
    {
        static const float2 pos_m[] = {
            // top
            float2(1, 1),
            float2(1.0, 0.0),
            float2(0.0, 0.0),
        };

        pos = pos_m[input.vid];
        pos *= 100;
    }

    float4x4 transform = {
        1, 0, 0, offset.x,
        0, 1, 0, offset.y,
        0, 0, 1, 0,
        0, 0, 0, 1,
    };

    float2 uv = pos / size;
    float4 p4 = float4(pos, 0, 1);
    p4 = mul(transform, p4);
    p4 = mul(data.TransformMatrix, p4);
    p4.z = 0.5 + data.Z;
    p4 = mul(VP, p4);

    output.PositionCS = p4;
    output.UV = uv;
    output.LocalPosition = pos;
    output.BorderIndex = border_index;
    output.Flags = flags;

    return output;
}

[Shader("pixel")]
float4 Box_Pixel(Box_Varying input) : SV_Target
{
    StructuredBuffer<BoxData> BoxDatas = BoxDataBuffers[NonUniformResourceIndex(input.BatchBuffer)];
    BoxData data = BoxDatas[input.BatchIndex];

    float4 color = data.BackgroundColor;
    if (HasFlag(input.Flags, Box_Varying_Flags::HasAnyBorder))
    {
        float4 border_color_arr[] =
        {
            data.BorderColor_Top,
            data.BorderColor_Right,
            data.BorderColor_Bottom,
            data.BorderColor_Left,
        };
        color = border_color_arr[input.BorderIndex];
    }

    return color;
}

// old

// struct Attrs
// {
//     uint vid: SV_VertexID;
//     uint iid: SV_InstanceID;
// };
//
// [Shader("vertex")]
// Varying Vertex(Attrs input)
// {
//     BoxData data = BoxDatas.Load(input.iid);
//
//     const float2 uvs[4] =
//     {
//         float2(0, 0),
//         float2(1, 0),
//         float2(1, 1),
//         float2(0, 1),
//     };
//
//     float2 uv = uvs[input.vid];
//     float2 position_local = data.LeftTopWidthHeight.zw * uv;
//     float2 screen_space_position = position_local + data.LeftTopWidthHeight.xy;
//     screen_space_position = mul(data.TransformMatrix, float4(screen_space_position, 0, 1)).xy;
//     float2 clip_space_position = ScreenToClip(screen_space_position, ViewSize);
//
//     Varying output;
//     output.iid = input.iid;
//     output.PositionCS = float4(clip_space_position, data.Z, 1);
//     output.PositionScreenSpace = screen_space_position;
//     output.PositionLocal = position_local;
//     output.PositionCenterLocal = position_local - data.LeftTopWidthHeight.zw * 0.5f;
//     output.UV = uv;
//
//     return output;
// }
//
// float2 Rotate45(float2 p)
// {
//     const float sincos45 = 0.707106781186548;
//     float2x2 mat = float2x2(sincos45, -sincos45, sincos45, sincos45);
//     return mul(mat, p);
// }
//
// float DDAA(float d, float m = 1.5)
// {
//     return saturate(d / (m * length(float2(ddx(d), ddy(d)))) + 0.5);
// }
//
// float color_min(float a, float b, float4 ca, float4 cb, out float4 co)
// {
//     co = lerp(ca, cb, b / (a + b));
//     return min(a, b);
// }
//
// float4 CalcBorderColor(
//     float4 col, in AA_t aa, in BoxData data,
//     float2 pos, float2 size,
//     float t, float r, float b, float l,
//     float2 blt, float2 brt, float2 blb, float2 brb,
//     float2 lt, float2 rt, float2 lb, float2 rb,
//     float2 ncl, float2 nct, float2 nblt, float2 nbrt, float2 nblb, float2 nbrb
// )
// {
//     float2 np = aa.Apply(pos);
//
//     float dcv = SdInfLineV(np, ncl, float2(1, 0));
//     float dch = SdInfLineV(np, nct, float2(0, 1));
//
//     float dlt = SdInfLineV(np, nblt, normalize(lt));
//     float drt = SdInfLineV(np, nbrt, normalize(rt - brt));
//     float dlb = SdInfLineV(np, nblb, normalize(lb - blb));
//     float drb = SdInfLineV(np, nbrb, normalize(rb - brb));
//
//     float dt = max(max(dlt, -drt), dcv);
//     float dr = max(max(-drb, drt), dch);
//     float db = max(max(-dlb, drb), -dcv);
//     float dl = max(max(-dlt, dlb), -dch);
//
//     // if (t > 0) col = dt <= 0 ? data.border_top_color : col;
//     // if (r > 0) col = dr <= 0 ? data.border_right_color : col;
//     // if (b > 0) col = db <= 0 ? data.border_bottom_color : col;
//     // if (l > 0) col = dl <= 0 ? data.border_left_color : col;
//
//     if (dt < aa.scale())
//     {
//         if (dr < aa.scale())
//         {
//             return lerp(data.BorderColor_Right, data.BorderColor_Top, SdAA2(dt, size.y));
//         }
//         else if (dl < aa.scale())
//         {
//             return lerp(data.BorderColor_Left, data.BorderColor_Top, SdAA2(dt, size.y));
//         }
//         else
//         {
//             return data.BorderColor_Top;
//         }
//     }
//     else if (db < aa.scale())
//     {
//         if (dr < aa.scale())
//         {
//             return lerp(data.BorderColor_Right, data.BorderColor_Bottom, SdAA2(db, size.y));
//         }
//         else if (dl < aa.scale())
//         {
//             return lerp(data.BorderColor_Left, data.BorderColor_Bottom, SdAA2(db, size.y));
//         }
//         else
//         {
//             return data.BorderColor_Bottom;
//         }
//     }
//     else if (dl < aa.scale())
//     {
//         if (dt < aa.scale())
//         {
//             return lerp(data.BorderColor_Top, data.BorderColor_Left, SdAA2(dl, size.y));
//         }
//         else if (db < aa.scale())
//         {
//             return lerp(data.BorderColor_Bottom, data.BorderColor_Left, SdAA2(dl, size.y));
//         }
//         else
//         {
//             return data.BorderColor_Left;
//         }
//     }
//     else if (dr < aa.scale())
//     {
//         if (dt < aa.scale())
//         {
//             return lerp(data.BorderColor_Top, data.BorderColor_Right, SdAA2(dr, size.y));
//         }
//         else if (db < aa.scale())
//         {
//             return lerp(data.BorderColor_Bottom, data.BorderColor_Right, SdAA2(dr, size.y));
//         }
//         else
//         {
//             return data.BorderColor_Right;
//         }
//     }
//
//     return col;
// }
//
// [Shader("pixel")]
// float4 Pixel(Varying input) : SV_Target
// {
//     BoxData data = BoxDatas.Load(input.iid);
//     float2 uv = input.UV;
//     float2 pos = input.PositionLocal;
//     float4 color = data.BackgroundColor;
//     float2 size = data.LeftTopWidthHeight.zw;
//     AA_t aa = BuildAA(size);
//     float4 border_round = min(data.BorderRound, min(size.x, size.y));
//
//     if (any(data.BorderSize_TopRightBottomLeft > 0))
//     {
//         float t = data.BorderSize_TopRightBottomLeft.x;
//         float r = data.BorderSize_TopRightBottomLeft.y;
//         float b = data.BorderSize_TopRightBottomLeft.z;
//         float l = data.BorderSize_TopRightBottomLeft.w;
//
//         float2 blt = 0;
//         float2 brt = float2(size.x, 0);
//         float2 blb = float2(0, size.y);
//         float2 brb = float2(size.x, size.y);
//
//         float2 lt = float2(l, t);
//         float2 rt = float2(size.x - r, t);
//         float2 lb = float2(l, size.y - b);
//         float2 rb = float2(size.x - r, size.y - b);
//
//         float2 b_uv = pos - float2(l, t);
//         float2 b_size = size - float2(r, b) - float2(l, t);
//         AA_t iaa = BuildAA(b_size);
//
//         float4 inner_border_round = border_round;
//         inner_border_round.x = max(0, inner_border_round.x - distance(brb, rb));
//         inner_border_round.y = max(0, inner_border_round.y - distance(brt, rt));
//         inner_border_round.z = max(0, inner_border_round.z - distance(blb, lb));
//         inner_border_round.w = max(0, inner_border_round.w - distance(blt, lt));
//         inner_border_round = min(inner_border_round, min(b_size.x, b_size.y));
//
//         float ti = TRoundBoxAA(b_uv, inner_border_round, data.BorderRadiusMode, iaa);
//
//         if (ti < 1)
//         {
//             float4 col;
//
//             if (
//                 all(
//                     data.BorderColor_Top == data.BorderColor_Right &
//                     data.BorderColor_Top == data.BorderColor_Bottom &
//                     data.BorderColor_Top == data.BorderColor_Left
//                 )
//             )
//             {
//                 col = data.BorderColor_Top;
//             }
//             else
//             {
//                 float2 ncl = aa.Apply(float2(0, size.y * 0.5));
//                 float2 nct = aa.Apply(float2(size.x * 0.5, 0));
//                 float2 nblt = aa.Apply(blt);
//                 float2 nbrt = aa.Apply(brt);
//                 float2 nblb = aa.Apply(blb);
//                 float2 nbrb = aa.Apply(brb);
//
//                 col = CalcBorderColor(
//                     color, aa, data,
//                     pos, size,
//                     t, r, b, l,
//                     blt, brt, blb, brb,
//                     lt, rt, lb, rb,
//                     ncl, nct, nblt, nbrt, nblb, nbrb
//                 );
//             }
//
//             color = lerp(col, color, ti);
//         }
//     }
//     float t = TRoundBoxAA(pos, border_round, data.BorderRadiusMode, aa);
//     color.a *= t;
//
//     return color;
// }
//
// // sdf debug
// // float d = min(min(dt, dr), min(db, dl));
// // col = (d > 0.0) ? float3(0.9, 0.6, 0.3) : float3(0.65, 0.85, 1.0);
// // col *= 1.0 - exp(-6.0 * abs(d));
// // col *= 0.8 + 0.2 * cos(150.0 * d);
// // col = lerp(col, 1, 1.0 - smoothstep(0.0, 0.01, abs(d)));
