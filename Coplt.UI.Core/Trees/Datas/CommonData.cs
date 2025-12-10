using System.Diagnostics.CodeAnalysis;
using Coplt.Dropping;
using Coplt.Mathematics;
using Coplt.UI.Collections;
using Coplt.UI.Native;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public partial record struct CommonData()
{
    [Drop]
    public NativeArc<TextData> m_text_data; // optional , only exists when text layout

    internal LayoutData FinalLayout;
    internal LayoutData UnRoundedLayout;
    internal LayoutCache LayoutCache;
    
    internal uint NodeId = uint.MaxValue;
    internal ViewNode ParentValue = new(uint.MaxValue);

    /// <summary>
    /// layout compute sync this to LayoutVersion
    /// </summary>
    internal uint LastLayoutVersion;
    /// <summary>
    /// dirty inc this
    /// </summary>
    internal uint LayoutVersion;

    internal bool HasParent = false;
    
    public bool IsLayoutDirty => LastLayoutVersion != LayoutVersion;

    public ViewNode? Parent
    {
        get => HasParent ? ParentValue : null;
        set
        {
            if (value.HasValue)
            {
                ParentValue = value.Value;
                HasParent = true;
            }
            else
            {
                HasParent = false;
                ParentValue = new(uint.MaxValue);
            }
        }
    }

    [UnscopedRef]
    public LayoutView Layout => new(ref this);
}

public ref struct LayoutView(ref CommonData Data)
{
    public ref CommonData Data = ref Data;

    public uint Order => Data.FinalLayout.Order;

    public float2 Location => new(Data.FinalLayout.LocationX, Data.FinalLayout.LocationY);
    public float2 Size => new(Data.FinalLayout.Width, Data.FinalLayout.Height);
    public float2 Content => new(Data.FinalLayout.ContentWidth, Data.FinalLayout.ContentHeight);
    public float2 Scroll => new(Data.FinalLayout.ScrollXSize, Data.FinalLayout.ScrollYSize);
    public float4 Margin => new(Data.FinalLayout.MarginTopSize, Data.FinalLayout.MarginRightSize, Data.FinalLayout.MarginBottomSize,
        Data.FinalLayout.MarginLeftSize);
    public float4 Border => new(Data.FinalLayout.BorderTopSize, Data.FinalLayout.BorderRightSize, Data.FinalLayout.BorderBottomSize,
        Data.FinalLayout.BorderLeftSize);
    public float4 Padding => new(Data.FinalLayout.PaddingTopSize, Data.FinalLayout.PaddingRightSize, Data.FinalLayout.PaddingBottomSize,
        Data.FinalLayout.PaddingLeftSize);

    public float4 BoundingBox => new(Location, Location + Size);

    public override string ToString() =>
        $"<view x=\"{Data.FinalLayout.LocationX}\" y=\"{Data.FinalLayout.LocationY}\" z=\"{Order}\" width=\"{Data.FinalLayout.Width}\" height=\"{Data.FinalLayout.Height}\" content=\"{Data.FinalLayout.ContentWidth} {Data.FinalLayout.ContentHeight}\" margin=\"{Data.FinalLayout.MarginTopSize} {Data.FinalLayout.MarginRightSize} {Data.FinalLayout.MarginBottomSize} {Data.FinalLayout.MarginLeftSize}\" padding=\"{Data.FinalLayout.PaddingTopSize} {Data.FinalLayout.PaddingRightSize} {Data.FinalLayout.PaddingBottomSize} {Data.FinalLayout.PaddingLeftSize}\" border=\"{Data.FinalLayout.BorderTopSize} {Data.FinalLayout.BorderRightSize} {Data.FinalLayout.BorderBottomSize} {Data.FinalLayout.BorderLeftSize}\" />";
}

public static class CommonDataEx
{
    extension(in CommonData data)
    {
        public uint NodeId => data.NodeId;
    }
}
