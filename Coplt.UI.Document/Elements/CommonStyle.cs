using System.Runtime.InteropServices;
using Coplt.Mathematics;
using Coplt.UI.Styles;

namespace Coplt.UI.Elements;

[StructLayout(LayoutKind.Auto)]
public record struct CommonStyle() //: IBlockContainerStyle, IFlexContainerStyle, IFlexItemStyle
{
    #region Default

    public static readonly CommonStyle Default = new();

    #endregion

    #region BoxLayout Style

    public Display Display = Display.Flex;
    public BoxSizing BoxSizing = BoxSizing.BorderBox;
    public Point<Overflow> Overflow = new(Styles.Overflow.Visible, Styles.Overflow.Visible);
    public Position Position = Position.Relative;

    public Rect<LengthPercentageAuto> Inset = new(LengthPercentageAuto.Auto);
    public Size<Dimension> Size = new(Dimension.Auto, Dimension.Auto);
    public Size<Dimension> MinSize = new(Dimension.Auto, Dimension.Auto);
    public Size<Dimension> MaxSize = new(Dimension.Auto, Dimension.Auto);

    public float? AspectRatio = null;

    public Rect<LengthPercentageAuto> Margin = new(LengthPercentageAuto.Zero);
    public Rect<LengthPercentage> Padding = new(LengthPercentage.Zero);
    public Rect<LengthPercentage> Border = new(LengthPercentage.Zero);

    public AlignItems? AlignItems = null;

    public AlignSelf? AlignSelf = null;
    public JustifyItems? JustifyItems = null;
    public JustifySelf? JustifySelf = null;
    public AlignContent? AlignContent = null;
    public JustifyContent? JustifyContent = null;
    public Size<LengthPercentage> Gap = new(LengthPercentage.Zero);

    public TextAlign TextAlign = TextAlign.Auto;

    public FlexDirection FlexDirection = FlexDirection.Column;
    public FlexWrap FlexWrap = FlexWrap.NoWrap;
    public Dimension FlexBias = Dimension.Auto;
    public float FlexGrow = 0;
    public float FlexShrink = 1;

    #endregion

    // #region Behavior Style
    //
    // public bool TextSelectable  = false;
    // public bool PointerEvents  = true;
    //
    // #endregion
}
