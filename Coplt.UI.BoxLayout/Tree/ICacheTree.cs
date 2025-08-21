using Coplt.UI.Styles;

namespace Coplt.UI.BoxLayouts;

public interface ICacheTree<in TNodeId>
{
    public LayoutOutput? CacheGet(
        TNodeId node_id, Size<float?> known_dimensions, Size<AvailableSpace> available_space, RunMode run_mode
    );

    public void CacheStore(
        TNodeId node_id, Size<float?> known_dimensions, Size<AvailableSpace> available_space, RunMode run_mode,
        LayoutOutput layout_output
    );

    public void CacheClear(TNodeId node_id);
}
