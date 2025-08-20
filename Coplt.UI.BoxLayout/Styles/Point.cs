using System;
using System.Runtime.CompilerServices;

namespace Coplt.UI.Styles;

public record struct Point<T>(T X, T Y)
{
    public T X = X;
    public T Y = Y;

    public Point(T All) : this(All, All) { }
}

public static partial class BoxStyleExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point<T> Transpose<T>(this Point<T> self) => new(self.Y, self.X);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point<U> Map<T, U>(this Point<T> self, Func<T, U> f) => new(f(self.X), f(self.Y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point<U> Map<A, T, U>(this Point<T> self, A arg, Func<A, T, U> f)
        where A : allows ref struct => new(f(arg, self.X), f(arg, self.Y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<T> ToSize<T>(this Point<T> self) => new(self.X, self.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly T Main<T>(this in Point<T> self, FlexDirection direction)
        => ref direction.IsRow() ? ref self.X : ref self.Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly T Cross<T>(this in Point<T> self, FlexDirection direction)
        => ref direction.IsRow() ? ref self.Y : ref self.X;
}
