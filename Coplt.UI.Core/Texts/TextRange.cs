namespace Coplt.UI.Texts;

public record struct TextRange
{
    public int Start, Length;
    public ScriptCode Script;
    public CharCategory Category;
    public bool ScriptIsRtl;
}
