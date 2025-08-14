using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Mathematics.Simt;

namespace Coplt.SoftGraphics;

public interface ISoftGraphicPipeline
{
    public ref readonly SoftGraphicPipelineState State { get; }
}

[StructLayout(LayoutKind.Auto)]
public record struct SoftGraphicPipelineState()
{
    public bool BlendEnable = false;
    public SoftBlend SrcBlend = SoftBlend.One;
    public SoftBlend DstBlend = SoftBlend.Zero;
    public SoftBlendOp BlendOp = SoftBlendOp.Add;
    public SoftBlend SrcAlphaBlend = SoftBlend.One;
    public SoftBlend DstAlphaBlend = SoftBlend.One;
    public SoftBlendOp AlphaBlendOp = SoftBlendOp.Max;
    public SoftColorWriteMask ColorBlendWriteMask = SoftColorWriteMask.RGBA;

    public SoftCullMode CullMode = SoftCullMode.Back; // todo
    public bool FrontCounterClockwise = true; // todo
    public bool DepthClipEnable = true; // todo

    // todo depth
    public bool DepthEnable = false;
    public bool DepthWrite = true;
    public SoftCmpFunc DepthFunc = SoftCmpFunc.LessEqual;
    public bool StencilEnable = false;
    public byte StencilReadMask = 255;
    public byte StencilWriteMask = 255;
    public SoftStencilState FrontStencil;
    public SoftStencilState BackStencil;
}

[StructLayout(LayoutKind.Auto)]
public record struct SoftStencilState()
{
    public SoftStencilOp Fail = SoftStencilOp.Keep;
    public SoftStencilOp DepthFail = SoftStencilOp.Keep;
    public SoftStencilOp Pass = SoftStencilOp.Keep;
    public SoftCmpFunc Func = SoftCmpFunc.Always;
}

public enum SoftBlend
{
    None,
    Zero,
    One,
    SrcColor,
    InvSrcColor,
    SrcAlpha,
    InvSrcAlpha,
    DstAlpha,
    InvDstAlpha,
    DstColor,
    InvDstColor,
}

public enum SoftBlendOp
{
    None,
    Add,
    Sub,
    RevSub,
    Min,
    Max,
}

[Flags]
public enum SoftColorWriteMask : byte
{
    None,
    R = 1 << 0,
    G = 1 << 1,
    B = 1 << 2,
    A = 1 << 3,
    RGB = R | G | B,
    RGBA = RGB | A,
}

public enum SoftCullMode : byte
{
    None,
    Front,
    Back,
}

public enum SoftCmpFunc
{
    None,
    Never,
    Less,
    Equal,
    LessEqual,
    Greater,
    NotEqual,
    GreaterEqual,
    Always,
}

public enum SoftStencilOp
{
    None,
    Keep,
    Zero,
    Replace,
    IncrSat,
    DecrSat,
    Invert,
    Incr,
    Decr,
}

public static partial class SoftGraphicsUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool NeedFetchDst(this SoftBlend blend, bool dst, float4_mt src) => dst
        ? blend switch
        {
            SoftBlend.None => false,
            SoftBlend.Zero => false,
            SoftBlend.One => true,
            SoftBlend.SrcColor => true,
            SoftBlend.InvSrcColor => true,
            SoftBlend.SrcAlpha => (src.a > 0f).lane_any(),
            SoftBlend.InvSrcAlpha => (src.a < 1f).lane_any(),
            SoftBlend.DstAlpha => true,
            SoftBlend.InvDstAlpha => true,
            SoftBlend.DstColor => true,
            SoftBlend.InvDstColor => true,
            _ => throw new ArgumentOutOfRangeException(nameof(blend), blend, null)
        }
        : blend switch
        {
            SoftBlend.None => false,
            SoftBlend.Zero => false,
            SoftBlend.One => false,
            SoftBlend.SrcColor => false,
            SoftBlend.InvSrcColor => false,
            SoftBlend.SrcAlpha => false,
            SoftBlend.InvSrcAlpha => false,
            SoftBlend.DstAlpha => true,
            SoftBlend.InvDstAlpha => true,
            SoftBlend.DstColor => true,
            SoftBlend.InvDstColor => true,
            _ => throw new ArgumentOutOfRangeException(nameof(blend), blend, null)
        };
}
