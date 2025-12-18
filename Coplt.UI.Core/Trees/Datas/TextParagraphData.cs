using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Native;
using Coplt.UI.Styles;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public unsafe partial struct TextParagraphData
{
    [Drop]
    internal OpaqueObject m_native_data;
    [Drop]
    public NString m_text;
    [Drop]
    public NativeBitSet m_break_points; // stored can break after char at same index
    [Drop]
    public NativeList<uint> m_grapheme_cluster;
    [Drop]
    public NativeList<TextData_ScriptRange> m_script_ranges;
    [Drop]
    public NativeList<TextData_BidiRange> m_bidi_ranges;
    [Drop]
    public NativeList<TextData_SameStyleRange> m_same_style_ranges;
    [Drop]
    public NativeList<TextData_LocaleRange> m_locale_ranges;
    [Drop]
    public NativeList<TextData_FontRange> m_font_ranges;

    /// <summary>
    /// layout compute sync this to <see cref="TextVersion"/>
    /// </summary>
    internal uint LastTextVersion;
    /// <summary>
    /// dirty inc this
    /// </summary>
    internal uint TextVersion;

    public bool IsTextDirty => LastTextVersion != TextVersion;

    public string Text
    {
        get => m_text.ToString();
        set
        {
            m_text.Dispose();
            m_text = NString.Create(value);
            TextVersion++;
        }
    }
}

public record struct TextData_ScriptRange
{
    public uint Start;
    public uint Length;
    public ushort Script;
}

public record struct TextData_SameStyleRange
{
    public uint Start;
    public uint Length;
    public TextSpanNode FirstSpanValue;
    public bool HasFirstSpan;

    public TextSpanNode? FirstSpan
    {
        readonly get => HasFirstSpan ? FirstSpanValue : null;
        set
        {
            if (value.HasValue)
            {
                FirstSpanValue = value.Value;
                HasFirstSpan = true;
            }
            else
            {
                HasFirstSpan = false;
                FirstSpanValue = default;
            }
        }
    }
}

public record struct TextData_LocaleRange
{
    public uint Start;
    public uint Length;
    public LocaleId Locale;
}

[Dropping]
public unsafe partial record struct TextData_FontRange
{
    public uint Start;
    public uint Length;
    [ComType<Ptr<IFontFace>>]
    public Rc<IFontFace> m_font_face;
}

public record struct TextData_BidiRange
{
    public uint Start;
    public uint Length;
    public BidiDirection Direction;
}

public enum BidiDirection : byte
{
    LeftToRight,
    RightToLeft,
}
