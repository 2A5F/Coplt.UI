namespace Coplt.UI.Styles;

public enum FontStretch
{
    Undefined,
    UltraCondensed,
    ExtraCondensed,
    Condensed,
    SemiCondensed,
    Normal,
    SemiExpanded,
    Expanded,
    ExtraExpanded,
    UltraExpanded,
}

public record struct FontWidth(float Width)
{
    public float Width = Width;

    public FontWidth(FontStretch stretch) : this(stretch switch
    {
        FontStretch.Undefined => 1,
        FontStretch.UltraCondensed => 0.5f,
        FontStretch.ExtraCondensed => 0.625f,
        FontStretch.Condensed => 0.75f,
        FontStretch.SemiCondensed => 0.875f,
        FontStretch.Normal => 1,
        FontStretch.SemiExpanded => 1.125f,
        FontStretch.Expanded => 1.25f,
        FontStretch.ExtraExpanded => 1.5f,
        FontStretch.UltraExpanded => 2f,
        _ => throw new ArgumentOutOfRangeException(nameof(stretch), stretch, null)
    }) { }

    public static implicit operator FontWidth(FontStretch stretch) => new(stretch);
}
