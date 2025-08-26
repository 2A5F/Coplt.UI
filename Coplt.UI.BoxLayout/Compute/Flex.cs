using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.UI.BoxLayouts.Utilities;
using Coplt.UI.BoxLayouts;
using Coplt.UI.Styles;
using static Coplt.UI.Layouts.FlexCompute;

namespace Coplt.UI.Layouts;

public static partial class BoxLayout
{
    public static LayoutOutput ComputeFlexBoxLayout<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
        ref TTree tree, TNodeId node, LayoutInput inputs
    )
        where TTree : ILayoutFlexboxContainer<TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>, allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        where TFlexBoxContainerStyle : IFlexContainerStyle, allows ref struct
        where TFlexboxItemStyle : IFlexItemStyle, allows ref struct
    {
        var (run_mode, _, _, known_dimensions, parent_size, _, _) = inputs;
        var style = tree.GetFlexBoxContainerStyle(node);

        // Pull these out earlier to avoid borrowing issues
        var aspect_ratio = style.AspectRatio;
        var padding = style.Padding.ResolveOrZero(parent_size.Width, ref tree);
        var border = style.Border.ResolveOrZero(parent_size.Width, ref tree);
        var padding_border_sum = padding.SumAxes().Add(border.SumAxes());
        var box_sizing_adjustment = style.BoxSizing == BoxSizing.ContentBox ? padding_border_sum : default;

        var min_size = style.MinSize
            .TryResolve(parent_size, ref tree)
            .TryApplyAspectRatio(aspect_ratio)
            .TryAdd(box_sizing_adjustment);
        var max_size = style.MaxSize
            .TryResolve(parent_size, ref tree)
            .TryApplyAspectRatio(aspect_ratio)
            .TryAdd(box_sizing_adjustment);
        var clamped_style_size = inputs.SizingMode != SizingMode.InherentSize
            ? default
            : style.Size
                .TryResolve(parent_size, ref tree)
                .TryApplyAspectRatio(aspect_ratio)
                .TryAdd(box_sizing_adjustment)
                .TryClamp(min_size, max_size);

        // If both min and max in a given axis are set and max <= min then this determines the size in that axis
        var min_max_definite_size = min_size.ZipMap(max_size, static (min, max) => (min, max) switch
        {
            ({ } Min, { } Max) when Max <= Min => min,
            _ => null,
        });

        // The size of the container should be floored by the padding and border
        var styled_based_known_dimensions = known_dimensions
            .Or(min_max_definite_size.Or(clamped_style_size).TryMax(padding_border_sum));

        if (run_mode == RunMode.ComputeSize)
        {
            if (styled_based_known_dimensions is { Width: { } w, Height: { } h })
            {
                return LayoutOutput.FromOuterSize(w, h);
            }
        }

        return ComputePreliminary<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
            ref tree, node, inputs with { KnownDimensions = styled_based_known_dimensions }
        );
    }
}

file static class FlexCompute
{
    public static LayoutOutput ComputePreliminary<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
        ref TTree tree, TNodeId node, LayoutInput inputs
    )
        where TTree : ILayoutFlexboxContainer<TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>, allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        where TFlexBoxContainerStyle : IFlexContainerStyle, allows ref struct
        where TFlexboxItemStyle : IFlexItemStyle, allows ref struct
    {
        var (run_mode, _, _, known_dimensions, parent_size, available_space, _) = inputs;

        // Define some general constants we will need for the remainder of the algorithm.
        var constants = ComputeConstants<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
            ref tree, tree.GetFlexBoxContainerStyle(node), known_dimensions, parent_size
        );

        // 9. Flex Layout Algorithm

        // 9.1. Initial Setup

        // 1. Generate anonymous flex items as described in §4 Flex Items.
        using var flex_items = GenerateAnonymousFlexItems<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
            ref tree, node, in constants
        );

        // 9.2. Line Length Determination

        // 2. Determine the available main and cross space for the flex items
        available_space = DetermineAvailableSpace(known_dimensions, available_space, in constants);

        // 3. Determine the flex base size and hypothetical main size of each item.
        DetermineFlexBaseSize<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
            ref tree, node, in constants, available_space, flex_items.AsSpan
        );

        // 4. Determine the main size of the flex container
        // This has already been done as part of compute_constants. The inner size is exposed as constants.node_inner_size.

        // 9.3. Main Size Determination

        // 5. Collect flex items into flex lines.
        using var flex_lines = CollectFlexLines(in constants, available_space, flex_items.AsSpan);

        // If container size is undefined, determine the container's main size
        // and then re-resolve gaps based on newly determined size
        if (constants.NodeInnerSize.Main(constants.Direction) is { } inner_main_size)
        {
            var outer_main_size = inner_main_size + constants.ContentBoxInset.MainAxisSum(constants.Direction);
            constants.InnerContainerSize.SetMain(constants.Direction, inner_main_size);
            constants.ContainerSize.SetMain(constants.Direction, outer_main_size);
        }
        else
        {
            DetermineContainerMainSize<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
                ref tree, node, available_space, flex_items.AsSpan, flex_lines.AsSpan, ref constants
            );
            constants.NodeInnerSize.SetMain(constants.Direction, constants.InnerContainerSize.Main(constants.Direction));
            constants.NodeOuterSize.SetMain(constants.Direction, constants.ContainerSize.Main(constants.Direction));

            // Re-resolve percentage gaps
            var style = tree.GetFlexBoxContainerStyle(node);
            var inner_container_size = constants.InnerContainerSize.Main(constants.Direction);
            var new_gap =
                style
                    .Gap
                    .Main(constants.Direction)
                    .TryResolve(inner_container_size, ref tree)
                ?? 0f;
            constants.Gap.SetMain(constants.Direction, new_gap);
        }

        // 6. Resolve the flexible lengths of all the flex items to find their used main size.
        foreach (ref var line in flex_lines)
        {
            ResolveFlexibleLengths(flex_items.AsSpan, ref line, in constants);
        }

        // 9.4. Cross Size Determination

        // 7. Determine the hypothetical cross size of each item.

        foreach (ref var line in flex_lines)
        {
            DetermineHypotheticalCrossSize<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
                ref tree, node, flex_items.AsSpan, ref line, in constants, available_space
            );
        }

        // Calculate child baselines. This function is internally smart and only computes child baselines
        // if they are necessary.
        CalculateChildrenBaseLines<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
            ref tree, node, known_dimensions, available_space, flex_items.AsSpan, flex_lines.AsSpan, in constants
        );

        // 8. Calculate the cross size of each flex line.
        CalculateCrossSize(flex_items.AsSpan, flex_lines.AsSpan, known_dimensions, in constants);

        // 9. Handle 'align-content: stretch'.
        HandleAlignContentStretch(flex_lines.AsSpan, known_dimensions, in constants);

        // 10. Collapse visibility:collapse items. If any flex items have visibility: collapse,
        //     note the cross size of the line they’re in as the item’s strut size, and restart
        //     layout from the beginning.
        //
        //     In this second layout round, when collecting items into lines, treat the collapsed
        //     items as having zero main size. For the rest of the algorithm following that step,
        //     ignore the collapsed items entirely (as if they were display:none) except that after
        //     calculating the cross size of the lines, if any line’s cross size is less than the
        //     largest strut size among all the collapsed items in the line, set its cross size to
        //     that strut size.
        //
        //     Skip this step in the second layout round.

        // visibility:collapse not supported yet

        // 11. Determine the used cross size of each flex item.
        DetermineUsedCrossSize<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
            ref tree, node, flex_items.AsSpan, flex_lines.AsSpan, in constants
        );

        // 9.5. Main-Axis Alignment

        // 12. Distribute any remaining free space.
        DistributeRemainingFreeSpace(flex_items.AsSpan, flex_lines.AsSpan, in constants);

        // 9.6. Cross-Axis Alignment

        // 13. Resolve cross-axis auto margins (also includes 14).
        ResolveCrossAxisAutoMargins(flex_items.AsSpan, flex_lines.AsSpan, in constants);

        // 15. Determine the flex container’s used cross size.
        var total_line_cross_size = DetermineContainerCrossSize(
            flex_lines.AsSpan, known_dimensions, ref constants
        );

        // We have the container size.
        // If our caller does not care about performing layout we are done now.
        if (run_mode == RunMode.ComputeSize) return LayoutOutput.FromOuterSize(constants.ContainerSize);

        // 16. Align all flex lines per align-content.
        AlignFlexLinesPerAlignContent(flex_lines.AsSpan, in constants, total_line_cross_size);

        // Do a final layout pass and gather the resulting layouts
        var inflow_content_size = FinalLayoutPass<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
            ref tree, node, flex_items.AsSpan, flex_lines.AsSpan, in constants
        );

        // Before returning we perform absolute layout on all absolutely positioned children
        var absolute_content_size =
            PerformAbsoluteLayoutOnAbsoluteChildren<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
                ref tree, node, in constants
            );

        var order = 0;
        foreach (var child in tree.ChildIds(node).AsEnumerable<TChildIter, TNodeId>())
        {
            if (tree.GetFlexboxChildStyle(child).BoxGenerationMode != BoxGenerationMode.None) continue;
            tree.SetUnroundedLayout(child, UnroundedLayout.WithOrder(order++));
            tree.PerformChildLayout(
                child,
                default,
                default,
                new(AvailableSpace.MaxContent),
                SizingMode.InherentSize,
                default
            );
        }

        // 8.5. Flex Container Baselines: calculate the flex container's first baseline
        // See https://www.w3.org/TR/css-flexbox-1/#flex-baselines
        float? first_vertical_baseline = null;
        if (flex_lines.Count > 0)
        {
            var items = GetItems(flex_lines[0], flex_items.AsSpan);
            var index = -1;
            var i = 0;
            foreach (ref var item in items)
            {
                var nth = i++;
                if (constants.IsColumn || item.AlignSelf == AlignSelf.Baseline)
                {
                    index = nth;
                    break;
                }
            }
            if (index < 0 && items.Length > 0) index = 0;
            if (index >= 0)
            {
                ref var child = ref items[index];
                var offset_vertical = constants.IsRow ? child.OffsetCross : child.OffsetMain;
                first_vertical_baseline = offset_vertical + child.BaseLine;
            }
        }

        return LayoutOutput.FromSizesAndBaselines(
            constants.ContainerSize,
            inflow_content_size.Max(absolute_content_size),
            new(null, first_vertical_baseline)
        );
    }

    [StructLayout(LayoutKind.Auto)]
    private record struct AlgoConstants
    {
        public FlexDirection Direction;
        public bool IsRow;
        public bool IsColumn;
        public bool IsWrap;
        public bool IsWrapReverse;

        public Size<float?> MinSize;
        public Size<float?> MaxSize;
        public Rect<float> Margin;
        public Rect<float> Border;
        public Rect<float> ContentBoxInset;
        public Point<float> ScrollbarGutter;
        public Size<float> Gap;
        public AlignItems AlignItems;
        public AlignContent AlignContent;
        public JustifyContent? JustifyContent;

        public Size<float?> NodeOuterSize;
        public Size<float?> NodeInnerSize;

        public Size<float> ContainerSize;
        public Size<float> InnerContainerSize;
    }

    /// Compute constants that can be reused during the flexbox algorithm.
    private static AlgoConstants ComputeConstants<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
        ref TTree tree, TFlexBoxContainerStyle style, Size<float?> known_dimensions, Size<float?> parent_size
    )
        where TTree : ILayoutFlexboxContainer<TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>, allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        where TFlexBoxContainerStyle : IFlexContainerStyle, allows ref struct
        where TFlexboxItemStyle : IFlexItemStyle, allows ref struct
    {
        var dir = style.FlexDirection;
        var is_row = dir.IsRow();
        var is_column = dir.IsColumn();
        var is_wrap = style.FlexWrap is FlexWrap.Wrap or FlexWrap.WrapReverse;
        var is_wrap_reverse = style.FlexWrap is FlexWrap.WrapReverse;

        var aspect_ratio = style.AspectRatio;
        var margin = style.Margin.ResolveOrZero(parent_size.Width, ref tree);
        var padding = style.Padding.ResolveOrZero(parent_size.Width, ref tree);
        var border = style.Border.ResolveOrZero(parent_size.Width, ref tree);
        var padding_border_sum = padding.SumAxes().Add(border.SumAxes());
        var box_sizing_adjustment = style.BoxSizing == BoxSizing.ContentBox ? padding_border_sum : default;

        var align_items = style.AlignItems ?? AlignItems.Stretch;
        var align_content = style.AlignContent ?? AlignContent.Stretch;
        var justify_content = style.JustifyContent;

        var scrollbar_gutter = style.Overflow.Transpose()
            .Map(style.ScrollbarWidth, static (scrollbar_width, overflow) => overflow is Overflow.Scroll ? scrollbar_width : 0);
        var content_box_inset = padding.Add(border);
        content_box_inset.Right += scrollbar_gutter.X;
        content_box_inset.Bottom += scrollbar_gutter.Y;

        var node_outer_size = known_dimensions;
        var node_inner_size = node_outer_size.TrySub(content_box_inset.SumAxes());
        var gap = style.Gap.ResolveOrZero(node_inner_size.Or(default(Size<float>)), ref tree);

        return new()
        {
            Direction = dir,
            IsRow = is_row,
            IsColumn = is_column,
            IsWrap = is_wrap,
            IsWrapReverse = is_wrap_reverse,
            MinSize = style.MinSize
                .TryResolve(parent_size, ref tree)
                .TryApplyAspectRatio(aspect_ratio)
                .TryAdd(box_sizing_adjustment),
            MaxSize = style.MaxSize
                .TryResolve(parent_size, ref tree)
                .TryApplyAspectRatio(aspect_ratio)
                .TryAdd(box_sizing_adjustment),
            Margin = margin,
            Border = border,
            ContentBoxInset = content_box_inset,
            ScrollbarGutter = scrollbar_gutter,
            Gap = gap,
            AlignItems = align_items,
            AlignContent = align_content,
            JustifyContent = justify_content,
            NodeOuterSize = node_outer_size,
            NodeInnerSize = node_inner_size,
            ContainerSize = default,
            InnerContainerSize = default
        };
    }

    [StructLayout(LayoutKind.Auto)]
    private struct FlexItem<TNodeId>
    {
        public TNodeId Node;

        public int Order;

        public Size<float?> Size;
        public Size<float?> MinSize;
        public Size<float?> MaxSize;
        public AlignSelf AlignSelf;

        public Point<Overflow> Overflow;
        public float ScrollbarWidth;
        public float FlexShrink;
        public float FlexGrow;

        public float ResolvedMinimumMainSize;

        public Rect<float?> Inset;
        public Rect<float> Margin;
        public Rect<bool> MarginIsAuto;
        public Rect<float> Padding;
        public Rect<float> Border;

        public float FlexBasis;
        public float InnerFlexBasis;
        public float Violation;
        public bool Frozen;

        public float ContentFlexFraction;

        public Size<float> HypotheticalInnerSize;
        public Size<float> HypotheticalOuterSize;
        public Size<float> TargetSize;
        public Size<float> OuterTargetSize;

        public float BaseLine;

        public float OffsetMain;
        public float OffsetCross;

        public bool IsScrollContainer() => Overflow.X.IsScrollContainer() | Overflow.Y.IsScrollContainer();
    }

    /// Generate anonymous flex items.
    ///
    /// # [9.1. Initial Setup](https://www.w3.org/TR/css-flexbox-1/#box-manip)
    ///
    /// - [**Generate anonymous flex items**](https://www.w3.org/TR/css-flexbox-1/#algo-anon-box) as described in [§4 Flex Items](https://www.w3.org/TR/css-flexbox-1/#flex-items).
    private static PooledList<FlexItem<TNodeId>> GenerateAnonymousFlexItems<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle,
        TFlexboxItemStyle>(
        ref TTree tree, TNodeId node, in AlgoConstants constants
    )
        where TTree : ILayoutFlexboxContainer<TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>, allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        where TFlexBoxContainerStyle : IFlexContainerStyle, allows ref struct
        where TFlexboxItemStyle : IFlexItemStyle, allows ref struct
    {
        using var list = new PooledList<FlexItem<TNodeId>>(tree.ChildCount(node));
        var i = 0;
        foreach (var child in tree.ChildIds(node).AsEnumerable<TChildIter, TNodeId>())
        {
            var index = i++;
            var style = tree.GetFlexboxChildStyle(child);
            if (style.Position == Position.Absolute) continue;
            if (style.BoxGenerationMode == BoxGenerationMode.None) continue;
            var aspect_ratio = style.AspectRatio;
            var padding = style.Padding.ResolveOrZero(constants.NodeInnerSize.Width, ref tree);
            var border = style.Border.ResolveOrZero(constants.NodeInnerSize.Width, ref tree);
            var pb_sum = padding.Add(border).SumAxes();
            var box_sizing_adjustment = style.BoxSizing == BoxSizing.ContentBox ? pb_sum : default;
            list.Add(new()
            {
                Node = child,
                Order = index,
                Size = style.Size
                    .TryResolve(constants.NodeInnerSize, ref tree)
                    .TryApplyAspectRatio(aspect_ratio)
                    .TryAdd(box_sizing_adjustment),
                MinSize = style.MinSize
                    .TryResolve(constants.NodeInnerSize, ref tree)
                    .TryApplyAspectRatio(aspect_ratio)
                    .TryAdd(box_sizing_adjustment),
                MaxSize = style.MaxSize
                    .TryResolve(constants.NodeInnerSize, ref tree)
                    .TryApplyAspectRatio(aspect_ratio)
                    .TryAdd(box_sizing_adjustment),
                AlignSelf = style.AlignSelf ?? (AlignSelf)constants.AlignItems,
                Overflow = style.Overflow,
                ScrollbarWidth = style.ScrollbarWidth,
                FlexShrink = style.FlexShrink,
                FlexGrow = style.FlexGrow,
                ResolvedMinimumMainSize = 0,
                Inset = style.Inset
                    .Zip(constants.NodeInnerSize, ref tree,
                        static (ref TTree tree, LengthPercentageAuto p, float? s) => p.TryResolve(s, ref tree)),
                Margin = style.Margin
                    .ResolveOrZero(constants.NodeInnerSize.Width, ref tree),
                MarginIsAuto = style.Margin.Map(static a => a.IsAuto),
                Padding = style.Padding
                    .ResolveOrZero(constants.NodeInnerSize.Width, ref tree),
                Border = style.Border
                    .ResolveOrZero(constants.NodeInnerSize.Width, ref tree),
                FlexBasis = 0,
                InnerFlexBasis = 0,
                Violation = 0,
                Frozen = false,
                ContentFlexFraction = 0,
                HypotheticalInnerSize = default,
                HypotheticalOuterSize = default,
                TargetSize = default,
                OuterTargetSize = default,
                BaseLine = 0,
                OffsetMain = 0,
                OffsetCross = 0,
            });
        }
        return list.Move();
    }

    /// Determine the available main and cross space for the flex items.
    ///
    /// # [9.2. Line Length Determination](https://www.w3.org/TR/css-flexbox-1/#line-sizing)
    ///
    /// - [**Determine the available main and cross space for the flex items**](https://www.w3.org/TR/css-flexbox-1/#algo-available).
    ///
    /// For each dimension, if that dimension of the flex container’s content box is a definite size, use that;
    /// if that dimension of the flex container is being sized under a min or max-content constraint, the available space in that dimension is that constraint;
    /// otherwise, subtract the flex container’s margin, border, and padding from the space available to the flex container in that dimension and use that value.
    /// **This might result in an infinite value**.
    private static Size<AvailableSpace> DetermineAvailableSpace(
        Size<float?> known_dimensions, Size<AvailableSpace> outer_available_space, in AlgoConstants constants
    )
    {
        var width = known_dimensions.Width is { } node_width
            ? AvailableSpace.MakeDefinite(node_width - constants.ContentBoxInset.HorizontalAxisSum())
            : outer_available_space.Width
                .TrySub(constants.Margin.HorizontalAxisSum())
                .TrySub(constants.ContentBoxInset.HorizontalAxisSum());

        var height = known_dimensions.Height is { } node_height
            ? AvailableSpace.MakeDefinite(node_height - constants.ContentBoxInset.VerticalAxisSum())
            : outer_available_space.Height
                .TrySub(constants.Margin.VerticalAxisSum())
                .TrySub(constants.ContentBoxInset.VerticalAxisSum());

        return new(width, height);
    }

    /// Determine the flex base size and hypothetical main size of each item.
    ///
    /// # [9.2. Line Length Determination](https://www.w3.org/TR/css-flexbox-1/#line-sizing)
    ///
    /// - [**Determine the flex base size and hypothetical main size of each item:**](https://www.w3.org/TR/css-flexbox-1/#algo-main-item)
    ///
    ///     - A. If the item has a definite used flex basis, that’s the flex base size.
    ///
    ///     - B. If the flex item has ...
    ///
    ///         - an intrinsic aspect ratio,
    ///         - a used flex basis of content, and
    ///         - a definite cross size,
    ///
    ///       then the flex base size is calculated from its inner cross size and the flex item’s intrinsic aspect ratio.
    ///
    ///     - C. If the used flex basis is content or depends on its available space, and the flex container is being sized under a min-content
    ///       or max-content constraint (e.g. when performing automatic table layout \[CSS21\]), size the item under that constraint.
    ///       The flex base size is the item’s resulting main size.
    ///
    ///     - E. Otherwise, size the item into the available space using its used flex basis in place of its main size, treating a value of content as max-content.
    ///       If a cross size is needed to determine the main size (e.g. when the flex item’s main size is in its block axis) and the flex item’s cross size is auto and not definite,
    ///       in this calculation use fit-content as the flex item’s cross size. The flex base size is the item’s resulting main size.
    ///
    ///   When determining the flex base size, the item’s min and max main sizes are ignored (no clamping occurs).
    ///   Furthermore, the sizing calculations that floor the content box size at zero when applying box-sizing are also ignored.
    ///   (For example, an item with a specified size of zero, positive padding, and box-sizing: border-box will have an outer flex base size of zero—and hence a negative inner flex base size.)
    private static void DetermineFlexBaseSize<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
        ref TTree tree, TNodeId node, in AlgoConstants constants, Size<AvailableSpace> available_space, Span<FlexItem<TNodeId>> flex_items
    )
        where TTree : ILayoutFlexboxContainer<TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>, allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        where TFlexBoxContainerStyle : IFlexContainerStyle, allows ref struct
        where TFlexboxItemStyle : IFlexItemStyle, allows ref struct
    {
        var dir = constants.Direction;

        foreach (ref var child in flex_items)
        {
            var style = tree.GetFlexboxChildStyle(child.Node);

            // Parent size for child sizing
            var cross_axis_parent_size = constants.NodeInnerSize.Cross(dir);
            var child_parent_size = Size.FromCross(dir, cross_axis_parent_size);

            // Available space for child sizing
            var cross_axis_margin_sum = constants.Margin.CrossAxisSum(dir);
            var child_min_cross = child.MinSize.Cross(dir).TryAdd(cross_axis_margin_sum);
            var child_max_cross = child.MaxSize.Cross(dir).TryAdd(cross_axis_margin_sum);

            // Clamp available space by min- and max- size
            AvailableSpace cross_axis_available_space;
            {
                cross_axis_available_space = available_space.Cross(dir) switch
                {
                    { Tag: AvailableSpace.Tags.Definite, Definite: var definite } => AvailableSpace.MakeDefinite(
                        (cross_axis_parent_size ?? definite).TryClamp(child_min_cross, child_max_cross)
                    ),
                    { Tag: AvailableSpace.Tags.MinContent } =>
                        child_min_cross is { } min ? AvailableSpace.MakeDefinite(min) : AvailableSpace.MinContent,
                    { Tag: AvailableSpace.Tags.MaxContent } =>
                        child_max_cross is { } max ? AvailableSpace.MakeDefinite(max) : AvailableSpace.MaxContent,
                    _ => throw new UnreachableException(),
                };
            }

            // Known dimensions for child sizing
            Size<float?> child_known_dimensions;
            {
                var ckd = child.Size.WithMain(dir, null);
                if (child.AlignSelf is AlignSelf.Stretch && ckd.Cross(dir) is null)
                {
                    ckd.SetCross(dir, cross_axis_available_space.TryGet().TrySub(child.Margin.CrossAxisSum(dir)));
                }
                child_known_dimensions = ckd;
            }

            var container_width = constants.NodeInnerSize.Main(dir);
            var box_sizing_adjustment = 0f;
            if (style.BoxSizing is BoxSizing.ContentBox)
            {
                var padding = style.Padding.ResolveOrZero(container_width, ref tree);
                var border = style.Border.ResolveOrZero(container_width, ref tree);
                box_sizing_adjustment = padding.Add(border).SumAxes().Main(dir);
            }
            var flex_basis = style.FlexBias
                .TryResolve(container_width, ref tree)
                .TryAdd(box_sizing_adjustment);

            #region Flex Basis

            {
                // A. If the item has a definite used flex basis, that’s the flex base size.

                // B. If the flex item has an intrinsic aspect ratio,
                //    a used flex basis of content, and a definite cross size,
                //    then the flex base size is calculated from its inner
                //    cross size and the flex item’s intrinsic aspect ratio.

                // Note: `child.size` has already been resolved against aspect_ratio in generate_anonymous_flex_items
                // So B will just work here by using main_size without special handling for aspect_ratio
                var main_size = child.Size.Main(dir);
                if ((flex_basis ?? main_size) is { } FlexBasis)
                {
                    child.FlexBasis = FlexBasis;
                    goto end_flex_basis;
                }

                // C. If the used flex basis is content or depends on its available space,
                //    and the flex container is being sized under a min-content or max-content
                //    constraint (e.g. when performing automatic table layout [CSS21]),
                //    size the item under that constraint. The flex base size is the item’s
                //    resulting main size.

                // This is covered by the implementation of E below, which passes the available_space constraint
                // through to the child size computation. It may need a separate implementation if/when D is implemented.

                // D. Otherwise, if the used flex basis is content or depends on its
                //    available space, the available main size is infinite, and the flex item’s
                //    inline axis is parallel to the main axis, lay the item out using the rules
                //    for a box in an orthogonal flow [CSS3-WRITING-MODES]. The flex base size
                //    is the item’s max-content main size.

                // E. Otherwise, size the item into the available space using its used flex basis
                //    in place of its main size, treating a value of content as max-content.
                //    If a cross size is needed to determine the main size (e.g. when the
                //    flex item’s main size is in its block axis) and the flex item’s cross size
                //    is auto and not definite, in this calculation use fit-content as the
                //    flex item’s cross size. The flex base size is the item’s resulting main size.

                var child_available_space = new Size<AvailableSpace>(AvailableSpace.MaxContent, AvailableSpace.MaxContent)
                    .WithMain(dir, available_space.Main(dir).Tag is AvailableSpace.Tags.MinContent ? AvailableSpace.MinContent : AvailableSpace.MaxContent)
                    .WithCross(dir, cross_axis_available_space);

                child.FlexBasis = BoxLayout.MeasureChildSize<TTree, TNodeId, TChildIter, TCoreContainerStyle>(
                    ref tree,
                    child.Node,
                    child_known_dimensions,
                    child_parent_size,
                    child_available_space,
                    SizingMode.ContentSize,
                    dir.MainAxis(),
                    default
                );
            }
            end_flex_basis: ;

            #endregion

            // Floor flex-basis by the padding_border_sum (floors inner_flex_basis at zero)
            // This seems to be in violation of the spec which explicitly states that the content box should not be floored at zero
            // (like it usually is) when calculating the flex-basis. But including this matches both Chrome and Firefox's behaviour.
            //
            // Spec: https://www.w3.org/TR/css-flexbox-1/#intrinsic-item-contributions
            // Spec: https://www.w3.org/TR/css-flexbox-1/#change-2016-max-contribution
            var padding_border_sum = child.Padding.MainAxisSum(constants.Direction) + child.Border.MainAxisSum(constants.Direction);
            child.FlexBasis = Math.Max(child.FlexBasis, padding_border_sum);

            // The hypothetical main size is the item’s flex base size clamped according to its
            // used min and max main sizes (and flooring the content box size at zero).

            child.InnerFlexBasis =
                child.FlexBasis - child.Padding.MainAxisSum(constants.Direction) - child.Border.MainAxisSum(constants.Direction);

            var padding_border_axes_sums = child.Padding.Add(child.Border).SumAxes().MapNullable();

            // Note that it is important that the `parent_size` parameter in the main axis is not set for this
            // function call as it used for resolving percentages, and percentage size in an axis should not contribute
            // to a min-content contribution in that same axis. However the `parent_size` and `available_space` *should*
            // be set to their usual values in the cross axis so that wrapping content can wrap correctly.
            //
            // See https://drafts.csswg.org/css-sizing-3/#min-percentage-contribution
            var style_min_main_size =
                child.MinSize.Or(child.Overflow.Map(static a => a.TryIntoAutomaticMinSize()).ToSize()).Main(dir);

            if (style_min_main_size is not null) child.ResolvedMinimumMainSize = style_min_main_size.GetValueOrDefault();
            else
            {
                var child_available_space = new Size<AvailableSpace>(AvailableSpace.MinContent).WithCross(dir, cross_axis_available_space);
                var min_content_main_size = BoxLayout.MeasureChildSize<TTree, TNodeId, TChildIter, TCoreContainerStyle>(
                    ref tree, child.Node,
                    child_known_dimensions,
                    child_parent_size,
                    child_available_space,
                    SizingMode.ContentSize,
                    dir.MainAxis(),
                    default
                );

                // 4.5. Automatic Minimum Size of Flex Items
                // https://www.w3.org/TR/css-flexbox-1/#min-size-auto
                var clamped_min_content_size = min_content_main_size
                    .TryMin(child.Size.Main(dir)).TryMin(child.MaxSize.Main(dir));
                child.ResolvedMinimumMainSize = clamped_min_content_size.TryMax(padding_border_axes_sums.Main(dir));
            }

            var hypothetical_inner_min_main =
                child.ResolvedMinimumMainSize.TryMax(padding_border_axes_sums.Main(constants.Direction));
            var hypothetical_inner_size =
                child.FlexBasis.TryClamp(hypothetical_inner_min_main, child.MaxSize.Main(constants.Direction));
            var hypothetical_outer_size = hypothetical_inner_size + child.Margin.MainAxisSum(constants.Direction);

            child.HypotheticalInnerSize.SetMain(constants.Direction, hypothetical_inner_size);
            child.HypotheticalOuterSize.SetMain(constants.Direction, hypothetical_outer_size);
        }
    }

    private struct FlexLine
    {
        public int ItemsStart;
        public int ItemsCount;
        public float CrossSize;
        public float OffsetCross;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Span<FlexItem<TNodeId>> GetItems<TNodeId>(in FlexLine line, Span<FlexItem<TNodeId>> flex_items) =>
        flex_items.Slice(line.ItemsStart, line.ItemsCount);

    /// Collect flex items into flex lines.
    ///
    /// # [9.3. Main Size Determination](https://www.w3.org/TR/css-flexbox-1/#main-sizing)
    ///
    /// - [**Collect flex items into flex lines**](https://www.w3.org/TR/css-flexbox-1/#algo-line-break):
    ///
    ///     - If the flex container is single-line, collect all the flex items into a single flex line.
    ///
    ///     - Otherwise, starting from the first uncollected item, collect consecutive items one by one until the first time that the next collected item would not fit into the flex container’s inner main size
    ///       (or until a forced break is encountered, see [§10 Fragmenting Flex Layout](https://www.w3.org/TR/css-flexbox-1/#pagination)).
    ///       If the very first uncollected item wouldn't fit, collect just it into the line.
    ///
    ///       For this step, the size of a flex item is its outer hypothetical main size. (**Note: This can be negative**.)
    ///
    ///       Repeat until all flex items have been collected into flex lines.
    ///
    ///       **Note that the "collect as many" line will collect zero-sized flex items onto the end of the previous line even if the last non-zero item exactly "filled up" the line**.
    private static PooledList<FlexLine> CollectFlexLines<TNodeId>(
        in AlgoConstants constants,
        Size<AvailableSpace> available_space,
        Span<FlexItem<TNodeId>> flex_items
    )
    {
        if (!constants.IsWrap) goto ret_all;
        var main_axis_available_space = constants.MaxSize.Main(constants.Direction) is { } MaxSize
            ? AvailableSpace.MakeDefinite(
                (available_space
                    .Main(constants.Direction)
                    .TryGet() ?? MaxSize)
                .TryMax(constants.MinSize.Main(constants.Direction))
            )
            : available_space.Main(constants.Direction);

        switch (main_axis_available_space.Tag)
        {
            // If we're sizing under a max-content constraint then the flex items will never wrap
            // (at least for now - future extensions to the CSS spec may add provisions for forced wrap points)
            case AvailableSpace.Tags.MaxContent: goto ret_all;
            // If flex-wrap is Wrap and we're sizing under a min-content constraint, then we take every possible wrapping opportunity
            // and place each item in it's own line
            case AvailableSpace.Tags.MinContent:
            {
                using var lines = new PooledList<FlexLine>(flex_items.Length);
                for (var i = 0; i < flex_items.Length; i++)
                {
                    lines.Add(new() { ItemsStart = i, ItemsCount = 1, CrossSize = 0, OffsetCross = 0 });
                }
                return lines.Move();
            }
            case AvailableSpace.Tags.Definite:
            {
                using var lines = new PooledList<FlexLine>(1);
                var main_axis_gap = constants.Gap.Main(constants.Direction);

                for (var cur = 0; cur < flex_items.Length;)
                {
                    var items = flex_items[cur..];
                    // Find index of the first item in the next line
                    // (or the last item if all remaining items are in the current line)
                    var line_length = 0f;
                    var idx = 0;
                    for (; idx < items.Length; idx++)
                    {
                        ref readonly var child = ref items[idx];
                        // Gaps only occur between items (not before the first one or after the last one)
                        // So first item in the line does not contribute a gap to the line length
                        var gap_contribution = idx == 0 ? 0 : main_axis_gap;
                        line_length += child.HypotheticalOuterSize.Main(constants.Direction) + gap_contribution;
                        if (line_length > main_axis_available_space && idx != 0) break;
                    }
                    lines.Add(new() { ItemsStart = cur, ItemsCount = idx, CrossSize = 0, OffsetCross = 0 });
                    cur += idx;
                }
                return lines.Move();
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        throw new UnreachableException();

        ret_all:
        {
            using var lines = new PooledList<FlexLine>(1);
            lines.Add(new() { ItemsStart = 0, ItemsCount = flex_items.Length, CrossSize = 0, OffsetCross = 0 });
            return lines.Move();
        }
    }

    /// Determine the container's main size (if not already known)
    private static void DetermineContainerMainSize<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
        ref TTree tree, TNodeId node, Size<AvailableSpace> available_space,
        Span<FlexItem<TNodeId>> flex_items, Span<FlexLine> lines, ref AlgoConstants constants
    )
        where TTree : ILayoutFlexboxContainer<TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>, allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        where TFlexBoxContainerStyle : IFlexContainerStyle, allows ref struct
        where TFlexboxItemStyle : IFlexItemStyle, allows ref struct
    {
        var dir = constants.Direction;
        var main_content_box_inset = constants.ContentBoxInset.MainAxisSum(dir);

        if (constants.NodeOuterSize.Main(constants.Direction) is not { } outer_main_size)
        {
            var main = available_space.Main(dir);
            switch (main.Tag)
            {
                case AvailableSpace.Tags.Definite:
                {
                    var longest_line_length = LongestLineLength(flex_items, lines, constants);
                    var size = longest_line_length + main_content_box_inset;
                    outer_main_size = lines.Length > 1 ? Math.Max(size, main.Definite) : size;
                    break;
                }
                case AvailableSpace.Tags.MinContent when constants.IsWrap:
                {
                    var longest_line_length = LongestLineLength(flex_items, lines, constants);
                    outer_main_size = longest_line_length + main_content_box_inset;
                    break;
                }
                case AvailableSpace.Tags.MinContent or AvailableSpace.Tags.MaxContent:
                {
                    // Define a base main_size variable. This is mutated once for iteration over the outer
                    // loop over the flex lines as:
                    //   "The flex container’s max-content size is the largest sum of the afore-calculated sizes of all items within a single line."
                    var main_size = 0f;

                    foreach (ref var line in lines)
                    {
                        var items = GetItems(line, flex_items);
                        foreach (ref var item in items)
                        {
                            var style_min = item.MinSize.Main(dir);
                            var style_preferred = item.Size.Main(dir);
                            var style_max = item.MaxSize.Main(dir);

                            // Spec: https://www.w3.org/TR/css-flexbox-1/#intrinsic-item-contributions
                            // Spec modification: https://www.w3.org/TR/css-flexbox-1/#change-2016-max-contribution
                            // Issue: https://github.com/w3c/csswg-drafts/issues/1435
                            // Gentest: padding_border_overrides_size_flex_basis_0.html
                            float? clamping_basis = item.FlexBasis.TryMax(style_preferred);
                            var flex_basis_min = item.FlexShrink == 0.0 ? clamping_basis : null;
                            var flex_basis_max = item.FlexGrow == 0.0 ? clamping_basis : null;

                            var min_main_size = Math.Max(
                                (style_min.TryMax(flex_basis_min) ?? flex_basis_min) ?? item.ResolvedMinimumMainSize,
                                item.ResolvedMinimumMainSize
                            );
                            var max_main_size =
                                (style_max.TryMin(flex_basis_max) ?? flex_basis_max) ?? float.PositiveInfinity;

                            var content_contribution = 0f;
                            switch ((min_main_size, style_preferred, max_main_size))
                            {
                                // If the clamping values are such that max <= min, then we can avoid the expensive step of computing the content size
                                // as we know that the clamping values will override it anyway
                                case (var min, { } pref, var max) when max <= min || max <= pref:
                                    content_contribution = Math.Max(Math.Min(pref, max), min) + item.Margin.MainAxisSum(dir);
                                    break;
                                case var (min, _, max) when max <= min:
                                    content_contribution = min + item.Margin.MainAxisSum(dir);
                                    break;

                                // Else compute the min- or -max content size and apply the full formula for computing the
                                // min- or max- content contribution
                                case var _ when item.IsScrollContainer():
                                    content_contribution = item.FlexBasis + item.Margin.MainAxisSum(dir);
                                    break;
                                default:
                                {
                                    // Parent size for child sizing
                                    var cross_axis_parent_size = constants.NodeInnerSize.Cross(dir);

                                    // Available space for child sizing
                                    var cross_axis_margin_sum = constants.Margin.CrossAxisSum(dir);
                                    var child_min_cross = item.MinSize.Cross(dir).TryAdd(cross_axis_margin_sum);
                                    var child_max_cross = item.MaxSize.Cross(dir).TryAdd(cross_axis_margin_sum);
                                    var cross_axis_available_space = available_space
                                        .Cross(dir)
                                        .MapDefiniteValue(
                                            cross_axis_parent_size,
                                            static (cross_axis_parent_size, val) => cross_axis_parent_size ?? val
                                        )
                                        .TryClamp(child_min_cross, child_max_cross);

                                    var child_available_space = available_space.WithCross(dir, cross_axis_available_space);

                                    // Known dimensions for child sizing
                                    Size<float?> child_known_dimensions = default;
                                    {
                                        var ckd = item.Size.WithMain(dir, null);
                                        if (item.AlignSelf == AlignSelf.Stretch && ckd.Cross(dir) is null)
                                        {
                                            ckd.SetCross(dir, cross_axis_available_space.TryGet().TrySub(item.Margin.CrossAxisSum(dir)));
                                        }
                                        child_known_dimensions = ckd;
                                    }

                                    // Either the min- or max- content size depending on which constraint we are sizing under.
                                    var content_main_size = BoxLayout.MeasureChildSize<TTree, TNodeId, TChildIter, TCoreContainerStyle>(
                                        ref tree,
                                        item.Node,
                                        child_known_dimensions,
                                        constants.NodeInnerSize,
                                        child_available_space,
                                        SizingMode.InherentSize,
                                        dir.MainAxis(),
                                        default
                                    ) + item.Margin.MainAxisSum(dir);

                                    // This is somewhat bizarre in that it's asymmetrical depending whether the flex container is a column or a row.
                                    //
                                    // I *think* this might relate to https://drafts.csswg.org/css-flexbox-1/#algo-main-container:
                                    //
                                    //    "The automatic block size of a block-level flex container is its max-content size."
                                    //
                                    // Which could suggest that flex-basis defining a vertical size does not shrink because it is in the block axis, and the automatic size
                                    // in the block axis is a MAX content size. Whereas a flex-basis defining a horizontal size does shrink because the automatic size in
                                    // inline axis is MIN content size (although I don't have a reference for that).
                                    //
                                    // Ultimately, this was not found by reading the spec, but by trial and error fixing tests to align with Webkit/Firefox output.
                                    // (see the `flex_basis_unconstraint_row` and `flex_basis_uncontraint_column` generated tests which demonstrate this)
                                    if (constants.IsRow)
                                    {
                                        content_contribution = content_main_size
                                            .TryClamp(style_min, style_max)
                                            .Max(main_content_box_inset);
                                    }
                                    else
                                    {
                                        content_contribution = content_main_size
                                            .Max(item.FlexBasis)
                                            .TryClamp(style_min, style_max)
                                            .Max(main_content_box_inset);
                                    }
                                    break;
                                }
                            }

                            {
                                var diff = content_contribution - item.FlexBasis;
                                if (diff > 0)
                                {
                                    item.ContentFlexFraction = diff / Math.Max(1f, item.FlexGrow);
                                }
                                else if (diff < 0)
                                {
                                    var scaled_shrink_factor = Math.Max(1f, item.FlexShrink * item.InnerFlexBasis);
                                    item.ContentFlexFraction = diff / scaled_shrink_factor;
                                }
                                else
                                {
                                    // We are assuming that diff is 0.0 here and that we haven't accidentally introduced a NaN
                                    item.ContentFlexFraction = 0;
                                }
                            }
                        }

                        // TODO Spec says to scale everything by the line's max flex fraction. But neither Chrome nor firefox implement this
                        // so we don't either. But if we did want to, we'd need this computation here (and to use it below):
                        //
                        // Within each line, find the largest max-content flex fraction among all the flex items.
                        // let line_flex_fraction = line
                        //     .items
                        //     .iter()
                        //     .map(|item| item.content_flex_fraction)
                        //     .max_by(|a, b| a.total_cmp(b))
                        //     .unwrap_or(0.0); // Unwrap case never gets hit because there is always at least one item a line

                        // Add each item’s flex base size to the product of:
                        //   - its flex grow factor (or scaled flex shrink factor,if the chosen max-content flex fraction was negative)
                        //   - the chosen max-content flex fraction
                        // then clamp that result by the max main size floored by the min main size.
                        //
                        // The flex container’s max-content size is the largest sum of the afore-calculated sizes of all items within a single line.
                        var item_main_size_sum = 0f;
                        foreach (ref var item in GetItems(line, flex_items))
                        {
                            var flex_fraction = item.ContentFlexFraction;

                            var flex_contribution = 0f;
                            if (item.ContentFlexFraction > 0)
                            {
                                flex_contribution = Math.Max(1f, item.FlexGrow) * flex_fraction;
                            }
                            else if (item.ContentFlexFraction < 0)
                            {
                                var scaled_shrink_factor = Math.Max(1f, item.FlexShrink) * item.InnerFlexBasis;
                                flex_contribution = scaled_shrink_factor * flex_fraction;
                            }

                            var size = item.FlexBasis + flex_contribution;
                            item.OuterTargetSize.SetMain(dir, size);
                            item.TargetSize.SetMain(dir, size);
                            item_main_size_sum += size;
                        }

                        var gap_sum = SumAxisGaps(constants.Gap.Main(dir), line.ItemsCount);
                        main_size = Math.Max(main_size, item_main_size_sum + gap_sum);
                    }

                    outer_main_size = main_size + main_content_box_inset;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        outer_main_size = outer_main_size
            .TryClamp(constants.MinSize.Main(dir), constants.MaxSize.Main(dir))
            .Max(main_content_box_inset - constants.ScrollbarGutter.Main(dir));

        var inner_main_size = Math.Max(outer_main_size - main_content_box_inset, 0f);
        constants.ContainerSize.SetMain(dir, outer_main_size);
        constants.InnerContainerSize.SetMain(dir, inner_main_size);
        constants.NodeInnerSize.SetMain(dir, inner_main_size);

        return;

        static float LongestLineLength(Span<FlexItem<TNodeId>> flex_items, Span<FlexLine> lines, in AlgoConstants constants)
        {
            var dir = constants.Direction;
            var longest_line_length = 0f;
            var first = true;
            foreach (ref readonly var line in lines)
            {
                var line_main_axis_gap = SumAxisGaps(constants.Gap.Main(dir), line.ItemsCount);
                var items = GetItems(line, flex_items);
                var total_target_size = 0f;
                foreach (ref readonly var child in items)
                {
                    var padding_border_sum = child.Padding.Add(child.Border).MainAxisSum(dir);
                    total_target_size += Math.Max(
                        child.FlexBasis.TryMax(child.MinSize.Main(dir)) + child.Margin.MainAxisSum(dir),
                        padding_border_sum
                    );
                }
                var val = total_target_size + line_main_axis_gap;
                if (first)
                {
                    first = false;
                    longest_line_length = val;
                }
                else longest_line_length = longest_line_length.MaxByTotalCmp(val);
            }
            return longest_line_length;
        }
    }

    /// Computes the total space taken up by gaps in an axis given:
    ///   - The size of each gap
    ///   - The number of items (children or flex-lines) between which there are gaps
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float SumAxisGaps(float gap, int num_items)
    {
        // Gaps only exist between items, so...
        if (num_items <= 1)
        {
            // ...if there are less than 2 items then there are no gaps
            return 0;
        }
        else
        {
            // ...otherwise there are (num_items - 1) gaps
            return gap * (num_items - 1);
        }
    }

    /// Resolve the flexible lengths of the items within a flex line.
    /// Sets the `main` component of each item's `target_size` and `outer_target_size`
    ///
    /// # [9.7. Resolving Flexible Lengths](https://www.w3.org/TR/css-flexbox-1/#resolve-flexible-lengths)
    private static void ResolveFlexibleLengths<TNodeId>(Span<FlexItem<TNodeId>> flex_items, ref FlexLine line, in AlgoConstants constants)
    {
        var dir = constants.Direction;

        var total_main_axis_gap = SumAxisGaps(constants.Gap.Main(dir), line.ItemsCount);

        // 1. Determine the used flex factor. Sum the outer hypothetical main sizes of all
        //    items on the line. If the sum is less than the flex container’s inner main size,
        //    use the flex grow factor for the rest of this algorithm; otherwise, use the
        //    flex shrink factor.

        var items = GetItems(line, flex_items);

        var total_hypothetical_outer_main_size = 0f;
        foreach (ref var child in items)
        {
            total_hypothetical_outer_main_size += child.HypotheticalOuterSize.Main(dir);
        }
        var used_flex_factor = total_main_axis_gap + total_hypothetical_outer_main_size;
        var growing = used_flex_factor < (constants.NodeInnerSize.Main(dir) ?? 0);
        var shrinking = used_flex_factor > (constants.NodeInnerSize.Main(dir) ?? 0);
        var exactly_sized = !growing & !shrinking;

        // 2. Size inflexible items. Freeze, setting its target main size to its hypothetical main size
        //    - Any item that has a flex factor of zero
        //    - If using the flex grow factor: any item that has a flex base size
        //      greater than its hypothetical main size
        //    - If using the flex shrink factor: any item that has a flex base size
        //      smaller than its hypothetical main size

        foreach (ref var child in items)
        {
            var inner_target_size = child.HypotheticalInnerSize.Main(dir);
            child.TargetSize.SetMain(dir, inner_target_size);

            if (
                exactly_sized
                || (child.FlexGrow == 0.0 && child.FlexShrink == 0.0)
                || (growing && child.FlexBasis > child.HypotheticalInnerSize.Main(dir))
                || (shrinking && child.FlexBasis < child.HypotheticalInnerSize.Main(dir))
            )
            {
                child.Frozen = true;
                var outer_target_size = inner_target_size + child.Margin.MainAxisSum(dir);
                child.OuterTargetSize.SetMain(dir, outer_target_size);
            }
        }

        if (exactly_sized) return;

        // 3. Calculate initial free space. Sum the outer sizes of all items on the line,
        //    and subtract this from the flex container’s inner main size. For frozen items,
        //    use their outer target main size; for other items, use their outer flex base size.

        float initial_free_space;
        {
            var used_space = total_main_axis_gap;
            foreach (ref var child in items)
            {
                used_space += child.Frozen
                    ? child.OuterTargetSize.Main(dir)
                    : child.FlexBasis + child.Margin.MainAxisSum(dir);
            }

            initial_free_space = constants.NodeInnerSize.Main(dir).TrySub(used_space) ?? 0;
        }

        // 4. Loop

        for (;;)
        {
            // a. Check for flexible items. If all the flex items on the line are frozen,
            //    free space has been distributed; exit this loop.

            foreach (ref readonly var item in items)
            {
                if (!item.Frozen) goto continue_loop;
            }
            break;
            continue_loop: ;

            // b. Calculate the remaining free space as for initial free space, above.
            //    If the sum of the unfrozen flex items’ flex factors is less than one,
            //    multiply the initial free space by this sum. If the magnitude of this
            //    value is less than the magnitude of the remaining free space, use this
            //    as the remaining free space.

            var used_space = total_main_axis_gap;
            foreach (ref readonly var child in items)
            {
                used_space += child.Frozen
                    ? child.OuterTargetSize.Main(dir)
                    : child.FlexBasis + child.Margin.MainAxisSum(dir);
            }

            using var unfrozen = new PooledList<int>(line.ItemsCount);
            for (var i = 0; i < items.Length; i++)
            {
                ref readonly var item = ref items[i];
                if (!item.Frozen) unfrozen.Add(i);
            }

            float sum_flex_grow = 0, sum_flex_shrink = 0;
            foreach (var i in unfrozen)
            {
                ref var item = ref items[i];
                sum_flex_grow += item.FlexGrow;
                sum_flex_shrink += item.FlexShrink;
            }

            float free_space;
            if (growing && sum_flex_grow < 1)
            {
                free_space = (initial_free_space * sum_flex_grow - total_main_axis_gap)
                    .TryMin(constants.NodeInnerSize.Main(dir).TrySub(used_space));
            }
            else if (shrinking && sum_flex_shrink < 1)
            {
                free_space = (initial_free_space * sum_flex_shrink - total_main_axis_gap)
                    .TryMax(constants.NodeInnerSize.Main(dir).TrySub(used_space));
            }
            else
            {
                free_space = constants.NodeInnerSize.Main(dir).TrySub(used_space) ?? used_flex_factor - used_space;
            }

            // c. Distribute free space proportional to the flex factors.
            //    - If the remaining free space is zero
            //        Do Nothing
            //    - If using the flex grow factor
            //        Find the ratio of the item’s flex grow factor to the sum of the
            //        flex grow factors of all unfrozen items on the line. Set the item’s
            //        target main size to its flex base size plus a fraction of the remaining
            //        free space proportional to the ratio.
            //    - If using the flex shrink factor
            //        For every unfrozen item on the line, multiply its flex shrink factor by
            //        its inner flex base size, and note this as its scaled flex shrink factor.
            //        Find the ratio of the item’s scaled flex shrink factor to the sum of the
            //        scaled flex shrink factors of all unfrozen items on the line. Set the item’s
            //        target main size to its flex base size minus a fraction of the absolute value
            //        of the remaining free space proportional to the ratio. Note this may result
            //        in a negative inner main size; it will be corrected in the next step.
            //    - Otherwise
            //        Do Nothing

            if (float.IsNormal(free_space))
            {
                if (growing && sum_flex_grow > 0)
                {
                    foreach (var i in unfrozen)
                    {
                        ref var child = ref items[i];
                        child.TargetSize.SetMain(dir, child.FlexBasis + free_space * (child.FlexGrow / sum_flex_grow));
                    }
                }
                else if (shrinking && sum_flex_shrink > 0)
                {
                    var sum_scaled_shrink_factor = 0f;
                    foreach (var i in unfrozen)
                    {
                        ref var child = ref items[i];
                        sum_scaled_shrink_factor += child.InnerFlexBasis * child.FlexShrink;
                    }

                    if (sum_scaled_shrink_factor > 0)
                    {
                        foreach (var i in unfrozen)
                        {
                            ref var child = ref items[i];
                            var scaled_shrink_factor = child.InnerFlexBasis * child.FlexShrink;
                            child.TargetSize.SetMain(
                                dir,
                                child.FlexBasis + free_space * (scaled_shrink_factor / sum_scaled_shrink_factor)
                            );
                        }
                    }
                }
            }

            // d. Fix min/max violations. Clamp each non-frozen item’s target main size by its
            //    used min and max main sizes and floor its content-box size at zero. If the
            //    item’s target main size was made smaller by this, it’s a max violation.
            //    If the item’s target main size was made larger by this, it’s a min violation.

            var total_violation = 0f;
            {
                foreach (var i in unfrozen)
                {
                    ref var child = ref items[i];
                    var resolved_min_main = child.ResolvedMinimumMainSize;
                    var max_main = child.MaxSize.Main(dir);
                    var clamped = child.TargetSize.Main(dir).TryClamp(resolved_min_main, max_main).Max(0f);
                    child.Violation = clamped - child.TargetSize.Main(dir);
                    child.TargetSize.SetMain(dir, clamped);
                    child.OuterTargetSize.SetMain(
                        dir,
                        child.TargetSize.Main(dir) + child.Margin.MainAxisSum(dir)
                    );

                    total_violation += child.Violation;
                }
            }

            // e. Freeze over-flexed items. The total violation is the sum of the adjustments
            //    from the previous step ∑(clamped size - unclamped size). If the total violation is:
            //    - Zero
            //        Freeze all items.
            //    - Positive
            //        Freeze all the items with min violations.
            //    - Negative
            //        Freeze all the items with max violations.

            foreach (var i in unfrozen)
            {
                ref var child = ref items[i];
                child.Frozen = total_violation switch
                {
                    > 0 => child.Violation > 0,
                    < 0 => child.Violation < 0,
                    _ => true
                };
            }

            // f. Return to the start of this loop.
        }
    }

    /// Determine the hypothetical cross size of each item.
    ///
    /// # [9.4. Cross Size Determination](https://www.w3.org/TR/css-flexbox-1/#cross-sizing)
    ///
    /// - [**Determine the hypothetical cross size of each item**](https://www.w3.org/TR/css-flexbox-1/#algo-cross-item)
    ///   by performing layout with the used main size and the available space, treating auto as fit-content.
    private static void DetermineHypotheticalCrossSize<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
        ref TTree tree, TNodeId node, Span<FlexItem<TNodeId>> flex_items, ref FlexLine line, in AlgoConstants constants, Size<AvailableSpace> available_space
    )
        where TTree : ILayoutFlexboxContainer<TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>, allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        where TFlexBoxContainerStyle : IFlexContainerStyle, allows ref struct
        where TFlexboxItemStyle : IFlexItemStyle, allows ref struct
    {
        var dir = constants.Direction;
        var items = GetItems(line, flex_items);
        foreach (ref var child in items)
        {
            var padding_border_sum = (child.Padding.Add(child.Border)).CrossAxisSum(dir);

            var child_known_main = AvailableSpace.MakeDefinite(constants.ContainerSize.Main(dir));

            var child_cross = child.Size.Cross(dir)
                .TryClamp(child.MinSize.Cross(dir), child.MaxSize.Cross(dir))
                .TryMax(padding_border_sum);

            var child_available_cross = available_space
                .Cross(dir)
                .TryClamp(child.MinSize.Cross(dir), child.MaxSize.Cross(dir))
                .TryMax(padding_border_sum);

            var child_inner_cross = child_cross ?? BoxLayout.MeasureChildSize<TTree, TNodeId, TChildIter, TCoreContainerStyle>(
                    ref tree,
                    child.Node,
                    new(
                        constants.IsRow ? child.TargetSize.Width : child_cross,
                        constants.IsRow ? child_cross : child.TargetSize.Height
                    ),
                    constants.NodeInnerSize,
                    new(
                        constants.IsRow ? child_known_main : child_available_cross,
                        constants.IsRow ? child_available_cross : child_known_main
                    ),
                    SizingMode.ContentSize,
                    dir.CrossAxis(),
                    default
                )
                .TryClamp(child.MinSize.Cross(dir), child.MaxSize.Cross(dir))
                .Max(padding_border_sum);
            var child_outer_cross = child_inner_cross + child.Margin.CrossAxisSum(dir);

            child.HypotheticalInnerSize.SetCross(dir, child_inner_cross);
            child.HypotheticalOuterSize.SetCross(dir, child_outer_cross);
        }
    }

    /// Calculate the base lines of the children.
    private static void CalculateChildrenBaseLines<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
        ref TTree tree, TNodeId node, Size<float?> node_size, Size<AvailableSpace> available_space,
        Span<FlexItem<TNodeId>> flex_items, Span<FlexLine> flex_lines, in AlgoConstants constants
    )
        where TTree : ILayoutFlexboxContainer<TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>, allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        where TFlexBoxContainerStyle : IFlexContainerStyle, allows ref struct
        where TFlexboxItemStyle : IFlexItemStyle, allows ref struct
    {
        // Only compute baselines for flex rows because we only support baseline alignment in the cross axis
        // where that axis is also the inline axis
        if (!constants.IsRow) return;

        foreach (var line in flex_lines)
        {
            var items = GetItems(line, flex_items);
            using var baseline_children = new PooledList<int>(items.Length);

            // If a flex line has one or zero items participating in baseline alignment then baseline alignment is a no-op so we skip
            for (var i = 0; i < items.Length; i++)
            {
                ref readonly var child = ref items[i];
                if (child.AlignSelf != AlignSelf.Baseline) continue;
                baseline_children.Add(i);
            }
            if (baseline_children.Count <= 1) continue;

            // Only calculate baselines for children participating in baseline alignment
            foreach (ref readonly var i in baseline_children)
            {
                ref var child = ref items[i];

                var measured_size_and_baselines = tree.PerformChildLayout(
                    child.Node,
                    new(
                        constants.IsRow ? child.TargetSize.Width : child.HypotheticalInnerSize.Width,
                        constants.IsRow ? child.HypotheticalInnerSize.Height : child.TargetSize.Height
                    ),
                    constants.NodeInnerSize,
                    new(
                        constants.IsRow ? constants.ContainerSize.Width : available_space.Width.TrySet(node_size.Width),
                        constants.IsRow ? available_space.Height.TrySet(node_size.Height) : constants.ContainerSize.Height
                    ),
                    SizingMode.ContentSize,
                    default
                );

                var baseline = measured_size_and_baselines.FirstBaseLines.Y;
                var height = measured_size_and_baselines.Size.Height;

                child.BaseLine = (baseline ?? height) + child.Margin.Top;
            }
        }
    }

    /// Calculate the cross size of each flex line.
    ///
    /// # [9.4. Cross Size Determination](https://www.w3.org/TR/css-flexbox-1/#cross-sizing)
    ///
    /// - [**Calculate the cross size of each flex line**](https://www.w3.org/TR/css-flexbox-1/#algo-cross-line).
    private static void CalculateCrossSize<TNodeId>(
        Span<FlexItem<TNodeId>> flex_items, Span<FlexLine> flex_lines, Size<float?> node_size, in AlgoConstants constants
    )
    {
        var dir = constants.Direction;

        // If the flex container is single-line and has a definite cross size,
        // the cross size of the flex line is the flex container’s inner cross size.
        if (!constants.IsWrap && node_size.Cross(dir).HasValue)
        {
            var cross_axis_padding_border = constants.ContentBoxInset.CrossAxisSum(dir);
            var cross_min_size = constants.MinSize.Cross(dir);
            var cross_max_size = constants.MaxSize.Cross(dir);
            flex_lines[0].CrossSize =
                node_size
                    .Cross(dir)
                    .TryClamp(cross_min_size, cross_max_size)
                    .TrySub(cross_axis_padding_border)
                    .TryMax(0f)
                ?? 0f;
        }
        else
        {
            // Otherwise, for each flex line:
            //
            //    1. Collect all the flex items whose inline-axis is parallel to the main-axis, whose
            //       align-self is baseline, and whose cross-axis margins are both non-auto. Find the
            //       largest of the distances between each item’s baseline and its hypothetical outer
            //       cross-start edge, and the largest of the distances between each item’s baseline
            //       and its hypothetical outer cross-end edge, and sum these two values.

            //    2. Among all the items not collected by the previous step, find the largest
            //       outer hypothetical cross size.

            //    3. The used cross-size of the flex line is the largest of the numbers found in the
            //       previous two steps and zero.
            foreach (ref var line in flex_lines)
            {
                var items = GetItems(line, flex_items);
                var max_baseline = 0f;
                foreach (ref var child in items)
                {
                    max_baseline = Math.Max(max_baseline, child.BaseLine);
                }
                line.CrossSize = 0;
                foreach (ref var child in items)
                {
                    var x =
                        child.AlignSelf == AlignSelf.Baseline
                        && !child.MarginIsAuto.CrossStart(dir)
                        && !child.MarginIsAuto.CrossEnd(dir)
                            ? max_baseline - child.BaseLine + child.HypotheticalOuterSize.Cross(dir)
                            : child.HypotheticalOuterSize.Cross(dir);
                    line.CrossSize = Math.Max(line.CrossSize, x);
                }
            }

            // If the flex container is single-line, then clamp the line’s cross-size to be within the container’s computed min and max cross sizes.
            // Note that if CSS 2.1’s definition of min/max-width/height applied more generally, this behavior would fall out automatically.
            if (!constants.IsWrap)
            {
                var cross_axis_padding_border = constants.ContentBoxInset.CrossAxisSum(dir);
                var cross_min_size = constants.MinSize.Cross(dir);
                var cross_max_size = constants.MaxSize.Cross(dir);
                flex_lines[0].CrossSize = flex_lines[0].CrossSize.TryClamp(
                    cross_min_size.TrySub(cross_axis_padding_border),
                    cross_max_size.TrySub(cross_axis_padding_border)
                );
            }
        }
    }

    /// Handle 'align-content: stretch'.
    ///
    /// # [9.4. Cross Size Determination](https://www.w3.org/TR/css-flexbox-1/#cross-sizing)
    ///
    /// - [**Handle 'align-content: stretch'**](https://www.w3.org/TR/css-flexbox-1/#algo-line-stretch). If the flex container has a definite cross size, align-content is stretch,
    ///   and the sum of the flex lines' cross sizes is less than the flex container’s inner cross size,
    ///   increase the cross size of each flex line by equal amounts such that the sum of their cross sizes exactly equals the flex container’s inner cross size.
    private static void HandleAlignContentStretch(
        Span<FlexLine> flex_lines, Size<float?> node_size, in AlgoConstants constants
    )
    {
        if (constants.AlignContent != AlignContent.Stretch) return;
        var dir = constants.Direction;
        var cross_axis_padding_border = constants.ContentBoxInset.CrossAxisSum(dir);
        var cross_min_size = constants.MinSize.Cross(dir);
        var cross_max_size = constants.MaxSize.Cross(dir);
        var container_min_inner_cross =
            (node_size
                .Cross(dir) ?? cross_min_size)
            .TryClamp(cross_min_size, cross_max_size)
            .TrySub(cross_axis_padding_border)
            .TryMax(0)
            ?? 0;

        var total_cross_axis_gap = SumAxisGaps(constants.Gap.Cross(dir), flex_lines.Length);
        var lines_total_cross = total_cross_axis_gap;
        foreach (ref readonly var line in flex_lines)
        {
            lines_total_cross += line.CrossSize;
        }

        if (lines_total_cross < container_min_inner_cross)
        {
            var remaining = container_min_inner_cross - lines_total_cross;
            var addition = remaining / flex_lines.Length;
            foreach (ref var line in flex_lines)
            {
                line.CrossSize += addition;
            }
        }
    }

    /// Determine the used cross size of each flex item.
    ///
    /// # [9.4. Cross Size Determination](https://www.w3.org/TR/css-flexbox-1/#cross-sizing)
    ///
    /// - [**Determine the used cross size of each flex item**](https://www.w3.org/TR/css-flexbox-1/#algo-stretch). If a flex item has align-self: stretch, its computed cross size property is auto,
    ///   and neither of its cross-axis margins are auto, the used outer cross size is the used cross size of its flex line, clamped according to the item’s used min and max cross sizes.
    ///   Otherwise, the used cross size is the item’s hypothetical cross size.
    ///
    ///   If the flex item has align-self: stretch, redo layout for its contents, treating this used size as its definite cross size so that percentage-sized children can be resolved.
    ///
    ///   **Note that this step does not affect the main size of the flex item, even if it has an intrinsic aspect ratio**.
    private static void DetermineUsedCrossSize<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
        ref TTree tree, TNodeId node, Span<FlexItem<TNodeId>> flex_items, Span<FlexLine> flex_lines, in AlgoConstants constants
    )
        where TTree : ILayoutFlexboxContainer<TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>, allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        where TFlexBoxContainerStyle : IFlexContainerStyle, allows ref struct
        where TFlexboxItemStyle : IFlexItemStyle, allows ref struct
    {
        var dir = constants.Direction;
        foreach (ref var line in flex_lines)
        {
            var line_cross_size = line.CrossSize;

            var items = GetItems(line, flex_items);
            foreach (ref var child in items)
            {
                var child_id = child.Node;
                var child_style = tree.GetFlexboxChildStyle(child_id);
                if (
                    child.AlignSelf == AlignSelf.Stretch
                    && !child.MarginIsAuto.CrossStart(dir)
                    && !child.MarginIsAuto.CrossEnd(dir)
                    && child_style.Size.Cross(dir).IsAuto
                )
                {
                    // For some reason this particular usage of max_width is an exception to the rule that max_width's transfer
                    // using the aspect_ratio (if set). Both Chrome and Firefox agree on this. And reading the spec, it seems like
                    // a reasonable interpretation. Although it seems to me that the spec *should* apply aspect_ratio here.
                    var padding = child_style.Padding
                        .ResolveOrZero(constants.NodeInnerSize, ref tree);
                    var border = child_style.Border
                        .ResolveOrZero(constants.NodeInnerSize, ref tree);
                    var pb_sum = padding.Add(border).SumAxes();
                    var box_sizing_adjustment = child_style.BoxSizing == BoxSizing.ContentBox ? pb_sum : default;

                    var max_size_ignoring_aspect_ratio = child_style
                        .MaxSize
                        .TryResolve(constants.NodeInnerSize, ref tree)
                        .TryAdd(box_sizing_adjustment);

                    child.TargetSize.SetCross(dir, (line_cross_size - child.Margin.CrossAxisSum(dir)).TryClamp(
                        child.MinSize.Cross(dir),
                        max_size_ignoring_aspect_ratio.Cross(dir)
                    ));
                }
                else
                {
                    child.TargetSize.SetCross(dir, child.HypotheticalInnerSize.Cross(dir));
                }

                child.OuterTargetSize.SetCross(
                    dir,
                    child.TargetSize.Cross(dir) + child.Margin.CrossAxisSum(dir)
                );
            }
        }
    }

    /// Distribute any remaining free space.
    ///
    /// # [9.5. Main-Axis Alignment](https://www.w3.org/TR/css-flexbox-1/#main-alignment)
    ///
    /// - [**Distribute any remaining free space**](https://www.w3.org/TR/css-flexbox-1/#algo-main-align). For each flex line:
    ///
    ///   1. If the remaining free space is positive and at least one main-axis margin on this line is `auto`, distribute the free space equally among these margins.
    ///      Otherwise, set all `auto` margins to zero.
    ///
    ///   2. Align the items along the main-axis per `justify-content`.
    private static void DistributeRemainingFreeSpace<TNodeId>(
        Span<FlexItem<TNodeId>> flex_items, Span<FlexLine> flex_lines, in AlgoConstants constants
    )
    {
        var dir = constants.Direction;
        foreach (ref var line in flex_lines)
        {
            var items = GetItems(line, flex_items);

            var total_main_axis_gap = SumAxisGaps(constants.Gap.Main(dir), line.ItemsCount);
            var used_space = total_main_axis_gap;
            foreach (ref readonly var child in items)
            {
                used_space += child.OuterTargetSize.Main(dir);
            }
            var free_space = constants.InnerContainerSize.Main(dir) - used_space;
            var num_auto_margins = 0;

            foreach (ref var child in items)
            {
                if (child.MarginIsAuto.MainStart(dir))
                {
                    num_auto_margins++;
                }
                if (child.MarginIsAuto.MainEnd(dir))
                {
                    num_auto_margins++;
                }
            }

            if (free_space > 0 && num_auto_margins > 0)
            {
                var margin = free_space / num_auto_margins;

                foreach (ref var child in items)
                {
                    if (child.MarginIsAuto.MainStart(dir))
                    {
                        if (constants.IsRow) child.Margin.Left = margin;
                        else child.Margin.Top = margin;
                    }
                    if (child.MarginIsAuto.MainEnd(dir))
                    {
                        if (constants.IsRow) child.Margin.Right = margin;
                        else child.Margin.Bottom = margin;
                    }
                }
            }
            else
            {
                var num_items = line.ItemsCount;
                var layout_reverse = dir.IsReverse();
                var gap = constants.Gap.Main(dir);
                var is_safe = false;
                var raw_justify_content_mode = constants.JustifyContent ?? JustifyContent.FlexStart;
                var justify_content_mode =
                    BoxLayout.ApplyAlignmentFallback(free_space, num_items, raw_justify_content_mode.ToAlignContent(), is_safe);

                if (layout_reverse)
                {
                    var end = items.Length - 1;
                    for (var i = end; i >= 0; i--)
                    {
                        ref var child = ref items[i];
                        var is_first = i == end;
                        child.OffsetMain = BoxLayout.ComputeAlignmentOffset(
                            free_space, num_items, gap, justify_content_mode, layout_reverse, is_first
                        );
                    }
                }
                else
                {
                    for (var i = 0; i < items.Length; i++)
                    {
                        ref var child = ref items[i];
                        var is_first = i == 0;
                        child.OffsetMain = BoxLayout.ComputeAlignmentOffset(
                            free_space, num_items, gap, justify_content_mode, layout_reverse, is_first
                        );
                    }
                }
            }
        }
    }

    /// Resolve cross-axis `auto` margins.
    ///
    /// # [9.6. Cross-Axis Alignment](https://www.w3.org/TR/css-flexbox-1/#cross-alignment)
    ///
    /// - [**Resolve cross-axis `auto` margins**](https://www.w3.org/TR/css-flexbox-1/#algo-cross-margins).
    ///   If a flex item has auto cross-axis margins:
    ///
    ///   - If its outer cross size (treating those auto margins as zero) is less than the cross size of its flex line,
    ///     distribute the difference in those sizes equally to the auto margins.
    ///
    ///   - Otherwise, if the block-start or inline-start margin (whichever is in the cross axis) is auto, set it to zero.
    ///     Set the opposite margin so that the outer cross size of the item equals the cross size of its flex line.
    private static void ResolveCrossAxisAutoMargins<TNodeId>(
        Span<FlexItem<TNodeId>> flex_items, Span<FlexLine> flex_lines, in AlgoConstants constants
    )
    {
        var dir = constants.Direction;
        foreach (ref var line in flex_lines)
        {
            var line_cross_size = line.CrossSize;
            var items = GetItems(line, flex_items);
            var max_baseline = 0f;
            foreach (ref readonly var child in items)
            {
                max_baseline = Math.Max(max_baseline, child.BaseLine);
            }

            foreach (ref var child in items)
            {
                var free_space = line_cross_size - child.OuterTargetSize.Cross(dir);

                if (child.MarginIsAuto.CrossStart(dir) && child.MarginIsAuto.CrossEnd(dir))
                {
                    if (constants.IsRow)
                    {
                        child.Margin.Top = free_space / 2;
                        child.Margin.Bottom = free_space / 2;
                    }
                    else
                    {
                        child.Margin.Left = free_space / 2;
                        child.Margin.Right = free_space / 2;
                    }
                }
                else if (child.MarginIsAuto.CrossStart(dir))
                {
                    if (constants.IsRow)
                    {
                        child.Margin.Top = free_space;
                    }
                    else
                    {
                        child.Margin.Left = free_space;
                    }
                }
                else if (child.MarginIsAuto.CrossEnd(dir))
                {
                    if (constants.IsRow)
                    {
                        child.Margin.Bottom = free_space;
                    }
                    else
                    {
                        child.Margin.Right = free_space;
                    }
                }
                else
                {
                    // 14. Align all flex items along the cross-axis.
                    child.OffsetCross = AlignFlexItemsAlongCrossAxis(ref child, free_space, max_baseline, constants);
                }
            }
        }
    }

    /// Align all flex items along the cross-axis.
    ///
    /// # [9.6. Cross-Axis Alignment](https://www.w3.org/TR/css-flexbox-1/#cross-alignment)
    ///
    /// - [**Align all flex items along the cross-axis**](https://www.w3.org/TR/css-flexbox-1/#algo-cross-align) per `align-self`,
    ///   if neither of the item's cross-axis margins are `auto`.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float AlignFlexItemsAlongCrossAxis<TNodeId>(
        ref readonly FlexItem<TNodeId> child, float free_space, float max_baseline, in AlgoConstants constants
    ) => child.AlignSelf switch
    {
        AlignSelf.Start => 0,
        AlignSelf.End => free_space,
        AlignSelf.FlexStart => constants.IsWrapReverse ? free_space : 0,
        AlignSelf.FlexEnd => constants.IsWrapReverse ? 0 : free_space,
        AlignSelf.Center => free_space / 2,
        AlignSelf.Baseline => constants.IsRow ? max_baseline - child.BaseLine :
            // Until we support vertical writing modes, baseline alignment only makes sense if
            // the constants.direction is row, so we treat it as flex-start alignment in columns.
            constants.IsWrapReverse ? free_space : 0,
        AlignSelf.Stretch => constants.IsWrapReverse ? free_space : 0,
        _ => throw new ArgumentOutOfRangeException()
    };

    /// Determine the flex container’s used cross size.
    ///
    /// # [9.6. Cross-Axis Alignment](https://www.w3.org/TR/css-flexbox-1/#cross-alignment)
    ///
    /// - [**Determine the flex container’s used cross size**](https://www.w3.org/TR/css-flexbox-1/#algo-cross-container):
    ///
    ///     - If the cross size property is a definite size, use that, clamped by the used min and max cross sizes of the flex container.
    ///
    ///     - Otherwise, use the sum of the flex lines' cross sizes, clamped by the used min and max cross sizes of the flex container.
    private static float DetermineContainerCrossSize(
        Span<FlexLine> flex_lines, Size<float?> node_size, ref AlgoConstants constants
    )
    {
        var dir = constants.Direction;
        var total_cross_axis_gap = SumAxisGaps(constants.Gap.Cross(dir), flex_lines.Length);
        var total_line_cross_size = 0f;
        foreach (ref readonly var line in flex_lines)
        {
            total_line_cross_size += line.CrossSize;
        }

        var padding_border_sum = constants.ContentBoxInset.CrossAxisSum(dir);
        var cross_scrollbar_gutter = constants.ScrollbarGutter.Cross(dir);
        var min_cross_size = constants.MinSize.Cross(dir);
        var max_cross_size = constants.MaxSize.Cross(dir);
        var outer_container_size =
            (node_size.Cross(dir)
             ?? total_line_cross_size + total_cross_axis_gap + padding_border_sum)
            .TryClamp(min_cross_size, max_cross_size)
            .Max(padding_border_sum - cross_scrollbar_gutter);
        var inner_container_size = Math.Max(outer_container_size - padding_border_sum, 0);

        constants.ContainerSize.SetCross(dir, outer_container_size);
        constants.InnerContainerSize.SetCross(dir, inner_container_size);

        return total_line_cross_size;
    }

    /// Align all flex lines per `align-content`.
    ///
    /// # [9.6. Cross-Axis Alignment](https://www.w3.org/TR/css-flexbox-1/#cross-alignment)
    ///
    /// - [**Align all flex lines**](https://www.w3.org/TR/css-flexbox-1/#algo-line-align) per `align-content`.
    private static void AlignFlexLinesPerAlignContent(
        Span<FlexLine> flex_lines, in AlgoConstants constants, float total_cross_size
    )
    {
        var dir = constants.Direction;
        var num_lines = flex_lines.Length;
        var gap = constants.Gap.Cross(dir);
        var total_cross_axis_gap = SumAxisGaps(gap, num_lines);
        var free_space = constants.InnerContainerSize.Cross(dir) - total_cross_size - total_cross_axis_gap;
        var is_safe = false;

        var align_content_mode = BoxLayout.ApplyAlignmentFallback(free_space, num_lines, constants.AlignContent, is_safe);

        if (constants.IsWrapReverse)
        {
            var end = flex_lines.Length - 1;
            for (var i = end; i >= 0; i--)
            {
                ref var line = ref flex_lines[i];
                line.OffsetCross = BoxLayout.ComputeAlignmentOffset(
                    free_space, num_lines, gap, align_content_mode, constants.IsWrapReverse, i == end
                );
            }
        }
        else
        {
            for (var i = 0; i < flex_lines.Length; i++)
            {
                ref var line = ref flex_lines[i];
                line.OffsetCross = BoxLayout.ComputeAlignmentOffset(
                    free_space, num_lines, gap, align_content_mode, constants.IsWrapReverse, i == 0
                );
            }
        }
    }

    /// Do a final layout pass and collect the resulting layouts.
    private static Size<float> FinalLayoutPass<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
        ref TTree tree, TNodeId node, Span<FlexItem<TNodeId>> flex_items, Span<FlexLine> flex_lines, in AlgoConstants constants
    )
        where TTree : ILayoutFlexboxContainer<TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>, allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        where TFlexBoxContainerStyle : IFlexContainerStyle, allows ref struct
        where TFlexboxItemStyle : IFlexItemStyle, allows ref struct
    {
        var dir = constants.Direction;
        var total_offset_cross = constants.ContentBoxInset.CrossStart(dir);

        Size<float> content_size = default;
        if (constants.IsWrapReverse)
        {
            var end = flex_lines.Length - 1;
            for (var i = end; i >= 0; i--)
            {
                CalculateLayoutLine<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
                    ref tree,
                    node,
                    flex_items,
                    ref flex_lines[i],
                    ref total_offset_cross,
                    ref content_size,
                    constants.ContainerSize,
                    constants.NodeInnerSize,
                    constants.ContentBoxInset,
                    dir
                );
            }
        }
        else
        {
            for (var i = 0; i < flex_lines.Length; i++)
            {
                CalculateLayoutLine<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
                    ref tree,
                    node,
                    flex_items,
                    ref flex_lines[i],
                    ref total_offset_cross,
                    ref content_size,
                    constants.ContainerSize,
                    constants.NodeInnerSize,
                    constants.ContentBoxInset,
                    dir
                );
            }
        }

        content_size.Width += constants.ContentBoxInset.Right - constants.Border.Right - constants.ScrollbarGutter.X;
        content_size.Height += constants.ContentBoxInset.Bottom - constants.Border.Bottom - constants.ScrollbarGutter.Y;

        return content_size;
    }

    /// Calculates the layout line
    private static void CalculateLayoutLine<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
        ref TTree tree, TNodeId node, Span<FlexItem<TNodeId>> flex_items, ref FlexLine line, ref float total_offset_cross, ref Size<float> content_size,
        Size<float> container_size, Size<float?> node_inner_size, Rect<float> padding_border, FlexDirection direction
    )
        where TTree : ILayoutFlexboxContainer<TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>, allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        where TFlexBoxContainerStyle : IFlexContainerStyle, allows ref struct
        where TFlexboxItemStyle : IFlexItemStyle, allows ref struct
    {
        var total_offset_main = padding_border.MainStart(direction);
        var line_offset_cross = line.OffsetCross;

        var items = GetItems(line, flex_items);
        if (direction.IsReverse())
        {
            var end = items.Length - 1;
            for (var i = end; i >= 0; i--)
            {
                CalculateFlexItem<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
                    ref tree,
                    node,
                    ref items[i],
                    ref total_offset_main,
                    total_offset_cross,
                    line_offset_cross,
                    ref content_size,
                    container_size,
                    node_inner_size,
                    direction
                );
            }
        }
        else
        {
            for (var i = 0; i < items.Length; i++)
            {
                CalculateFlexItem<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
                    ref tree,
                    node,
                    ref items[i],
                    ref total_offset_main,
                    total_offset_cross,
                    line_offset_cross,
                    ref content_size,
                    container_size,
                    node_inner_size,
                    direction
                );
            }
        }

        total_offset_cross += line_offset_cross + line.CrossSize;
    }

    /// Calculates the layout for a flex-item
    private static void CalculateFlexItem<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
        ref TTree tree, TNodeId node, ref FlexItem<TNodeId> item, ref float total_offset_main, float total_offset_cross, float line_offset_cross,
        ref Size<float> total_content_size, Size<float> container_size, Size<float?> node_inner_size, FlexDirection direction
    )
        where TTree : ILayoutFlexboxContainer<TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>, allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        where TFlexBoxContainerStyle : IFlexContainerStyle, allows ref struct
        where TFlexboxItemStyle : IFlexItemStyle, allows ref struct
    {
        var item_node = item.Node;
        var layout_output = tree.PerformChildLayout(
            item_node,
            item.TargetSize.MapNullable(),
            node_inner_size,
            container_size.Map(AvailableSpace.MakeDefinite),
            SizingMode.ContentSize,
            default
        );
        var size = layout_output.Size;
        var content_size = layout_output.ContentSize;

        var offset_main =
            total_offset_main
            + item.OffsetMain
            + item.Margin.MainStart(direction)
            + ((item.Inset.MainStart(direction) ?? item.Inset.MainEnd(direction).TryNeg()) ?? 0);

        var offset_cross =
            total_offset_cross
            + item.OffsetCross
            + line_offset_cross
            + item.Margin.CrossStart(direction)
            + ((item.Inset.CrossStart(direction) ?? item.Inset.CrossEnd(direction).TryNeg()) ?? 0);

        if (direction.IsRow())
        {
            var baseline_offset_cross = total_offset_cross + item.OffsetCross + item.Margin.CrossStart(direction);
            var inner_baseline = layout_output.FirstBaseLines.Y ?? size.Height;
            item.BaseLine = baseline_offset_cross + inner_baseline;
        }
        else
        {
            var baseline_offset_main = total_offset_main + item.OffsetMain + item.Margin.MainStart(direction);
            var inner_baseline = layout_output.FirstBaseLines.Y ?? size.Height;
            item.BaseLine = baseline_offset_main + inner_baseline;
        }

        Point<float> location = direction.IsRow()
            ? new(offset_main, offset_cross)
            : new(offset_cross, offset_main);
        Size<float> scrollbar_size = new(
            item.Overflow.Y == Overflow.Scroll ? item.ScrollbarWidth : 0,
            item.Overflow.X == Overflow.Scroll ? item.ScrollbarWidth : 0
        );

        tree.SetUnroundedLayout(
            item_node,
            new UnroundedLayout
            {
                Order = item.Order,
                Location = location,
                Size = size,
                ContentSize = content_size,
                ScrollbarSize = scrollbar_size,
                Border = item.Border,
                Padding = item.Padding,
                Margin = item.Margin,
            }
        );

        total_offset_main += item.OffsetMain + item.Margin.MainAxisSum(direction) + size.Main(direction);
        total_content_size =
            total_content_size.Max(BoxLayout.ComputeContentSizeContribution(location, size, content_size, item.Overflow));
    }

    /// Perform absolute layout on all absolutely positioned children.
    private static Size<float> PerformAbsoluteLayoutOnAbsoluteChildren<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle,
        TFlexboxItemStyle>(
        ref TTree tree, TNodeId node, in AlgoConstants constants
    )
        where TTree : ILayoutFlexboxContainer<TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>, allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        where TFlexBoxContainerStyle : IFlexContainerStyle, allows ref struct
        where TFlexboxItemStyle : IFlexItemStyle, allows ref struct
    {
        var container_width = constants.ContainerSize.Width;
        var container_height = constants.ContainerSize.Height;

        var inset_relative_size =
            constants.ContainerSize.Sub(constants.Border.SumAxes()).Sub(constants.ScrollbarGutter);

        Size<float> content_size = default;

        var order = 0;
        foreach (var child in tree.ChildIds(node).AsEnumerable<TChildIter, TNodeId>())
        {
            var child_style = tree.GetFlexboxChildStyle(child);

            // Skip items that are display:none or are not position:absolute
            if (child_style.BoxGenerationMode == BoxGenerationMode.None || child_style.Position != Position.Absolute)
                continue;

            var overflow = child_style.Overflow;
            var scrollbar_width = child_style.ScrollbarWidth;
            var aspect_ratio = child_style.AspectRatio;
            var align_self = child_style.AlignSelf ?? (AlignSelf)constants.AlignItems;
            var margin = child_style.Margin.TryResolve(inset_relative_size.Width, ref tree);
            var padding = child_style.Padding.ResolveOrZero(inset_relative_size.Width, ref tree);
            var border = child_style.Border.ResolveOrZero(inset_relative_size.Width, ref tree);
            var padding_border_sum = padding.Add(border).SumAxes();
            var box_sizing_adjustment = child_style.BoxSizing == BoxSizing.ContentBox ? padding_border_sum : default;

            // Resolve inset
            // Insets are resolved against the container size minus border
            var left =
                child_style.Inset.Left.TryResolve(inset_relative_size.Width, ref tree);
            var right =
                child_style.Inset.Right.TryResolve(inset_relative_size.Width, ref tree);
            var top = child_style.Inset.Top.TryResolve(inset_relative_size.Height, ref tree);
            var bottom =
                child_style.Inset.Bottom.TryResolve(inset_relative_size.Height, ref tree);

            // Compute known dimensions from min/max/inherent size styles
            var style_size = child_style
                .Size
                .TryResolve(inset_relative_size, ref tree)
                .TryApplyAspectRatio(aspect_ratio)
                .TryAdd(box_sizing_adjustment);
            var min_size = child_style
                .MinSize
                .TryResolve(inset_relative_size, ref tree)
                .TryApplyAspectRatio(aspect_ratio)
                .TryAdd(box_sizing_adjustment)
                .Or(padding_border_sum.MapNullable())
                .TryMax(padding_border_sum);
            var max_size = child_style
                .MaxSize
                .TryResolve(inset_relative_size, ref tree)
                .TryApplyAspectRatio(aspect_ratio)
                .TryAdd(box_sizing_adjustment);
            var known_dimensions = style_size.TryClamp(min_size, max_size);

            // Fill in width from left/right and reapply aspect ratio if:
            //   - Width is not already known
            //   - Item has both left and right inset properties set
            if ((known_dimensions.Width, left, right) is (null, { } left1, { } right1))
            {
                var new_width_raw = inset_relative_size.Width.TrySub(margin.Left).TrySub(margin.Right) - left1 - right1;
                known_dimensions.Width = Math.Max(new_width_raw, 0);
                known_dimensions = known_dimensions.TryApplyAspectRatio(aspect_ratio).TryClamp(min_size, max_size);
            }

            // Fill in height from top/bottom and reapply aspect ratio if:
            //   - Height is not already known
            //   - Item has both top and bottom inset properties set
            if ((known_dimensions.Height, top, bottom) is (null, { } top1, { } bottom1))
            {
                var new_height_raw = inset_relative_size.Height.TrySub(margin.Top).TrySub(margin.Bottom) - top1 - bottom1;
                known_dimensions.Height = Math.Max(new_height_raw, 0);
                known_dimensions = known_dimensions.TryApplyAspectRatio(aspect_ratio).TryClamp(min_size, max_size);
            }

            var layout_output = tree.PerformChildLayout(
                child,
                known_dimensions,
                constants.NodeInnerSize,
                new(
                    AvailableSpace.MakeDefinite(container_width.TryClamp(min_size.Width, max_size.Width)),
                    AvailableSpace.MakeDefinite(container_height.TryClamp(min_size.Height, max_size.Height))
                ),
                SizingMode.InherentSize,
                default
            );
            var measured_size = layout_output.Size;
            var final_size = known_dimensions.Or(measured_size).TryClamp(min_size, max_size);

            var non_auto_margin = margin.Map(static a => a ?? 0);

            var free_space = new Size<float>(
                constants.ContainerSize.Width - final_size.Width - non_auto_margin.HorizontalAxisSum(),
                constants.ContainerSize.Height - final_size.Height - non_auto_margin.VerticalAxisSum()
            ).Max(default);

            // Expand auto margins to fill available space
            Rect<float> resolved_margin;
            {
                Size<float> auto_margin_size = default;
                {
                    var auto_margin_count = (margin.Left is null ? 1 : 0) + (margin.Right is null ? 1 : 0);
                    auto_margin_size.Width = auto_margin_count > 0 ? free_space.Width / auto_margin_count : 0;
                }
                {
                    var auto_margin_count = (margin.Top is null ? 1 : 0) + (margin.Bottom is null ? 1 : 0);
                    auto_margin_size.Height = auto_margin_count > 0 ? free_space.Height / auto_margin_count : 0;
                }

                resolved_margin = new(
                    margin.Top ?? auto_margin_size.Height,
                    margin.Right ?? auto_margin_size.Width,
                    margin.Bottom ?? auto_margin_size.Height,
                    margin.Left ?? auto_margin_size.Width
                );
            }

            // Determine flex-relative insets
            var (start_main, end_main) = constants.IsRow ? (left, right) : (top, bottom);
            var (start_cross, end_cross) = constants.IsRow ? (top, bottom) : (left, right);


            // Apply main-axis alignment
            float offset_main;
            if (start_main is { } start1)
            {
                offset_main = start1 + constants.Border.MainStart(constants.Direction) + resolved_margin.MainStart(constants.Direction);
            }
            else if (end_main is { } end1)
            {
                offset_main =
                    constants.ContainerSize.Main(constants.Direction)
                    - constants.Border.MainEnd(constants.Direction)
                    - constants.ScrollbarGutter.Main(constants.Direction)
                    - final_size.Main(constants.Direction)
                    - end1
                    - resolved_margin.MainEnd(constants.Direction);
            }
            else
            {
                // Stretch is an invalid value for justify_content in the flexbox algorithm, so we
                // treat it as if it wasn't set (and thus we default to FlexStart behaviour)
                offset_main = (constants.JustifyContent ?? JustifyContent.Start, constants.IsWrapReverse) switch
                {
                    (JustifyContent.SpaceBetween, _)
                        or (JustifyContent.Start, _)
                        or (JustifyContent.Stretch, false)
                        or (JustifyContent.FlexStart, false)
                        or (JustifyContent.FlexEnd, true) =>
                        constants.ContentBoxInset.MainStart(constants.Direction) + resolved_margin.MainStart(constants.Direction),
                    (JustifyContent.End, _)
                        or (JustifyContent.FlexEnd, false)
                        or (JustifyContent.FlexStart, true)
                        or (JustifyContent.Stretch, true) =>
                        constants.ContainerSize.Main(constants.Direction)
                        - constants.ContentBoxInset.MainEnd(constants.Direction)
                        - final_size.Main(constants.Direction)
                        - resolved_margin.MainEnd(constants.Direction),
                    (JustifyContent.SpaceEvenly, _) or (JustifyContent.SpaceAround, _) or (JustifyContent.Center, _) =>
                        (constants.ContainerSize.Main(constants.Direction)
                         + constants.ContentBoxInset.MainStart(constants.Direction)
                         + constants.ContentBoxInset.MainEnd(constants.Direction)
                         - final_size.Main(constants.Direction)
                         + resolved_margin.MainStart(constants.Direction)
                         - resolved_margin.MainEnd(constants.Direction))
                        / 2,
                    _ => throw new ArgumentOutOfRangeException(),
                };
            }

            // Apply cross-axis alignment
            // let free_cross_space = free_space.cross(constants.dir) - resolved_margin.cross_axis_sum(constants.dir);
            float offset_cross;
            if (start_cross is { } start2)
            {
                offset_cross = start2 + constants.Border.CrossStart(constants.Direction) + resolved_margin.CrossStart(constants.Direction);
            }
            else if (end_cross is { } end2)
            {
                offset_cross =
                    constants.ContainerSize.Cross(constants.Direction)
                    - constants.Border.CrossEnd(constants.Direction)
                    - constants.ScrollbarGutter.Cross(constants.Direction)
                    - final_size.Cross(constants.Direction)
                    - end2
                    - resolved_margin.CrossEnd(constants.Direction);
            }
            else
            {
                offset_cross = (align_self, constants.IsWrapReverse) switch
                {
                    // Stretch alignment does not apply to absolutely positioned items
                    // See "Example 3" at https://www.w3.org/TR/css-flexbox-1/#abspos-items
                    // Note: Stretch should be FlexStart not Start when we support both
                    (AlignSelf.Start, _)
                        or (AlignSelf.Baseline or AlignSelf.Stretch or AlignSelf.FlexStart, false)
                        or (AlignSelf.FlexEnd, true) =>
                        constants.ContentBoxInset.CrossStart(constants.Direction) + resolved_margin.CrossStart(constants.Direction),
                    (AlignSelf.End, _)
                        or (AlignSelf.Baseline or AlignSelf.Stretch or AlignSelf.FlexStart, true)
                        or (AlignSelf.FlexEnd, false) =>
                        constants.ContainerSize.Cross(constants.Direction)
                        - constants.ContentBoxInset.CrossEnd(constants.Direction)
                        - final_size.Cross(constants.Direction)
                        - resolved_margin.CrossEnd(constants.Direction),
                    (AlignSelf.Center, _) =>
                        (constants.ContainerSize.Cross(constants.Direction)
                         + constants.ContentBoxInset.CrossStart(constants.Direction)
                         - constants.ContentBoxInset.CrossEnd(constants.Direction)
                         - final_size.Cross(constants.Direction)
                         + resolved_margin.CrossStart(constants.Direction)
                         - resolved_margin.CrossEnd(constants.Direction))
                        / 2,
                    _ => throw new ArgumentOutOfRangeException(),
                };
            }

            Point<float> location = constants.IsRow
                ? new(offset_main, offset_cross)
                : new(offset_cross, offset_main);
            Size<float> scrollbar_size = new(
                overflow.Y == Overflow.Scroll ? scrollbar_width : 0,
                overflow.X == Overflow.Scroll ? scrollbar_width : 0
            );
            tree.SetUnroundedLayout(
                child,
                new()
                {
                    Order = order++,
                    Location = location,
                    Size = final_size,
                    ContentSize = layout_output.ContentSize,
                    ScrollbarSize = scrollbar_size,
                    Border = border,
                    Padding = padding,
                    Margin = resolved_margin,
                }
            );

            Size<float> size_content_size_contribution = new(
                overflow.X is Overflow.Visible
                    ? Math.Max(final_size.Width, layout_output.ContentSize.Width)
                    : final_size.Width,
                overflow.Y is Overflow.Visible
                    ? Math.Max(final_size.Height, layout_output.ContentSize.Height)
                    : final_size.Height
            );
            if (size_content_size_contribution.HasNonZeroArea())
            {
                Size<float> content_size_contribution = new(
                    location.X + size_content_size_contribution.Width,
                    location.Y + size_content_size_contribution.Height
                );
                content_size = content_size.Max(content_size_contribution);
            }
        }

        return content_size;
    }
}
