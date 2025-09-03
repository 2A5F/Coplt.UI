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
    // t r b l
    float4 BorderColor[4];
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
    HasAnyBorder = 1 << 0,
    IsContent = 1 << 1,
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

    float4 bc_t = data.BorderColor[0];
    float4 bc_r = data.BorderColor[1];
    float4 bc_b = data.BorderColor[2];
    float4 bc_l = data.BorderColor[3];

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
            Line2d border_line;
            bool dir_v;

            // calc border line
            {
                Ray2d ray_lt = ray2d_to(0, bp_lt);
                Ray2d ray_rb = ray2d_to(size, bp_rb);
                Ray2d ray_rt = ray2d_to(float2(size.x, 0), bp_rt);
                Ray2d ray_lb = ray2d_to(float2(0, size.y), bp_lb);

                float2 hit_lt_rt, hit_lb_rb, hit_lt_lb, hit_rt_rb;
                intersect(ray_lt, ray_rt, hit_lt_rt);
                intersect(ray_lb, ray_rb, hit_lb_rb);
                intersect(ray_lt, ray_lb, hit_lt_lb);
                intersect(ray_rt, ray_rb, hit_rt_rb);

                float diff_lt_rt = dir_diff(hit_lt_rt);
                float diff_lb_rb = dir_diff(hit_lb_rb);
                
                dir_v = diff_lt_rt > diff_lb_rb;

                border_line.start = dir_v ? hit_lt_rt : hit_lt_lb;
                border_line.end = dir_v ? hit_lb_rb : hit_rt_rb;
            }

            float2 pos_src[] =
            {
                /*  0 */ bp_lt,
                /*  1 */ bp_rt,
                /*  2 */ bp_lb,
                /*  3 */ bp_rb,
                /*  4 */ border_line.start,
                /*  5 */ border_line.end,
                /*  6 */ float2(0, 0),
                /*  7 */ float2(size.x, 0),
                /*  8 */ float2(0, size.y),
                /*  9 */ size,
            };

            if (dir_v)
            {
                static const uint index_arr[] =
                {
                    // it
                    4, 0, 1,

                    // ir
                    1, 3, 4,
                    4, 3, 5,

                    // ib
                    5, 3, 2,

                    // il
                    2, 0, 5,
                    5, 0, 4,

                    // ot
                    0, 6, 1,
                    1, 6, 7,

                    // or,
                    7, 9, 1,
                    1, 9, 3,

                    // ob,
                    3, 9, 2,
                    2, 9, 8,

                    // ol,
                    8, 6, 2,
                    2, 6, 0,
                };
                static const uint border_index_arr[] =
                {
                    // it
                    0, 0, 0,
                    // ir
                    1, 1, 1, 1, 1, 1,
                    // ib,
                    2, 2, 2,
                    // il
                    3, 3, 3, 3, 3, 3,

                    // ot
                    0, 0, 0, 0, 0, 0,
                    // or
                    1, 1, 1, 1, 1, 1,
                    // ob
                    2, 2, 2, 2, 2, 2,
                    // ol
                    3, 3, 3, 3, 3, 3,
                };
                pos = pos_src[index_arr[input.vid]];
                border_index = border_index_arr[input.vid];
            }
            else
            {
                static const uint index_arr[] =
                {
                    // it
                    0, 1, 4,
                    4, 1, 5,

                    // ir
                    5, 1, 3,

                    // ib
                    3, 2, 5,
                    5, 2, 4,

                    // il
                    4, 2, 0,

                    // ot
                    0, 6, 1,
                    1, 6, 7,

                    // or,
                    7, 9, 1,
                    1, 9, 3,

                    // ob,
                    3, 9, 2,
                    2, 9, 8,

                    // ol,
                    8, 6, 2,
                    2, 6, 0,
                };
                static const uint border_index_arr[] =
                {
                    // it
                    0, 0, 0, 0, 0, 0,
                    // ir
                    1, 1, 1,
                    // ib,
                    2, 2, 2, 2, 2, 2,
                    // il
                    3, 3, 3,

                    // ot
                    0, 0, 0, 0, 0, 0,
                    // or
                    1, 1, 1, 1, 1, 1,
                    // ob
                    2, 2, 2, 2, 2, 2,
                    // ol
                    3, 3, 3, 3, 3, 3,
                };
                pos = pos_src[index_arr[input.vid]];
                border_index = border_index_arr[input.vid];
            }

            AddFlag(flags, input.vid < 18 ? Box_Varying_Flags::IsContent : Box_Varying_Flags::None);
        }
    }
    else
    {
        // todo
        static const float2 pos_m[] = {
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

void CalcInnerBorderRadius(
    float2 size, float2 pos, float4 border_size, float4 border_radius,
    out float4 inner_border_radius, out float2 b_uv, out float2 b_size
)
{
    float t = border_size.x;
    float r = border_size.y;
    float b = border_size.z;
    float l = border_size.w;

    float2 blt = 0;
    float2 brt = float2(size.x, 0);
    float2 blb = float2(0, size.y);
    float2 brb = float2(size.x, size.y);

    float2 lt = float2(l, t);
    float2 rt = float2(size.x - r, t);
    float2 lb = float2(l, size.y - b);
    float2 rb = float2(size.x - r, size.y - b);

    b_uv = pos - float2(l, t);
    b_size = size - float2(r, b) - float2(l, t);

    inner_border_radius = border_radius;
    inner_border_radius.x = max(0, inner_border_radius.x - distance(brb, rb));
    inner_border_radius.y = max(0, inner_border_radius.y - distance(brt, rt));
    inner_border_radius.z = max(0, inner_border_radius.z - distance(blb, lb));
    inner_border_radius.w = max(0, inner_border_radius.w - distance(blt, lt));
    inner_border_radius = min(inner_border_radius, min(b_size.x, b_size.y));
}

[Shader("pixel")]
float4 Box_Pixel(Box_Varying input) : SV_Target
{
    StructuredBuffer<BoxData> BoxDatas = BoxDataBuffers[NonUniformResourceIndex(input.BatchBuffer)];
    BoxData data = BoxDatas[input.BatchIndex];

    float2 size = data.LeftTopWidthHeight.zw;
    float2 half_size = size * 0.5f;
    float min_half_size = min(size.x, size.y);
    float4 color = data.BackgroundColor;
    float4 border_color = data.BorderColor[input.BorderIndex];

    float4 border_radius = clamp(data.BorderRound, 0, min_half_size);
    bool has_border_radius = any(border_radius > 0);

    if (has_border_radius)
    {
        AA_t aa = BuildAA(size);
        float2 pos = input.LocalPosition;
        float t = TRoundBoxAA(pos, border_radius, data.BorderRadiusMode, aa);

        if (HasFlag(input.Flags, Box_Varying_Flags::HasAnyBorder))
        {
            if (HasFlag(input.Flags, Box_Varying_Flags::IsContent))
            {
                float4 inner_border_radius;
                float2 b_uv, b_size;
                CalcInnerBorderRadius(
                    size, pos, data.BorderSize_TopRightBottomLeft, border_radius,
                    inner_border_radius, b_uv, b_size
                );
                if (any(inner_border_radius > 0))
                {
                    AA_t iaa = BuildAA(b_size);
                    float b_t = TRoundBoxAA(b_uv, inner_border_radius, data.BorderRadiusMode, iaa);
                    float3 bg = color.a == 0 ? border_color.rgb : color.rgb;
                    float3 bc = border_color.a == 0 ? color.rgb : border_color.rgb;
                    float3 col = lerp(bc, bg, b_t);
                    float a = lerp(border_color.a, color.a, b_t);
                    color = float4(col.rgb, a);
                }
            }
            else
            {
                color = border_color;
            }
        }
        color.a *= t;
    }
    else
    {
        if (
            HasFlag(input.Flags, Box_Varying_Flags::HasAnyBorder)
            && !HasFlag(input.Flags, Box_Varying_Flags::IsContent)
        )
        {
            color = border_color;
        }
    }

    return color;
}
