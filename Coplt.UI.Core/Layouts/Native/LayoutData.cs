using Coplt.UI.Layouts;

namespace Coplt.UI.Native;

public record struct LayoutData
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

public record struct LayoutOutput
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

public record struct LayoutCollapsibleMarginSet
{
    public float Positive;
    public float Negative;
}

public struct LayoutCacheEntryLayoutOutput
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

public struct LayoutCacheEntrySize
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

public struct LayoutCache
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
    public bool HasFinalLayoutEntry;
    public bool HasMeasureEntries0;
    public bool HasMeasureEntries1;
    public bool HasMeasureEntries2;
    public bool HasMeasureEntries3;
    public bool HasMeasureEntries4;
    public bool HasMeasureEntries5;
    public bool HasMeasureEntries6;
    public bool HasMeasureEntries7;
    public bool HasMeasureEntries8;
    public bool IsEmpty;
}
