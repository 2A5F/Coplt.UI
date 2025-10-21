using Coplt.UI.Layouts;

namespace Coplt.UI.Trees.Datas;

public struct RootData()
{
    public float AvailableSpaceXValue;
    public float AvailableSpaceYValue;
    public AvailableSpaceType AvailableSpaceX = AvailableSpaceType.MinContent;
    public AvailableSpaceType AvailableSpaceY = AvailableSpaceType.MinContent;
    public bool UseRounding = true;
}
