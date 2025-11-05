namespace Coplt.UI.Styles;

/// <summary>
/// Behavior when locale is not specified
/// </summary>
public enum LocaleMode : byte
{
    /// <summary>
    /// It will not attempt to auto determine the locale.
    /// Generally, the system locale is used, depending on the implementation.
    /// </summary>
    Normal,
    /// <summary>
    /// The locale will be determined based on the text script.
    /// </summary>
    ByScript,
}

public enum TextAlign : byte
{
    Auto,
    Left,
    Right,
    Center,
}

public enum FontWeight
{
    None = 0,
    Thin = 100,
    ExtraLight = 200,
    Light = 300,
    SemiLight = 350,
    Normal = 400,
    Medium = 500,
    SemiBold = 600,
    Bold = 700,
    ExtraBold = 800,
    Black = 900,
    ExtraBlack = 950,
}

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

public readonly record struct FontWidth(float Width)
{
    public readonly float Width = Width;

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
    public static explicit operator FontStretch(FontWidth width) => width.Width switch
    {
        0.5f => FontStretch.UltraCondensed,
        0.625f => FontStretch.ExtraCondensed,
        0.75f => FontStretch.Condensed,
        0.875f => FontStretch.SemiCondensed,
        1f => FontStretch.Normal,
        1.125f => FontStretch.SemiExpanded,
        1.25f => FontStretch.Expanded,
        1.5f => FontStretch.ExtraExpanded,
        2f => FontStretch.UltraExpanded,
        _ => FontStretch.Undefined,
    };

    public override string ToString() => Width switch
    {
        0.5f => nameof(FontStretch.UltraCondensed),
        0.625f => nameof(FontStretch.ExtraCondensed),
        0.75f => nameof(FontStretch.Condensed),
        0.875f => nameof(FontStretch.SemiCondensed),
        1f => nameof(FontStretch.Normal),
        1.125f => nameof(FontStretch.SemiExpanded),
        1.25f => nameof(FontStretch.Expanded),
        1.5f => nameof(FontStretch.ExtraExpanded),
        2f => nameof(FontStretch.UltraExpanded),
        _ => $"{Width}",
    };
}

public enum TextDirection : byte
{
    Forward,
    Reverse,
    LeftToRight,
    RightToLeft,
}

public enum WritingDirection : byte
{
    Horizontal,
    Vertical,
}

public enum WhiteSpaceMerge : byte
{
    Keep,
    Merge,
}

public enum WhiteSpaceWrap : byte
{
    WrapAll,
    /// <summary>
    /// wrap on <c>\n</c> or <c>\r</c>
    /// </summary>
    WrapLine,
}

public enum TextWrap : byte
{
    Wrap,
    NoWrap,
}

public enum WordBreak : byte
{
    Auto,
    BreakAll,
    KeepAll,
}

public enum TextOrientation : byte
{
    Mixed,
    Upright,
    Sideways,
}

public enum TextOverflow : byte
{
    Clip,
    Ellipsis,
}
