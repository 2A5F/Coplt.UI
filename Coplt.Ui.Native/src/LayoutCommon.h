#pragma once

#include "Com.h"
#include "Geometry.h"

namespace Coplt::LayoutCalc
{
    using namespace Coplt::Geometry;

    enum class LayoutRunMode : u8
    {
        PerformLayout,
        ComputeSize,
        PerformHiddenLayout,
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

    inline u32 ComputeCacheSlot(
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
    Size<std::optional<f32>> GetKnownSize(const T& inputs)
    {
        return Size{
            .Width = inputs.HasKnownWidth ? std::optional{inputs.KnownWidth} : std::nullopt,
            .Height = inputs.HasKnownHeight ? std::optional{inputs.KnownHeight} : std::nullopt,
        };
    }

    inline Size<f32> GetSize(const LayoutOutput& output)
    {
        return Size{
            .Width = output.Width,
            .Height = output.Height,
        };
    }

    template <CacheEntryLike T>
    Size<std::pair<AvailableSpaceType, f32>> GetAvailableSpace(const T& inputs)
    {
        return Size{
            .Width = std::make_pair(inputs.AvailableSpaceWidth, inputs.AvailableSpaceWidthValue),
            .Height = std::make_pair(inputs.AvailableSpaceHeight, inputs.AvailableSpaceHeightValue),
        };
    }

    inline bool IsRoughlyEqual(const std::pair<AvailableSpaceType, f32> a, const std::pair<AvailableSpaceType, f32> b)
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

    inline LayoutOutput LayoutOutputFromOuterSize(const Size<f32> size)
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
}
