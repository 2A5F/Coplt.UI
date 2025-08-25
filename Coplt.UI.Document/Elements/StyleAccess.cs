namespace Coplt.UI.Elements;

public readonly ref struct StyleAccess<TRd, TEd>(UIElement<TRd, TEd> Element)
{
    public readonly UIElement<TRd, TEd> Element = Element;
}
