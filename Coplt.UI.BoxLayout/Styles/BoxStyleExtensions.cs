using System;
using System.Runtime.CompilerServices;

namespace Coplt.UI.Styles;

public static partial class BoxStyleExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int TotalCmp(this float a, float b)
    {
        var left = Unsafe.BitCast<float, int>(a);
        var right = Unsafe.BitCast<float, int>(b);
        left ^= (left >> 31) >>> 1;
        right ^= (right >> 31) >>> 1;
        return left.CompareTo(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MaxByTotalCmp(this float a, float b) => TotalCmp(a, b) switch
    {
        < 0 => b,
        _ => a, // equal or greater
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float? TryClamp(this float? self, float? Min, float? Max) => (self, Min, Max) switch
    {
        ({ } bas, { } min, { } max) => Math.Clamp(bas, min, max),
        ({ } bas, null, { } max) => Math.Min(bas, max),
        ({ } bas, { } min, null) => Math.Max(bas, min),
        (not null, null, null) => self,
        (null, _, _) => null,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float TryClamp(this float self, float? Min, float? Max) => (Min, Max) switch
    {
        ({ } min, { } max) => Math.Clamp(self, min, max),
        (null, { } max) => Math.Min(self, max),
        ({ } min, null) => Math.Max(self, min),
        (null, null) => self,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float? TrySub(this float? self, float other) => self is { } v ? v - other : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float? TryAdd(this float? self, float other) => self is { } v ? v + other : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float? TryMin(this float? self, float other) => self is { } v ? Math.Min(v, other) : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float TryMin(this float self, float? other) => other is { } v ? Math.Min(self, v) : self;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float? TryMin(this float? self, float? other) => self is { } v && other is { } o ? Math.Min(v, o) : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float? TryMax(this float? self, float other) => self is { } v ? Math.Max(v, other) : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float TryMax(this float self, float? other) => other is { } v ? Math.Max(self, v) : self;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float? TryMax(this float? self, float? other) => self is { } v && other is { } o ? Math.Max(v, o) : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Min(this float self, float other) => Math.Min(self, other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Max(this float self, float other) => Math.Max(self, other);
}

public static partial class BoxStyleStructExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? Nullable<T>(this T value) where T : struct => value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static U? Map<T, U>(this T? self, Func<T, U> f)
        where T : struct where U : struct
        => self.HasValue ? f(self.GetValueOrDefault()) : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static U? Map<T, U, A>(this T? self, A a, Func<A, T, U> f)
        where T : struct where U : struct where A : allows ref struct
        => self.HasValue ? f(a, self.GetValueOrDefault()) : null;
}
