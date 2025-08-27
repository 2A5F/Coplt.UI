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

#endif
