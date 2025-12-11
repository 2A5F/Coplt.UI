using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Native;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public unsafe partial struct TextParagraphData
{
    [Drop]
    internal OpaqueObject m_native_data;
    [Drop]
    public NString m_text;
    [Drop]
    public NativeList<uint> m_break_points;
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

[Dropping]
public unsafe partial struct TextData_FontRange
{
    public uint Start;
    public uint Length;
    [ComType<Ptr<IFontFace>>]
    public Rc<IFontFace> m_font_face;
}
