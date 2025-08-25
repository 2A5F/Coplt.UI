using System.Runtime.InteropServices;
using Coplt.Mathematics;
using Coplt.UI.Styles;

namespace Coplt.UI.Elements;

[StructLayout(LayoutKind.Auto)]
public record struct CommonStyle() : IBlockContainerStyle, IFlexContainerStyle, IFlexItemStyle
{
    #region Default

    public static readonly CommonStyle Default = new();

    #endregion

    #region BoxLayout Style

    public Display Display { get; set; } = Display.Flex;
    public BoxSizing BoxSizing { get; set; } = BoxSizing.BorderBox;
    public Point<Overflow> Overflow { get; set; } = new(Styles.Overflow.Visible, Styles.Overflow.Visible);
    public Position Position { get; set; } = Position.Relative;

    public Rect<LengthPercentageAuto> Inset { get; set; } = new(LengthPercentageAuto.Auto);
    public Size<Dimension> Size { get; set; } = new(Dimension.Auto, Dimension.Auto);
    public Size<Dimension> MinSize { get; set; } = new(Dimension.Auto, Dimension.Auto);
    public Size<Dimension> MaxSize { get; set; } = new(Dimension.Auto, Dimension.Auto);

    public float? AspectRatio { get; set; } = null;

    public Rect<LengthPercentageAuto> Margin { get; set; } = new(LengthPercentageAuto.Zero);
    public Rect<LengthPercentage> Padding { get; set; } = new(LengthPercentage.Zero);
    public Rect<LengthPercentage> Border { get; set; } = new(LengthPercentage.Zero);

    public AlignItems? AlignItems { get; set; } = null;

    public AlignSelf? AlignSelf { get; set; } = null;
    public JustifyItems? JustifyItems { get; set; } = null;
    public JustifySelf? JustifySelf { get; set; } = null;
    public AlignContent? AlignContent { get; set; } = null;
    public JustifyContent? JustifyContent { get; set; } = null;
    public Size<LengthPercentage> Gap { get; set; } = new(LengthPercentage.Zero);

    public TextAlign TextAlign { get; set; } = TextAlign.Auto;

    public FlexDirection FlexDirection { get; set; } = FlexDirection.Column;
    public FlexWrap FlexWrap { get; set; } = FlexWrap.NoWrap;
    public Dimension FlexBias { get; set; } = Dimension.Auto;
    public float FlexGrow { get; set; } = 0;
    public float FlexShrink { get; set; } = 1;

    BoxGenerationMode ICoreStyle.BoxGenerationMode => Display == Display.None ? BoxGenerationMode.None : BoxGenerationMode.Normal;
    bool ICoreStyle.IsBlock => Display == Display.Block;
    bool ICoreStyle.IsCompressibleReplaced => false;

    float ICoreStyle.ScrollbarWidth => 0;

    #endregion

    // #region Behavior Style
    //
    // public bool TextSelectable { get; set; } = false;
    // public bool PointerEvents { get; set; } = true;
    //
    // #endregion
}
