using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Coplt.UI.BoxLayouts;
using Coplt.Union;

namespace Coplt.UI.Styles;

[Union]
public readonly partial struct Length : ITryResolve<float?>
{
    [UnionTemplate]
    private interface Template
    {
        [Variant(Tag = 0)]
        float Fixed();
        CalcId Calc();
    }

    public static Length Zero => MakeFixed(0);

    public static implicit operator Length(float self) => MakeFixed(self);
    
    public static implicit operator AnyLength(Length self) => self.Tag switch
    {
        Tags.Fixed => AnyLength.MakeFixed(self.Fixed),
        Tags.Calc => AnyLength.MakeCalc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator LengthPercentage(Length self) => self.Tag switch
    {
        Tags.Fixed => LengthPercentage.MakeFixed(self.Fixed),
        Tags.Calc => LengthPercentage.MakeCalc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator LengthPercentageAuto(Length self) => self.Tag switch
    {
        Tags.Fixed => LengthPercentageAuto.MakeFixed(self.Fixed),
        Tags.Calc => LengthPercentageAuto.MakeCalc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator Dimension(Length self) => self.Tag switch
    {
        Tags.Fixed => Dimension.MakeFixed(self.Fixed),
        Tags.Calc => Dimension.MakeCalc(self.Calc),
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

[Union]
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

    public static LengthPercentage Zero => MakeFixed(0);

    public static implicit operator LengthPercentage(float self) => MakeFixed(self);

    public static implicit operator AnyLength(LengthPercentage self) => self.Tag switch
    {
        Tags.Fixed => AnyLength.MakeFixed(self.Fixed),
        Tags.Percent => AnyLength.MakePercent(self.Percent),
        Tags.Calc => AnyLength.MakeCalc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator LengthPercentageAuto(LengthPercentage self) => self.Tag switch
    {
        Tags.Fixed => LengthPercentageAuto.MakeFixed(self.Fixed),
        Tags.Percent => LengthPercentageAuto.MakePercent(self.Percent),
        Tags.Calc => LengthPercentageAuto.MakeCalc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator Dimension(LengthPercentage self) => self.Tag switch
    {
        Tags.Fixed => Dimension.MakeFixed(self.Fixed),
        Tags.Percent => Dimension.MakePercent(self.Percent),
        Tags.Calc => Dimension.MakeCalc(self.Calc),
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

[Union]
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

    public static LengthPercentageAuto Zero => MakeFixed(0);

    public static LengthPercentageAuto Auto = MakeAuto();

    public static implicit operator LengthPercentageAuto(float self) => MakeFixed(self);

    public static implicit operator AnyLength(LengthPercentageAuto self) => self.Tag switch
    {
        Tags.Auto => AnyLength.MakeAuto(),
        Tags.Fixed => AnyLength.MakeFixed(self.Fixed),
        Tags.Percent => AnyLength.MakePercent(self.Percent),
        Tags.Calc => AnyLength.MakeCalc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static explicit operator Dimension(LengthPercentageAuto self) => self.Tag switch
    {
        Tags.Auto => Dimension.MakeAuto(),
        Tags.Fixed => Dimension.MakeFixed(self.Fixed),
        Tags.Percent => Dimension.MakePercent(self.Percent),
        Tags.Calc => Dimension.MakeCalc(self.Calc),
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

[Union]
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

    public static Dimension Zero => MakeFixed(0);

    public static Dimension Auto = MakeAuto();

    public static implicit operator Dimension(float self) => MakeFixed(self);

    public static implicit operator AnyLength(Dimension self) => self.Tag switch
    {
        Tags.Auto => AnyLength.MakeAuto(),
        Tags.Fixed => AnyLength.MakeFixed(self.Fixed),
        Tags.Percent => AnyLength.MakePercent(self.Percent),
        Tags.Calc => AnyLength.MakeCalc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static explicit operator LengthPercentageAuto(Dimension self) => self.Tag switch
    {
        Tags.Auto => LengthPercentageAuto.MakeAuto(),
        Tags.Fixed => LengthPercentageAuto.MakeFixed(self.Fixed),
        Tags.Percent => LengthPercentageAuto.MakePercent(self.Percent),
        Tags.Calc => LengthPercentageAuto.MakeCalc(self.Calc),
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
