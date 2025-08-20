using Coplt.UI.BoxLayouts.Utilities;

namespace Coplt.UI.BoxLayouts;

public interface ITraverseTree<TNodeId, out TChildIter> : ITraversePartialTree<TNodeId, TChildIter>
    where TChildIter : IIterator<TNodeId>, allows ref struct;
