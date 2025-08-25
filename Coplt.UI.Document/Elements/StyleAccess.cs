namespace Coplt.UI.Elements;

public readonly ref struct StyleAccess<TRd, TEd>(UIElement<TRd, TEd> Element)
    where TRd : new() where TEd : new()
{
    public readonly UIElement<TRd, TEd> Element = Element;
}
