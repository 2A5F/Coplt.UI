using System.Drawing;
using Coplt.UI.Styles;

namespace Coplt.UI.Elements;

public struct StyleSet() : IBlockContainerStyle, IFlexContainerStyle, IFlexItemStyle
{
    #region BoxLayout Style

    public Display Display { get; set; } = Display.Flex;
    public BoxSizing BoxSizing { get; set; } = BoxSizing.BorderBox;
    public Point<Overflow> Overflow { get; set; } = new(Styles.Overflow.Visible, Styles.Overflow.Visible);
    public Position Position { get; set; } = Position.Relative;

    public Rect<LengthPercentageAuto> Inset { get; set; } = new(LengthPercentageAuto.Auto);
    public Size<Dimension> Size { get; set; } = new(Dimension.Auto, Dimension.Auto);
    public Size<Dimension> MinSize { get; set; } = new(Dimension.Auto, Dimension.Auto);
    public Size<Dimension> MaxSize { get; set; } = new(Dimension.Auto, Dimension.Auto);

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
    float? ICoreStyle.AspectRatio => null;

    #endregion

    #region Rendering Style

    public Color BackgroundColor { get; set; } = Color.Transparent;
    public UIImage BackgroundImage { get; set; } = UIImage.None;
    public Color BackgroundImageTint { get; set; } = Color.White;

    public Rect<Color> BorderColor { get; set; } = new(Color.Transparent);
    public Rect<float> BorderRadius { get; set; } = new(0);
    public BorderRadiusMode BorderRadiusMode = BorderRadiusMode.Circle;

    public Color TextColor { get; set; } = Color.Black;
    public LengthPercentage TextSize { get; set; } = 16.Fx();

    #endregion
}
