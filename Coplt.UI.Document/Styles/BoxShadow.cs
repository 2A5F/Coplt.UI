using Coplt.Mathematics;

namespace Coplt.UI.Styles;

public record struct BoxShadow(Point<Length> Offset, Length Blur, Length Spread, Color Color, bool Inset = false)
{
    public BoxShadow(Length XOffset, Length YOffset, Color Color, bool Inset = false)
        : this(new Point<Length>(XOffset, YOffset), default, default, Color, Inset) { }

    public BoxShadow(Length XOffset, Length YOffset, Length Blur, Color Color, bool Inset = false)
        : this(new Point<Length>(XOffset, YOffset), Blur, default, Color, Inset) { }

    public BoxShadow(Length XOffset, Length YOffset, Length Blur, Length Spread, Color Color, bool Inset = false)
        : this(new Point<Length>(XOffset, YOffset), Blur, Spread, Color, Inset) { }
}
