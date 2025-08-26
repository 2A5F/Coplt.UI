using System;
using Coplt.UI.BoxLayouts.Utilities;
using Coplt.UI.BoxLayouts;
using Coplt.UI.Styles;

namespace Coplt.UI.Layouts;

public static partial class BoxLayout
{
    #region ComputeRootLayout

    public static void ComputeRootLayout<TTree, TNodeId, TChildIter, TCoreContainerStyle>
        (ref TTree tree, TNodeId root, Size<AvailableSpace> available_space)
        where TTree : ILayoutPartialTree<TNodeId, TChildIter, TCoreContainerStyle>, allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
    {
        Size<float?> known_dimensions = new();
        var style = tree.GetCoreContainerStyle(root);

        #region Block

        {
            var parent_size = available_space.TryGet();

            if (style.IsBlock)
            {
                // Pull these out earlier to avoid borrowing issues
                var aspect_ratio = style.AspectRatio;
                var margin = style.Margin.ResolveOrZero(parent_size.Width, ref tree);
                var padding = style.Padding.ResolveOrZero(parent_size.Width, ref tree);
                var border = style.Border.ResolveOrZero(parent_size.Width, ref tree);
                var padding_border_size = padding.Add(border).SumAxes();
                var box_sizing_adjustment = style.BoxSizing == BoxSizing.ContentBox ? padding_border_size : default;

                var min_size = style.MinSize
                    .TryResolve(parent_size, ref tree)
                    .TryApplyAspectRatio(aspect_ratio)
                    .TryAdd(box_sizing_adjustment);
                var max_size = style.MaxSize
                    .TryResolve(parent_size, ref tree)
                    .TryApplyAspectRatio(aspect_ratio)
                    .TryAdd(box_sizing_adjustment);
                var clamped_style_size = style.Size
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

                // Block nodes automatically stretch fit their width to fit available space if available space is definite
                var available_space_based_size = new Size<float?>(
                    available_space.Width.TryGet().TrySub(margin.HorizontalAxisSum()),
                    null
                );

                var styled_based_known_dimensions = known_dimensions
                    .Or(min_max_definite_size)
                    .Or(clamped_style_size)
                    .Or(available_space_based_size)
                    .TryMax(padding_border_size);

                known_dimensions = styled_based_known_dimensions;
            }
        }

        #endregion

        #region Other

        {
            // Recursively compute node layout
            var output = tree.PerformChildLayout(
                root,
                known_dimensions,
                available_space.TryGet(),
                available_space,
                SizingMode.InherentSize,
                default
            );

            var padding = style.Padding.ResolveOrZero(available_space.Width.TryGet(), ref tree);
            var border = style.Border.ResolveOrZero(available_space.Width.TryGet(), ref tree);
            var margin = style.Margin.ResolveOrZero(available_space.Width.TryGet(), ref tree);
            var scrollbar_size = new Size<float>(
                style.Overflow.Y == Overflow.Scroll ? style.ScrollbarWidth : 0,
                style.Overflow.X == Overflow.Scroll ? style.ScrollbarWidth : 0
            );

            tree.SetUnroundedLayout(
                root,
                new UnroundedLayout
                {
                    Order = 0,
                    Location = default,
                    Size = output.Size,
                    ContentSize = output.ContentSize,
                    ScrollbarSize = scrollbar_size,
                    Border = border,
                    Padding = padding,
                    Margin = margin,
                }
            );
        }

        #endregion
    }

    #endregion

    #region RoundLayout

    public static void RoundLayout<TTree, TNodeId, TChildIter>(ref TTree tree, TNodeId node)
        where TTree : IRoundTree<TNodeId, TChildIter>, allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        => RoundLayout<TTree, TNodeId, TChildIter>(ref tree, node, 0, 0, 0, 0);

    private static void RoundLayout<TTree, TNodeId, TChildIter>
        (ref TTree tree, TNodeId node, float cumulative_x_, float cumulative_y_, float base_x, float base_y)
        where TTree : IRoundTree<TNodeId, TChildIter>, allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
    {
        var unrounded_layout = tree.GetUnroundedLayout(node);
        var layout = unrounded_layout.ToLayout();

        var cumulative_x = cumulative_x_ + unrounded_layout.Location.X;
        var cumulative_y = cumulative_y_ + unrounded_layout.Location.Y;

        layout.Location.X = MathF.Round(unrounded_layout.Location.X);
        layout.Location.Y = MathF.Round(unrounded_layout.Location.Y);
        layout.RootLocation.X = base_x + layout.Location.X;
        layout.RootLocation.Y = base_y + layout.Location.Y;
        layout.Size.Width = MathF.Round(cumulative_x + unrounded_layout.Size.Width) - MathF.Round(cumulative_x);
        layout.Size.Height = MathF.Round(cumulative_y + unrounded_layout.Size.Height) - MathF.Round(cumulative_y);
        layout.ScrollbarSize.Width = MathF.Round(unrounded_layout.ScrollbarSize.Width);
        layout.ScrollbarSize.Height = MathF.Round(unrounded_layout.ScrollbarSize.Height);
        layout.Border.Left = MathF.Round(cumulative_x + unrounded_layout.Border.Left) - MathF.Round(cumulative_x);
        layout.Border.Right = MathF.Round(cumulative_x + unrounded_layout.Size.Width)
                              - MathF.Round(cumulative_x + unrounded_layout.Size.Width - unrounded_layout.Border.Right);
        layout.Border.Top = MathF.Round(cumulative_y + unrounded_layout.Border.Top) - MathF.Round(cumulative_y);
        layout.Border.Bottom = MathF.Round(cumulative_y + unrounded_layout.Size.Height)
                               - MathF.Round(cumulative_y + unrounded_layout.Size.Height - unrounded_layout.Border.Bottom);
        layout.Padding.Left = MathF.Round(cumulative_x + unrounded_layout.Padding.Left) - MathF.Round(cumulative_x);
        layout.Padding.Right = MathF.Round(cumulative_x + unrounded_layout.Size.Width)
                               - MathF.Round(cumulative_x + unrounded_layout.Size.Width - unrounded_layout.Padding.Right);
        layout.Padding.Top = MathF.Round(cumulative_y + unrounded_layout.Padding.Top) - MathF.Round(cumulative_y);
        layout.Padding.Bottom = MathF.Round(cumulative_y + unrounded_layout.Size.Height)
                                - MathF.Round(cumulative_y + unrounded_layout.Size.Height - unrounded_layout.Padding.Bottom);

        layout.ContentSize.Width = MathF.Round(cumulative_x + unrounded_layout.ContentSize.Width) - MathF.Round(cumulative_x);
        layout.ContentSize.Height = MathF.Round(cumulative_y + unrounded_layout.ContentSize.Height) - MathF.Round(cumulative_y);

        tree.SetFinalLayout(node, layout);

        foreach (var child in tree.ChildIds(node).AsEnumerable<TChildIter, TNodeId>())
        {
            RoundLayout<TTree, TNodeId, TChildIter>(
                ref tree, child, cumulative_x, cumulative_y,
                layout.RootLocation.X, layout.RootLocation.Y
            );
        }
    }

    #endregion

    #region ComputeCachedLayout

    public static LayoutOutput ComputeCachedLayout<TTree, TNodeId>(
        ref TTree tree, TNodeId node, LayoutInput inputs, CachedLayoutComputeFunction<TTree, TNodeId> compute_uncached
    ) where TTree : ICacheTree<TNodeId>
    {
        var (run_mode, _, _, known_dimensions, _, available_space, _) = inputs;

        // First we check if we have a cached result for the given input
        var cache_entry = tree.CacheGet(node, known_dimensions, available_space, run_mode);
        if (cache_entry is { } cached_size_and_baselines) return cached_size_and_baselines;

        var computed_size_and_baselines = compute_uncached(ref tree, node, inputs);

        // Cache result
        tree.CacheStore(node, known_dimensions, available_space, run_mode, computed_size_and_baselines);

        return computed_size_and_baselines;
    }

    public static LayoutOutput ComputeCachedLayout<TTree, TNodeId, TArg>(
        ref TTree tree, TNodeId node, LayoutInput inputs, TArg arg, CachedLayoutComputeFunction<TTree, TNodeId, TArg> compute_uncached
    ) where TTree : ICacheTree<TNodeId>
    {
        var (run_mode, _, _, known_dimensions, _, available_space, _) = inputs;

        // First we check if we have a cached result for the given input
        var cache_entry = tree.CacheGet(node, known_dimensions, available_space, run_mode);
        if (cache_entry is { } cached_size_and_baselines) return cached_size_and_baselines;

        var computed_size_and_baselines = compute_uncached(ref tree, node, inputs, arg);

        // Cache result
        tree.CacheStore(node, known_dimensions, available_space, run_mode, computed_size_and_baselines);

        return computed_size_and_baselines;
    }

    #endregion

    #region ComputeHiddenLayout

    public static LayoutOutput ComputeHiddenLayout<TTree, TNodeId, TChildIter, TCoreContainerStyle>
        (ref TTree tree, TNodeId node)
        where TTree : ILayoutPartialTree<TNodeId, TChildIter, TCoreContainerStyle>, ICacheTree<TNodeId>, allows ref struct
        where TChildIter : IIterator<TNodeId>, allows ref struct
        where TCoreContainerStyle : ICoreStyle, allows ref struct
    {
        tree.CacheClear(node);
        tree.SetUnroundedLayout(node, UnroundedLayout.WithOrder(0));

        foreach (var child in tree.ChildIds(node).AsEnumerable<TChildIter, TNodeId>())
        {
            tree.ComputeChildLayout(child, LayoutInput.Hidden);
        }

        return LayoutOutput.Hidden;
    }

    #endregion
}

public delegate LayoutOutput CachedLayoutComputeFunction<TTree, TNodeId>(
    ref TTree tree, TNodeId node, LayoutInput inputs
) where TTree : ICacheTree<TNodeId>;

public delegate LayoutOutput CachedLayoutComputeFunction<TTree, TNodeId, TArg>(
    ref TTree tree, TNodeId node, LayoutInput inputs, TArg arg
) where TTree : ICacheTree<TNodeId>;
