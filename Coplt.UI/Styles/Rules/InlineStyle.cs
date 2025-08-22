using Coplt.UI.Elements;

namespace Coplt.UI.Styles.Rules;

internal class InlineStyle
{
    internal StyleSheet m_sheet;
}

public ref struct InlineStyleAccess(UIElement Element)
{
    public UIElement Element = Element;
}
