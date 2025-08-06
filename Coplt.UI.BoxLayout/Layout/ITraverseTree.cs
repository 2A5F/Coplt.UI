using Coplt.UI.BoxLayout.Utilities;

namespace Coplt.UI.BoxLayouts;

public interface ITraverseTree<TNodeId, out TChildIter> : ITraversePartialTree<TNodeId, TChildIter>
    where TNodeId : allows ref struct
    where TChildIter : IIterator<TNodeId>, allows ref struct;
