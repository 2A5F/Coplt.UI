using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Coplt.Union;

namespace Coplt.UI.Styles;

/// <summary>
/// The amount of space available to a node in a given axis<br/>
/// https://www.w3.org/TR/css-sizing-3/#available
/// </summary>
[Union2]
public readonly partial struct AvailableSpace
{
    [UnionTemplate]
    private interface Template
    {
        [Variant(Tag = 0)]
        float Definite();
        void MinContent();
        void MaxContent();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float? TryGet() => Tag is Tags.Definite ? Definite : null;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float TryGet(float fallback) => Tag is Tags.Definite ? Definite : fallback;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AvailableSpace Or(AvailableSpace fallback) => Tag is Tags.Definite ? this : fallback;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ComputeFreeSpace(float UsedSpace) => Tag switch
    {
        Tags.Definite => Definite - UsedSpace,
        Tags.MinContent => 0,
        Tags.MaxContent => float.PositiveInfinity,
        _ => throw new ArgumentOutOfRangeException()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsRoughlyEqual(AvailableSpace other) => (Tag, other.Tag) switch
    {
        (Tags.Definite, Tags.Definite) => Math.Abs(Definite - other.Definite) < float.Epsilon,
        (Tags.MinContent, Tags.MinContent) => true,
        (Tags.MaxContent, Tags.MaxContent) => true,
        _ => false,
    };

    public static implicit operator AvailableSpace(float value) => AvailableSpace.Definite(value);

    public static implicit operator AvailableSpace(float? value)
        => value.HasValue ? AvailableSpace.Definite(value.Value) : AvailableSpace.MaxContent;

    public static AvailableSpace From(float value) => AvailableSpace.Definite(value);
}

public static partial class BoxStyleExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float?> TryGet(this Size<AvailableSpace> self) => new(self.Width.TryGet(), self.Height.TryGet());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AvailableSpace ToAvailableSpace(this float self) => AvailableSpace.From(self);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AvailableSpace TrySet(this AvailableSpace self, float? value)
        => value is { } v ? AvailableSpace.Definite(v) : self;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AvailableSpace TrySub(this AvailableSpace self, float other) => self.Tag switch
    {
        AvailableSpace.Tags.Definite => AvailableSpace.Definite(self.Definite - other),
        _ => self,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AvailableSpace TryMin(this AvailableSpace self, float other) => self.Tag switch
    {
        AvailableSpace.Tags.Definite => AvailableSpace.Definite(Math.Min(self.Definite, other)),
        _ => self,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AvailableSpace TryMax(this AvailableSpace self, float other) => self.Tag switch
    {
        AvailableSpace.Tags.Definite => AvailableSpace.Definite(Math.Max(self.Definite, other)),
        _ => self,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AvailableSpace TryClamp(this AvailableSpace self, float? min, float? max) => (self.Tag, min, max) switch
    {
        (AvailableSpace.Tags.Definite, { } Min, { } Max) => AvailableSpace.Definite(Math.Clamp(self.Definite, Min, Max)),
        (AvailableSpace.Tags.Definite, null, { } Max) => AvailableSpace.Definite(Math.Min(self.Definite, Max)),
        (AvailableSpace.Tags.Definite, { } Min, null) => AvailableSpace.Definite(Math.Max(self.Definite, Min)),
        _ => self,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AvailableSpace MapDefiniteValue(this AvailableSpace self, Func<float, float> f) => self.Tag switch
    {
        AvailableSpace.Tags.Definite => AvailableSpace.Definite(f(self.Definite)),
        _ => self,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AvailableSpace MapDefiniteValue<Arg>(this AvailableSpace self, Arg arg, Func<Arg, float, float> f)
        where Arg : allows ref struct => self.Tag switch
    {
        AvailableSpace.Tags.Definite => AvailableSpace.Definite(f(arg, self.Definite)),
        _ => self,
    };
}
