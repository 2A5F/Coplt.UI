using System.Runtime.CompilerServices;
using Coplt.UI.Styles;

namespace Coplt.UI.BoxLayouts;

public record struct LayoutOutput
{
    public Size<float> Size;
    public Size<float> ContentSize;
    public Point<float?> FirstBaseLines;
    public CollapsibleMarginSet TopMargin;
    public CollapsibleMarginSet BottomMargin;
    public bool MarginsCanCollapseThrough;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LayoutOutput FromOuterSize(float width, float height)
        => FromOuterSize(new(width, height));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LayoutOutput FromOuterSize(Size<float> size)
        => FromSizes(size, default);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LayoutOutput FromSizes(Size<float> size, Size<float> content_size)
        => FromSizesAndBaselines(size, content_size, default);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LayoutOutput FromSizesAndBaselines(Size<float> size, Size<float> content_size, Point<float?> first_baselines)
        => new()
        {
            Size = size,
            ContentSize = content_size,
            FirstBaseLines = first_baselines,
            TopMargin = default,
            BottomMargin = default,
            MarginsCanCollapseThrough = false,
        };
}
