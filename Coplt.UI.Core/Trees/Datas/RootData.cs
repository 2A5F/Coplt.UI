using Coplt.UI.Layouts;
using Coplt.UI.Styles;

namespace Coplt.UI.Trees.Datas;

public struct RootData()
{
    public LocaleId DefaultLocale;
    public NodeId Node;
    public float AvailableSpaceXValue;
    public float AvailableSpaceYValue;
    public AvailableSpaceType AvailableSpaceX = AvailableSpaceType.MinContent;
    public AvailableSpaceType AvailableSpaceY = AvailableSpaceType.MinContent;
    public float Dpi = 96;
    public bool UseRounding = true;
}
