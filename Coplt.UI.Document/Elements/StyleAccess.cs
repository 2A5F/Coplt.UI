using System.Runtime.CompilerServices;
using Coplt.UI.Styles;

namespace Coplt.UI.Elements;

public readonly struct StyleAccess<TRd, TEd>(UIElement<TRd, TEd> Element)
    where TRd : new() where TEd : new()
{
    public readonly UIElement<TRd, TEd> Element = Element;
}

public static class CommonStyleExtensions
{
    extension<TRd, TEd>(ref StyleAccess<TRd, TEd> access) where TRd : new() where TEd : new()
    {
        private ref CommonStyle Style => ref Unsafe.AsRef(in access.Element.CommonStyle);

        // todo set mark dirty

        public Display Display
        {
            get => access.Style.Display;
            set => access.Style.Display = value;
        }

        public BoxSizing BoxSizing
        {
            get => access.Style.BoxSizing;
            set => access.Style.BoxSizing = value;
        }

        public Point<Overflow> Overflow
        {
            get => access.Style.Overflow;
            set => access.Style.Overflow = value;
        }

        public Overflow OverflowX
        {
            get => access.Style.Overflow.X;
            set => access.Style.Overflow.X = value;
        }

        public Overflow OverflowY
        {
            get => access.Style.Overflow.Y;
            set => access.Style.Overflow.Y = value;
        }

        public Position Position
        {
            get => access.Style.Position;
            set => access.Style.Position = value;
        }

        public Rect<LengthPercentageAuto> Inset
        {
            get => access.Style.Inset;
            set => access.Style.Inset = value;
        }

        public LengthPercentageAuto Top
        {
            get => access.Style.Inset.Top;
            set => access.Style.Inset.Top = value;
        }

        public LengthPercentageAuto Right
        {
            get => access.Style.Inset.Right;
            set => access.Style.Inset.Right = value;
        }

        public LengthPercentageAuto Bottom
        {
            get => access.Style.Inset.Bottom;
            set => access.Style.Inset.Bottom = value;
        }

        public LengthPercentageAuto Left
        {
            get => access.Style.Inset.Left;
            set => access.Style.Inset.Left = value;
        }

        public Size<Dimension> Size
        {
            get => access.Style.Size;
            set => access.Style.Size = value;
        }

        public Dimension Width
        {
            get => access.Style.Size.Width;
            set => access.Style.Size.Width = value;
        }

        public Dimension Height
        {
            get => access.Style.Size.Height;
            set => access.Style.Size.Height = value;
        }

        public Size<Dimension> MinSize
        {
            get => access.Style.MinSize;
            set => access.Style.MinSize = value;
        }

        public Dimension MinWidth
        {
            get => access.Style.MinSize.Width;
            set => access.Style.MinSize.Width = value;
        }

        public Dimension MinHeight
        {
            get => access.Style.MinSize.Height;
            set => access.Style.MinSize.Height = value;
        }

        public Size<Dimension> MaxSize
        {
            get => access.Style.MaxSize;
            set => access.Style.MaxSize = value;
        }

        public Dimension MaxWidth
        {
            get => access.Style.MaxSize.Width;
            set => access.Style.MaxSize.Width = value;
        }

        public Dimension MaxHeight
        {
            get => access.Style.MaxSize.Height;
            set => access.Style.MaxSize.Height = value;
        }

        public float? AspectRatio
        {
            get => access.Style.AspectRatio;
            set => access.Style.AspectRatio = value;
        }

        public Rect<LengthPercentageAuto> Margin
        {
            get => access.Style.Margin;
            set => access.Style.Margin = value;
        }

        public LengthPercentageAuto MarginTop
        {
            get => access.Style.Margin.Top;
            set => access.Style.Margin.Top = value;
        }

        public LengthPercentageAuto MarginRight
        {
            get => access.Style.Margin.Right;
            set => access.Style.Margin.Right = value;
        }

        public LengthPercentageAuto MarginBottom
        {
            get => access.Style.Margin.Bottom;
            set => access.Style.Margin.Bottom = value;
        }

        public LengthPercentageAuto MarginLeft
        {
            get => access.Style.Margin.Left;
            set => access.Style.Margin.Left = value;
        }

        public Rect<LengthPercentage> Padding
        {
            get => access.Style.Padding;
            set => access.Style.Padding = value;
        }

        public LengthPercentage PaddingTop
        {
            get => access.Style.Padding.Top;
            set => access.Style.Padding.Top = value;
        }

        public LengthPercentage PaddingRight
        {
            get => access.Style.Padding.Right;
            set => access.Style.Padding.Right = value;
        }

        public LengthPercentage PaddingBottom
        {
            get => access.Style.Padding.Bottom;
            set => access.Style.Padding.Bottom = value;
        }

        public LengthPercentage PaddingLeft
        {
            get => access.Style.Padding.Left;
            set => access.Style.Padding.Left = value;
        }

        public Rect<LengthPercentage> Border
        {
            get => access.Style.Border;
            set => access.Style.Border = value;
        }

        public LengthPercentage BorderTop
        {
            get => access.Style.Border.Top;
            set => access.Style.Border.Top = value;
        }

        public LengthPercentage BorderRight
        {
            get => access.Style.Border.Right;
            set => access.Style.Border.Right = value;
        }

        public LengthPercentage BorderBottom
        {
            get => access.Style.Border.Bottom;
            set => access.Style.Border.Bottom = value;
        }

        public LengthPercentage BorderLeft
        {
            get => access.Style.Border.Left;
            set => access.Style.Border.Left = value;
        }

        public AlignItems? AlignItems
        {
            get => access.Style.AlignItems;
            set => access.Style.AlignItems = value;
        }

        public AlignSelf? AlignSelf
        {
            get => access.Style.AlignSelf;
            set => access.Style.AlignSelf = value;
        }

        public JustifyItems? JustifyItems
        {
            get => access.Style.JustifyItems;
            set => access.Style.JustifyItems = value;
        }

        public JustifySelf? JustifySelf
        {
            get => access.Style.JustifySelf;
            set => access.Style.JustifySelf = value;
        }

        public AlignContent? AlignContent
        {
            get => access.Style.AlignContent;
            set => access.Style.AlignContent = value;
        }

        public JustifyContent? JustifyContent
        {
            get => access.Style.JustifyContent;
            set => access.Style.JustifyContent = value;
        }

        public Size<LengthPercentage> Gap
        {
            get => access.Style.Gap;
            set => access.Style.Gap = value;
        }

        public LengthPercentage GapX
        {
            get => access.Style.Gap.Width;
            set => access.Style.Gap.Width = value;
        }

        public LengthPercentage GapY
        {
            get => access.Style.Gap.Height;
            set => access.Style.Gap.Height = value;
        }

        public TextAlign TextAlign
        {
            get => access.Style.TextAlign;
            set => access.Style.TextAlign = value;
        }

        public FlexDirection FlexDirection
        {
            get => access.Style.FlexDirection;
            set => access.Style.FlexDirection = value;
        }

        public FlexWrap FlexWrap
        {
            get => access.Style.FlexWrap;
            set => access.Style.FlexWrap = value;
        }

        public Dimension FlexBias
        {
            get => access.Style.FlexBias;
            set => access.Style.FlexBias = value;
        }

        public float FlexGrow
        {
            get => access.Style.FlexGrow;
            set => access.Style.FlexGrow = value;
        }

        public float FlexShrink
        {
            get => access.Style.FlexShrink;
            set => access.Style.FlexShrink = value;
        }
    }
}
