using Coplt.UI.Elements;

namespace Coplt.UI.Styles.Rules;

internal record struct InlineStyle()
{
    
}

public ref struct InlineStyleAccess(UIElement Element)
{
    public UIElement Element = Element;
}
