using Coplt.UI.BoxLayouts.Utilities;

namespace Coplt.UI.BoxLayouts;

public interface ITraversePartialTree<TNodeId, out TChildIter>
    where TChildIter : IIterator<TNodeId>, allows ref struct
{
    public TChildIter ChildIds(TNodeId parent_node_id);
    
    public int ChildCount(TNodeId parent_node_id);
}
