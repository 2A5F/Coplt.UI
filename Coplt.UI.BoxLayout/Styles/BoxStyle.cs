using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Coplt.UI.Styles;

[StructLayout(LayoutKind.Auto)]
public record struct BoxStyle() : IBlockContainerStyle, IFlexContainerStyle, IFlexItemStyle, GridVecIGridContainerStyle, IGridItemStyle
{
    public static readonly BoxStyle Default = new();

    public Display Display { get; set; } = Display.Flex;
    public bool ItemIsTable { get; set; } = false;
    public bool ItemIsReplaced { get; set; } = false;
    public BoxSizing BoxSizing { get; set; } = BoxSizing.BorderBox;

    public Point<Overflow> Overflow { get; set; } = new(Styles.Overflow.Visible, Styles.Overflow.Visible);
    public float ScrollbarWidth { get; set; } = 0;

    public Position Position { get; set; } = Position.Relative;
    public Rect<LengthPercentageAuto> Inset { get; set; } = new(LengthPercentageAuto.Auto);

    public Size<Dimension> Size { get; set; } = new(Dimension.Auto, Dimension.Auto);
    public Size<Dimension> MinSize { get; set; } = new(Dimension.Auto, Dimension.Auto);
    public Size<Dimension> MaxSize { get; set; } = new(Dimension.Auto, Dimension.Auto);
    public float? AspectRatio { get; set; } = null;

    public Rect<LengthPercentageAuto> Margin { get; set; } = new(LengthPercentageAuto.Zero);
    public Rect<LengthPercentage> Padding { get; set; } = new(LengthPercentage.Zero);
    public Rect<LengthPercentage> Border { get; set; } = new(LengthPercentage.Zero);

    public AlignItems? AlignItems { get; set; } = null;
    public AlignSelf? AlignSelf { get; set; } = null;
    public JustifyItems? JustifyItems { get; set; } = null;
    public JustifySelf? JustifySelf { get; set; } = null;
    public AlignContent? AlignContent { get; set; } = null;
    public JustifyContent? JustifyContent { get; set; } = null;
    public Size<LengthPercentage> Gap { get; set; } = new(LengthPercentage.Zero);

    public TextAlign TextAlign { get; set; } = TextAlign.Auto;

    public FlexDirection FlexDirection { get; set; } = FlexDirection.Column;
    public FlexWrap FlexWrap { get; set; } = FlexWrap.NoWrap;
    public Dimension FlexBias { get; set; } = Dimension.Auto;
    public float FlexGrow { get; set; } = 0;
    public float FlexShrink { get; set; } = 1;

    public List<GridVecTrackSizingFunction>? GridTemplateRows { get; set; } = null;
    public List<GridVecTrackSizingFunction>? GridTemplateColumns { get; set; } = null;
    public List<NonRepeatedTrackSizingFunction>? GridAutoRows { get; set; } = null;
    public List<NonRepeatedTrackSizingFunction>? GridAutoColumns { get; set; } = null;
    public GridAutoFlow GridAutoFlow { get; set; } = GridAutoFlow.Column;

    public Line<GridPlacement> GridRow { get; set; } = new(Styles.GridPlacement.Auto);
    public Line<GridPlacement> GridColumn { get; set; } = new(Styles.GridPlacement.Auto);


    public BoxGenerationMode BoxGenerationMode => Display == Display.None ? BoxGenerationMode.None : BoxGenerationMode.Normal;
    public bool IsBlock => Display == Display.Block;
    public bool IsCompressibleReplaced => ItemIsReplaced;

    ReadOnlySpan<GridVecTrackSizingFunction> GridVecIGridContainerStyle.GridTemplateRows => CollectionsMarshal.AsSpan(GridTemplateRows);
    ReadOnlySpan<GridVecTrackSizingFunction> GridVecIGridContainerStyle.GridTemplateColumns => CollectionsMarshal.AsSpan(GridTemplateColumns);
    ReadOnlySpan<NonRepeatedTrackSizingFunction> GridVecIGridContainerStyle.GridAutoRows => CollectionsMarshal.AsSpan(GridAutoRows);
    ReadOnlySpan<NonRepeatedTrackSizingFunction> GridVecIGridContainerStyle.GridAutoColumns => CollectionsMarshal.AsSpan(GridAutoColumns);

    public ReadOnlySpan<GridVecTrackSizingFunction> GridTemplateTracks(AbsoluteAxis axis) => axis switch
    {
        AbsoluteAxis.Horizontal => CollectionsMarshal.AsSpan(GridTemplateColumns),
        AbsoluteAxis.Vertical => CollectionsMarshal.AsSpan(GridTemplateRows),
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
    };

    public AlignContent GridAlignContent(AbstractAxis axis) => axis switch
    {
        AbstractAxis.Inline => JustifyContent?.ToAlignContent() ?? Styles.AlignContent.Stretch,
        AbstractAxis.Block => AlignContent ?? Styles.AlignContent.Stretch,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
    };
    
    public Line<GridPlacement> GridPlacement(AbsoluteAxis axis) => axis switch
    {
        AbsoluteAxis.Horizontal => GridColumn,
        AbsoluteAxis.Vertical => GridRow,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
    };
}
