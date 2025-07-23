using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.UI.BoxLayout.Utilities;
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
        where TNodeId : allows ref struct
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
        where TNodeId : allows ref struct
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

        // todo
        return default;
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
        where TNodeId : allows ref struct
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
    private struct FlexItem
    {
        public int NodeIndex;

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
    private static PooledList<FlexItem> GenerateAnonymousFlexItems<TTree, TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>(
        ref TTree tree, TNodeId node, in AlgoConstants constants
    )
        where TTree : ILayoutFlexboxContainer<TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>, allows ref struct
        where TNodeId : allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        where TFlexBoxContainerStyle : IFlexContainerStyle, allows ref struct
        where TFlexboxItemStyle : IFlexItemStyle, allows ref struct
    {
        using var list = new PooledList<FlexItem>(tree.ChildCount(node));
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
                NodeIndex = index,
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
        ref TTree tree, TNodeId node, in AlgoConstants constants, Size<AvailableSpace> available_space, Span<FlexItem> flex_items
    )
        where TTree : ILayoutFlexboxContainer<TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>, allows ref struct
        where TNodeId : allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        where TFlexBoxContainerStyle : IFlexContainerStyle, allows ref struct
        where TFlexboxItemStyle : IFlexItemStyle, allows ref struct
    {
        var dir = constants.Direction;

        foreach (ref var child in flex_items)
        {
            var child_id = tree.GetChildId(node, child.NodeIndex);
            var style = tree.GetFlexboxChildStyle(child_id);

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
                    child_id,
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
                    ref tree, child_id,
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<FlexItem> GetItems(Span<FlexItem> flex_items) => flex_items.Slice(ItemsStart, ItemsCount);
    }

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
    private static PooledList<FlexLine> CollectFlexLines(
        in AlgoConstants constants,
        Size<AvailableSpace> available_space,
        Span<FlexItem> flex_items
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
        Span<FlexItem> flex_items, Span<FlexLine> lines, ref AlgoConstants constants
    )
        where TTree : ILayoutFlexboxContainer<TNodeId, TChildIter, TCoreContainerStyle, TFlexBoxContainerStyle, TFlexboxItemStyle>, allows ref struct
        where TNodeId : allows ref struct
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
                        var items = line.GetItems(flex_items);
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
                                        tree.GetChildId(node, item.NodeIndex),
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
                        foreach (ref var item in line.GetItems(flex_items))
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

        static float LongestLineLength(Span<FlexItem> flex_items, Span<FlexLine> lines, in AlgoConstants constants)
        {
            var dir = constants.Direction;
            var longest_line_length = 0f;
            var first = true;
            foreach (ref readonly var line in lines)
            {
                var line_main_axis_gap = SumAxisGaps(constants.Gap.Main(dir), line.ItemsCount);
                var items = line.GetItems(flex_items);
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
}
