using System;
using System.Runtime.CompilerServices;

namespace Coplt.UI.Styles;

public interface IFlexContainerStyle : ICoreStyle
{
    public FlexDirection FlexDirection => BoxStyle.Default.FlexDirection;
    public FlexWrap FlexWrap => BoxStyle.Default.FlexWrap;
    public Size<LengthPercentage> Gap => BoxStyle.Default.Gap;

    public AlignContent? AlignContent => BoxStyle.Default.AlignContent;
    public AlignItems? AlignItems => BoxStyle.Default.AlignItems;
    public JustifyContent? JustifyContent => BoxStyle.Default.JustifyContent;
}

public interface IFlexItemStyle : ICoreStyle
{
    public Dimension FlexBias => BoxStyle.Default.FlexBias;
    public float FlexGrow => BoxStyle.Default.FlexGrow;
    public float FlexShrink => BoxStyle.Default.FlexShrink;

    public AlignSelf? AlignSelf => BoxStyle.Default.AlignSelf;
}

public readonly ref struct RefFlexContainerStyle<T>(ref readonly T Target) : IFlexContainerStyle
    where T : IFlexContainerStyle
{
    public readonly ref readonly T Target = ref Target;
    
    public BoxGenerationMode BoxGenerationMode => Target.BoxGenerationMode;
    public bool IsBlock => Target.IsBlock;
    public bool IsCompressibleReplaced => Target.IsCompressibleReplaced;
    public BoxSizing BoxSizing => Target.BoxSizing;
    public Point<Overflow> Overflow => Target.Overflow;
    public float ScrollbarWidth => Target.ScrollbarWidth;
    public Position Position => Target.Position;
    public Rect<LengthPercentageAuto> Inset => Target.Inset;
    public Size<Dimension> Size => Target.Size;
    public Size<Dimension> MinSize => Target.MinSize;
    public Size<Dimension> MaxSize => Target.MaxSize;
    public float? AspectRatio => Target.AspectRatio;
    public Rect<LengthPercentageAuto> Margin => Target.Margin;
    public Rect<LengthPercentage> Padding => Target.Padding;
    public Rect<LengthPercentage> Border => Target.Border;
    public FlexDirection FlexDirection => Target.FlexDirection;
    public FlexWrap FlexWrap => Target.FlexWrap;
    public Size<LengthPercentage> Gap => Target.Gap;
    public AlignContent? AlignContent => Target.AlignContent;
    public AlignItems? AlignItems => Target.AlignItems;
    public JustifyContent? JustifyContent => Target.JustifyContent;
}

public readonly ref struct RefFlexItemStyle<T>(ref readonly T Target) : IFlexItemStyle
    where T : IFlexItemStyle
{
    public readonly ref readonly T Target = ref Target;

    public BoxGenerationMode BoxGenerationMode => Target.BoxGenerationMode;
    public bool IsBlock => Target.IsBlock;
    public bool IsCompressibleReplaced => Target.IsCompressibleReplaced;
    public BoxSizing BoxSizing => Target.BoxSizing;
    public Point<Overflow> Overflow => Target.Overflow;
    public float ScrollbarWidth => Target.ScrollbarWidth;
    public Position Position => Target.Position;
    public Rect<LengthPercentageAuto> Inset => Target.Inset;
    public Size<Dimension> Size => Target.Size;
    public Size<Dimension> MinSize => Target.MinSize;
    public Size<Dimension> MaxSize => Target.MaxSize;
    public float? AspectRatio => Target.AspectRatio;
    public Rect<LengthPercentageAuto> Margin => Target.Margin;
    public Rect<LengthPercentage> Padding => Target.Padding;
    public Rect<LengthPercentage> Border => Target.Border;
    public Dimension FlexBias => Target.FlexBias;
    public float FlexGrow => Target.FlexGrow;
    public float FlexShrink => Target.FlexShrink;
    public AlignSelf? AlignSelf => Target.AlignSelf;
}

public enum FlexWrap : byte
{
    NoWrap,
    Wrap,
    WrapReverse,
}

public enum FlexDirection : byte
{
    Column,
    Row,
    ColumnReverse,
    RowReverse,
}

public static partial class BoxStyleExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRow(this FlexDirection self) => self is FlexDirection.Row or FlexDirection.RowReverse;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsColumn(this FlexDirection self) => self is FlexDirection.Column or FlexDirection.ColumnReverse;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReverse(this FlexDirection self) => self is FlexDirection.RowReverse or FlexDirection.ColumnReverse;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AbsoluteAxis MainAxis(this FlexDirection self) => self switch
    {
        FlexDirection.Row or FlexDirection.RowReverse => AbsoluteAxis.Horizontal,
        FlexDirection.Column or FlexDirection.ColumnReverse => AbsoluteAxis.Vertical,
        _ => throw new ArgumentOutOfRangeException(nameof(self), self, null)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AbsoluteAxis CrossAxis(this FlexDirection self) => self switch
    {
        FlexDirection.Row or FlexDirection.RowReverse => AbsoluteAxis.Vertical,
        FlexDirection.Column or FlexDirection.ColumnReverse => AbsoluteAxis.Horizontal,
        _ => throw new ArgumentOutOfRangeException(nameof(self), self, null)
    };
}
