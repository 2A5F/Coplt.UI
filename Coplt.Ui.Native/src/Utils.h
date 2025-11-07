#pragma once

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
}
