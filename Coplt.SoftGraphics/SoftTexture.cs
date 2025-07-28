using System.Runtime.CompilerServices;
using Coplt.Mathematics;

namespace Coplt.SoftGraphics;

public sealed class SoftTexture
{
    public uint Width { get; }
    public uint Height { get; }
    public SoftTextureFormat Format { get; }

    internal readonly byte[]? m_blob0;
    internal readonly byte[]? m_blob1;
    internal readonly byte[]? m_blob2;
    internal readonly byte[]? m_blob3;

    public SoftTexture()
    {
        throw new NotImplementedException("todo");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Sample(
        ref readonly SoftLineContext ctx,
        in SoftSamplerState state,
        float4 u,
        float4 v,
        out float4 r,
        out float4 g,
        out float4 b,
        out float4 a
    ) => throw new NotImplementedException();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SampleDepth(
        ref readonly SoftLineContext ctx,
        in SoftSamplerState state,
        float4 u,
        float4 v,
        out float4 depth
    ) => throw new NotImplementedException();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Load(
        ref readonly SoftLineContext ctx,
        uint4 u,
        uint4 v,
        out float4 r,
        out float4 g,
        out float4 b,
        out float4 a
    ) => throw new NotImplementedException();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LoadDepthStencil(
        ref readonly SoftLineContext ctx,
        uint4 u,
        uint4 v,
        out float4 depth,
        out uint4 stencil
    ) => throw new NotImplementedException();
}

public enum SoftTextureFormat
{
    R8_G8_B8_A8_UNorm,
    R32_G32_B32_A32_Float,
    D24_UNorm_S8_UInt,
}
