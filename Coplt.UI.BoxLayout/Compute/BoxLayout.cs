using Coplt.UI.BoxLayouts.Utilities;
using Coplt.UI.BoxLayouts;
using Coplt.UI.Styles;

namespace Coplt.UI.Layouts;

public static partial class BoxLayout
{
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
                new Layout
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
}
