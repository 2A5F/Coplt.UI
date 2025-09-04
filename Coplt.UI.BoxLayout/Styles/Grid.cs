using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Coplt.UI.BoxLayouts.Utilities;
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

    public Line<GridPlacement> GridPlacement(AbsoluteAxis axis) => axis switch
    {
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

[Union2]
public readonly partial struct OriginZeroGridPlacement
{
    [UnionTemplate]
    private interface Template
    {
        void Auto();
        OriginZeroLine Line();
        ushort Span();
    }
}

[Union2]
public readonly partial struct GridPlacement
{
    [UnionTemplate]
    private interface Template
    {
        void Auto();
        GridLine Line();
        ushort Span();
    }
}

[Union2]
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

    public static MaxTrackSizingFunction Zero => MaxTrackSizingFunction.Fixed(0);

    public static MaxTrackSizingFunction FitContent(LengthPercentage lp) => lp.Tag switch
    {
        LengthPercentage.Tags.Fixed => MaxTrackSizingFunction.FitContentFixed(lp.Fixed),
        LengthPercentage.Tags.Percent => MaxTrackSizingFunction.FitContentPercent(lp.Percent),
        LengthPercentage.Tags.Calc => MaxTrackSizingFunction.Calc(lp.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator AnyLength(MaxTrackSizingFunction self) => self.Tag switch
    {
        Tags.Auto => AnyLength.Auto,
        Tags.Fixed => AnyLength.Fixed(self.Fixed),
        Tags.Percent => AnyLength.Percent(self.Percent),
        Tags.Calc => AnyLength.Calc(self.Calc),
        Tags.Fr => AnyLength.Fr(self.Fr),
        Tags.MinContent => AnyLength.MinContent,
        Tags.MaxContent => AnyLength.MaxContent,
        Tags.FitContentFixed => AnyLength.FitContentFixed(self.FitContentFixed),
        Tags.FitContentPercent => AnyLength.FitContentPercent(self.FitContentPercent),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator MaxTrackSizingFunction(LengthPercentage self) => self.Tag switch
    {
        LengthPercentage.Tags.Fixed => MaxTrackSizingFunction.Fixed(self.Fixed),
        LengthPercentage.Tags.Percent => MaxTrackSizingFunction.Percent(self.Percent),
        LengthPercentage.Tags.Calc => MaxTrackSizingFunction.Calc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator MaxTrackSizingFunction(LengthPercentageAuto self) => self.Tag switch
    {
        LengthPercentageAuto.Tags.Auto => MaxTrackSizingFunction.Auto,
        LengthPercentageAuto.Tags.Fixed => MaxTrackSizingFunction.Fixed(self.Fixed),
        LengthPercentageAuto.Tags.Percent => MaxTrackSizingFunction.Percent(self.Percent),
        LengthPercentageAuto.Tags.Calc => MaxTrackSizingFunction.Calc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };
}

[Union2]
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

    public static MinTrackSizingFunction Zero => MinTrackSizingFunction.Fixed(0);

    public static implicit operator AnyLength(MinTrackSizingFunction self) => self.Tag switch
    {
        Tags.Auto => AnyLength.Auto,
        Tags.Fixed => AnyLength.Fixed(self.Fixed),
        Tags.Percent => AnyLength.Percent(self.Percent),
        Tags.Calc => AnyLength.Calc(self.Calc),
        Tags.Fr => AnyLength.Fr(self.Fr),
        Tags.MinContent => AnyLength.MinContent,
        Tags.MaxContent => AnyLength.MaxContent,
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator MinTrackSizingFunction(LengthPercentage self) => self.Tag switch
    {
        LengthPercentage.Tags.Fixed => MinTrackSizingFunction.Fixed(self.Fixed),
        LengthPercentage.Tags.Percent => MinTrackSizingFunction.Percent(self.Percent),
        LengthPercentage.Tags.Calc => MinTrackSizingFunction.Calc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static implicit operator MinTrackSizingFunction(LengthPercentageAuto self) => self.Tag switch
    {
        LengthPercentageAuto.Tags.Auto => MinTrackSizingFunction.Auto,
        LengthPercentageAuto.Tags.Fixed => MinTrackSizingFunction.Fixed(self.Fixed),
        LengthPercentageAuto.Tags.Percent => MinTrackSizingFunction.Percent(self.Percent),
        LengthPercentageAuto.Tags.Calc => MinTrackSizingFunction.Calc(self.Calc),
        _ => throw new ArgumentOutOfRangeException()
    };
}

[Union2]
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

[Union2]
public readonly partial struct TrackSizingFunction<TList>
    where TList : IAsReadOnlySpan<NonRepeatedTrackSizingFunction>
{
    [UnionTemplate]
    private interface Template
    {
        NonRepeatedTrackSizingFunction Single();
        void Repeat(GridTrackRepetition Repetition, TList Tracks);
    }

    public static TrackSizingFunction<TList> Zero = TrackSizingFunction<TList>.Single(NonRepeatedTrackSizingFunction.Zero);

    public static TrackSizingFunction<TList> Auto = TrackSizingFunction<TList>.Single(NonRepeatedTrackSizingFunction.Auto);
    public static TrackSizingFunction<TList> MinContent = TrackSizingFunction<TList>.Single(NonRepeatedTrackSizingFunction.MinContent);
    public static TrackSizingFunction<TList> MaxContent = TrackSizingFunction<TList>.Single(NonRepeatedTrackSizingFunction.MaxContent);
}

public static partial class TrackSizingFunctionExtensions
{
    extension<TList>(global::Coplt.UI.Styles.TrackSizingFunction<TList>)
        where TList : global::Coplt.UI.BoxLayouts.Utilities.IAsReadOnlySpan<global::Coplt.UI.Styles.NonRepeatedTrackSizingFunction>
    {
        public static TrackSizingFunction<TList> FitContent(LengthPercentage lp) =>
            TrackSizingFunction<TList>.Single(NonRepeatedTrackSizingFunction.FitContent(lp));
    }
}

public readonly record struct GridVec<T>(List<T> List) : IAsReadOnlySpan<T>
{
    public List<T> List { get; } = List;

    public ReadOnlySpan<T> AsReadOnlySpan => CollectionsMarshal.AsSpan(List);
}
