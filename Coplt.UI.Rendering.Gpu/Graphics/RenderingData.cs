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

public enum RenderFlags : uint
{
    None = 0,
    ContentBox = 1 << 0,
}

public record struct BoxData
{
    public float4x4 TransformMatrix;
    public float4 LeftTopWidthHeight;
    public float4 BorderSize_TopRightBottomLeft;
    /// <summary>
    /// br, tr, bl, tl
    /// </summary>
    public float4 BorderRound;
    public float4 BackgroundColor;
    public float4 BackgroundImageTint;
    public float4 BorderColor_Top;
    public float4 BorderColor_Right;
    public float4 BorderColor_Bottom;
    public float4 BorderColor_Left;
    public float Opaque;
    public float Z;
    public RenderFlags Flags;
    public SamplerType BackgroundImageSampler;
    public BorderRadiusMode BorderRadiusMode;
    public uint BackgroundImage;
}
