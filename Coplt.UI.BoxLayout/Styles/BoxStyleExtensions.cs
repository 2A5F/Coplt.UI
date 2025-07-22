using System;

namespace Coplt.UI.Styles;

public static partial class BoxStyleExtensions
{
    public static float? TryClamp(this float? self, float? Min, float? Max) => (self, Min, Max) switch
    {
        ({ } bas, { } min, { } max) => Math.Clamp(bas, min, max),
        ({ } bas, null, { } max) => Math.Min(bas, max),
        ({ } bas, { } min, null) => Math.Max(bas, min),
        (not null, null, null) => self,
        (null, _, _) => null,
    };
    public static float TryClamp(this float self, float? Min, float? Max) => (Min, Max) switch
    {
        ({ } min, { } max) => Math.Clamp(self, min, max),
        (null, { } max) => Math.Min(self, max),
        ({ } min, null) => Math.Max(self, min),
        (null, null) => self,
    };

    public static float? TrySub(this float? self, float other) => self is { } v ? v - other : null;

    public static float? TryAdd(this float? self, float other) => self is { } v ? v + other : null;
}

public static partial class BoxStyleStructExtensions
{
    public static T? Nullable<T>(this T value) where T : struct => value;

    public static U? Map<T, U>(this T? self, Func<T, U> f)
        where T : struct where U : struct
        => self.HasValue ? f(self.GetValueOrDefault()) : null;
    public static U? Map<T, U, A>(this T? self, A a, Func<A, T, U> f)
        where T : struct where U : struct where A : allows ref struct
        => self.HasValue ? f(a, self.GetValueOrDefault()) : null;
}
