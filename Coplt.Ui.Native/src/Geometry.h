#pragma once

#include <algorithm>
#include <optional>
#include <utility>

#include "Com.h"

namespace Coplt::Geometry
{
    using AvailableSpace = std::pair<AvailableSpaceType, f32>;
    using Length = std::pair<LengthType, f32>;

    template <class T, class U>
    concept HasMemberTryResolve = requires(const T& a, const U& b)
    {
        { a.TryResolve(b) };
    };

    template <class T, class U>
    concept HasMemberResolveOrZero = requires(const T& a, const U& b)
    {
        { a.ResolveOrZero(b) };
    };

    template <class T, class U> requires HasMemberResolveOrZero<T, U>
    COPLT_RELEASE_FORCE_INLINE auto TryResolve(const T& a, const U& b)
    {
        return a.TryResolve(b);
    }

    template <class T, class U> requires HasMemberResolveOrZero<T, U>
    COPLT_RELEASE_FORCE_INLINE auto ResolveOrZero(const T& a, const U& b)
    {
        return a.ResolveOrZero(b);
    }

    COPLT_RELEASE_FORCE_INLINE inline std::optional<f32> TryResolve(const Length& length, const std::optional<f32>& ctx)
    {
        switch (length.first)
        {
        case LengthType::Fixed:
            return length.second;
        case LengthType::Percent:
            return ctx.has_value() ? std::optional{ctx.value() * length.second} : std::nullopt;
        case LengthType::Auto:
            return std::nullopt;
        }
        std::unreachable();
    }

    COPLT_RELEASE_FORCE_INLINE inline f32 ResolveOrZero(const Length& length, const std::optional<f32>& ctx)
    {
        switch (length.first)
        {
        case LengthType::Fixed:
            return length.second;
        case LengthType::Percent:
            return ctx.has_value() ? ctx.value() * length.second : 0;
        case LengthType::Auto:
            return 0;
        }
        std::unreachable();
    }

    COPLT_RELEASE_FORCE_INLINE inline f32 Resolve(const Length& length, const f32 ctx)
    {
        switch (length.first)
        {
        case LengthType::Fixed:
            return length.second;
        case LengthType::Percent:
            return ctx * length.second;
        case LengthType::Auto:
            return ctx;
        }
        std::unreachable();
    }

    template <class T, class U>
    concept HasResolveOrZero = requires(const T& a, const U& b)
    {
        { ResolveOrZero(a, b) };
    };

    template <class T, class U>
    concept HasTryResolve = requires(const T& a, const U& b)
    {
        { TryResolve(a, b) };
    };

    COPLT_RELEASE_FORCE_INLINE inline f32 TryMin(const std::optional<f32> value, const f32 other)
    {
        if (value.has_value())
            return std::min(value.value(), other);
        return other;
    }

    COPLT_RELEASE_FORCE_INLINE inline f32 TryMax(const std::optional<f32> value, const f32 other)
    {
        if (value.has_value())
            return std::max(value.value(), other);
        return other;
    }

    COPLT_RELEASE_FORCE_INLINE inline std::optional<f32> TryMin(
        const std::optional<f32> value,
        const std::optional<f32> other
    )
    {
        if (value.has_value() && other.has_value())
            return std::min(value.value(), other.value());
        if (value.has_value()) return value;
        if (other.has_value()) return other;
        return std::nullopt;
    }

    COPLT_RELEASE_FORCE_INLINE inline std::optional<f32> TryMax(
        const std::optional<f32> value,
        const std::optional<f32> other
    )
    {
        if (value.has_value() && other.has_value())
            return std::max(value.value(), other.value());
        if (value.has_value()) return value;
        if (other.has_value()) return other;
        return std::nullopt;
    }

    COPLT_RELEASE_FORCE_INLINE inline std::optional<f32> TryClamp(
        const std::optional<f32> value, const std::optional<f32> min, const std::optional<f32> max
    )
    {
        if (value.has_value() && min.has_value() && max.has_value())
            return std::clamp(value.value(), min.value(), max.value());
        if (value.has_value() && !min.has_value() && max.has_value())
            return std::min(value.value(), max.value());
        if (value.has_value() && min.has_value() && !max.has_value())
            return std::max(value.value(), min.value());
        return value;
    }

    COPLT_RELEASE_FORCE_INLINE inline std::optional<f32> Or(
        const AvailableSpace value, const std::optional<f32> other
    )
    {
        switch (value.first)
        {
        case AvailableSpaceType::Definite:
            return value.second;
            break;
        case AvailableSpaceType::MinContent:
            if (other.has_value()) return other;
            return 0;
        case AvailableSpaceType::MaxContent:
            if (other.has_value()) return other;
            return std::nullopt;
        }
        std::unreachable();
    }

    enum class Axis : u8
    {
        Horizontal,
        Vertical,
    };

    template <class T>
    struct Point
    {
        T X;
        T Y;
    };

    template <class T>
    struct Line
    {
        T Start;
        T End;

        COPLT_RELEASE_FORCE_INLINE T Sum()
        {
            return Start + End;
        }
    };

    template <class T>
    struct Size
    {
        T Width;
        T Height;

        COPLT_RELEASE_FORCE_INLINE T& MainAxis(const Axis axis)
        {
            switch (axis)
            {
            case Axis::Horizontal:
                return Width;
            case Axis::Vertical:
                return Height;
            }
            std::unreachable();
        }

        COPLT_RELEASE_FORCE_INLINE T& CrossAxis(const Axis axis)
        {
            switch (axis)
            {
            case Axis::Horizontal:
                return Height;
            case Axis::Vertical:
                return Width;
            }
            std::unreachable();
        }

        COPLT_RELEASE_FORCE_INLINE const T& MainAxis(const Axis axis) const
        {
            switch (axis)
            {
            case Axis::Horizontal:
                return Width;
            case Axis::Vertical:
                return Height;
            }
            std::unreachable();
        }

        COPLT_RELEASE_FORCE_INLINE const T& CrossAxis(const Axis axis) const
        {
            switch (axis)
            {
            case Axis::Horizontal:
                return Height;
            case Axis::Vertical:
                return Width;
            }
            std::unreachable();
        }

        COPLT_RELEASE_FORCE_INLINE Size operator+(const Size b) const
        {
            return {Width + b.Width, Height + b.Height};
        }

        COPLT_RELEASE_FORCE_INLINE Size operator-(const Size b) const
        {
            return {Width - b.Width, Height - b.Height};
        }

        template <class U>
        COPLT_RELEASE_FORCE_INLINE auto TryResolve(Size<U> ctx) const requires HasTryResolve<T, U>
        {
            using Output = decltype(Geometry::TryResolve(Width, ctx.Width));
            return Size<Output>{
                .Width = Geometry::TryResolve(Width, ctx.Width),
                .Height = Geometry::TryResolve(Height, ctx.Height),
            };
        }

        template <class U>
        COPLT_RELEASE_FORCE_INLINE auto ResolveOrZero(Size<U> ctx) const requires HasResolveOrZero<T, U>
        {
            using Output = decltype(Geometry::ResolveOrZero(Width, ctx.Width));
            return Size<Output>{
                .Width = Geometry::ResolveOrZero(Width, ctx.Width),
                .Height = Geometry::ResolveOrZero(Height, ctx.Height),
            };
        }

        COPLT_RELEASE_FORCE_INLINE Size TryApplyAspectRatio(std::optional<f32> aspect_ratio) const requires std::same_as
            <T, std::optional<f32>>
        {
            if (!aspect_ratio.has_value()) return *this;
            if (Width.has_value() && Height.has_value()) return *this;
            if (Width.has_value())
            {
                return Size{
                    .Width = Width.value(),
                    .Height = Width.value() / aspect_ratio.value(),
                };
            }
            else if (Height.has_value())
            {
                return Size{
                    .Width = Height.value() * aspect_ratio.value(),
                    .Height = Height.value(),
                };
            }
            else std::unreachable();
        }

        COPLT_RELEASE_FORCE_INLINE Size TryAdd(Size<f32> other) const requires std::same_as<T, std::optional<f32>>
        {
            return Size{
                .Width = Width.has_value() ? std::optional{Width.value() + other.Width} : std::nullopt,
                .Height = Height.has_value() ? std::optional{Height.value() + other.Height} : std::nullopt,
            };
        }

        COPLT_RELEASE_FORCE_INLINE Size TrySub(Size<f32> other) const requires std::same_as<T, std::optional<f32>>
        {
            return Size{
                .Width = Width.has_value() ? std::optional{Width.value() - other.Width} : std::nullopt,
                .Height = Height.has_value() ? std::optional{Height.value() - other.Height} : std::nullopt,
            };
        }

        COPLT_RELEASE_FORCE_INLINE Size TryAdd(Size<f32> other) const requires std::same_as<T, AvailableSpace>
        {
            return Size{
                .Width = Width.first == AvailableSpaceType::Definite ?
                std::make_pair(Width.first, std::max(Width.second + other.Width, 0.0f)) : Width,
                .Height = Height.first == AvailableSpaceType::Definite ?
                std::make_pair(Height.first, std::max(Height.second + other.Height, 0.0f)) : Height,
            };
        }

        COPLT_RELEASE_FORCE_INLINE Size TrySub(Size<f32> other) const requires std::same_as<T, AvailableSpace>
        {
            return Size{
                .Width = Width.first == AvailableSpaceType::Definite ?
                std::make_pair(Width.first, std::max(Width.second - other.Width, 0.0f)) : Width,
                .Height = Height.first == AvailableSpaceType::Definite ?
                std::make_pair(Height.first, std::max(Height.second - other.Height, 0.0f)) : Height,
            };
        }

        COPLT_RELEASE_FORCE_INLINE Size TryMin(Size<f32> other) const
            requires std::same_as<T, std::optional<f32>>
        {
            return Size{
                .Width = Geometry::TryMin(Width, other.Width),
                .Height = Geometry::TryMin(Height, other.Height),
            };
        }

        COPLT_RELEASE_FORCE_INLINE Size TryMax(Size<f32> other) const
            requires std::same_as<T, std::optional<f32>>
        {
            return Size{
                .Width = Geometry::TryMax(Width, other.Width),
                .Height = Geometry::TryMax(Height, other.Height),
            };
        }

        COPLT_RELEASE_FORCE_INLINE Size TryMin(Size<std::optional<f32>> other) const
            requires std::same_as<T, std::optional<f32>>
        {
            return Size{
                .Width = Geometry::TryMin(Width, other.Width),
                .Height = Geometry::TryMin(Height, other.Height),
            };
        }

        COPLT_RELEASE_FORCE_INLINE Size TryMax(Size<std::optional<f32>> other) const
            requires std::same_as<T, std::optional<f32>>
        {
            return Size{
                .Width = Geometry::TryMax(Width, other.Width),
                .Height = Geometry::TryMax(Height, other.Height),
            };
        }

        COPLT_RELEASE_FORCE_INLINE Size TryClamp(Size<std::optional<f32>> min, Size<std::optional<f32>> max) const
            requires std::same_as<T, std::optional<f32>>
        {
            return Size{
                .Width = Geometry::TryClamp(Width, min.Width, max.Width),
                .Height = Geometry::TryClamp(Height, min.Height, max.Height),
            };
        }

        COPLT_RELEASE_FORCE_INLINE Size<f32> Or(Size<f32> other) const requires std::same_as<T, std::optional<f32>>
        {
            return Size{
                .Width = Width.has_value() ? Width.value() : other.Width,
                .Height = Height.has_value() ? Height.value() : other.Height,
            };
        }

        COPLT_RELEASE_FORCE_INLINE Size Or(Size<std::optional<f32>> other) const requires std::same_as<
            T, std::optional<f32>>
        {
            return Size{
                .Width = Width.has_value() ? Width : other.Width,
                .Height = Height.has_value() ? Height : other.Height,
            };
        }

        COPLT_RELEASE_FORCE_INLINE Size<std::optional<f32>> Or(Size<std::optional<f32>> other) const requires
            std::same_as<T, AvailableSpace>
        {
            return Size<std::optional<f32>>{
                .Width = Geometry::Or(Width, other.Width),
                .Height = Geometry::Or(Height, other.Height),
            };
        }
    };

    template <class T>
    struct Rect
    {
        T Top;
        T Right;
        T Bottom;
        T Left;

        COPLT_RELEASE_FORCE_INLINE Rect operator+(const Rect b) const
        {
            return {Top + b.Top, Right + b.Right, Bottom + b.Bottom, Left + b.Left};
        }

        COPLT_RELEASE_FORCE_INLINE Rect operator-(const Rect b) const
        {
            return {Top - b.Top, Right - b.Right, Bottom - b.Bottom, Left - b.Left};
        }

        template <class U>
        COPLT_RELEASE_FORCE_INLINE auto ResolveOrZero(Size<U> ctx) const requires HasResolveOrZero<T, U>
        {
            using Output = decltype(Geometry::ResolveOrZero(Top, ctx.Height));
            return Rect<Output>{
                .Top = Geometry::ResolveOrZero(Top, ctx.Height),
                .Right = Geometry::ResolveOrZero(Right, ctx.Width),
                .Bottom = Geometry::ResolveOrZero(Bottom, ctx.Height),
                .Left = Geometry::ResolveOrZero(Left, ctx.Width),
            };
        }

        COPLT_RELEASE_FORCE_INLINE Line<T> HorizontalComponents() const
        {
            return Line<T>{
                .Start = Left,
                .End = Right,
            };
        }

        COPLT_RELEASE_FORCE_INLINE Line<T> VerticalComponents() const
        {
            return Line<T>{
                .Start = Top,
                .End = Bottom,
            };
        }

        COPLT_RELEASE_FORCE_INLINE T HorizontalAxisSum() const
        {
            return Left + Right;
        }

        COPLT_RELEASE_FORCE_INLINE T VerticalAxisSum() const
        {
            return Top + Bottom;
        }

        COPLT_RELEASE_FORCE_INLINE Size<T> SumAxes() const
        {
            return Size<T>{
                .Width = HorizontalAxisSum(),
                .Height = VerticalAxisSum(),
            };
        }
    };
}
