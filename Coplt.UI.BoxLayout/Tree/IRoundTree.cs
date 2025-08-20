using Coplt.UI.BoxLayouts.Utilities;

namespace Coplt.UI.BoxLayouts;

public interface IRoundTree<TNodeId, out TChildIter> : ITraversePartialTree<TNodeId, TChildIter>
    where TChildIter : IIterator<TNodeId>, allows ref struct
{
    public ref readonly Layout GetUnroundedLayout(TNodeId node_id);

    public void SetFinalLayout(TNodeId node_id, in Layout layout);
}
