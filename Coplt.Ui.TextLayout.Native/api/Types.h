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

    struct Str16;

    struct Str8;

    struct FontFamilyNameInfo;

    struct Str16
    {
        ::Coplt::char16* Data;
        ::Coplt::u32 Size;
    };

    struct Str8
    {
        ::Coplt::u8* Data;
        ::Coplt::u32 Size;
    };

    struct FontFamilyNameInfo
    {
        ::Coplt::Str16 Name;
        ::Coplt::u32 Local;
    };

} // namespace Coplt

#endif //COPLT_UL_TEXT_LAYOUT_TYPES_H
