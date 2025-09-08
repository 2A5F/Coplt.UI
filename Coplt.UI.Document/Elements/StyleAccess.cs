using System.Runtime.CompilerServices;
using Coplt.UI.Document.Interfaces;
using Coplt.UI.Styles;

namespace Coplt.UI.Elements;

public static class CommonStyleExtensions
{
    extension<TRd, TEd>(UIElement<TRd, TEd> element)
        where TRd : IRenderData, new() where TEd : new()
    {
        private ref CommonStyle RefStyle => ref Unsafe.AsRef(in element.CommonStyle);

        // todo set mark dirty

        public Display Display
        {
            get => element.RefStyle.Display;
            set => element.RefStyle.Display = value;
        }

        public BoxSizing BoxSizing
        {
            get => element.RefStyle.BoxSizing;
            set => element.RefStyle.BoxSizing = value;
        }

        public Point<Overflow> Overflow
        {
            get => element.RefStyle.Overflow;
            set => element.RefStyle.Overflow = value;
        }

        public Overflow OverflowX
        {
            get => element.RefStyle.Overflow.X;
            set => element.RefStyle.Overflow.X = value;
        }

        public Overflow OverflowY
        {
            get => element.RefStyle.Overflow.Y;
            set => element.RefStyle.Overflow.Y = value;
        }

        public Position Position
        {
            get => element.RefStyle.Position;
            set => element.RefStyle.Position = value;
        }

        public Rect<LengthPercentageAuto> Inset
        {
            get => element.RefStyle.Inset;
            set => element.RefStyle.Inset = value;
        }

        public LengthPercentageAuto Top
        {
            get => element.RefStyle.Inset.Top;
            set => element.RefStyle.Inset.Top = value;
        }

        public LengthPercentageAuto Right
        {
            get => element.RefStyle.Inset.Right;
            set => element.RefStyle.Inset.Right = value;
        }

        public LengthPercentageAuto Bottom
        {
            get => element.RefStyle.Inset.Bottom;
            set => element.RefStyle.Inset.Bottom = value;
        }

        public LengthPercentageAuto Left
        {
            get => element.RefStyle.Inset.Left;
            set => element.RefStyle.Inset.Left = value;
        }

        public Size<Dimension> Size
        {
            get => element.RefStyle.Size;
            set => element.RefStyle.Size = value;
        }

        public Dimension Width
        {
            get => element.RefStyle.Size.Width;
            set => element.RefStyle.Size.Width = value;
        }

        public Dimension Height
        {
            get => element.RefStyle.Size.Height;
            set => element.RefStyle.Size.Height = value;
        }

        public Size<Dimension> MinSize
        {
            get => element.RefStyle.MinSize;
            set => element.RefStyle.MinSize = value;
        }

        public Dimension MinWidth
        {
            get => element.RefStyle.MinSize.Width;
            set => element.RefStyle.MinSize.Width = value;
        }

        public Dimension MinHeight
        {
            get => element.RefStyle.MinSize.Height;
            set => element.RefStyle.MinSize.Height = value;
        }

        public Size<Dimension> MaxSize
        {
            get => element.RefStyle.MaxSize;
            set => element.RefStyle.MaxSize = value;
        }

        public Dimension MaxWidth
        {
            get => element.RefStyle.MaxSize.Width;
            set => element.RefStyle.MaxSize.Width = value;
        }

        public Dimension MaxHeight
        {
            get => element.RefStyle.MaxSize.Height;
            set => element.RefStyle.MaxSize.Height = value;
        }

        public float? AspectRatio
        {
            get => element.RefStyle.AspectRatio;
            set => element.RefStyle.AspectRatio = value;
        }

        public Rect<LengthPercentageAuto> Margin
        {
            get => element.RefStyle.Margin;
            set => element.RefStyle.Margin = value;
        }

        public LengthPercentageAuto MarginTop
        {
            get => element.RefStyle.Margin.Top;
            set => element.RefStyle.Margin.Top = value;
        }

        public LengthPercentageAuto MarginRight
        {
            get => element.RefStyle.Margin.Right;
            set => element.RefStyle.Margin.Right = value;
        }

        public LengthPercentageAuto MarginBottom
        {
            get => element.RefStyle.Margin.Bottom;
            set => element.RefStyle.Margin.Bottom = value;
        }

        public LengthPercentageAuto MarginLeft
        {
            get => element.RefStyle.Margin.Left;
            set => element.RefStyle.Margin.Left = value;
        }

        public Rect<LengthPercentage> Padding
        {
            get => element.RefStyle.Padding;
            set => element.RefStyle.Padding = value;
        }

        public LengthPercentage PaddingTop
        {
            get => element.RefStyle.Padding.Top;
            set => element.RefStyle.Padding.Top = value;
        }

        public LengthPercentage PaddingRight
        {
            get => element.RefStyle.Padding.Right;
            set => element.RefStyle.Padding.Right = value;
        }

        public LengthPercentage PaddingBottom
        {
            get => element.RefStyle.Padding.Bottom;
            set => element.RefStyle.Padding.Bottom = value;
        }

        public LengthPercentage PaddingLeft
        {
            get => element.RefStyle.Padding.Left;
            set => element.RefStyle.Padding.Left = value;
        }

        public Rect<LengthPercentage> Border
        {
            get => element.RefStyle.Border;
            set => element.RefStyle.Border = value;
        }

        public LengthPercentage BorderTop
        {
            get => element.RefStyle.Border.Top;
            set => element.RefStyle.Border.Top = value;
        }

        public LengthPercentage BorderRight
        {
            get => element.RefStyle.Border.Right;
            set => element.RefStyle.Border.Right = value;
        }

        public LengthPercentage BorderBottom
        {
            get => element.RefStyle.Border.Bottom;
            set => element.RefStyle.Border.Bottom = value;
        }

        public LengthPercentage BorderLeft
        {
            get => element.RefStyle.Border.Left;
            set => element.RefStyle.Border.Left = value;
        }

        public AlignItems? AlignItems
        {
            get => element.RefStyle.AlignItems;
            set => element.RefStyle.AlignItems = value;
        }

        public AlignSelf? AlignSelf
        {
            get => element.RefStyle.AlignSelf;
            set => element.RefStyle.AlignSelf = value;
        }

        public JustifyItems? JustifyItems
        {
            get => element.RefStyle.JustifyItems;
            set => element.RefStyle.JustifyItems = value;
        }

        public JustifySelf? JustifySelf
        {
            get => element.RefStyle.JustifySelf;
            set => element.RefStyle.JustifySelf = value;
        }

        public AlignContent? AlignContent
        {
            get => element.RefStyle.AlignContent;
            set => element.RefStyle.AlignContent = value;
        }

        public JustifyContent? JustifyContent
        {
            get => element.RefStyle.JustifyContent;
            set => element.RefStyle.JustifyContent = value;
        }

        public Size<LengthPercentage> Gap
        {
            get => element.RefStyle.Gap;
            set => element.RefStyle.Gap = value;
        }

        public LengthPercentage GapX
        {
            get => element.RefStyle.Gap.Width;
            set => element.RefStyle.Gap.Width = value;
        }

        public LengthPercentage GapY
        {
            get => element.RefStyle.Gap.Height;
            set => element.RefStyle.Gap.Height = value;
        }

        public TextAlign TextAlign
        {
            get => element.RefStyle.TextAlign;
            set => element.RefStyle.TextAlign = value;
        }

        public FlexDirection FlexDirection
        {
            get => element.RefStyle.FlexDirection;
            set => element.RefStyle.FlexDirection = value;
        }

        public FlexWrap FlexWrap
        {
            get => element.RefStyle.FlexWrap;
            set => element.RefStyle.FlexWrap = value;
        }

        public Dimension FlexBias
        {
            get => element.RefStyle.FlexBias;
            set => element.RefStyle.FlexBias = value;
        }

        public float FlexGrow
        {
            get => element.RefStyle.FlexGrow;
            set => element.RefStyle.FlexGrow = value;
        }

        public float FlexShrink
        {
            get => element.RefStyle.FlexShrink;
            set => element.RefStyle.FlexShrink = value;
        }
    }
}
