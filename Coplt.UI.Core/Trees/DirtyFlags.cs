namespace Coplt.UI.Trees;

[Flags]
public enum DirtyFlags : uint
{
    None = 0,
    Layout = 1 << 0,
    Render = 1 << 1,
    TextLayout = 1 << 2,
}
