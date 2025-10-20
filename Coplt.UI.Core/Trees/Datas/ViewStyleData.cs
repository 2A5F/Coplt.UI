using Coplt.UI.Styles;

namespace Coplt.UI.Trees.Datas;

public record struct ViewStyleData()
{
    public float ColorR = 1;
    public float ColorG = 1;
    public float ColorB = 1;
    public float ColorA = 0;

    public float InsertTopValue = 0;
    public float InsertRightValue = 0;
    public float InsertBottomValue = 0;
    public float InsertLeftValue = 0;

    public float WidthValue = 0;
    public float HeightValue = 0;

    public float MinWidthValue = 0;
    public float MinHeightValue = 0;

    public float MaxWidthValue = 0;
    public float MaxHeightValue = 0;

    public float AspectRatioValue = 0;

    public float MarginTopValue = 0;
    public float MarginRightValue = 0;
    public float MarginBottomValue = 0;
    public float MarginLeftValue = 0;

    public float PaddingTopValue = 0;
    public float PaddingRightValue = 0;
    public float PaddingBottomValue = 0;
    public float PaddingLeftValue = 0;

    public float BorderTopValue = 0;
    public float BorderRightValue = 0;
    public float BorderBottomValue = 0;
    public float BorderLeftValue = 0;

    public float GapXValue = 0;
    public float GapYValue = 0;

    public Display Display = Display.Flex;
    public BoxSizing BoxSizing = BoxSizing.BorderBox;
    public Overflow OverflowX = Overflow.Visible;
    public Overflow OverflowY = Overflow.Visible;
    public Position Position = Position.Relative;

    public LengthType InsertTop = LengthType.Auto;
    public LengthType InsertRight = LengthType.Auto;
    public LengthType InsertBottomV = LengthType.Auto;
    public LengthType InsertLeft = LengthType.Auto;

    public LengthType Width = LengthType.Auto;
    public LengthType Height = LengthType.Auto;

    public LengthType MinWidth = LengthType.Auto;
    public LengthType MinHeight = LengthType.Auto;

    public LengthType MaxMinWidth = LengthType.Auto;
    public LengthType MaxMinHeight = LengthType.Auto;

    public bool HasAspectRatio = false;

    public LengthType MarginTop = LengthType.Fixed;
    public LengthType MarginRight = LengthType.Fixed;
    public LengthType MarginBottomV = LengthType.Fixed;
    public LengthType MarginLeft = LengthType.Fixed;

    public LengthType PaddingTop = LengthType.Fixed;
    public LengthType PaddingRight = LengthType.Fixed;
    public LengthType PaddingBottomV = LengthType.Fixed;
    public LengthType PaddingLeft = LengthType.Fixed;

    public LengthType BorderTop = LengthType.Fixed;
    public LengthType BorderRight = LengthType.Fixed;
    public LengthType BorderBottomV = LengthType.Fixed;
    public LengthType BorderLeft = LengthType.Fixed;

    public AlignType AlignItems = AlignType.None;
    public AlignType AlignSelf = AlignType.None;
    public AlignType JustifyItems = AlignType.None;
    public AlignType JustifySelf = AlignType.None;
    public AlignType AlignContent = AlignType.None;
    public AlignType JustifyContent = AlignType.None;

    public LengthType GapX = LengthType.Fixed;
    public LengthType GapY = LengthType.Fixed;
}
