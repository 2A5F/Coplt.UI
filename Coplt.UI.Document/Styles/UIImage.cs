namespace Coplt.UI.Styles;

public record struct UIImage(object? Object, ulong Id)
{
    public static UIImage None => default;
}
