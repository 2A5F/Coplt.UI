#pragma once

#include "Com.h"
#include "Geometry.h"

namespace Coplt::LayoutCalc
{
    using namespace Coplt::Geometry;

    COPLT_RELEASE_FORCE_INLINE inline Axis ToAxis(const WritingDirection direction)
    {
        switch (direction)
        {
        case WritingDirection::Horizontal:
            return Axis::Horizontal;
        case WritingDirection::Vertical:
            return Axis::Vertical;
        }
        std::unreachable();
    }

    enum class LayoutRunMode : u8
    {
        PerformLayout,
        ComputeSize,
        PerformHiddenLayout,
    };

    enum class LayoutSizingMode : u8
    {
        ContentSize,
        InherentSize,
    };

    enum class LayoutRequestedAxis : u8
    {
        Horizontal,
        Vertical,
        Both,
    };

    struct LayoutInputs
    {
        f32 KnownWidth;
        f32 KnownHeight;
        f32 ParentWidth;
        f32 ParentHeight;
        f32 AvailableSpaceWidthValue;
        f32 AvailableSpaceHeightValue;
        bool HasKnownWidth;
        bool HasKnownHeight;
        bool HasParentWidth;
        bool HasParentHeight;
        AvailableSpaceType AvailableSpaceWidth;
        AvailableSpaceType AvailableSpaceHeight;
        LayoutRunMode RunMode;
        LayoutSizingMode SizingMode;
        LayoutRequestedAxis Axis;
    };

    struct CacheEntryBase
    {
        f32 KnownWidth;
        f32 KnownHeight;
        f32 AvailableSpaceWidthValue;
        f32 AvailableSpaceHeightValue;
        bool HasKnownWidth;
        bool HasKnownHeight;
        AvailableSpaceType AvailableSpaceWidth;
        AvailableSpaceType AvailableSpaceHeight;
    };

    COPLT_RELEASE_FORCE_INLINE inline u32 ComputeCacheSlot(
        bool HasKnownWidth,
        bool HasKnownHeight,
        AvailableSpaceType AvailableSpaceWidth,
        AvailableSpaceType AvailableSpaceHeight
    )
    {
        // Slot 0: Both known_dimensions were set
        if (HasKnownWidth && HasKnownHeight) return 0;

        // Slot 1: width but not height known_dimension was set and the other dimension was either a MaxContent or Definite available space constraint
        // Slot 2: width but not height known_dimension was set and the other dimension was a MinContent constraint
        if (HasKnownWidth) return 1 + (AvailableSpaceHeight == AvailableSpaceType::MinContent);

        // Slot 3: height but not width known_dimension was set and the other dimension was either a MaxContent or Definite available space constraint
        // Slot 4: height but not width known_dimension was set and the other dimension was a MinContent constraint
        if (HasKnownHeight) return 3 + (AvailableSpaceWidth == AvailableSpaceType::MinContent);

        // Slots 5-8: Neither known_dimensions were set and:
        // Slot 5: x-axis available space is MaxContent or Definite and y-axis available space is MaxContent or Definite
        if (
            (AvailableSpaceWidth == AvailableSpaceType::MaxContent
                || AvailableSpaceWidth == AvailableSpaceType::Definite)
            && (AvailableSpaceHeight == AvailableSpaceType::MaxContent
                || AvailableSpaceHeight == AvailableSpaceType::Definite)
        )
            return 5;
        // Slot 6: x-axis available space is MaxContent or Definite and y-axis available space is MinContent
        if (
            (AvailableSpaceWidth == AvailableSpaceType::MaxContent
                || AvailableSpaceWidth == AvailableSpaceType::Definite)
            && AvailableSpaceHeight == AvailableSpaceType::MinContent
        )
            return 6;
        // Slot 7: x-axis available space is MinContent and y-axis available space is MaxContent or Definite
        if (
            AvailableSpaceWidth == AvailableSpaceType::MinContent
            && (AvailableSpaceHeight == AvailableSpaceType::MaxContent
                || AvailableSpaceHeight == AvailableSpaceType::Definite)
        )
            return 7;
        // Slot 8: x-axis available space is MinContent and y-axis available space is MinContent
        if (
            AvailableSpaceWidth == AvailableSpaceType::MinContent
            && AvailableSpaceHeight == AvailableSpaceType::MinContent
        )
            return 8;

        std::unreachable();
    }

    template <class T>
    concept CacheEntryLike = requires(T& t)
    {
        { t.KnownWidth } -> std::convertible_to<f32>;
        { t.KnownHeight } -> std::convertible_to<f32>;
        { t.AvailableSpaceWidthValue } -> std::convertible_to<f32>;
        { t.AvailableSpaceHeightValue } -> std::convertible_to<f32>;
        { t.HasKnownWidth } -> std::convertible_to<bool>;
        { t.HasKnownHeight } -> std::convertible_to<bool>;
        { t.AvailableSpaceWidth } -> std::convertible_to<AvailableSpaceType>;
        { t.AvailableSpaceHeight } -> std::convertible_to<AvailableSpaceType>;
    };

    template <CacheEntryLike T>
    COPLT_RELEASE_FORCE_INLINE Size<std::optional<f32>> GetKnownSize(const T& inputs)
    {
        return Size{
            .Width = inputs.HasKnownWidth ? std::optional{inputs.KnownWidth} : std::nullopt,
            .Height = inputs.HasKnownHeight ? std::optional{inputs.KnownHeight} : std::nullopt,
        };
    }

    COPLT_RELEASE_FORCE_INLINE inline Size<f32> GetSize(const LayoutOutput& output)
    {
        return Size{
            .Width = output.Width,
            .Height = output.Height,
        };
    }

    template <CacheEntryLike T>
    COPLT_RELEASE_FORCE_INLINE Size<AvailableSpace> GetAvailableSpace(const T& inputs)
    {
        return Size{
            .Width = std::make_pair(inputs.AvailableSpaceWidth, inputs.AvailableSpaceWidthValue),
            .Height = std::make_pair(inputs.AvailableSpaceHeight, inputs.AvailableSpaceHeightValue),
        };
    }

    COPLT_RELEASE_FORCE_INLINE inline bool IsRoughlyEqual(const AvailableSpace a, const AvailableSpace b)
    {
        if (a.first != b.first) return false;
        switch (a.first)
        {
        case AvailableSpaceType::Definite:
            return std::abs(a.second - b.second) < std::numeric_limits<f32>::epsilon();
        case AvailableSpaceType::MinContent:
        case AvailableSpaceType::MaxContent:
            return true;
        }
        return false;
    }

    COPLT_RELEASE_FORCE_INLINE inline LayoutOutput LayoutOutputFromOuterSize(const Size<f32> size)
    {
        return LayoutOutput{
            .Width = size.Width,
            .Height = size.Height,
            .ContentWidth = 0,
            .ContentHeight = 0,
            .FirstBaselinesX = 0,
            .FirstBaselinesY = 0,
            .TopMargin = 0,
            .BottomMargin = 0,
            .HasFirstBaselinesX = false,
            .HasFirstBaselinesY = false,
            .MarginsCanCollapseThrough = false,
        };
    }

    COPLT_RELEASE_FORCE_INLINE inline Size<std::optional<f32>> GetParentSize(const LayoutInputs& inputs)
    {
        return Size{
            .Width = inputs.HasParentWidth ? std::optional{inputs.ParentWidth} : std::nullopt,
            .Height = inputs.HasParentHeight ? std::optional{inputs.ParentHeight} : std::nullopt,
        };
    }

    COPLT_RELEASE_FORCE_INLINE inline Rect<Length> GetPadding(const StyleData& style)
    {
        return Rect{
            .Top = std::make_pair(style.PaddingTop, style.PaddingTopValue),
            .Right = std::make_pair(style.PaddingRight, style.PaddingRightValue),
            .Bottom = std::make_pair(style.PaddingBottom, style.PaddingBottomValue),
            .Left = std::make_pair(style.PaddingLeft, style.PaddingLeftValue),
        };
    }

    COPLT_RELEASE_FORCE_INLINE inline Rect<Length> GetMargin(const StyleData& style)
    {
        return Rect{
            .Top = std::make_pair(style.MarginTop, style.MarginTopValue),
            .Right = std::make_pair(style.MarginRight, style.MarginRightValue),
            .Bottom = std::make_pair(style.MarginBottom, style.MarginBottomValue),
            .Left = std::make_pair(style.MarginLeft, style.MarginLeftValue),
        };
    }

    COPLT_RELEASE_FORCE_INLINE inline Rect<Length> GetBorder(const StyleData& style)
    {
        return Rect{
            .Top = std::make_pair(style.BorderTop, style.BorderTopValue),
            .Right = std::make_pair(style.BorderRight, style.BorderRightValue),
            .Bottom = std::make_pair(style.BorderBottom, style.BorderBottomValue),
            .Left = std::make_pair(style.BorderLeft, style.BorderLeftValue),
        };
    }

    COPLT_RELEASE_FORCE_INLINE inline Size<Length> GetSize(const StyleData& style)
    {
        return Size{
            .Width = std::make_pair(style.Width, style.WidthValue),
            .Height = std::make_pair(style.Height, style.HeightValue),
        };
    }

    COPLT_RELEASE_FORCE_INLINE inline Size<Length> GetMinSize(const StyleData& style)
    {
        return Size{
            .Width = std::make_pair(style.MinWidth, style.MinWidthValue),
            .Height = std::make_pair(style.MinHeight, style.MinHeightValue),
        };
    }

    COPLT_RELEASE_FORCE_INLINE inline Size<Length> GetMaxSize(const StyleData& style)
    {
        return Size{
            .Width = std::make_pair(style.MaxWidth, style.MaxWidthValue),
            .Height = std::make_pair(style.MaxHeight, style.MaxHeightValue),
        };
    }

    COPLT_RELEASE_FORCE_INLINE inline std::optional<f32> GetAspectRatio(const StyleData& style)
    {
        return style.HasAspectRatio ? std::optional(style.AspectRatioValue) : std::nullopt;
    }

    COPLT_RELEASE_FORCE_INLINE inline Length GetLineHeight(const StyleData& style)
    {
        return std::make_pair(style.LineHeight, style.LineHeightValue);
    }

    struct ParagraphLineInfo
    {
        f32 Ascent;
        f32 Descent;
        f32 LineGap;
    };

    enum class ParagraphSpanType : u8
    {
        Common,
        Space,
        NewLine,
    };

    inline const char16* ToStr16Pad(const ParagraphSpanType type)
    {
        switch (type)
        {
        case ParagraphSpanType::Common:
            return COPLT_STR16("Common ");
        case ParagraphSpanType::Space:
            return COPLT_STR16("Space  ");
        case ParagraphSpanType::NewLine:
            return COPLT_STR16("NewLine");
        }
        std::unreachable();
    }

    struct ParagraphSpan
    {
        u32 NthLine;
        u32 CharStart;
        u32 CharLength;
        u32 GlyphStart;
        u32 GlyphLength;
        // Horizontal is x, Vertical is y
        f32 Offset;
        // Horizontal is width, Vertical is height
        f32 Size;
        ParagraphSpanType Type;
        bool NeedReShape;
    };

    struct ParagraphLine
    {
        ParagraphLineInfo Info{};

        // Horizontal is x, Vertical is y
        f32 AlignOffset{};
        // Horizontal is y, Vertical is x
        f32 LineOffset{};
        // Horizontal is width, Vertical is height
        f32 LineSize{};
        // Horizontal is height, Vertical is width
        f32 LineHeight{};

        std::vector<ParagraphSpan> Spans{}; // todo flat storage in ParagraphData
    };
}
