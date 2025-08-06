using System.Runtime.CompilerServices;
using Coplt.Mathematics;
using Coplt.Mathematics.Simt;

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

    internal readonly uint2_mt16 m_size_cache;

    public SoftTexture(uint width, uint height, SoftTextureFormat format)
    {
        Width = width;
        Height = height;
        Format = format;
        m_size_cache = new(width, height);
        switch (format)
        {
            case SoftTextureFormat.R8_G8_B8_A8_UNorm:
                Impl_R8_G8_B8_A8_UNorm.Init(
                    this,
                    ref m_blob0,
                    ref m_blob1,
                    ref m_blob2,
                    ref m_blob3
                );
                break;
            case SoftTextureFormat.R32_G32_B32_A32_Float:
                Impl_R32_G32_B32_A32_Float.Init(
                    this,
                    ref m_blob0,
                    ref m_blob1,
                    ref m_blob2,
                    ref m_blob3
                );
                break;
            case SoftTextureFormat.D24_UNorm_S8_UInt:
                throw new NotImplementedException("todo");
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float4_mt16 Sample(
        ref readonly SoftLaneContext ctx, in SoftSamplerState state, float2_mt16 uv, out float4_mt16 color
    ) => throw new NotImplementedException();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float_mt16 SampleDepth(
        ref readonly SoftLaneContext ctx, in SoftSamplerState state, float2_mt16 uv
    ) => throw new NotImplementedException();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float4_mt16 Load(
        ref readonly SoftLaneContext ctx, uint2_mt16 uv
    ) => Format switch
    {
        SoftTextureFormat.R8_G8_B8_A8_UNorm => Impl_R8_G8_B8_A8_UNorm.Load(this, in ctx, uv),
        SoftTextureFormat.R32_G32_B32_A32_Float => Impl_R32_G32_B32_A32_Float.Load(this, in ctx, uv),
        SoftTextureFormat.D24_UNorm_S8_UInt => throw new NotSupportedException(),
        _ => throw new ArgumentOutOfRangeException()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (float_mt16 depth, uint_mt16 stencil) LoadDepthStencil(
        ref readonly SoftLaneContext ctx, uint2_mt16 uv
    ) => throw new NotImplementedException();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Store(
        ref readonly SoftLaneContext ctx, uint2_mt16 uv, float4_mt16 value
    )
    {
        switch (Format)
        {
            case SoftTextureFormat.R8_G8_B8_A8_UNorm: Impl_R8_G8_B8_A8_UNorm.Store(this, in ctx, uv, value); break;
            case SoftTextureFormat.R32_G32_B32_A32_Float: Impl_R32_G32_B32_A32_Float.Store(this, in ctx, uv, value); break;
            case SoftTextureFormat.D24_UNorm_S8_UInt: throw new NotSupportedException();
            default: throw new ArgumentOutOfRangeException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StoreDepthStencil(
        ref readonly SoftLaneContext ctx, uint2_mt16 uv, float_mt16 depth, uint_mt16 stencil
    ) => throw new NotImplementedException();

    static class Impl_R8_G8_B8_A8_UNorm
    {
        public static void Init(
            SoftTexture texture,
            ref byte[]? m_blob0,
            ref byte[]? m_blob1,
            ref byte[]? m_blob2,
            ref byte[]? m_blob3
        )
        {
            var max_size = math.max(texture.Width, texture.Height).up2pow2();
            var byte_size = max_size;
            var arr_size = byte_size * byte_size + 15;
            m_blob0 = new byte[arr_size];
            m_blob1 = new byte[arr_size];
            m_blob2 = new byte[arr_size];
            m_blob3 = new byte[arr_size];
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static float4_mt16 Load(
            SoftTexture texture,
            ref readonly SoftLaneContext ctx,
            uint2_mt16 uv
        )
        {
            var p = uv.min(texture.m_size_cache);
            var offset = Utils.EncodeZOrder(p).asi();
            var r = Utils.GatherUNorm(ref texture.m_blob0![0], offset, ctx.ActiveLanes);
            var g = Utils.GatherUNorm(ref texture.m_blob1![0], offset, ctx.ActiveLanes);
            var b = Utils.GatherUNorm(ref texture.m_blob2![0], offset, ctx.ActiveLanes);
            var a = Utils.GatherUNorm(ref texture.m_blob3![0], offset, ctx.ActiveLanes);
            return new(r, g, b, a);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void Store(
            SoftTexture texture,
            ref readonly SoftLaneContext ctx,
            uint2_mt16 uv,
            float4_mt16 value
        )
        {
            var p = uv.min(texture.m_size_cache);
            var offset = Utils.EncodeZOrder(p).asi();
            Utils.ScatterUNorm(value.x, ref texture.m_blob0![0], offset, ctx.ActiveLanes);
            Utils.ScatterUNorm(value.y, ref texture.m_blob1![0], offset, ctx.ActiveLanes);
            Utils.ScatterUNorm(value.z, ref texture.m_blob2![0], offset, ctx.ActiveLanes);
            Utils.ScatterUNorm(value.w, ref texture.m_blob3![0], offset, ctx.ActiveLanes);
        }
    }

    static class Impl_R32_G32_B32_A32_Float
    {
        public static void Init(
            SoftTexture texture,
            ref byte[]? m_blob0,
            ref byte[]? m_blob1,
            ref byte[]? m_blob2,
            ref byte[]? m_blob3
        )
        {
            var max_size = math.max(texture.Width, texture.Height).up2pow2();
            var byte_size = max_size * sizeof(float);
            var arr_size = byte_size * byte_size;
            m_blob0 = new byte[arr_size];
            m_blob1 = new byte[arr_size];
            m_blob2 = new byte[arr_size];
            m_blob3 = new byte[arr_size];
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static float4_mt16 Load(
            SoftTexture texture,
            ref readonly SoftLaneContext ctx,
            uint2_mt16 uv
        )
        {
            var p = uv.min(texture.m_size_cache);
            var offset = Utils.EncodeZOrder(p).asi();
            var r = Utils.Gather(ref Unsafe.As<byte, float>(ref texture.m_blob0![0]), offset, ctx.ActiveLanes);
            var g = Utils.Gather(ref Unsafe.As<byte, float>(ref texture.m_blob1![0]), offset, ctx.ActiveLanes);
            var b = Utils.Gather(ref Unsafe.As<byte, float>(ref texture.m_blob2![0]), offset, ctx.ActiveLanes);
            var a = Utils.Gather(ref Unsafe.As<byte, float>(ref texture.m_blob3![0]), offset, ctx.ActiveLanes);
            return new(r, g, b, a);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void Store(
            SoftTexture texture,
            ref readonly SoftLaneContext ctx,
            uint2_mt16 uv,
            float4_mt16 value
        )
        {
            var p = uv.min(texture.m_size_cache);
            var offset = Utils.EncodeZOrder(p).asi();
            Utils.Scatter(value.x, ref Unsafe.As<byte, float>(ref texture.m_blob0![0]), offset, ctx.ActiveLanes);
            Utils.Scatter(value.y, ref Unsafe.As<byte, float>(ref texture.m_blob1![0]), offset, ctx.ActiveLanes);
            Utils.Scatter(value.z, ref Unsafe.As<byte, float>(ref texture.m_blob2![0]), offset, ctx.ActiveLanes);
            Utils.Scatter(value.w, ref Unsafe.As<byte, float>(ref texture.m_blob3![0]), offset, ctx.ActiveLanes);
        }
    }
}

public enum SoftTextureFormat
{
    R8_G8_B8_A8_UNorm,
    R32_G32_B32_A32_Float,
    D24_UNorm_S8_UInt,
}
