#pragma once
#ifndef __COMMON_HLSL__
#define __COMMON_HLSL__

enum class SamplerType : uint
{
    LinearClamp,
    LinearWrap,
    PointClamp,
    PointWrap,
};

Texture2D Images[] : register(t0, space1);
SamplerState Samplers[] : register(s0, space2);

template <typename Enum>
void AddFlag(inout Enum value, Enum flag)
{
    value = (Enum)((uint)value | (uint)flag);
}

template <typename Enum>
bool HasFlag(Enum value, Enum flag)
{
    return ((uint)value & (uint)flag) == (uint)flag;
}

float cross(float2 a, float2 b)
{
    return a.x * b.y - a.y * b.x;
}

struct Line2d
{
    float2 start;
    float2 end;
};

Line2d line2d(float2 start, float2 end)
{
    Line2d self;
    self.start = start;
    self.end = end;
    return self;
}

struct Ray2d
{
    float2 origin;
    float2 direction;
};

Ray2d ray2d(float2 origin, float2 direction)
{
    Ray2d self;
    self.origin = origin;
    self.direction = direction;
    return self;
}

Ray2d ray2d_to(float2 origin, float2 to)
{
    Ray2d self;
    self.origin = origin;
    self.direction = normalize(to - origin);
    return self;
}

bool intersect(Ray2d a, Ray2d b, out float2 hit)
{
    float d = cross(a.direction, b.direction);

    float2 diff = b.origin - a.origin;
    float t = cross(diff, b.direction) / d;

    hit = a.origin + t * a.direction;

    return abs(d) >= 1e-8;
}

#endif
