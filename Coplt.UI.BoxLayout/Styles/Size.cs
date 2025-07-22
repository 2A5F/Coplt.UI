using System;
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
    public static Size<float?> TryResolve<T, TCalc>(this Size<T> self, Size<float?> ctx, ref TCalc calc)
        where TCalc : ICalc, allows ref struct
        where T : ITryResolve<float?>
        => new(
            self.Width.TryResolve(ctx.Width, ref calc),
            self.Height.TryResolve(ctx.Height, ref calc)
        );

    public static Size<float> ResolveOrZero<T, TCalc>(this Size<T> self, Size<float?> ctx, ref TCalc calc)
        where TCalc : ICalc, allows ref struct
        where T : ITryResolve<float?>
        => new(
            self.Width.TryResolve(ctx.Width, ref calc) ?? 0,
            self.Height.TryResolve(ctx.Height, ref calc) ?? 0
        );

    public static Size<float> ResolveOrZero<T, TCalc>(this Size<T> self, Size<float> ctx, ref TCalc calc)
        where TCalc : ICalc, allows ref struct
        where T : ITryResolve<float?>
        => new(
            self.Width.TryResolve(ctx.Width, ref calc) ?? 0,
            self.Height.TryResolve(ctx.Height, ref calc) ?? 0
        );

    public static Size<float?> TryApplyAspectRatio(this Size<float?> self, float? AspectRatio)
        => AspectRatio is { } ratio
            ? self switch
            {
                (Width: { } width, Height: null) => new(width, width / ratio),
                (Width: null, Height: { } height) => new(height * ratio, height),
                _ => self,
            }
            : self;

    public static Size<float?> TryAdd(this Size<float?> self, Size<float> value)
        => new(
            self.Width is { } width ? width + value.Width : null,
            self.Width is { } height ? height + value.Height : null
        );

    public static Size<float?> TrySub(this Size<float?> self, Size<float> value)
        => new(
            self.Width is { } width ? width - value.Width : null,
            self.Width is { } height ? height - value.Height : null
        );

    public static Size<float?> TryClamp(this Size<float?> self, Size<float?> min, Size<float?> max)
        => new(
            self.Width.TryClamp(min.Width, max.Width),
            self.Height.TryClamp(min.Height, max.Height)
        );

    public static Size<float> TryClamp(this Size<float> self, Size<float?> min, Size<float?> max)
        => new(
            self.Width.TryClamp(min.Width, max.Width),
            self.Height.TryClamp(min.Height, max.Height)
        );

    public static Size<float?> Or(this Size<float?> self, Size<float?> other) => new(
        self.Width ?? other.Width,
        self.Height ?? other.Height
    );

    public static Size<float> Or(this Size<float?> self, Size<float> other) => new(
        self.Width ?? other.Width,
        self.Height ?? other.Height
    );

    public static Size<U> ZipMap<T, U>(this Size<T> self, Size<T> other, Func<T, T, U> f) =>
        new(f(self.Width, other.Width), f(self.Height, other.Height));

    public static Size<float?> TryMax(this Size<float?> self, Size<float> other) => new(
        self.Width is { } width ? Math.Max(width, other.Width) : null,
        self.Height is { } height ? Math.Max(height, other.Height) : null
    );

    public static Size<float> Max(this Size<float> self, Size<float> other) => new(
        Math.Max(self.Width, other.Width),
        Math.Max(self.Height, other.Height)
    );

    public static Size<float> Add(this Size<float> self, Size<float> other) => new(
        self.Width + other.Width,
        self.Height + other.Height
    );

    public static ref T CrossRef<T>(this ref Size<T> self, FlexDirection direction)
        => ref direction.IsRow() ? ref self.Height : ref self.Width;

    public static ref T MainRef<T>(this ref Size<T> self, FlexDirection direction)
        => ref direction.IsRow() ? ref self.Width : ref self.Height;

    public static ref readonly T Cross<T>(this in Size<T> self, FlexDirection direction)
        => ref direction.IsRow() ? ref self.Height : ref self.Width;

    public static ref readonly T Main<T>(this in Size<T> self, FlexDirection direction)
        => ref direction.IsRow() ? ref self.Width : ref self.Height;

    public static Size<T> WithCross<T>(this Size<T> self, FlexDirection direction, T value)
    {
        var r = self;
        r.CrossRef(direction) = value;
        return r;
    }

    public static Size<T> WithMain<T>(this Size<T> self, FlexDirection direction, T value)
    {
        var r = self;
        r.MainRef(direction) = value;
        return r;
    }

    public static void SetCross<T>(this ref Size<T> self, FlexDirection direction, T value)
    {
        self.CrossRef(direction) = value;
    }

    public static void SetMain<T>(this ref Size<T> self, FlexDirection direction, T value)
    {
        self.MainRef(direction) = value;
    }

    public static T GetAbs<T>(this Size<T> self, AbsoluteAxis axis) => axis switch
    {
        AbsoluteAxis.Horizontal => self.Width,
        AbsoluteAxis.Vertical => self.Height,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
    };
}
