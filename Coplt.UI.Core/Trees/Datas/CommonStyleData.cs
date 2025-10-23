using Coplt.UI.Core.Styles;
using Coplt.UI.Styles;

namespace Coplt.UI.Trees.Datas;

public record struct CommonStyleData()
{
    public int ZIndex;

    public float Opacity = 1;

    public float BackgroundColorR = 1;
    public float BackgroundColorG = 1;
    public float BackgroundColorB = 1;
    public float BackgroundColorA = 0;

    public float InsertTopValue = 0;
    public float InsertRightValue = 0;
    public float InsertBottomValue = 0;
    public float InsertLeftValue = 0;

    public float MarginTopValue = 0;
    public float MarginRightValue = 0;
    public float MarginBottomValue = 0;
    public float MarginLeftValue = 0;

    public float FlexGrow = 0;
    public float FlexShrink = 1;
    public float FlexBasisValue = 0;

    public GridPlacement GridRowStart = GridPlacement.Auto;
    public GridPlacement GridRowEnd = GridPlacement.Auto;
    public GridPlacement GridColumnStart = GridPlacement.Auto;
    public GridPlacement GridColumnEnd = GridPlacement.Auto;

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

    public AlignType AlignSelf = AlignType.None;
    public AlignType JustifySelf = AlignType.None;

    public LengthType FlexBasis = LengthType.Auto;
}
