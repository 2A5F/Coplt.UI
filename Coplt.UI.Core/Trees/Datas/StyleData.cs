using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Core.Styles;
using Coplt.UI.Native;
using Coplt.UI.Styles;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public partial record struct StyleData()
{
    [Drop]
    public NativeArc<GridContainerStyle> Grid;
    
    /// <summary>
    /// Optional, default use system font fallback
    /// </summary>
    [Drop]
    [ComType<Ptr<IFontFallback>>]
    public Rc<IFontFallback> FontFallback;
    public LanguageId LanguageId;
    
    public int ZIndex;

    public float TextColorR = 1;
    public float TextColorG = 1;
    public float TextColorB = 1;
    public float TextColorA = 1;

    public float Opacity = 1;

    public float BackgroundColorR = 1;
    public float BackgroundColorG = 1;
    public float BackgroundColorB = 1;
    public float BackgroundColorA = 0;

    public float ScrollBarSize = 0;

    public float WidthValue = 0;
    public float HeightValue = 0;

    public float MinWidthValue = 0;
    public float MinHeightValue = 0;

    public float MaxWidthValue = 0;
    public float MaxHeightValue = 0;

    public float AspectRatioValue = 0;

    public float InsertTopValue = 0;
    public float InsertRightValue = 0;
    public float InsertBottomValue = 0;
    public float InsertLeftValue = 0;

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

    public float TextSizeValue = 16;
    public float TabSizeValue = 4;
    
    public GridPlacement GridRowStart = GridPlacement.Auto;
    public GridPlacement GridRowEnd = GridPlacement.Auto;
    public GridPlacement GridColumnStart = GridPlacement.Auto;
    public GridPlacement GridColumnEnd = GridPlacement.Auto;

    public Visible Visible = Visible.Visible;
    public Position Position = Position.Relative;
    public Container Container = Container.Flex;
    public TextMode TextMode = TextMode.Block;
    public BoxSizing BoxSizing = BoxSizing.BorderBox;
    public FloatInText Float = FloatInText.None; // not support yet
    
    public CursorType Cursor = CursorType.Default;
    public PointerEvents PointerEvents = PointerEvents.Auto;
    
    public Overflow OverflowX = Overflow.Visible;
    public Overflow OverflowY = Overflow.Visible;

    public LengthType Width = LengthType.Auto;
    public LengthType Height = LengthType.Auto;

    public LengthType MinWidth = LengthType.Auto;
    public LengthType MinHeight = LengthType.Auto;

    public LengthType MaxWidth = LengthType.Auto;
    public LengthType MaxHeight = LengthType.Auto;

    public LengthType InsertTop = LengthType.Auto;
    public LengthType InsertRight = LengthType.Auto;
    public LengthType InsertBottom = LengthType.Auto;
    public LengthType InsertLeft = LengthType.Auto;

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

    public AlignType AlignSelf = AlignType.None;
    public AlignType JustifySelf = AlignType.None;

    public LengthType FlexBasis = LengthType.Auto;

    public TextAlign TextAlign = TextAlign.Auto;

    public LengthType TextSize = LengthType.Fixed;
    public LengthType TabSize = LengthType.Percent;
    
    public FontWeight FontWeight = FontWeight.Normal;
    public FontStyle FontStyle = FontStyle.Normal;
    public FontStretch FontStretch = FontStretch.Normal;

    public TextDirection TextDirection = TextDirection.Forward;
    public WritingDirection WritingDirection = WritingDirection.Horizontal;
    public WhiteSpaceMerge WhiteSpaceMerge = WhiteSpaceMerge.Keep;
    public WhiteSpaceWrap WhiteSpaceWrap = WhiteSpaceWrap.WrapLine;
    public TextWrap TextWrap = TextWrap.Wrap;
    public WordBreak WordBreak = WordBreak.Auto;
    public TextOrientation TextOrientation = TextOrientation.Mixed;
    public TextOverflow TextOverflow = TextOverflow.Clip;
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
