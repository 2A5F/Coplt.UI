using Coplt.UI.BoxLayouts.Utilities;
using Coplt.UI.Styles;

namespace Coplt.UI.BoxLayouts;

public interface ILayoutFlexboxContainer<TNodeId, out TChildIter, out TCoreContainerStyle, out TFlexBoxContainerStyle, out TFlexboxItemStyle>
    : ILayoutPartialTree<TNodeId, TChildIter, TCoreContainerStyle>
    where TChildIter : IIterator<TNodeId>, allows ref struct
    where TCoreContainerStyle : ICoreStyle, allows ref struct
    where TFlexBoxContainerStyle : IFlexContainerStyle, allows ref struct
    where TFlexboxItemStyle : IFlexItemStyle, allows ref struct
{
    public TFlexBoxContainerStyle GetFlexBoxContainerStyle(TNodeId node_id);

    public TFlexboxItemStyle GetFlexboxChildStyle(TNodeId child_node_id);
}
