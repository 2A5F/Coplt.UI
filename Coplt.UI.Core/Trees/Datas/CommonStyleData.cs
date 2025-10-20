using Coplt.UI.Styles;

namespace Coplt.UI.Trees.Datas;

public record struct CommonStyleData()
{
    public int ZIndex;
    public float Opacity = 1;

    public VisibleMode Visible = VisibleMode.Visible;

    public TextAlign TextAlign = TextAlign.Auto;
}
