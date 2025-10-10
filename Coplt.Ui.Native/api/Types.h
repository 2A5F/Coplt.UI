#pragma once
#ifndef COPLT_UI_TYPES_H
#define COPLT_UI_TYPES_H

#include "CoCom.h"

namespace Coplt {

    struct Str16;

    struct Str8;

    struct FontFamilyNameInfo;

    struct LayoutCache;

    struct LayoutCacheEntryLayoutOutput;

    struct LayoutCacheEntrySize;

    struct LayoutCollapsibleMarginSet;

    struct LayoutData;

    struct LayoutOutput;

    struct NFontInfo;

    struct NFontPair;

    struct UiNodeData;

    struct FontWidth;

    struct StyleData;

    struct FontMetrics;

    struct IFont;

    struct IFontCollection;

    struct IFontFace;

    struct IFontFamily;

    struct ILib;

    struct IStub;

    enum class AvailableSpaceType : ::Coplt::i32
    {
        Definite = 0,
        MinContent = 1,
        MaxContent = 2,
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

    enum class AlignType : ::Coplt::u8
    {
        None = 0,
        Start = 1,
        End = 2,
        FlexStart = 3,
        FlexEnd = 4,
        Center = 5,
        Baseline = 6,
        Stretch = 7,
        SpaceBetween = 8,
        SpaceEvenly = 9,
        SpaceAround = 10,
    };

    enum class BorderRadiusMode : ::Coplt::u32
    {
        Circle = 0,
        Parabola = 1,
        Cosine = 2,
        Cubic = 3,
    };

    enum class BoxSizing : ::Coplt::u8
    {
        BorderBox = 0,
        ContentBox = 1,
    };

    enum class CursorType : ::Coplt::u8
    {
        Default = 0,
        Pointer = 1,
        ContextMenu = 2,
        Help = 3,
        Progress = 4,
        Wait = 5,
        Cell = 6,
        Crosshair = 7,
        Text = 8,
        VerticalText = 9,
        Alias = 10,
        Copy = 11,
        Move = 12,
        NoDrop = 13,
        NotAllowed = 14,
        Grab = 15,
        Grabbing = 16,
        AllScroll = 17,
        ColResize = 18,
        RowResize = 19,
        NResize = 20,
        EResize = 21,
        SResize = 22,
        WResize = 23,
        NeResize = 24,
        NwResize = 25,
        SeResize = 26,
        SwResize = 27,
        EwResize = 28,
        NsResize = 29,
        NeSwResize = 30,
        NwSeResize = 31,
        ZoomIn = 32,
        ZoomOut = 33,
    };

    enum class Display : ::Coplt::u8
    {
        Flex = 0,
        Grid = 1,
        Block = 2,
        Inline = 3,
        None = 4,
    };

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

    enum class LengthType : ::Coplt::u8
    {
        Fixed = 0,
        Percent = 1,
        Auto = 2,
    };

    enum class Overflow : ::Coplt::u8
    {
        Visible = 0,
        Clip = 1,
        Hidden = 2,
        Scroll = 3,
    };

    enum class PointerEvents : ::Coplt::u8
    {
        Auto = 0,
        None = 1,
    };

    enum class Position : ::Coplt::u8
    {
        Relative = 0,
        Absolute = 1,
    };

    enum class SamplerType : ::Coplt::u32
    {
        LinearClamp = 0,
        LinearWrap = 1,
        PointClamp = 2,
        PointWrap = 3,
    };

    enum class TextAlign : ::Coplt::u8
    {
        Auto = 0,
        Left = 1,
        Right = 2,
        Center = 3,
    };

    COPLT_ENUM_FLAGS(FontFlags, ::Coplt::i32)
    {
        None = 0,
        Color = 1,
        Monospaced = 2,
    };

    struct LayoutCollapsibleMarginSet
    {
        ::Coplt::f32 Positive;
        ::Coplt::f32 Negative;
    };

    struct LayoutOutput
    {
        ::Coplt::f32 Width;
        ::Coplt::f32 Height;
        ::Coplt::f32 ContentWidth;
        ::Coplt::f32 ContentHeight;
        ::Coplt::f32 FirstBaselinesX;
        ::Coplt::f32 FirstBaselinesY;
        ::Coplt::LayoutCollapsibleMarginSet TopMargin;
        ::Coplt::LayoutCollapsibleMarginSet BottomMargin;
        bool HasFirstBaselinesX;
        bool HasFirstBaselinesY;
        bool MarginsCanCollapseThrough;
    };

    struct LayoutCacheEntryLayoutOutput
    {
        ::Coplt::f32 KnownDimensionsWidthValue;
        ::Coplt::f32 KnownDimensionsHeightValue;
        ::Coplt::f32 AvailableSpaceWidthValue;
        ::Coplt::f32 AvailableSpaceHeightValue;
        bool HasKnownDimensionsWidth;
        bool HasKnownDimensionsHeight;
        ::Coplt::AvailableSpaceType AvailableSpaceWidth;
        ::Coplt::AvailableSpaceType AvailableSpaceHeight;
        ::Coplt::LayoutOutput Content;
    };

    struct LayoutCacheEntrySize
    {
        ::Coplt::f32 KnownDimensionsWidthValue;
        ::Coplt::f32 KnownDimensionsHeightValue;
        ::Coplt::f32 AvailableSpaceWidthValue;
        ::Coplt::f32 AvailableSpaceHeightValue;
        bool HasKnownDimensionsWidth;
        bool HasKnownDimensionsHeight;
        ::Coplt::AvailableSpaceType AvailableSpaceWidth;
        ::Coplt::AvailableSpaceType AvailableSpaceHeight;
        ::Coplt::f32 ContentWidth;
        ::Coplt::f32 ContentHeight;
    };

    struct Str16
    {
        ::Coplt::char16 const* Data;
        ::Coplt::u32 Size;
    };

    struct LayoutCache
    {
        ::Coplt::LayoutCacheEntryLayoutOutput FinalLayoutEntry;
        ::Coplt::LayoutCacheEntrySize MeasureEntries0;
        ::Coplt::LayoutCacheEntrySize MeasureEntries1;
        ::Coplt::LayoutCacheEntrySize MeasureEntries2;
        ::Coplt::LayoutCacheEntrySize MeasureEntries3;
        ::Coplt::LayoutCacheEntrySize MeasureEntries4;
        ::Coplt::LayoutCacheEntrySize MeasureEntries5;
        ::Coplt::LayoutCacheEntrySize MeasureEntries6;
        ::Coplt::LayoutCacheEntrySize MeasureEntries7;
        ::Coplt::LayoutCacheEntrySize MeasureEntries8;
        bool HasFinalLayoutEntry;
        bool HasMeasureEntries0;
        bool HasMeasureEntries1;
        bool HasMeasureEntries2;
        bool HasMeasureEntries3;
        bool HasMeasureEntries4;
        bool HasMeasureEntries5;
        bool HasMeasureEntries6;
        bool HasMeasureEntries7;
        bool HasMeasureEntries8;
        bool IsEmpty;
    };

    struct LayoutData
    {
        ::Coplt::u32 Order;
        ::Coplt::f32 LocationX;
        ::Coplt::f32 LocationY;
        ::Coplt::f32 Width;
        ::Coplt::f32 Height;
        ::Coplt::f32 ContentWidth;
        ::Coplt::f32 ContentHeight;
        ::Coplt::f32 ScrollXSize;
        ::Coplt::f32 ScrollYSize;
        ::Coplt::f32 BorderTopSize;
        ::Coplt::f32 BorderRightSize;
        ::Coplt::f32 BorderBottomSize;
        ::Coplt::f32 BorderLeftSize;
        ::Coplt::f32 PaddingTopSize;
        ::Coplt::f32 PaddingRightSize;
        ::Coplt::f32 PaddingBottomSize;
        ::Coplt::f32 PaddingLeftSize;
        ::Coplt::f32 MarginTopSize;
        ::Coplt::f32 MarginRightSize;
        ::Coplt::f32 MarginBottomSize;
        ::Coplt::f32 MarginLeftSize;
    };

    struct FontWidth
    {
        ::Coplt::f32 Width;
    };

    struct StyleData
    {
        void* Image;
        ::Coplt::i32 ZIndex;
        ::Coplt::f32 Opacity;
        ::Coplt::f32 ColorR;
        ::Coplt::f32 ColorG;
        ::Coplt::f32 ColorB;
        ::Coplt::f32 ColorA;
        ::Coplt::f32 ImageTintR;
        ::Coplt::f32 ImageTintG;
        ::Coplt::f32 ImageTintB;
        ::Coplt::f32 ImageTintA;
        ::Coplt::f32 ScrollbarWidth;
        ::Coplt::f32 InsertTopValue;
        ::Coplt::f32 InsertRightValue;
        ::Coplt::f32 InsertBottomValue;
        ::Coplt::f32 InsertLeftValue;
        ::Coplt::f32 WidthValue;
        ::Coplt::f32 HeightValue;
        ::Coplt::f32 MinWidthValue;
        ::Coplt::f32 MinHeightValue;
        ::Coplt::f32 MaxWidthValue;
        ::Coplt::f32 MaxHeightValue;
        ::Coplt::f32 AspectRatioValue;
        ::Coplt::f32 MarginTopValue;
        ::Coplt::f32 MarginRightValue;
        ::Coplt::f32 MarginBottomValue;
        ::Coplt::f32 MarginLeftValue;
        ::Coplt::f32 PaddingTopValue;
        ::Coplt::f32 PaddingRightValue;
        ::Coplt::f32 PaddingBottomValue;
        ::Coplt::f32 PaddingLeftValue;
        ::Coplt::f32 BorderTopValue;
        ::Coplt::f32 BorderRightValue;
        ::Coplt::f32 BorderBottomValue;
        ::Coplt::f32 BorderLeftValue;
        ::Coplt::f32 GapXValue;
        ::Coplt::f32 GapYValue;
        ::Coplt::f32 TextColorR;
        ::Coplt::f32 TextColorG;
        ::Coplt::f32 TextColorB;
        ::Coplt::f32 TextColorA;
        ::Coplt::f32 TextSizeValue;
        ::Coplt::BorderRadiusMode BorderMode;
        ::Coplt::SamplerType BackgroundSampler;
        ::Coplt::Display Display;
        ::Coplt::BoxSizing BoxSizing;
        ::Coplt::Overflow OverflowX;
        ::Coplt::Overflow OverflowY;
        ::Coplt::Position Position;
        ::Coplt::LengthType InsertTop;
        ::Coplt::LengthType InsertRight;
        ::Coplt::LengthType InsertBottomV;
        ::Coplt::LengthType InsertLeft;
        ::Coplt::LengthType Width;
        ::Coplt::LengthType Height;
        ::Coplt::LengthType MinWidth;
        ::Coplt::LengthType MinHeight;
        ::Coplt::LengthType MaxMinWidth;
        ::Coplt::LengthType MaxMinHeight;
        bool HasAspectRatio;
        ::Coplt::LengthType MarginTop;
        ::Coplt::LengthType MarginRight;
        ::Coplt::LengthType MarginBottomV;
        ::Coplt::LengthType MarginLeft;
        ::Coplt::LengthType PaddingTop;
        ::Coplt::LengthType PaddingRight;
        ::Coplt::LengthType PaddingBottomV;
        ::Coplt::LengthType PaddingLeft;
        ::Coplt::LengthType BorderTop;
        ::Coplt::LengthType BorderRight;
        ::Coplt::LengthType BorderBottomV;
        ::Coplt::LengthType BorderLeft;
        ::Coplt::AlignType AlignItems;
        ::Coplt::AlignType AlignSelf;
        ::Coplt::AlignType JustifyItems;
        ::Coplt::AlignType JustifySelf;
        ::Coplt::AlignType AlignContent;
        ::Coplt::AlignType JustifyContent;
        ::Coplt::LengthType GapX;
        ::Coplt::LengthType GapY;
        ::Coplt::TextAlign TextAlign;
        ::Coplt::LengthType TextSize;
        ::Coplt::CursorType Cursor;
        ::Coplt::PointerEvents PointerEvents;
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
        ::Coplt::u8 const* Data;
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

    struct UiNodeData
    {
        void* Object;
        ::Coplt::StyleData Style;
        ::Coplt::LayoutData Layout;
        ::Coplt::LayoutData FinalLayout;
        ::Coplt::LayoutCache LayoutCache;
    };

} // namespace Coplt

#endif //COPLT_UI_TYPES_H
