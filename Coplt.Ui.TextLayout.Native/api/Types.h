#pragma once
#ifndef COPLT_UL_TEXT_LAYOUT_TYPES_H
#define COPLT_UL_TEXT_LAYOUT_TYPES_H

#include "CoCom.h"

namespace Coplt {

    enum class LogLevel : ::Coplt::u8
    {
        Fatal = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4,
        Verbose = 5,
    };

} // namespace Coplt

#endif //COPLT_UL_TEXT_LAYOUT_TYPES_H
