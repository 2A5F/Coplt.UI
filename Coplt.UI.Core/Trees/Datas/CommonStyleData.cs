using Coplt.UI.Core.Styles;
using Coplt.UI.Styles;

namespace Coplt.UI.Trees.Datas;

public record struct CommonStyleData()
{
    public int ZIndex;

    public float Opacity = 1;

    public float BoxColorR = 1;
    public float BoxColorG = 1;
    public float BoxColorB = 1;
    public float BoxColorA = 0;

    public float InsertTopValue = 0;
    public float InsertRightValue = 0;
    public float InsertBottomValue = 0;
    public float InsertLeftValue = 0;

    public float MarginTopValue = 0;
    public float MarginRightValue = 0;
    public float MarginBottomValue = 0;
    public float MarginLeftValue = 0;

    public float BorderTopValue = 0;
    public float BorderRightValue = 0;
    public float BorderBottomValue = 0;
    public float BorderLeftValue = 0;

    public float FlexGrow = 0;
    public float FlexShrink = 1;
    public float FlexBasisValue = 0;

    public Visible Visible = Visible.Visible;

    public Position Position = Position.Relative;

    public LengthType InsertTop = LengthType.Auto;
    public LengthType InsertRight = LengthType.Auto;
    public LengthType InsertBottom = LengthType.Auto;
    public LengthType InsertLeft = LengthType.Auto;

    public LengthType MarginTop = LengthType.Fixed;
    public LengthType MarginRight = LengthType.Fixed;
    public LengthType MarginBottom = LengthType.Fixed;
    public LengthType MarginLeft = LengthType.Fixed;

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

    public LengthType FlexBasis = LengthType.Auto;

    public GridAutoFlow GridAutoFlow = GridAutoFlow.Row;

    public GridPlacement GridRowStart;
    public GridPlacement GridRowEnd;
    public GridPlacement GridColumnStart;
    public GridPlacement GridColumnEnd;
}
