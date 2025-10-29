using Coplt.Com;
using Coplt.Dropping;
using Coplt.Mathematics;
using Coplt.UI.Collections;
using Coplt.UI.Native;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public partial record struct ContainerLayoutData
{
    [Drop]
    [ComType<Ptr<ITextLayout>>]
    internal Rc<ITextLayout> TextLayoutObject;
    internal LayoutData FinalLayout;
    internal LayoutData Layout;
    internal LayoutCache LayoutCache;

    public uint Order => FinalLayout.Order;

    public float2 Location => new(FinalLayout.LocationX, FinalLayout.LocationY);
    public float2 Size => new(FinalLayout.Width, FinalLayout.Height);
    public float2 Content => new(FinalLayout.ContentWidth, FinalLayout.ContentHeight);
    public float2 Scroll => new(FinalLayout.ScrollXSize, FinalLayout.ScrollYSize);
    public float4 Margin => new(FinalLayout.MarginTopSize, FinalLayout.MarginRightSize, FinalLayout.MarginBottomSize, FinalLayout.MarginLeftSize);
    public float4 Border => new(FinalLayout.BorderTopSize, FinalLayout.BorderRightSize, FinalLayout.BorderBottomSize, FinalLayout.BorderLeftSize);
    public float4 Padding => new(FinalLayout.PaddingTopSize, FinalLayout.PaddingRightSize, FinalLayout.PaddingBottomSize, FinalLayout.PaddingLeftSize);

    public override string ToString() =>
        $"<view x=\"{FinalLayout.LocationX}\" y=\"{FinalLayout.LocationY}\" z=\"{Order}\" width=\"{FinalLayout.Width}\" height=\"{FinalLayout.Height}\" content=\"{FinalLayout.ContentWidth} {FinalLayout.ContentHeight}\" margin=\"{FinalLayout.MarginTopSize} {FinalLayout.MarginRightSize} {FinalLayout.MarginBottomSize} {FinalLayout.MarginLeftSize}\" padding=\"{FinalLayout.PaddingTopSize} {FinalLayout.PaddingRightSize} {FinalLayout.PaddingBottomSize} {FinalLayout.PaddingLeftSize}\" border=\"{FinalLayout.BorderTopSize} {FinalLayout.BorderRightSize} {FinalLayout.BorderBottomSize} {FinalLayout.BorderLeftSize}\" />";
}
