using System;
using System.Runtime.CompilerServices;
using Coplt.UI.Styles;

namespace Coplt.UI.Layouts;

public static partial class BoxLayout
{
    /// Determine how much width/height a given node contributes to it's parent's content size
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Size<float> ComputeContentSizeContribution(
        Point<float> location, Size<float> size, Size<float> content_size, Point<Overflow> overflow
    )
    {
        Size<float> size_content_size_contribution = new(
            overflow.X is Overflow.Visible ? Math.Max(size.Width, content_size.Width) : size.Width,
            overflow.Y is Overflow.Visible ? Math.Max(size.Height, content_size.Height) : size.Height
        );
        if (size_content_size_contribution.Width > 0 && size_content_size_contribution.Height > 0)
        {
            return new(
                location.X + size_content_size_contribution.Width,
                location.Y + size_content_size_contribution.Height
            );
        }
        else return default;
    }
}
