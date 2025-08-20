using System.Runtime.CompilerServices;
using Coplt.UI.BoxLayouts.Utilities;
using Coplt.UI.Styles;

namespace Coplt.UI.BoxLayouts;

public interface ILayoutPartialTree<TNodeId, out TChildIter, out TCoreContainerStyle> : ITraversePartialTree<TNodeId, TChildIter>, ICalc
    where TChildIter : IIterator<TNodeId>, allows ref struct
    where TCoreContainerStyle : ICoreStyle, allows ref struct
{
    public TCoreContainerStyle GetCoreContainerStyle(TNodeId node_id);

    public void SetUnroundedLayout(TNodeId node_id, in Layout layout);

    public LayoutOutput ComputeChildLayout(TNodeId node_id, LayoutInput inputs);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutOutput PerformChildLayout(
        TNodeId node_id,
        Size<float?> known_dimensions,
        Size<float?> parent_size,
        Size<AvailableSpace> available_space,
        SizingMode sizing_mode,
        Line<bool> vertical_margins_are_collapsible
    ) => ComputeChildLayout(node_id, new()
    {
        RunMode = RunMode.PerformLayout,
        SizingMode = sizing_mode,
        Axis = RequestedAxis.Both,
        KnownDimensions = known_dimensions,
        ParentSize = parent_size,
        AvailableSpace = available_space,
        VerticalMarginsAreCollapsible = vertical_margins_are_collapsible,
    });
}
