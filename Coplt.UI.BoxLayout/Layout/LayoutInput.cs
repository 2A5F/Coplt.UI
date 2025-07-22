using Coplt.UI.Styles;

namespace Coplt.UI.BoxLayouts;

public record struct LayoutInput(
    RunMode RunMode,
    SizingMode SizingMode,
    RequestedAxis Axis,
    Size<float?> KnownDimensions,
    Size<float?> ParentSize,
    Size<AvailableSpace> AvailableSpace,
    Line<bool> VerticalMarginsAreCollapsible
)
{
    public RunMode RunMode = RunMode;
    public SizingMode SizingMode = SizingMode;
    public RequestedAxis Axis = Axis;

    public Size<float?> KnownDimensions = KnownDimensions;
    public Size<float?> ParentSize = ParentSize;
    public Size<AvailableSpace> AvailableSpace = AvailableSpace;
    public Line<bool> VerticalMarginsAreCollapsible = VerticalMarginsAreCollapsible;
}
