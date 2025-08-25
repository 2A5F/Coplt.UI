using Coplt.Mathematics;
using Coplt.UI.Styles;

namespace Coplt.UI.Rendering.Gpu.Graphics;

public struct BatchData
{
    public uint Start;
    public uint Length;
}

public struct BoxData
{
    public float4x4 TransformMatrix;
    public float4 LeftTopWidthHeight;
    public float4 BorderSize_LeftTopRightBottom;
    public float4 BackgroundColor;
    public float4 BackgroundImageTint;
    public float4 BorderColor_Left;
    public float4 BorderColor_Top;
    public float4 BorderColor_Right;
    public float4 BorderColor_Bottom;
    public float Opaque;
    public float Z;
    public BorderRadiusMode BorderRadiusMode;
    public uint BackgroundImage;
}
