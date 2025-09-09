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
    MixBorder = 1 << 2,
};

struct Box_Varying
{
    float4 PositionCS : SV_Position;
    float2 UV: UV;
    float2 LocalPosition: LocalPosition;
    nointerpolation uint BorderIndex: BorderIndex;
    nointerpolation uint BorderIndex2: BorderIndex2;
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

void Box_Mesh_Rect(
    in Box_Attrs input, out float2 pos, out uint border_index, inout Box_Varying_Flags flags,
    float2 size
)
{
    float2 pos_src[] =
    {
        /*  0 */ float2(0, 0),
        /*  1 */ float2(size.x, 0),
        /*  2 */ float2(0, size.y),
        /*  3 */ size,
    };

    static const uint index_arr[] = {
        0, 1, 3,
        3, 2, 0,
    };

    pos = pos_src[index_arr[input.vid]];
    border_index = 0;
}

void Box_Mesh_SingleSide(
    in Box_Attrs input, out float2 pos, out uint border_index, inout Box_Varying_Flags flags,
    float2 size, float b_t, float b_r, float b_b, float b_l, uint4 border_mask
)
{
    border_index = csum(border_mask & uint4(0, 1, 2, 3));

    float4 border_select[] =
    {
        /* t */ float4(0, b_t, size.x, b_t),
        /* r */ float4(size.x - b_r, 0, size.x - b_r, size.y),
        /* b */ float4(0, size.y - b_b, size.x, size.y - b_b),
        /* l */ float4(b_l, 0, b_l, size.y),
    };
    float4 border_pos_2 = border_select[border_index];
    float2 pos_src[] =
    {
        /*  0 */ border_pos_2.xy,
        /*  1 */ border_pos_2.zw,
        /*  2 */ float2(0, 0),
        /*  3 */ float2(size.x, 0),
        /*  4 */ float2(0, size.y),
        /*  5 */ size,
    };

    static const uint index_arr[] = {
        // t
        0, 2, 3, /**/ 3, 1, 0,
        0, 1, 5, /**/ 5, 4, 0,
        // r
        0, 3, 5, /**/ 5, 1, 0,
        0, 1, 4, /**/ 4, 2, 0,
        // b
        0, 1, 5, /**/ 5, 4, 0,
        0, 2, 3, /**/ 3, 1, 0,
        // l
        0, 1, 4, /**/ 4, 2, 0,
        0, 3, 5, /**/ 5, 1, 0,
    };

    pos = pos_src[index_arr[border_index * 12 + input.vid]];

    AddFlag(flags, input.vid < 6 ? Box_Varying_Flags::None : Box_Varying_Flags::IsContent);
}

void Box_Mesh_DoubleSide(
    in Box_Attrs input, out float2 pos, out uint border_index, inout Box_Varying_Flags flags,
    float2 size, float b_t, float b_r, float b_b, float b_l, uint4 border_mask,
    float2 inner_size, float2 inner_offset
)
{
    float2 half_inner_size = inner_size * 0.5f;
    float2 inner_center = inner_offset + half_inner_size;

    bool dir_v = all(border_mask.xz);
    uint4 border_index_source = border_mask & uint4(0, 1, 2, 3);
    uint border_index_a = csum(border_index_source.xy);
    uint border_index_b = csum(border_index_source.zw);
    border_index = input.vid < 12 ? border_index_a : border_index_b;

    float4 border_select[] =
    {
        /* t */ float4(0, b_t, size.x, b_t),
        /* r */ float4(size.x - b_r, 0, size.x - b_r, size.y),
        /* b */ float4(0, size.y - b_b, size.x, size.y - b_b),
        /* l */ float4(b_l, 0, b_l, size.y),
    };
    float4 border_pos_2_a = border_select[border_index_a];
    float4 border_pos_2_b = border_select[border_index_b];

    float2 pos_src[] =
    {
        /*  0 */ border_pos_2_a.xy,
        /*  1 */ border_pos_2_a.zw,
        /*  2 */ border_pos_2_b.xy,
        /*  3 */ border_pos_2_b.zw,
        /*  4 */ float2(0, 0),
        /*  5 */ float2(size.x, 0),
        /*  6 */ float2(0, size.y),
        /*  7 */ size,
        /*  8 */ dir_v ? float2(0, inner_center.y) : float2(inner_center.x, 0),
        /*  9 */ dir_v ? float2(size.x, inner_center.y) : float2(inner_center.x, size.y),
    };
    static const uint index_arr[] =
    {
        // h
        // r
        0, 5, 7, /**/ 7, 1, 0,
        0, 1, 9, /**/ 9, 8, 0,
        // l
        3, 6, 4, /**/ 4, 2, 3,
        3, 2, 8, /**/ 8, 9, 3,

        // v
        // t
        0, 4, 5, /**/ 5, 1, 0,
        0, 1, 9, /**/ 9, 8, 0,
        // b
        3, 7, 6, /**/ 6, 2, 3,
        3, 2, 8, /**/ 8, 9, 3,
    };

    pos = pos_src[index_arr[dir_v * 24 + input.vid]];
    uint local_vid = input.vid % 12;
    AddFlag(flags, local_vid < 6 ? Box_Varying_Flags::None : Box_Varying_Flags::IsContent);
}

void Box_Mesh_ThreeSide(
    in Box_Attrs input, out float2 pos, out uint border_index, inout Box_Varying_Flags flags,
    float2 size, float b_t, float b_r, float b_b, float b_l, uint4 border_mask,
    out uint border_index2
)
{
    uint side_index = csum((~border_mask) & uint4(0, 1, 2, 3));
    static const float4 side_line_arr[] =
    {
        /* t */ float4(0, 0, 1, 0),
        /* r */ float4(1, 0, 1, 1),
        /* b */ float4(0, 1, 1, 1),
        /* l */ float4(0, 0, 0, 1),
    };
    float4 side_line = side_line_arr[side_index] * float4(size, size);
    Ray2d ray_side = ray2d_to(side_line.xy, side_line.zw);

    float2 bp_lt = float2(b_l, b_t);
    float2 bp_rt = float2(size.x - b_r, b_t);
    float2 bp_lb = float2(b_l, size.y - b_b);
    float2 bp_rb = size - float2(b_r, b_b);

    Ray2d ray_arr[] =
    {
        /* 0 lt */ ray2d_to(0, bp_lt),
        /* 1 rb */ ray2d_to(size, bp_rb),
        /* 2 rt */ ray2d_to(float2(size.x, 0), bp_rt),
        /* 3 lb */ ray2d_to(float2(0, size.y), bp_lb),
    };
    static const uint2 ray_index_arr[] =
    {
        /* t */ uint2(3, 1),
        /* r */ uint2(3, 0),
        /* b */ uint2(0, 2),
        /* l */ uint2(1, 2),
    };
    uint2 ray_index = ray_index_arr[side_index];
    Ray2d ray_a = ray_arr[ray_index.x], ray_b = ray_arr[ray_index.y];
    float2 hit_a, hit_b, hit_ab;
    intersect(ray_a, ray_side, hit_a);
    intersect(ray_b, ray_side, hit_b);
    intersect(ray_a, ray_b, hit_ab);
    bool intersected = all(hit_ab > 0 & hit_ab < size);

    hit_b = intersected ? hit_ab : hit_b;

    float2 pos_src[] =
    {
        /*  0 */ bp_lt,
        /*  1 */ bp_rt,
        /*  2 */ bp_lb,
        /*  3 */ bp_rb,
        /*  4 */ hit_a,
        /*  5 */ hit_b,
        /*  6 */ float2(0, 0),
        /*  7 */ float2(size.x, 0),
        /*  8 */ float2(0, size.y),
        /*  9 */ size,
    };

    static const uint index_arr[] =
    {
        // side t
        // r
        5, 1, 3, /**/ 3, 1, 7, /**/ 7, 9, 3,
        // b
        3, 9, 8, /**/ 8, 2, 3, /**/ 3, 2, 5, /**/ 5, 2, 4,
        // l
        4, 2, 0, /**/ 0, 2, 6, /**/ 6, 2, 8,

        // side r
        // b
        5, 3, 2, /**/ 2, 3, 9, /**/ 9, 8, 2,
        // l,
        2, 8, 6, /**/ 6, 0, 2, /**/ 2, 0, 5, /**/ 5, 0, 4,
        // t
        4, 0, 1, /**/ 1, 0, 7, /**/ 7, 0, 6,

        // side b
        // l
        4, 2, 0, /**/ 0, 2, 8, /**/ 8, 6, 0,
        // t,
        0, 6, 7, /**/ 7, 1, 0, /**/ 0, 1, 4, /**/ 4, 1, 5,
        // r
        5, 1, 3, /**/ 3, 1, 9, /**/ 9, 1, 7,

        // side l
        // t
        4, 0, 1, /**/ 1, 0, 6, /**/ 6, 7, 1,
        // r,
        1, 7, 9, /**/ 9, 3, 1, /**/ 1, 3, 4, /**/ 4, 3, 5,
        // b
        5, 3, 2, /**/ 2, 3, 8, /**/ 8, 3, 9,


        // cross t
        // inner
        0, 1, 5, /**/ 5, 1, 3, /**/ 3, 2, 5, /**/ 5, 2, 0,
        // l
        0, 2, 6, /**/ 6, 2, 8,
        // b
        8, 2, 9, /**/ 9, 2, 3,
        // r
        3, 1, 7, /**/ 7, 9, 3,

        // cross r
        // inner
        1, 3, 5, /**/ 5, 3, 2, /**/ 2, 0, 5, /**/ 5, 0, 1,
        // t
        1, 0, 7, /**/ 7, 0, 6,
        // l
        6, 0, 8, /**/ 8, 0, 2,
        // b
        2, 3, 9, /**/ 9, 8, 2,

        // cross b
        // inner
        3, 2, 5, /**/ 5, 2, 0, /**/ 0, 1, 5, /**/ 5, 1, 3,
        // r
        3, 1, 9, /**/ 9, 1, 7,
        // t
        7, 1, 6, /**/ 6, 1, 0,
        // l
        0, 2, 8, /**/ 8, 0, 6,

        // cross l
        // inner
        2, 0, 5, /**/ 5, 0, 1, /**/ 1, 3, 5, /**/ 5, 3, 2,
        // b
        2, 3, 8, /**/ 8, 3, 9,
        // r
        9, 3, 7, /**/ 7, 3, 1,
        // t
        1, 0, 6, /**/ 6, 7, 1,
    };

    static const uint border_index_arr[] =
    {
        // side t
        // r
        1, 1, 1, /**/ 1, 1, 1, /**/ 1, 1, 1,
        // b,
        2, 2, 2, /**/ 2, 2, 2, /**/ 2, 2, 2, /**/ 2, 2, 2,
        // l
        3, 3, 3, /**/ 3, 3, 3, /**/ 3, 3, 3,

        // side r
        // b
        2, 2, 2, /**/ 2, 2, 2, /**/ 2, 2, 2,
        // l,
        3, 3, 3, /**/ 3, 3, 3, /**/ 3, 3, 3, /**/ 3, 3, 3,
        // t
        0, 0, 0, /**/ 0, 0, 0, /**/ 0, 0, 0,

        // side b
        // l
        3, 3, 3, /**/ 3, 3, 3, /**/ 3, 3, 3,
        // t,
        0, 0, 0, /**/ 0, 0, 0, /**/ 0, 0, 0, /**/ 0, 0, 0,
        // r
        1, 1, 1, /**/ 1, 1, 1, /**/ 1, 1, 1,

        // side l
        // t
        0, 0, 0, /**/ 0, 0, 0, /**/ 0, 0, 0,
        // r,
        1, 1, 1, /**/ 1, 1, 1, /**/ 1, 1, 1, /**/ 1, 1, 1,
        // b
        2, 2, 2, /**/ 2, 2, 2, /**/ 2, 2, 2,


        // cross t
        // inner
        1, 1, 1, /**/ 1, 1, 1, /**/ 2, 2, 2, /**/ 3, 3, 3,
        // l
        3, 3, 3, /**/ 3, 3, 3,
        // b
        2, 2, 2, /**/ 2, 2, 2,
        // r
        1, 1, 1, /**/ 1, 1, 1,

        // cross r
        // inner
        2, 2, 2, /**/ 2, 2, 2, /**/ 3, 3, 3, /**/ 0, 0, 0,
        // t
        0, 0, 0, /**/ 0, 0, 0,
        // l
        3, 3, 3, /**/ 3, 3, 3,
        // b
        2, 2, 2, /**/ 2, 2, 2,

        // cross b
        // inner
        3, 3, 3, /**/ 3, 3, 3, /**/ 0, 0, 0, /**/ 1, 1, 1,
        // r
        1, 1, 1, /**/ 1, 1, 1,
        // t
        0, 0, 0, /**/ 0, 0, 0,
        // l
        3, 3, 3, /**/ 3, 3, 3,

        // cross l
        // inner
        0, 0, 0, /**/ 0, 0, 0, /**/ 1, 1, 1, /**/ 2, 2, 1,
        // b
        2, 2, 2, /**/ 2, 2, 2,
        // r
        1, 1, 1, /**/ 1, 1, 1,
        // t
        0, 0, 0, /**/ 0, 0, 0,
    };

    static const uint border_index2_arr[] =
    {
        // cross t
        // inner
        3, 3, 3, /**/ 1, 1, 1, /**/ 2, 2, 2, /**/ 3, 3, 3,
        // l
        3, 3, 3, /**/ 3, 3, 3,
        // b
        2, 2, 2, /**/ 2, 2, 2,
        // r
        1, 1, 1, /**/ 1, 1, 1,

        // cross r
        // inner
        0, 0, 0, /**/ 2, 2, 2, /**/ 3, 3, 3, /**/ 0, 0, 0,
        // t
        0, 0, 0, /**/ 0, 0, 0,
        // l
        3, 3, 3, /**/ 3, 3, 3,
        // b
        2, 2, 2, /**/ 2, 2, 2,

        // cross b
        // inner
        1, 1, 1, /**/ 3, 3, 3, /**/ 0, 0, 0, /**/ 1, 1, 1,
        // r
        1, 1, 1, /**/ 1, 1, 1,
        // t
        0, 0, 0, /**/ 0, 0, 0,
        // l
        3, 3, 3, /**/ 3, 3, 3,

        // cross l
        // inner
        2, 2, 2, /**/ 0, 0, 0, /**/ 1, 1, 1, /**/ 2, 2, 1,
        // b
        2, 2, 2, /**/ 2, 2, 2,
        // r
        1, 1, 1, /**/ 1, 1, 1,
        // t
        0, 0, 0, /**/ 0, 0, 0,
    };

    static const uint flags_arr[] =
    {
        // side t
        // r
        2, 2, 2, /**/ 0, 0, 0, /**/ 0, 0, 0,
        // b,
        0, 0, 0, /**/ 0, 0, 0, /**/ 2, 2, 2, /**/ 2, 2, 2,
        // l
        2, 2, 2, /**/ 0, 0, 0, /**/ 0, 0, 0,

        // side r
        // b
        2, 2, 2, /**/ 0, 0, 0, /**/ 0, 0, 0,
        // l,
        0, 0, 0, /**/ 0, 0, 0, /**/ 2, 2, 2, /**/ 2, 2, 2,
        // t
        2, 2, 2, /**/ 0, 0, 0, /**/ 0, 0, 0,

        // side b
        // l
        2, 2, 2, /**/ 0, 0, 0, /**/ 0, 0, 0,
        // t,
        0, 0, 0, /**/ 0, 0, 0, /**/ 2, 2, 2, /**/ 2, 2, 2,
        // r
        2, 2, 2, /**/ 0, 0, 0, /**/ 0, 0, 0,

        // side l
        // t
        2, 2, 2, /**/ 0, 0, 0, /**/ 0, 0, 0,
        // r,
        0, 0, 0, /**/ 0, 0, 0, /**/ 2, 2, 2, /**/ 2, 2, 2,
        // b
        2, 2, 2, /**/ 0, 0, 0, /**/ 0, 0, 0,


        // cross t
        // inner
        2, 2, 2, /**/ 2, 2, 2, /**/ 2, 2, 2, /**/ 2, 2, 2,
        // l
        0, 0, 0, /**/ 0, 0, 0,
        // b
        0, 0, 0, /**/ 0, 0, 0,
        // r
        0, 0, 0, /**/ 0, 0, 0,

        // cross r
        // inner
        2, 2, 2, /**/ 2, 2, 2, /**/ 2, 2, 2, /**/ 2, 2, 2,
        // t
        0, 0, 0, /**/ 0, 0, 0,
        // l
        0, 0, 0, /**/ 0, 0, 0,
        // b
        0, 0, 0, /**/ 0, 0, 0,

        // cross b
        // inner
        2, 2, 2, /**/ 2, 2, 2, /**/ 2, 2, 2, /**/ 2, 2, 2,
        // r
        0, 0, 0, /**/ 0, 0, 0,
        // t
        0, 0, 0, /**/ 0, 0, 0,
        // l
        0, 0, 0, /**/ 0, 0, 0,

        // cross l
        // inner
        2, 2, 2, /**/ 2, 2, 2, /**/ 2, 2, 2, /**/ 2, 2, 2,
        // b
        0, 0, 0, /**/ 0, 0, 0,
        // r
        0, 0, 0, /**/ 0, 0, 0,
        // t
        0, 0, 0, /**/ 0, 0, 0,
    };

    uint index_offset = side_index * 30 + intersected * 120;
    uint index_offset2 = side_index * 30;

    pos = pos_src[index_arr[input.vid + index_offset]];
    border_index = border_index_arr[input.vid + index_offset];
    border_index2 = border_index2_arr[input.vid + index_offset2];
    AddFlag(flags, (Box_Varying_Flags)flags_arr[input.vid + index_offset]);
    AddFlag(flags, intersected ? Box_Varying_Flags::MixBorder : Box_Varying_Flags::None);
}

void Box_Mesh_FourSide(
    in Box_Attrs input, out float2 pos, out uint border_index, inout Box_Varying_Flags flags,
    float2 size, float b_t, float b_r, float b_b, float b_l
)
{
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

        static const uint index_arr[] =
        {
            // dir_h
            // it
            0, 1, 4, /**/ 4, 1, 5,
            // ir
            5, 1, 3,
            // ib
            3, 2, 5, /**/ 5, 2, 4,
            // il
            4, 2, 0,
            // ot
            0, 6, 1, /**/ 1, 6, 7,
            // or,
            7, 9, 1, /**/ 1, 9, 3,
            // ob,
            3, 9, 2, /**/ 2, 9, 8,
            // ol,
            8, 6, 2, /**/ 2, 6, 0,

            // dir_v
            // it
            4, 0, 1,
            // ir
            1, 3, 4, /**/ 4, 3, 5,
            // ib
            5, 3, 2,
            // il
            2, 0, 5, /**/ 5, 0, 4,
            // ot
            0, 6, 1, /**/ 1, 6, 7,
            // or,
            7, 9, 1, /**/ 1, 9, 3,
            // ob,
            3, 9, 2, /**/ 2, 9, 8,
            // ol,
            8, 6, 2, /**/ 2, 6, 0,
        };
        static const uint border_index_arr[] =
        {
            // dir_h
            // it
            0, 0, 0, /**/ 0, 0, 0,
            // ir
            1, 1, 1,
            // ib,
            2, 2, 2, /**/ 2, 2, 2,
            // il
            3, 3, 3,
            // ot
            0, 0, 0, /**/ 0, 0, 0,
            // or
            1, 1, 1, /**/ 1, 1, 1,
            // ob
            2, 2, 2, /**/ 2, 2, 2,
            // ol
            3, 3, 3, /**/ 3, 3, 3,

            // dir_v
            // it
            0, 0, 0,
            // ir
            1, 1, 1, /**/ 1, 1, 1,
            // ib,
            2, 2, 2,
            // il
            3, 3, 3, /**/ 3, 3, 3,
            // ot
            0, 0, 0, /**/ 0, 0, 0,
            // or
            1, 1, 1, /**/ 1, 1, 1,
            // ob
            2, 2, 2, /**/ 2, 2, 2,
            // ol
            3, 3, 3, /**/ 3, 3, 3,
        };

        pos = pos_src[index_arr[dir_v * 42 + input.vid]];
        border_index = border_index_arr[dir_v * 42 + input.vid];

        AddFlag(flags, input.vid < 18 ? Box_Varying_Flags::IsContent : Box_Varying_Flags::None);
    }
}

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
    float4 border_size = abs(data.BorderSize_TopRightBottomLeft);
    float2 inner_size = abs(size - (border_size.yx + border_size.wz));
    float2 inner_offset = border_size.wx;

    float4 bc_t = data.BorderColor[0];
    float4 bc_r = data.BorderColor[1];
    float4 bc_b = data.BorderColor[2];
    float4 bc_l = data.BorderColor[3];

    Box_Varying_Flags flags = Box_Varying_Flags::None;
    bool4 border_cond = border_size > 0 & float4(bc_t.a, bc_r.a, bc_b.a, bc_l.a) != 0;
    uint4 border_mask = border_cond * 0xFFFFFFFF;
    int border_count = csum(border_cond);

    float b_t = border_size.x;
    float b_r = border_size.y;
    float b_b = border_size.z;
    float b_l = border_size.w;

    float2 pos = 0;
    uint border_index = 0, border_index2 = 0;

    bool is_single_dir = all(border_cond == bool4(true, false, true, false)) || all(
        border_cond == bool4(false, true, false, true));

    AddFlag(flags, border_count > 0 ? Box_Varying_Flags::HasAnyBorder : Box_Varying_Flags::None);

    switch (border_count)
    {
    case 0:
        if (input.vid >= 6) return output;
        Box_Mesh_Rect(
            input, pos, border_index, flags,
            size
        );
        break;
    case 1:
        if (input.vid >= 12) return output;
        Box_Mesh_SingleSide(
            input, pos, border_index, flags,
            size, b_t, b_r, b_b, b_l, border_mask
        );
        break;
    case 2:
        if (is_single_dir)
        {
            if (input.vid >= 24) return output;
            Box_Mesh_DoubleSide(
                input, pos, border_index, flags,
                size, b_t, b_r, b_b, b_l, border_mask,
                inner_size, inner_offset
            );
            break;
        }
        if (input.vid >= 42) return output;
        Box_Mesh_FourSide(
            input, pos, border_index, flags,
            size, b_t, b_r, b_b, b_l
        );
        break;
    case 3:
        if (input.vid >= 30) return output;
        Box_Mesh_ThreeSide(
            input, pos, border_index, flags,
            size, b_t, b_r, b_b, b_l, border_mask,
            border_index2
        );
        break;
    default:
        if (input.vid >= 42) return output;
        Box_Mesh_FourSide(
            input, pos, border_index, flags,
            size, b_t, b_r, b_b, b_l
        );
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
    output.BorderIndex2 = border_index2;
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

    uint2 quadrant2 = uint2(input.UV.x < 0.5f, input.UV.y > 0.5f) * 2 + uint2(1, 0);
    bool same_quadrant1 = any(input.BorderIndex == quadrant2);
    bool same_quadrant2 = any(input.BorderIndex2 == quadrant2) && HasFlag(input.Flags, Box_Varying_Flags::MixBorder);
    bool same_quadrant = same_quadrant1 || same_quadrant2;

    float2 size = data.LeftTopWidthHeight.zw;
    float2 half_size = size * 0.5f;
    float min_half_size = min(size.x, size.y);
    float4 color = data.BackgroundColor;
    float4 border_color = data.BorderColor[same_quadrant1 ? input.BorderIndex : input.BorderIndex2];

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
                    float3 bc = same_quadrant ? border_color.a == 0 ? color.rgb : border_color.rgb : color.rgb;
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
