using Coplt.UI.Native;

namespace Coplt.UI.Texts;

public record struct TextRange
{
    public CWStr Locale;
    public int Start, Length;
    public ScriptCode Script;
    public CharCategory Category;
    public bool ScriptIsRtl;
}
