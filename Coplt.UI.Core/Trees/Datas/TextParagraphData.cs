using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Core.Geometry;
using Coplt.UI.Native;
using Coplt.UI.Styles;
using Coplt.UI.Texts;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public unsafe partial struct TextParagraphData
{
    internal ulong TextDirtyFrame;
    internal ulong TextStyleDirtyFrame;
    internal ulong DirtySyncFrame;

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
    [Drop]
    public NativeList<TextData_RunRange> m_run_ranges;
    [Drop]
    public NativeList<GlyphData> m_glyph_datas;

    [Drop]
    public NativeList<AABB2DF> m_bounding_boxes;

    public bool IsTextDirty(Document doc) => TextDirtyFrame == 0 || TextDirtyFrame == doc.CurrentFrame;
    public bool IsTextStyleDirty(Document doc) => TextStyleDirtyFrame == 0 || TextStyleDirtyFrame == doc.CurrentFrame;
    public void MarkTextDirty(Document doc) => TextDirtyFrame = doc.CurrentFrame;
    public void MarkTextStyleDirty(Document doc) => TextStyleDirtyFrame = doc.CurrentFrame;

    public string Text => m_text.ToString();

    public void SetText(Document doc, string text)
    {
        m_text.Dispose();
        m_text = NString.Create(text);
        MarkTextDirty(doc);
    }

    public ReadOnlySpan<AABB2DF> BoundingBoxes => m_bounding_boxes.AsSpan;
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

    public float ComputedFontSize;

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
    public uint StyleRange;

    public uint Length => End - Start;

    public FontFace? FontFace => !m_font_face ? null : m_font_face.Manager;
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

public record struct TextData_RunRange
{
    public uint Start;
    public uint End;
    public uint ScriptRange;
    public uint BidiRange;
    public uint StyleRange;
    public uint FontRange;

    public uint Length => End - Start;

    public uint GlyphStart;
    public uint GlyphEnd;

    public uint GlyphLength => GlyphEnd - GlyphStart;

    public float Ascent;
    public float Descent;
    public float Leading;

    public float FontLineHeight => Ascent + -Descent + Leading;
}

public record struct GlyphData
{
    public uint Cluster;
    public float Advance;
    public float Offset;
    public ushort GlyphId;
    public GlyphDataFlags Flags;
    public GlyphType Type;
}

[Flags]
public enum GlyphDataFlags : byte
{
    None = 0,
    UnsafeToBreak = 1 << 0,
}

public enum GlyphType : byte
{
    Invalid,
    Outline,
    Color,
    Bitmap,
}
