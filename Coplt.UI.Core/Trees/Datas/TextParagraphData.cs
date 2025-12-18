using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Native;
using Coplt.UI.Styles;
using Coplt.UI.Texts;

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
    public uint End;
    public ushort Script;

    public uint Length => End - Start;
}

public record struct TextData_SameStyleRange
{
    public uint Start;
    public uint End;
    public TextSpanNode FirstSpanValue;
    public bool HasFirstSpan;

    public uint Length => End - Start;

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
    public uint End;
    public LocaleId Locale;

    public uint Length => End - Start;
}

[Dropping]
public unsafe partial record struct TextData_FontRange
{
    public uint Start;
    public uint End;
    [ComType<Ptr<IFontFace>>]
    public Rc<IFontFace> m_font_face;

    public uint Length => End - Start;
}

public record struct TextData_BidiRange
{
    public uint Start;
    public uint End;
    public BidiDirection Direction;

    public uint Length => End - Start;
}

public enum BidiDirection : byte
{
    LeftToRight,
    RightToLeft,
}
