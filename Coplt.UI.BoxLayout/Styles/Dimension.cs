using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Coplt.UI.BoxLayouts;
using Coplt.Union;

namespace Coplt.UI.Styles;

[Union2]
public readonly partial struct Length : ITryResolve<float?>
{
    [UnionTemplate]
    private interface Template
    {
        [Variant(Tag = 0)]
        float Fixed();
        CalcId Calc();
    }

    public static Length Zero => Length.Fixed(0);

    public static implicit operator Length(float self) => Length.Fixed(self);

    public static implicit operator AnyLength(Length self) => self.Tag switch
    {
        Tags.Fixed => AnyLength.Fixed(self.Fixed),
        Tags.Calc => AnyLength.Calc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator LengthPercentage(Length self) => self.Tag switch
    {
        Tags.Fixed => LengthPercentage.Fixed(self.Fixed),
        Tags.Calc => LengthPercentage.Calc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator LengthPercentageAuto(Length self) => self.Tag switch
    {
        Tags.Fixed => LengthPercentageAuto.Fixed(self.Fixed),
        Tags.Calc => LengthPercentageAuto.Calc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator Dimension(Length self) => self.Tag switch
    {
        Tags.Fixed => Dimension.Fixed(self.Fixed),
        Tags.Calc => Dimension.Calc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float? TryResolve<TCalc>(float? ctx, ref TCalc calc)
        where TCalc : ICalc, allows ref struct => Tag switch
    {
        Tags.Fixed => Fixed,
        Tags.Calc => ctx is { } dim ? calc.Calc(Calc, dim) : null,
        _ => throw new ArgumentOutOfRangeException()
    };
}

[Union2]
public readonly partial struct LengthPercentage : ITryResolve<float?>
{
    [UnionTemplate]
    private interface Template
    {
        [Variant(Tag = 0)]
        float Fixed();
        float Percent();
        CalcId Calc();
    }

    public static LengthPercentage Zero => LengthPercentage.Fixed(0);

    public static implicit operator LengthPercentage(float self) => LengthPercentage.Fixed(self);

    public static implicit operator AnyLength(LengthPercentage self) => self.Tag switch
    {
        Tags.Fixed => AnyLength.Fixed(self.Fixed),
        Tags.Percent => AnyLength.Percent(self.Percent),
        Tags.Calc => AnyLength.Calc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator LengthPercentageAuto(LengthPercentage self) => self.Tag switch
    {
        Tags.Fixed => LengthPercentageAuto.Fixed(self.Fixed),
        Tags.Percent => LengthPercentageAuto.Percent(self.Percent),
        Tags.Calc => LengthPercentageAuto.Calc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator Dimension(LengthPercentage self) => self.Tag switch
    {
        Tags.Fixed => Dimension.Fixed(self.Fixed),
        Tags.Percent => Dimension.Percent(self.Percent),
        Tags.Calc => Dimension.Calc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float? TryResolve<TCalc>(float? ctx, ref TCalc calc)
        where TCalc : ICalc, allows ref struct => Tag switch
    {
        Tags.Fixed => Fixed,
        Tags.Percent => ctx is { } dim ? dim * Percent : null,
        Tags.Calc => ctx is { } dim ? calc.Calc(Calc, dim) : null,
        _ => throw new ArgumentOutOfRangeException()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ResolveOrZero<TCalc>(float? ctx, ref TCalc calc)
        where TCalc : ICalc, allows ref struct => (Tag, ctx) switch
    {
        (Tags.Fixed, _) => Fixed,
        (Tags.Percent, { } dim) => dim * Percent,
        (Tags.Calc, { } dim) => calc.Calc(Calc, dim),
        (Tags.Percent or Tags.Calc, null) => 0,
        _ => throw new ArgumentOutOfRangeException()
    };
}

[Union2]
public readonly partial struct LengthPercentageAuto : ITryResolve<float?>
{
    [UnionTemplate]
    private interface Template
    {
        void Auto();
        float Fixed();
        float Percent();
        CalcId Calc();
    }

    public static LengthPercentageAuto Zero => LengthPercentageAuto.Fixed(0);

    public static implicit operator LengthPercentageAuto(float self) => LengthPercentageAuto.Fixed(self);

    public static implicit operator AnyLength(LengthPercentageAuto self) => self.Tag switch
    {
        Tags.Auto => AnyLength.Auto,
        Tags.Fixed => AnyLength.Fixed(self.Fixed),
        Tags.Percent => AnyLength.Percent(self.Percent),
        Tags.Calc => AnyLength.Calc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static explicit operator Dimension(LengthPercentageAuto self) => self.Tag switch
    {
        Tags.Auto => Dimension.Auto,
        Tags.Fixed => Dimension.Fixed(self.Fixed),
        Tags.Percent => Dimension.Percent(self.Percent),
        Tags.Calc => Dimension.Calc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float? TryResolve<TCalc>(float? ctx, ref TCalc calc)
        where TCalc : ICalc, allows ref struct => Tag switch
    {
        Tags.Auto => null,
        Tags.Fixed => Fixed,
        Tags.Percent => ctx is { } dim ? dim * Percent : null,
        Tags.Calc => ctx is { } dim ? calc.Calc(Calc, dim) : null,
        _ => throw new ArgumentOutOfRangeException()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ResolveOrZero<TCalc>(float? ctx, ref TCalc calc)
        where TCalc : ICalc, allows ref struct => (Tag, ctx) switch
    {
        (Tags.Auto, _) => 0,
        (Tags.Fixed, _) => Fixed,
        (Tags.Percent, { } dim) => dim * Percent,
        (Tags.Calc, { } dim) => calc.Calc(Calc, dim),
        (Tags.Percent or Tags.Calc, null) => 0,
        _ => throw new ArgumentOutOfRangeException()
    };
}

[Union2]
public readonly partial struct Dimension : ITryResolve<float?>
{
    [UnionTemplate]
    private interface Template
    {
        void Auto();
        float Fixed();
        float Percent();
        CalcId Calc();
    }

    public static Dimension Zero => Dimension.Fixed(0);

    public static implicit operator Dimension(float self) => Dimension.Fixed(self);

    public static implicit operator AnyLength(Dimension self) => self.Tag switch
    {
        Tags.Auto => AnyLength.Auto,
        Tags.Fixed => AnyLength.Fixed(self.Fixed),
        Tags.Percent => AnyLength.Percent(self.Percent),
        Tags.Calc => AnyLength.Calc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static explicit operator LengthPercentageAuto(Dimension self) => self.Tag switch
    {
        Tags.Auto => LengthPercentageAuto.Auto,
        Tags.Fixed => LengthPercentageAuto.Fixed(self.Fixed),
        Tags.Percent => LengthPercentageAuto.Percent(self.Percent),
        Tags.Calc => LengthPercentageAuto.Calc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float? TryResolve<TCalc>(float? ctx, ref TCalc calc)
        where TCalc : ICalc, allows ref struct => Tag switch
    {
        Tags.Auto => null,
        Tags.Fixed => Fixed,
        Tags.Percent => ctx is { } dim ? dim * Percent : null,
        Tags.Calc => ctx is { } dim ? calc.Calc(Calc, dim) : null,
        _ => throw new ArgumentOutOfRangeException()
    };
}
