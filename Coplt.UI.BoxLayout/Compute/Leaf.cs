using System;
using System.Diagnostics;
using Coplt.UI.BoxLayout.Utilities;
using Coplt.UI.BoxLayouts;
using Coplt.UI.Styles;

namespace Coplt.UI.Layouts;

public static partial class BoxLayout
{
    public static LayoutOutput ComputeLeafLayout<TCoreContainerStyle, TCalc>(
        LayoutInput inputs, TCoreContainerStyle style, ref TCalc calc,
        Func<Size<float?>, Size<AvailableSpace>, Size<float>> measure_function
    )
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        where TCalc : ICalc, allows ref struct
        => ComputeLeafLayout(
            inputs, style, ref calc, measure_function,
            static (measure_function, known_dimensions, available_space) =>
                measure_function(known_dimensions, available_space)
        );

    public static LayoutOutput ComputeLeafLayout<TCoreContainerStyle, TCalc, TArg>(
        LayoutInput inputs, TCoreContainerStyle style, ref TCalc calc, TArg arg,
        Func<TArg, Size<float?>, Size<AvailableSpace>, Size<float>> measure_function
    )
        where TCoreContainerStyle : ICoreStyle, allows ref struct
        where TCalc : ICalc, allows ref struct
        where TArg : allows ref struct
    {
        var (run_mode, sizing_mode, _, known_dimensions, parent_size, available_space, _) = inputs;

        var margin = style.Margin.ResolveOrZero(parent_size.Width, ref calc);
        var padding = style.Padding.ResolveOrZero(parent_size.Width, ref calc);
        var border = style.Border.ResolveOrZero(parent_size.Width, ref calc);

        var padding_border = padding.Add(border);
        var pb_sum = padding_border.SumAxes();
        var box_sizing_adjustment = style.BoxSizing == BoxSizing.ContentBox ? pb_sum : default;

        Size<float?> node_size, node_min_size, node_max_size;
        float? aspect_ratio;
        if (sizing_mode == SizingMode.ContentSize)
        {
            node_size = known_dimensions;
            node_min_size = default;
            node_max_size = default;
            aspect_ratio = null;
        }
        else if (sizing_mode == SizingMode.InherentSize)
        {
            aspect_ratio = style.AspectRatio;
            var style_size = style
                .Size
                .TryResolve(parent_size, ref calc)
                .TryApplyAspectRatio(aspect_ratio)
                .TryAdd(box_sizing_adjustment);
            node_min_size = style
                .MinSize
                .TryResolve(parent_size, ref calc)
                .TryApplyAspectRatio(aspect_ratio)
                .TryAdd(box_sizing_adjustment);
            node_max_size =
                style.MaxSize.TryResolve(parent_size, ref calc).TryAdd(box_sizing_adjustment);

            node_size = known_dimensions.Or(style_size);
        }
        else throw new ArgumentOutOfRangeException();

        var scrollbar_gutter = style.Overflow.Transpose()
            .Map(style.ScrollbarWidth, static (ScrollbarWidth, overflow) =>
                overflow is Overflow.Scroll ? ScrollbarWidth : 0.0f);
        var content_box_inset = padding_border;
        content_box_inset.Right += scrollbar_gutter.X;
        content_box_inset.Bottom += scrollbar_gutter.Y;

        var has_styles_preventing_being_collapsed_through =
            !style.IsBlock
            || style.Overflow.X.IsScrollContainer()
            || style.Overflow.Y.IsScrollContainer()
            || style.Position == Position.Absolute
            || padding.Top > 0
            || padding.Bottom > 0
            || border.Top > 0
            || border.Bottom > 0
            || node_size.Height is > 0
            || node_min_size.Height is > 0;

        if (run_mode == RunMode.ComputeSize && has_styles_preventing_being_collapsed_through)
        {
            if (node_size is { Width: { } width, Height: { } height })
            {
                var size = new Size<float>(width, height)
                    .TryClamp(node_min_size, node_max_size)
                    .Max(padding_border.SumAxes());
                return new()
                {
                    Size = size,
                    ContentSize = default,
                    FirstBaseLines = default,
                    TopMargin = default,
                    BottomMargin = default,
                    MarginsCanCollapseThrough = false,
                };
            }
        }

        // Compute available space
        available_space = new Size<AvailableSpace>(
            Width: (known_dimensions.Width.Map(AvailableSpace.From) ?? available_space.Width)
            .TrySub(margin.HorizontalAxisSum())
            .TrySet(known_dimensions.Width)
            .TrySet(node_size.Width)
            .MapDefiniteValue((node_min_size, node_max_size, content_box_inset), static (a, size)
                => size.TryClamp(a.node_min_size.Width, a.node_max_size.Width) - a.content_box_inset.HorizontalAxisSum()),
            Height: (known_dimensions.Height.Map(AvailableSpace.From) ?? available_space.Height)
            .TrySub(margin.VerticalAxisSum())
            .TrySet(known_dimensions.Height)
            .TrySet(node_size.Height)
            .MapDefiniteValue((node_min_size, node_max_size, content_box_inset), static (a, size)
                => size.TryClamp(a.node_min_size.Height, a.node_max_size.Height) - a.content_box_inset.VerticalAxisSum())
        );

        // Measure node
        {
            var measured_size = measure_function(
                arg, run_mode switch
                {
                    RunMode.PerformLayout => default,
                    RunMode.ComputeSize => known_dimensions,
                    RunMode.PerformHiddenLayout => throw new UnreachableException(),
                    _ => throw new ArgumentOutOfRangeException()
                }, available_space
            );
            var clamped_size = known_dimensions
                .Or(node_size)
                .Or(measured_size.Add(content_box_inset.SumAxes()))
                .TryClamp(node_min_size, node_max_size);
            var size = new Size<float>(
                clamped_size.Width,
                Math.Max(clamped_size.Height,
                    aspect_ratio.Map(clamped_size, static (clamped_size, ratio) => clamped_size.Width / ratio) ?? 0)
            );
            size = size.Max(padding_border.SumAxes());

            return new LayoutOutput
            {
                Size = size,
                ContentSize = measured_size.Add(padding.SumAxes()),
                FirstBaseLines = default,
                TopMargin = default,
                BottomMargin = default,
                MarginsCanCollapseThrough =
                    !has_styles_preventing_being_collapsed_through
                    && size.Height == 0 && measured_size.Height == 0
            };
        }
    }
}
