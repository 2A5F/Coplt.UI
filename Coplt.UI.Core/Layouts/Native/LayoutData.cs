using Coplt.UI.Layouts;

namespace Coplt.UI.Native;

internal record struct LayoutData
{
    public uint Order;
    public float LocationX;
    public float LocationY;
    public float Width;
    public float Height;
    public float ContentWidth;
    public float ContentHeight;
    public float ScrollXSize;
    public float ScrollYSize;
    public float BorderTopSize;
    public float BorderRightSize;
    public float BorderBottomSize;
    public float BorderLeftSize;
    public float PaddingTopSize;
    public float PaddingRightSize;
    public float PaddingBottomSize;
    public float PaddingLeftSize;
    public float MarginTopSize;
    public float MarginRightSize;
    public float MarginBottomSize;
    public float MarginLeftSize;
}

internal record struct LayoutOutput
{
    public float Width;
    public float Height;
    public float ContentWidth;
    public float ContentHeight;
    public float FirstBaselinesX;
    public float FirstBaselinesY;
    public LayoutCollapsibleMarginSet TopMargin;
    public LayoutCollapsibleMarginSet BottomMargin;
    public bool HasFirstBaselinesX;
    public bool HasFirstBaselinesY;
    public bool MarginsCanCollapseThrough;
}

internal record struct LayoutCollapsibleMarginSet
{
    public float Positive;
    public float Negative;
}

internal struct LayoutCacheEntryLayoutOutput
{
    public float KnownDimensionsWidthValue;
    public float KnownDimensionsHeightValue;
    public float AvailableSpaceWidthValue;
    public float AvailableSpaceHeightValue;
    public bool HasKnownDimensionsWidth;
    public bool HasKnownDimensionsHeight;
    public AvailableSpaceType AvailableSpaceWidth;
    public AvailableSpaceType AvailableSpaceHeight;
    public LayoutOutput Content;
}

internal struct LayoutCacheEntrySize
{
    public float KnownDimensionsWidthValue;
    public float KnownDimensionsHeightValue;
    public float AvailableSpaceWidthValue;
    public float AvailableSpaceHeightValue;
    public bool HasKnownDimensionsWidth;
    public bool HasKnownDimensionsHeight;
    public AvailableSpaceType AvailableSpaceWidth;
    public AvailableSpaceType AvailableSpaceHeight;
    public float ContentWidth;
    public float ContentHeight;
}

internal struct LayoutCache
{
    public LayoutCacheEntryLayoutOutput FinalLayoutEntry;
    public LayoutCacheEntrySize MeasureEntries0;
    public LayoutCacheEntrySize MeasureEntries1;
    public LayoutCacheEntrySize MeasureEntries2;
    public LayoutCacheEntrySize MeasureEntries3;
    public LayoutCacheEntrySize MeasureEntries4;
    public LayoutCacheEntrySize MeasureEntries5;
    public LayoutCacheEntrySize MeasureEntries6;
    public LayoutCacheEntrySize MeasureEntries7;
    public LayoutCacheEntrySize MeasureEntries8;
    public LayoutCacheFlags Flags;
}

[Flags]
public enum LayoutCacheFlags : ushort
{
    Empty = 0,
    Final = 1 << 0,
    Measure0 = 1 << 1,
    Measure1 = 1 << 2,
    Measure2 = 1 << 3,
    Measure3 = 1 << 4,
    Measure4 = 1 << 5,
    Measure5 = 1 << 6,
    Measure6 = 1 << 7,
    Measure7 = 1 << 8,
    Measure8 = 1 << 9,
}
