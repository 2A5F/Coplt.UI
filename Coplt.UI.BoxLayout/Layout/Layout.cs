using Coplt.UI.Styles;

namespace Coplt.UI.BoxLayouts;

public record struct Layout
{
    public uint Order;
    public Point<float> Location;
    public Size<float> Size;
    public Size<float> ContentSize;
    public Size<float> ScrollbarSize;
    public Rect<float> Border;
    public Rect<float> Padding;
    public Rect<float> Margin;
}
