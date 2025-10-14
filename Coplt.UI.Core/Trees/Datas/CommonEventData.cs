using Coplt.UI.Styles;

namespace Coplt.UI.Trees.Datas;

public record struct CommonEventData()
{
    public CursorType Cursor = CursorType.Default;
    public PointerEvents PointerEvents = PointerEvents.Auto;
}
