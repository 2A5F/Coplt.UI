namespace Coplt.UI.Styles;

public interface ICoreStyle
{
    public BoxGenerationMode BoxGenerationMode => BoxGenerationMode.Normal;
    public bool IsBlock => false;
    public bool IsCompressibleReplaced => false;
    public BoxSizing BoxSizing => BoxStyle.Default.BoxSizing;
    public Point<Overflow> Overflow => BoxStyle.Default.Overflow;
    public float ScrollbarWidth => BoxStyle.Default.ScrollbarWidth;
    public Position Position => BoxStyle.Default.Position;
    public Rect<LengthPercentageAuto> Inset => BoxStyle.Default.Inset;
    public Size<Dimension> Size => BoxStyle.Default.Size;
    public Size<Dimension> MinSize => BoxStyle.Default.MinSize;
    public Size<Dimension> MaxSize => BoxStyle.Default.MaxSize;
    public float? AspectRatio => BoxStyle.Default.AspectRatio;
    public Rect<LengthPercentageAuto> Margin => BoxStyle.Default.Margin;
    public Rect<LengthPercentage> Padding => BoxStyle.Default.Padding;
    public Rect<LengthPercentage> Border => BoxStyle.Default.Border;
}

public readonly ref struct RefCoreStyle<T>(ref readonly T Target) : ICoreStyle
    where T : ICoreStyle
{
    public readonly ref readonly T Target = ref Target;

    public BoxGenerationMode BoxGenerationMode => Target.BoxGenerationMode;
    public bool IsBlock => Target.IsBlock;
    public bool IsCompressibleReplaced => Target.IsCompressibleReplaced;
    public BoxSizing BoxSizing => Target.BoxSizing;
    public Point<Overflow> Overflow => Target.Overflow;
    public float ScrollbarWidth => Target.ScrollbarWidth;
    public Position Position => Target.Position;
    public Rect<LengthPercentageAuto> Inset => Target.Inset;
    public Size<Dimension> Size => Target.Size;
    public Size<Dimension> MinSize => Target.MinSize;
    public Size<Dimension> MaxSize => Target.MaxSize;
    public float? AspectRatio => Target.AspectRatio;
    public Rect<LengthPercentageAuto> Margin => Target.Margin;
    public Rect<LengthPercentage> Padding => Target.Padding;
    public Rect<LengthPercentage> Border => Target.Border;
}
