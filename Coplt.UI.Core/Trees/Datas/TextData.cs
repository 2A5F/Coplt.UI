using Coplt.Dropping;
using Coplt.UI.Collections;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public partial struct TextData
{
    // todo pooling
    [Drop]
    public NativeList<char> m_text;

    public void SetText(string text)
    {
        m_text.Clear();
        m_text.AddRange(text);
    }
}
