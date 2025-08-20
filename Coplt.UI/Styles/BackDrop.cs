using System.Drawing;
using Coplt.Union;

namespace Coplt.UI.Styles;

[Union]
public partial struct FilterFunc
{
    [UnionTemplate]
    private interface Template
    {
        void None();
        void Blur(Length Radius);
        void DropShadow(Point<Length> Offset, Length Blur, Color Color);
        void Custom(object? Object, ulong Id);
    }

    public static readonly FilterFunc None = MakeNone();
}
