using Coplt.UI.Core.Styles;
using Coplt.UI.Styles;

namespace Coplt.UI.Trees.Datas;

public record struct CommonStyleData()
{
    public int ZIndex;

    public float Opacity = 1;

    public float ScrollBarSize = 0;

    public float BoxColorR = 1;
    public float BoxColorG = 1;
    public float BoxColorB = 1;
    public float BoxColorA = 0;

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

    public float FlexGrow = 0;
    public float FlexShrink = 1;
    public float FlexBasisValue = 0;

    public Visible Visible = Visible.Visible;

    public Container Container = Container.Flex;
    public BoxSizing BoxSizing = BoxSizing.BorderBox;
    public Overflow OverflowX = Overflow.Visible;
    public Overflow OverflowY = Overflow.Visible;
    public Position Position = Position.Relative;

    public LengthType InsertTop = LengthType.Auto;
    public LengthType InsertRight = LengthType.Auto;
    public LengthType InsertBottom = LengthType.Auto;
    public LengthType InsertLeft = LengthType.Auto;

    public LengthType Width = LengthType.Auto;
    public LengthType Height = LengthType.Auto;

    public LengthType MinWidth = LengthType.Auto;
    public LengthType MinHeight = LengthType.Auto;

    public LengthType MaxWidth = LengthType.Auto;
    public LengthType MaxHeight = LengthType.Auto;

    public bool HasAspectRatio = false;

    public LengthType MarginTop = LengthType.Fixed;
    public LengthType MarginRight = LengthType.Fixed;
    public LengthType MarginBottom = LengthType.Fixed;
    public LengthType MarginLeft = LengthType.Fixed;

    public LengthType PaddingTop = LengthType.Fixed;
    public LengthType PaddingRight = LengthType.Fixed;
    public LengthType PaddingBottom = LengthType.Fixed;
    public LengthType PaddingLeft = LengthType.Fixed;

    public LengthType BorderTop = LengthType.Fixed;
    public LengthType BorderRight = LengthType.Fixed;
    public LengthType BorderBottom = LengthType.Fixed;
    public LengthType BorderLeft = LengthType.Fixed;

    public AlignType AlignItems = AlignType.None;
    public AlignType AlignSelf = AlignType.None;
    public AlignType JustifyItems = AlignType.None;
    public AlignType JustifySelf = AlignType.None;
    public AlignType AlignContent = AlignType.None;
    public AlignType JustifyContent = AlignType.None;

    public LengthType GapX = LengthType.Fixed;
    public LengthType GapY = LengthType.Fixed;

    public FlexDirection FlexDirection = FlexDirection.Column;
    public FlexWrap FlexWrap = FlexWrap.NoWrap;
    public LengthType FlexBasis = LengthType.Auto;

    public GridAutoFlow GridAutoFlow = GridAutoFlow.Row;

    public TextAlign TextAlign = TextAlign.Auto;

    public GridPlacement GridRowStart;
    public GridPlacement GridRowEnd;
    public GridPlacement GridColumnStart;
    public GridPlacement GridColumnEnd;
}
