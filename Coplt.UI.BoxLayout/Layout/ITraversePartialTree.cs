using Coplt.UI.BoxLayout.Utilities;

namespace Coplt.UI.BoxLayouts;

public interface ITraversePartialTree<TNodeId, out TChildIter>
    where TNodeId : allows ref struct
    where TChildIter : IIterator<TNodeId>, allows ref struct
{
    public TChildIter ChildIds(TNodeId parent_node_id);
    
    public int ChildCount(TNodeId parent_node_id);
    
    public TNodeId GetChildId(TNodeId parent_node_id, int index);
}
