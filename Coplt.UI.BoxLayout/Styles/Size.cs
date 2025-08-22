using System;
using System.Runtime.CompilerServices;
using Coplt.UI.BoxLayouts;

namespace Coplt.UI.Styles;

public record struct Size<T>(T Width, T Height)
{
    public T Width = Width;
    public T Height = Height;

    public Size(T All) : this(All, All) { }
}

public static class Size
{
    public static Size<float?> FromCross(FlexDirection direction, float? value)
    {
        Size<float?> n = default;
        if (direction.IsRow()) n.Height = value;
        else n.Width = value;
        return n;
    }
}

public static partial class BoxStyleStructExtensions
{
    public static Size<T?> MapNullable<T>(this Size<T> self)
        where T : struct
        => new(self.Width, self.Height);
}

public static partial class BoxStyleExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasNonZeroArea(this Size<float> self) => self is { Width: > 0, Height: > 0 };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float?> TryResolve<T, TCalc>(this Size<T> self, Size<float?> ctx, ref TCalc calc)
        where TCalc : ICalc, allows ref struct
        where T : ITryResolve<float?>
        => new(
            self.Width.TryResolve(ctx.Width, ref calc),
            self.Height.TryResolve(ctx.Height, ref calc)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float?> TryResolve<T, TCalc>(this Size<T> self, Size<float> ctx, ref TCalc calc)
        where TCalc : ICalc, allows ref struct
        where T : ITryResolve<float?>
        => new(
            self.Width.TryResolve(ctx.Width, ref calc),
            self.Height.TryResolve(ctx.Height, ref calc)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float> ResolveOrZero<T, TCalc>(this Size<T> self, Size<float?> ctx, ref TCalc calc)
        where TCalc : ICalc, allows ref struct
        where T : ITryResolve<float?>
        => new(
            self.Width.TryResolve(ctx.Width, ref calc) ?? 0,
            self.Height.TryResolve(ctx.Height, ref calc) ?? 0
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float> ResolveOrZero<T, TCalc>(this Size<T> self, Size<float> ctx, ref TCalc calc)
        where TCalc : ICalc, allows ref struct
        where T : ITryResolve<float?>
        => new(
            self.Width.TryResolve(ctx.Width, ref calc) ?? 0,
            self.Height.TryResolve(ctx.Height, ref calc) ?? 0
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float> ResolveOrZero<TCalc>(this Size<LengthPercentage> self, Size<float> ctx, ref TCalc calc)
        where TCalc : ICalc, allows ref struct
        => new(
            self.Width.ResolveOrZero(ctx.Width, ref calc),
            self.Height.ResolveOrZero(ctx.Height, ref calc)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float?> TryApplyAspectRatio(this Size<float?> self, float? AspectRatio)
        => AspectRatio is { } ratio
            ? self switch
            {
                (Width: { } width, Height: null) => new(width, width / ratio),
                (Width: null, Height: { } height) => new(height * ratio, height),
                _ => self,
            }
            : self;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float?> TryAdd(this Size<float?> self, Size<float> value)
        => new(
            self.Width is { } width ? width + value.Width : null,
            self.Height is { } height ? height + value.Height : null
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float?> TrySub(this Size<float?> self, Size<float> value)
        => new(
            self.Width is { } width ? width - value.Width : null,
            self.Height is { } height ? height - value.Height : null
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float?> TryClamp(this Size<float?> self, Size<float?> min, Size<float?> max)
        => new(
            self.Width.TryClamp(min.Width, max.Width),
            self.Height.TryClamp(min.Height, max.Height)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float> TryClamp(this Size<float> self, Size<float?> min, Size<float?> max)
        => new(
            self.Width.TryClamp(min.Width, max.Width),
            self.Height.TryClamp(min.Height, max.Height)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float?> Or(this Size<float?> self, Size<float?> other) => new(
        self.Width ?? other.Width,
        self.Height ?? other.Height
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float> Or(this Size<float?> self, Size<float> other) => new(
        self.Width ?? other.Width,
        self.Height ?? other.Height
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<U> Map<T, U>(this Size<T> self, Func<T, U> f) =>
        new(f(self.Width), f(self.Height));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<U> ZipMap<T, U>(this Size<T> self, Size<T> other, Func<T, T, U> f) =>
        new(f(self.Width, other.Width), f(self.Height, other.Height));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float?> TryMax(this Size<float?> self, Size<float> other) => new(
        self.Width is { } width ? Math.Max(width, other.Width) : null,
        self.Height is { } height ? Math.Max(height, other.Height) : null
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float> Max(this Size<float> self, Size<float> other) => new(
        Math.Max(self.Width, other.Width),
        Math.Max(self.Height, other.Height)
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float> Add(this Size<float> self, Size<float> other) => new(
        self.Width + other.Width,
        self.Height + other.Height
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float> Sub(this Size<float> self, Size<float> value)
        => new(
            self.Width - value.Width,
            self.Height - value.Height
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float> Sub(this Size<float> self, Point<float> value)
        => new(
            self.Width - value.X,
            self.Height - value.Y
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T CrossRef<T>(this ref Size<T> self, FlexDirection direction)
        => ref direction.IsRow() ? ref self.Height : ref self.Width;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T MainRef<T>(this ref Size<T> self, FlexDirection direction)
        => ref direction.IsRow() ? ref self.Width : ref self.Height;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly T CrossRoRef<T>(this in Size<T> self, FlexDirection direction)
        => ref direction.IsRow() ? ref self.Height : ref self.Width;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly T MainRoRef<T>(this in Size<T> self, FlexDirection direction)
        => ref direction.IsRow() ? ref self.Width : ref self.Height;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Cross<T>(this in Size<T> self, FlexDirection direction)
        => direction.IsRow() ? self.Height : self.Width;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Main<T>(this in Size<T> self, FlexDirection direction)
        => direction.IsRow() ? self.Width : self.Height;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<T> WithCross<T>(this Size<T> self, FlexDirection direction, T value)
        => direction.IsRow() ? self with { Height = value } : self with { Width = value };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<T> WithMain<T>(this Size<T> self, FlexDirection direction, T value)
        => direction.IsRow() ? self with { Width = value } : self with { Height = value };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetCross<T>(this ref Size<T> self, FlexDirection direction, T value)
    {
        if (direction.IsRow()) self.Height = value;
        else self.Width = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetMain<T>(this ref Size<T> self, FlexDirection direction, T value)
    {
        if (direction.IsRow()) self.Width = value;
        else self.Height = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetAbs<T>(this Size<T> self, AbsoluteAxis axis) => axis switch
    {
        AbsoluteAxis.Horizontal => self.Width,
        AbsoluteAxis.Vertical => self.Height,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
    };
}
