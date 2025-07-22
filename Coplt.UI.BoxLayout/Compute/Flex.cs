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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            var padding_border_axes_sums = child.Padding .Add(child.Border).SumAxes().MapNullable();
            
            // todo
        }
    }
}
