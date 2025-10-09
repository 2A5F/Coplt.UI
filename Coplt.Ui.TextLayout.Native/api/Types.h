#pragma once
#ifndef COPLT_UL_TEXT_LAYOUT_TYPES_H
#define COPLT_UL_TEXT_LAYOUT_TYPES_H

#include "CoCom.h"

namespace Coplt {

    struct Str16;

    struct Str8;

    struct FontFamilyNameInfo;

    struct NFontInfo;

    struct NFontPair;

    struct FontWidth;

    struct FontMetrics;

    struct IFont;

    struct IFontCollection;

    struct IFontFace;

    struct IFontFamily;

    struct ILibTextLayout;

    enum class FontStyle : ::Coplt::u8
    {
        Normal = 0,
        Italic = 1,
        Oblique = 2,
    };

    enum class FontWeight : ::Coplt::i32
    {
        None = 0,
        Thin = 100,
        ExtraLight = 200,
        Light = 300,
        SemiLight = 350,
        Normal = 400,
        Medium = 500,
        SemiBold = 600,
        Bold = 700,
        ExtraBold = 800,
        Black = 900,
        ExtraBlack = 950,
    };

    COPLT_ENUM_FLAGS(FontFlags, ::Coplt::i32)
    {
        None = 0,
        Color = 1,
        Monospaced = 2,
    };

    enum class LogLevel : ::Coplt::u8
    {
        Fatal = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4,
        Verbose = 5,
    };

    struct Str16
    {
        ::Coplt::char16* Data;
        ::Coplt::u32 Size;
    };

    struct FontWidth
    {
        ::Coplt::f32 Width;
    };

    struct FontMetrics
    {
        ::Coplt::f32 Ascent;
        ::Coplt::f32 Descent;
        ::Coplt::f32 Leading;
        ::Coplt::f32 LineHeight;
        ::Coplt::u16 UnitsPerEm;
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

    struct NFontInfo
    {
        ::Coplt::FontMetrics Metrics;
        ::Coplt::FontWidth Width;
        ::Coplt::FontWeight Weight;
        ::Coplt::FontStyle Style;
        ::Coplt::FontFlags Flags;
    };

    struct NFontPair
    {
        IFont* Font;
        ::Coplt::NFontInfo* Info;
    };

} // namespace Coplt

#endif //COPLT_UL_TEXT_LAYOUT_TYPES_H
