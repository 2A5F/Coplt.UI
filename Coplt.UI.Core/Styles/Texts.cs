namespace Coplt.UI.Styles;

public enum TextAlign : byte
{
    /// <summary>
    /// For horizontal text, this is left ; for vertical text, this is top. RTL will not affect this.
    /// </summary>
    Start,
    /// <summary>
    /// For horizontal text, this is right ; for vertical text, this is bottom. RTL will not affect this.
    /// </summary>
    End,
    /// <summary>
    /// Center alignment
    /// </summary>
    Center,
}

/// <summary>
/// How inline box align in text lines
/// </summary>
public enum LineAlign : byte
{
    /// <summary>
    /// Inline box bottom align to baseline
    /// </summary>
    Baseline,
    /// <summary>
    /// Inline box bottom align to line start, for horizontal text, this is top ; for vertical text, this is left. RTL will not affect this.
    /// </summary>
    Start,
    /// <summary>
    /// Inline box bottom align to line end, for horizontal text, this is bottom ; for vertical text, this is right. RTL will not affect this.
    /// </summary>
    End,
    /// <summary>
    /// Center alignment
    /// </summary>
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
    LeftToRight = Forward,
    RightToLeft = Reverse,
}

public enum WritingDirection : byte
{
    Horizontal,
    Vertical,
}

[Flags]
public enum WrapFlags : byte
{
    None = 0,
    /// <summary>
    /// If not set, newline <c>\n</c> | <c>\r</c> will be treated as spaces (0x0020).
    /// </summary>
    AllowNewLine = 1 << 0,
    /// <summary>
    /// If set, it allows line breaks within spaces (0x0020), not just word boundaries.
    /// </summary>
    WrapInSpace = 1 << 1,
    /// <summary>
    /// Remove leading spaces from each line; not support yet
    /// </summary>
    TrimStart = 1 << 2,
    /// <summary>
    /// Remove trailing spaces from each line; not support yet
    /// </summary>
    TrimEnd = 1 << 3,
    Trim = TrimStart | TrimEnd,
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
