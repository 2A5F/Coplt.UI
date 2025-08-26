using Coplt.Mathematics;
using Coplt.UI.Collections;
using Coplt.UI.Styles;

namespace Coplt.UI.Rendering.Gpu.Graphics;

public record struct Batch
{
    public EmbedList<object> m_nodes;
    public uint Left;
    public uint Top;
    public uint Width;
    public uint Height;
}

public record struct BoxData
{
    public float4x4 TransformMatrix;
    public float4 LeftTopWidthHeight;
    public float4 BorderSize_TopRightBottomLeft;
    public float4 BackgroundColor;
    public float4 BackgroundImageTint;
    public float4 BorderColor_Top;
    public float4 BorderColor_Right;
    public float4 BorderColor_Bottom;
    public float4 BorderColor_Left;
    public float Opaque;
    public float Z;
    public BorderRadiusMode BorderRadiusMode;
    public BoxSizing BoxSizing;
    public uint BackgroundImage;
}
