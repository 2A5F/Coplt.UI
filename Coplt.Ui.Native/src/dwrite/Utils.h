#pragma once

#include <dwrite_3.h>
#include <string>
#include <fmt/xchar.h>

#include "../Com.h"
#include "Error.h"

namespace Coplt
{
    inline DWRITE_FONT_WEIGHT to_dwrite(FontWeight value)
    {
        switch (value)
        {
        case FontWeight::None:
            break;
        case FontWeight::Thin:
            return DWRITE_FONT_WEIGHT_THIN;
        case FontWeight::ExtraLight:
            return DWRITE_FONT_WEIGHT_EXTRA_LIGHT;
        case FontWeight::Light:
            return DWRITE_FONT_WEIGHT_LIGHT;
        case FontWeight::SemiLight:
            return DWRITE_FONT_WEIGHT_SEMI_LIGHT;
        case FontWeight::Normal:
            return DWRITE_FONT_WEIGHT_NORMAL;
        case FontWeight::Medium:
            return DWRITE_FONT_WEIGHT_MEDIUM;
        case FontWeight::SemiBold:
            return DWRITE_FONT_WEIGHT_SEMI_BOLD;
        case FontWeight::Bold:
            return DWRITE_FONT_WEIGHT_BOLD;
        case FontWeight::ExtraBold:
            return DWRITE_FONT_WEIGHT_EXTRA_BOLD;
        case FontWeight::Black:
            return DWRITE_FONT_WEIGHT_BLACK;
        case FontWeight::ExtraBlack:
            return DWRITE_FONT_WEIGHT_EXTRA_BLACK;
        }
        return DWRITE_FONT_WEIGHT_NORMAL;
    }

    std::wstring GetFontFamilyName(const Rc<IDWriteFontFamily>& family);

    std::wstring GetFontFaceName(const Rc<IDWriteFont>& font);

    std::wstring GetFontFamilyName(const Rc<IDWriteFont>& font);

    std::wstring GetFontName(const Rc<IDWriteFont>& font);

    std::wstring GetFamilyNames(const Rc<IDWriteFontFace5>& font);

    std::wstring GetFaceNames(const Rc<IDWriteFontFace5>& font);
}
