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

    inline DWRITE_FONT_STYLE to_dwrite(FontStyle value)
    {
        switch (value)
        {
        case FontStyle::Normal:
            return DWRITE_FONT_STYLE_NORMAL;
        case FontStyle::Italic:
            return DWRITE_FONT_STYLE_ITALIC;
        case FontStyle::Oblique:
            return DWRITE_FONT_STYLE_OBLIQUE;
        }
        return DWRITE_FONT_STYLE_NORMAL;
    }

    inline DWRITE_FONT_STRETCH to_dwrite(FontStretch value)
    {
        switch (value)
        {
        case FontStretch::Undefined:
            return DWRITE_FONT_STRETCH_UNDEFINED;
        case FontStretch::UltraCondensed:
            return DWRITE_FONT_STRETCH_ULTRA_CONDENSED;
        case FontStretch::ExtraCondensed:
            return DWRITE_FONT_STRETCH_EXTRA_CONDENSED;
        case FontStretch::Condensed:
            return DWRITE_FONT_STRETCH_CONDENSED;
        case FontStretch::SemiCondensed:
            return DWRITE_FONT_STRETCH_SEMI_CONDENSED;
        case FontStretch::Normal:
            return DWRITE_FONT_STRETCH_NORMAL;
        case FontStretch::SemiExpanded:
            return DWRITE_FONT_STRETCH_SEMI_EXPANDED;
        case FontStretch::Expanded:
            return DWRITE_FONT_STRETCH_EXPANDED;
        case FontStretch::ExtraExpanded:
            return DWRITE_FONT_STRETCH_EXTRA_EXPANDED;
        case FontStretch::UltraExpanded:
            return DWRITE_FONT_STRETCH_ULTRA_EXPANDED;
        }
        return DWRITE_FONT_STRETCH_NORMAL;
    }

    inline std::wstring GetFontFamilyName(const Rc<IDWriteFontFamily>& family)
    {
        Rc<IDWriteLocalizedStrings> strings{};
        if (const auto hr = family->GetFamilyNames(strings.put()); FAILED(hr))
            throw ComException(hr, "Failed to get family names");

        std::wstring name{};
        const auto count = strings->GetCount();
        for (int i = 0; i < count; i++)
        {
            u32 len{};
            if (const auto hr = strings->GetLocaleNameLength(i, &len); FAILED(hr))
                throw ComException(hr, "Failed to get locale name length");
            std::wstring locale(len + 1, 0);
            if (const auto hr = strings->GetLocaleName(i, locale.data(), locale.size()); FAILED(hr))
                throw ComException(hr, "Failed to get locale name");

            if (const auto hr = strings->GetStringLength(i, &len); FAILED(hr))
                throw ComException(hr, "Failed to get string length");
            std::wstring str(len + 1, 0);
            if (const auto hr = strings->GetString(i, str.data(), str.size()); FAILED(hr))
                throw ComException(hr, "Failed to get string");

            if (i != 0) name.append(L", ");
            name.append(fmt::format(L"{}: {}", locale.c_str(), str.c_str()));
        }

        return name;
    }

    inline std::wstring GetFontFaceName(const Rc<IDWriteFont>& font)
    {
        Rc<IDWriteLocalizedStrings> strings{};
        if (const auto hr = font->GetFaceNames(strings.put()); FAILED(hr))
            throw ComException(hr, "Failed to get family names");

        std::wstring name{};
        const auto count = strings->GetCount();
        for (int i = 0; i < count; i++)
        {
            u32 len{};
            if (const auto hr = strings->GetLocaleNameLength(i, &len); FAILED(hr))
                throw ComException(hr, "Failed to get locale name length");
            std::wstring locale(len + 1, 0);
            if (const auto hr = strings->GetLocaleName(i, locale.data(), locale.size()); FAILED(hr))
                throw ComException(hr, "Failed to get locale name");

            if (const auto hr = strings->GetStringLength(i, &len); FAILED(hr))
                throw ComException(hr, "Failed to get string length");
            std::wstring str(len + 1, 0);
            if (const auto hr = strings->GetString(i, str.data(), str.size()); FAILED(hr))
                throw ComException(hr, "Failed to get string");

            if (i != 0) name.append(L", ");
            name.append(fmt::format(L"{}: {}", locale.c_str(), str.c_str()));
        }

        return name;
    }

    inline std::wstring GetFontFamilyName(const Rc<IDWriteFont>& font)
    {
        Rc<IDWriteFontFamily> family{};
        if (const auto hr = font->GetFontFamily(family.put()); FAILED(hr))
            throw ComException(hr, "Failed to get font family");

        return GetFontFamilyName(family);
    }

    inline std::wstring GetFontName(const Rc<IDWriteFont>& font)
    {
        std::wstring name{};

        name.append(L"[");
        name.append(GetFontFamilyName(font));
        name.append(L"] => [");
        name.append(GetFontFaceName(font));
        name.append(L"]");

        return name;
    }
}
