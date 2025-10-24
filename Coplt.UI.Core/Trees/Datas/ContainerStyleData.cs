using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Core.Styles;
using Coplt.UI.Styles;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public partial record struct ContainerStyleData()
{
    [Drop]
    public NativeArc<GridContainerStyle> Grid;
    [Drop]
    public NativeArc<FontSet> FontSet;

    public float ScrollBarSize = 0;

    public float WidthValue = 0;
    public float HeightValue = 0;

    public float MinWidthValue = 0;
    public float MinHeightValue = 0;

    public float MaxWidthValue = 0;
    public float MaxHeightValue = 0;

    public float AspectRatioValue = 0;

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

    public float TextColorR = 1;
    public float TextColorG = 1;
    public float TextColorB = 1;
    public float TextColorA = 1;

    public float TextSizeValue = 16;
    public float TabSizeValue = 4;

    public Container Container = Container.Flex;
    public BoxSizing BoxSizing = BoxSizing.BorderBox;
    public Overflow OverflowX = Overflow.Visible;
    public Overflow OverflowY = Overflow.Visible;

    public LengthType Width = LengthType.Auto;
    public LengthType Height = LengthType.Auto;

    public LengthType MinWidth = LengthType.Auto;
    public LengthType MinHeight = LengthType.Auto;

    public LengthType MaxWidth = LengthType.Auto;
    public LengthType MaxHeight = LengthType.Auto;

    public LengthType PaddingTop = LengthType.Fixed;
    public LengthType PaddingRight = LengthType.Fixed;
    public LengthType PaddingBottom = LengthType.Fixed;
    public LengthType PaddingLeft = LengthType.Fixed;

    public LengthType BorderTop = LengthType.Fixed;
    public LengthType BorderRight = LengthType.Fixed;
    public LengthType BorderBottom = LengthType.Fixed;
    public LengthType BorderLeft = LengthType.Fixed;

    public bool HasAspectRatio = false;

    public FlexDirection FlexDirection = FlexDirection.Column;
    public FlexWrap FlexWrap = FlexWrap.NoWrap;

    public GridAutoFlow GridAutoFlow = GridAutoFlow.Row;

    public LengthType GapX = LengthType.Fixed;
    public LengthType GapY = LengthType.Fixed;

    public AlignType AlignContent = AlignType.None;
    public AlignType JustifyContent = AlignType.None;
    public AlignType AlignItems = AlignType.None;
    public AlignType JustifyItems = AlignType.None;

    public TextAlign TextAlign = TextAlign.Auto;

    public LengthType TextSize = LengthType.Fixed;
    public LengthType TabSize = LengthType.Percent;
}

[Dropping]
public partial record struct GridContainerStyle
{
    [Drop]
    public NativeList<GridTemplateComponent> GridTemplateRows;
    [Drop]
    public NativeList<GridTemplateComponent> GridTemplateColumns;
    [Drop]
    public NativeList<TrackSizingFunction> GridAutoRows;
    [Drop]
    public NativeList<TrackSizingFunction> GridAutoColumns;

    [Drop]
    public NativeList<GridTemplateArea> GridTemplateAreas;
    [Drop]
    public NativeList<NativeList<GridName>> GridTemplateColumnNames;
    [Drop]
    public NativeList<NativeList<GridName>> GridTemplateRowNames;
}
