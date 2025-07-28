using System.Runtime.CompilerServices;
using Coplt.UI.Styles;

namespace Coplt.UI.BoxLayouts;

public record struct Layout
{
    public int Order;
    public Point<float> Location;
    public Size<float> Size;
    public Size<float> ContentSize;
    public Size<float> ScrollbarSize;
    public Rect<float> Border;
    public Rect<float> Padding;
    public Rect<float> Margin;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Layout WithOrder(int order) => new()
    {
        Order = order,
        Location = default,
        Size = default,
        ContentSize = default,
        ScrollbarSize = default,
        Border = default,
        Padding = default,
        Margin = default,
    };
}
