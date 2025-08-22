namespace Coplt.UI.Elements;

public readonly ref struct StyleAccess(UIElement Element)
{
    private readonly UIElement Element = Element;
}
