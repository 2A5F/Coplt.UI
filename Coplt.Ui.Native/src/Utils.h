#pragma once

#include <vector>

#include "Com.h"

namespace Coplt
{
    inline bool IsZeroLength(LengthType type, f32 value)
    {
        switch (type)
        {
        case LengthType::Fixed:
        case LengthType::Percent:
            return value == 0;
        case LengthType::Auto:
            return true;
        }
        return true;
    }

    inline void HashCombine(std::size_t& seed, std::size_t value)
    {
        seed ^= value + 0x9e3779b9 + (seed << 6) + (seed >> 2);
    }

    template <typename... Args>
    std::size_t HashValues(const Args&... args)
    {
        std::size_t seed = 0;
        (HashCombine(seed, std::hash<Args>{}(args)), ...);
        return seed;
    }
}
