using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Coplt.UI.BoxLayout.Utilities;
using Coplt.UI.BoxLayouts;

namespace Coplt.UI.Styles;

public record struct Rect<T>(T Top, T Right, T Bottom, T Left)
{
    public T Top = Top;
    public T Right = Right;
    public T Bottom = Bottom;
    public T Left = Left;

    public Rect(T All) : this(All, All, All, All) { }
    public Rect(T Top, T LeftRight, T Bottom) : this(Top, LeftRight, Bottom, LeftRight) { }
    public Rect(T TopBottom, T LeftRight) : this(TopBottom, LeftRight, TopBottom, LeftRight) { }
}

public static partial class BoxStyleExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect<float> ResolveOrZero<T, TCalc>(this Rect<T> self, float? ctx, ref TCalc calc)
        where TCalc : ICalc, allows ref struct
        where T : ITryResolve<float?>
        => new(
            self.Top.TryResolve(ctx, ref calc) ?? 0,
            self.Right.TryResolve(ctx, ref calc) ?? 0,
            self.Bottom.TryResolve(ctx, ref calc) ?? 0,
            self.Left.TryResolve(ctx, ref calc) ?? 0
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect<T> Add<T>(this Rect<T> self, Rect<T> other)
        where T : IAdditionOperators<T, T, T>
        => new(self.Top + other.Top, self.Right + other.Right, self.Bottom + other.Bottom, self.Left + other.Left);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T HorizontalAxisSum<T>(this Rect<T> self)
        where T : IAdditionOperators<T, T, T>
        => self.Left + self.Right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T VerticalAxisSum<T>(this Rect<T> self)
        where T : IAdditionOperators<T, T, T>
        => self.Top + self.Bottom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<T> SumAxes<T>(this Rect<T> self)
        where T : IAdditionOperators<T, T, T>
        => new(self.HorizontalAxisSum(), self.VerticalAxisSum());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect<R> Zip<T, U, R>(this Rect<T> self, Size<U> size, Func<T, U, R> f)
        => new(
            f(self.Top, size.Height),
            f(self.Right, size.Width),
            f(self.Bottom, size.Height),
            f(self.Left, size.Width)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect<R> Zip<T, U, R, A>(this Rect<T> self, Size<U> size, ref A arg, RefFunc<A, T, U, R> f)
        where A : allows ref struct
        => new(
            f(ref arg, self.Top, size.Height),
            f(ref arg, self.Right, size.Width),
            f(ref arg, self.Bottom, size.Height),
            f(ref arg, self.Left, size.Width)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect<R> Map<T, R>(this Rect<T> self, Func<T, R> f)
        => new(
            f(self.Top),
            f(self.Right),
            f(self.Bottom),
            f(self.Left)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CrossAxisSum<T>(this Rect<T> self, FlexDirection direction)
        where T : IAdditionOperators<T, T, T>
        => direction.IsRow() ? self.VerticalAxisSum() : self.HorizontalAxisSum();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T MainAxisSum<T>(this Rect<T> self, FlexDirection direction)
        where T : IAdditionOperators<T, T, T>
        => direction.IsRow() ? self.HorizontalAxisSum() : self.VerticalAxisSum();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CrossStart<T>(this Rect<T> self, FlexDirection direction)
        => direction.IsRow() ? self.Top : self.Left;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CrossEnd<T>(this Rect<T> self, FlexDirection direction)
        => direction.IsRow() ? self.Bottom : self.Right;
}
