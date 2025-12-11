#pragma once

#include "List.h"

namespace Coplt
{
    void SplitTexts(List<TextRange>& out, const char16* str, i32 len);

    namespace UnicodeUtils
    {
        char16 const* LikelyLocale(const UScriptCode script);
    }
}
