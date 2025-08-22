namespace Coplt.UI.Elements;

[Flags]
public enum DirtyFlags
{
    None,
    Layout = 1 << 0,
    Visual = 1 << 1,
    Selector =  1 << 2,
}
