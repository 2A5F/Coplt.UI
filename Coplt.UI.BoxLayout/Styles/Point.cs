using System;

namespace Coplt.UI.Styles;

public record struct Point<T>(T X, T Y)
{
    public T X = X;
    public T Y = Y;
}

public static partial class BoxStyleExtensions
{
    public static Point<T> Transpose<T>(this Point<T> self) => new(self.Y, self.X);

    public static Point<U> Map<T, U>(this Point<T> self, Func<T, U> f) => new(f(self.X), f(self.Y));

    public static Point<U> Map<A, T, U>(this Point<T> self, A arg, Func<A, T, U> f)
        where A : allows ref struct => new(f(arg, self.X), f(arg, self.Y));
}
