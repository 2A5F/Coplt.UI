using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Coplt.UI.BoxLayout.Utilities;
using Coplt.Union;

namespace Coplt.UI.Styles;

public interface IGridContainerStyle<TTrackSizingFunctionList> : ICoreStyle
    where TTrackSizingFunctionList : IAsReadOnlySpan<NonRepeatedTrackSizingFunction>
{
    public ReadOnlySpan<TrackSizingFunction<TTrackSizingFunctionList>> GridTemplateRows => [];
    public ReadOnlySpan<TrackSizingFunction<TTrackSizingFunctionList>> GridTemplateColumns => [];
    public ReadOnlySpan<NonRepeatedTrackSizingFunction> GridAutoRows => [];
    public ReadOnlySpan<NonRepeatedTrackSizingFunction> GridAutoColumns => [];

    public GridAutoFlow GridAutoFlow => BoxStyle.Default.GridAutoFlow;

    public Size<LengthPercentage> Gap => BoxStyle.Default.Gap;

    public AlignContent? AlignContent => BoxStyle.Default.AlignContent;
    public JustifyContent? JustifyContent => BoxStyle.Default.JustifyContent;
    public AlignItems? AlignItems => BoxStyle.Default.AlignItems;
    public JustifyItems? JustifyItems => BoxStyle.Default.JustifyItems;

    public ReadOnlySpan<TrackSizingFunction<TTrackSizingFunctionList>> GridTemplateTracks(AbsoluteAxis axis) => axis switch
    {
        AbsoluteAxis.Horizontal => GridTemplateColumns,
        AbsoluteAxis.Vertical => GridTemplateRows,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
    };

    public AlignContent GridAlignContent(AbstractAxis axis) => axis switch
    {
        AbstractAxis.Inline => JustifyContent?.ToAlignContent() ?? Styles.AlignContent.Stretch,
        AbstractAxis.Block => AlignContent ?? Styles.AlignContent.Stretch,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
    };
}

public interface IGridItemStyle : ICoreStyle
{
    public Line<GridPlacement> GridRow => BoxStyle.Default.GridRow;
    public Line<GridPlacement> GridColumn => BoxStyle.Default.GridColumn;

    public AlignSelf? AlignSelf => BoxStyle.Default.AlignSelf;
    public JustifySelf? JustifySelf => BoxStyle.Default.JustifySelf;

    public Line<GridPlacement> GridPlacement(AbsoluteAxis axis) => axis switch {
        AbsoluteAxis.Horizontal => GridColumn,
        AbsoluteAxis.Vertical => GridRow,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
    };
}

public enum GridAutoFlow : byte
{
    Column,
    Row,
    ColumnDense,
    RowDense,
}

public static partial class BoxStyleExtensions
{
    public static bool IsDense(this GridAutoFlow self) => self switch
    {
        GridAutoFlow.Row or GridAutoFlow.Column => false,
        GridAutoFlow.RowDense or GridAutoFlow.ColumnDense => true,
        _ => throw new ArgumentOutOfRangeException(nameof(self), self, null)
    };

    public static AbsoluteAxis PrimaryAxis(this GridAutoFlow self) => self switch
    {
        GridAutoFlow.Row or GridAutoFlow.RowDense => AbsoluteAxis.Horizontal,
        GridAutoFlow.Column or GridAutoFlow.ColumnDense => AbsoluteAxis.Vertical,
        _ => throw new ArgumentOutOfRangeException(nameof(self), self, null)
    };
}

public readonly record struct OriginZeroLine(short Value)
{
    public short Value { get; } = Value;

    public static implicit operator OriginZeroLine(short value) => new(value);
}

public readonly record struct GridLine(short Value)
{
    public short Value { get; } = Value;

    public static implicit operator GridLine(short value) => new(value);
}

[Union]
public readonly partial struct OriginZeroGridPlacement
{
    [UnionTemplate]
    private interface Template
    {
        void Auto();
        OriginZeroLine Line();
        ushort Span();
    }

    public static OriginZeroGridPlacement Auto = MakeAuto();
}

[Union]
public readonly partial struct GridPlacement
{
    [UnionTemplate]
    private interface Template
    {
        void Auto();
        GridLine Line();
        ushort Span();
    }

    public static GridPlacement Auto = MakeAuto();
}

[Union]
public readonly partial struct MaxTrackSizingFunction
{
    [UnionTemplate]
    private interface Template
    {
        void Auto();
        float Fixed();
        float Percent();
        float Fr();
        void MinContent();
        void MaxContent();
        float FitContentFixed();
        float FitContentPercent();
        CalcId Calc();
    }

    public static MaxTrackSizingFunction Zero => MakeFixed(0);

    public static MaxTrackSizingFunction Auto = MakeAuto();
    public static MaxTrackSizingFunction MinContent = MakeMinContent();
    public static MaxTrackSizingFunction MaxContent = MakeMaxContent();

    public static MaxTrackSizingFunction FitContent(LengthPercentage lp) => lp.Tag switch
    {
        LengthPercentage.Tags.Fixed => MakeFitContentFixed(lp.Fixed),
        LengthPercentage.Tags.Percent => MakeFitContentPercent(lp.Percent),
        LengthPercentage.Tags.Calc => MakeCalc(lp.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator AnyLength(MaxTrackSizingFunction self) => self.Tag switch
    {
        Tags.Auto => AnyLength.MakeAuto(),
        Tags.Fixed => AnyLength.MakeFixed(self.Fixed),
        Tags.Percent => AnyLength.MakePercent(self.Percent),
        Tags.Calc => AnyLength.MakeCalc(self.Calc),
        Tags.Fr => AnyLength.MakeFr(self.Fr),
        Tags.MinContent => AnyLength.MinContent,
        Tags.MaxContent => AnyLength.MaxContent,
        Tags.FitContentFixed => AnyLength.MakeFitContentFixed(self.FitContentFixed),
        Tags.FitContentPercent => AnyLength.MakeFitContentPercent(self.FitContentPercent),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator MaxTrackSizingFunction(LengthPercentage self) => self.Tag switch
    {
        LengthPercentage.Tags.Fixed => MakeFixed(self.Fixed),
        LengthPercentage.Tags.Percent => MakePercent(self.Percent),
        LengthPercentage.Tags.Calc => MakeCalc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator MaxTrackSizingFunction(LengthPercentageAuto self) => self.Tag switch
    {
        LengthPercentageAuto.Tags.Auto => Auto,
        LengthPercentageAuto.Tags.Fixed => MakeFixed(self.Fixed),
        LengthPercentageAuto.Tags.Percent => MakePercent(self.Percent),
        LengthPercentageAuto.Tags.Calc => MakeCalc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };
}

[Union]
public readonly partial struct MinTrackSizingFunction
{
    [UnionTemplate]
    private interface Template
    {
        void Auto();
        float Fixed();
        float Percent();
        float Fr();
        void MinContent();
        void MaxContent();
        CalcId Calc();
    }

    public static MinTrackSizingFunction Zero => MakeFixed(0);

    public static MinTrackSizingFunction Auto = MakeAuto();
    public static MinTrackSizingFunction MinContent = MakeMinContent();
    public static MinTrackSizingFunction MaxContent = MakeMaxContent();

    public static implicit operator AnyLength(MinTrackSizingFunction self) => self.Tag switch
    {
        Tags.Auto => AnyLength.MakeAuto(),
        Tags.Fixed => AnyLength.MakeFixed(self.Fixed),
        Tags.Percent => AnyLength.MakePercent(self.Percent),
        Tags.Calc => AnyLength.MakeCalc(self.Calc),
        Tags.Fr => AnyLength.MakeFr(self.Fr),
        Tags.MinContent => AnyLength.MinContent,
        Tags.MaxContent => AnyLength.MaxContent,
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator MinTrackSizingFunction(LengthPercentage self) => self.Tag switch
    {
        LengthPercentage.Tags.Fixed => MakeFixed(self.Fixed),
        LengthPercentage.Tags.Percent => MakePercent(self.Percent),
        LengthPercentage.Tags.Calc => MakeCalc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator MinTrackSizingFunction(LengthPercentageAuto self) => self.Tag switch
    {
        LengthPercentageAuto.Tags.Auto => Auto,
        LengthPercentageAuto.Tags.Fixed => MakeFixed(self.Fixed),
        LengthPercentageAuto.Tags.Percent => MakePercent(self.Percent),
        LengthPercentageAuto.Tags.Calc => MakeCalc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };
}

[Union]
public readonly partial struct GridTrackRepetition
{
    [UnionTemplate]
    private interface Template
    {
        void AutoFill();
        void AutoFit();
        ushort Count();
    }
}

public record struct NonRepeatedTrackSizingFunction(MinTrackSizingFunction Min, MaxTrackSizingFunction Max)
{
    public MinTrackSizingFunction Min { get; set; } = Min;
    public MaxTrackSizingFunction Max { get; set; } = Max;

    public static NonRepeatedTrackSizingFunction Zero = new(MinTrackSizingFunction.Zero, MaxTrackSizingFunction.Zero);
    public static NonRepeatedTrackSizingFunction Auto = new(MinTrackSizingFunction.Auto, MaxTrackSizingFunction.Auto);
    public static NonRepeatedTrackSizingFunction MinContent = new(MinTrackSizingFunction.MinContent, MaxTrackSizingFunction.MinContent);
    public static NonRepeatedTrackSizingFunction MaxContent = new(MinTrackSizingFunction.MinContent, MaxTrackSizingFunction.MaxContent);

    public static NonRepeatedTrackSizingFunction FitContent(LengthPercentage lp) =>
        new(MinTrackSizingFunction.Auto, MaxTrackSizingFunction.FitContent(lp));
}

[Union]
public readonly partial struct TrackSizingFunction<TList>
    where TList : IAsReadOnlySpan<NonRepeatedTrackSizingFunction>
{
    [UnionTemplate]
    private interface Template
    {
        NonRepeatedTrackSizingFunction Single();
        void Repeat(GridTrackRepetition Repetition, TList Tracks);
    }

    public static TrackSizingFunction<TList> Zero = MakeSingle(NonRepeatedTrackSizingFunction.Zero);

    public static TrackSizingFunction<TList> Auto = MakeSingle(NonRepeatedTrackSizingFunction.Auto);
    public static TrackSizingFunction<TList> MinContent = MakeSingle(NonRepeatedTrackSizingFunction.MinContent);
    public static TrackSizingFunction<TList> MaxContent = MakeSingle(NonRepeatedTrackSizingFunction.MaxContent);

    public static TrackSizingFunction<TList> MakeFitContent(LengthPercentage lp) =>
        MakeSingle(NonRepeatedTrackSizingFunction.FitContent(lp));
}

public readonly record struct GridVec<T>(List<T> List) : IAsReadOnlySpan<T>
{
    public List<T> List { get; } = List;

    public ReadOnlySpan<T> AsReadOnlySpan => CollectionsMarshal.AsSpan(List);
}
