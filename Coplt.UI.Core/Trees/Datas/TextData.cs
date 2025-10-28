using Coplt.Dropping;
using Coplt.UI.Collections;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public partial struct TextData
{
    [Drop]
    public NativeList<char> m_text;
    public ulong m_version;
    public ulong m_inner_version;

    /// <summary>
    /// No string identification check will be performed. Setting this will invalidate the cache.
    /// </summary>
    /// <param name="text"></param>
    public void SetText(string text)
    {
        m_text.Clear();
        m_text.AddRange(text);
        m_version++;
    }

    public string GetText() => m_text.ToString();
}
