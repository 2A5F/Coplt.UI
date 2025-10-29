using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Native;
using Coplt.UI.Texts;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public unsafe partial struct TextData
{
    [Drop]
    [ComType<Ptr<ITextData>>]
    internal Rc<ITextData> m_obj;
    [Drop]
    internal NativeList<char> m_text;
    [Drop]
    internal NativeList<TextRange> m_ranges_0;
    internal ulong m_version;
    internal ulong m_inner_version;

    public string GetText() => m_text.ToString();

    /// <summary>
    /// No string identification check will be performed. Setting this will invalidate the cache.
    /// </summary>
    public void SetText(string text)
    {
        m_text.Clear();
        m_text.AddRange(text);
        fixed (NativeList<TextRange>* p_ranges_0 = &m_ranges_0)
        {
            NativeLib.Instance.SplitTexts(p_ranges_0, m_text.Raw, m_text.Count);
        }
        // todo bidi
        m_version++;
    }
}
