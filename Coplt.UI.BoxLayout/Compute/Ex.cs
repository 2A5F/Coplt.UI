using Coplt.UI.BoxLayout.Utilities;
using Coplt.UI.BoxLayouts;
using Coplt.UI.Styles;

namespace Coplt.UI.Layouts;

public static partial class BoxLayout
{
    internal static float MeasureChildSize<TTree, TNodeId, TChildIter, TCoreContainerStyle>(
        ref TTree tree,
        TNodeId node_id,
        Size<float?> known_dimensions,
        Size<float?> parent_size,
        Size<AvailableSpace> available_space,
        SizingMode sizing_mode,
        AbsoluteAxis axis,
        Line<bool> vertical_margins_are_collapsible
    )
        where TTree : ILayoutPartialTree<TNodeId, TChildIter, TCoreContainerStyle>, allows ref struct
        where TNodeId : allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        => tree.ComputeChildLayout(node_id, new()
            {
                RunMode = RunMode.ComputeSize,
                SizingMode = sizing_mode,
                Axis = axis.ToRequestedAxis(),
                KnownDimensions = known_dimensions,
                ParentSize = parent_size,
                AvailableSpace = available_space,
                VerticalMarginsAreCollapsible = vertical_margins_are_collapsible,
            })
            .Size
            .GetAbs(axis);
}
