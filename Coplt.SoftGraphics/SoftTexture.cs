using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Coplt.Mathematics;
using Coplt.Mathematics.Simt;

namespace Coplt.SoftGraphics;

public abstract class SoftTexture
{
    #region Metas

    public uint Width { get; }
    public uint Height { get; }
    public SoftTextureFormat Format { get; }

    public bool HasColor { get; protected set; }
    public bool HasDepth { get; protected set; }
    public bool HasStencil { get; protected set; }

    public bool HasDepthStencil => HasDepth || HasStencil;

    #endregion

    #region Fields

    protected readonly uint2_mt m_size_cache;

    #endregion

    #region Ctor

    protected SoftTexture(uint width, uint height, SoftTextureFormat format)
    {
        Width = width;
        Height = height;
        Format = format;
        m_size_cache = new(width, height);
    }

    #endregion

    #region Create

    public static SoftTexture Create(SoftTextureFormat format, uint width, uint height, bool zeroed = true) => format switch
    {
        SoftTextureFormat.R8_G8_B8_A8_UNorm => new Impl_R8_G8_B8_A8_UNorm(width, height, zeroed),
        SoftTextureFormat.R32_G32_B32_A32_Float => new Impl_R32_G32_B32_A32_Float(width, height, zeroed),
        SoftTextureFormat.D24_UNorm_S8_UInt => throw new NotImplementedException("todo"),
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
    };

    #endregion

    #region Sample

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract float4_mt Sample(
        ref readonly SoftLaneContext ctx, in SoftSamplerState state, float2_mt uv, out float4_mt color
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract float_mt SampleDepth(
        ref readonly SoftLaneContext ctx, in SoftSamplerState state, float2_mt uv
    );

    #endregion

    #region Load

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract float4_mt Load(
        ref readonly SoftLaneContext ctx, uint2_mt uv
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract (float_mt depth, uint_mt stencil) LoadDepthStencil(
        ref readonly SoftLaneContext ctx, uint2_mt uv
    );

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public abstract (float r, float g, float b, float a) Load(uint u, uint v);

    #endregion

    #region Store

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void Store(
        ref readonly SoftLaneContext ctx, uint2_mt uv, float4_mt value
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void StoreDepthStencil(
        ref readonly SoftLaneContext ctx, uint2_mt uv, float_mt depth, uint_mt stencil
    );

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public abstract void Store(uint u, uint v, float r, float g, float b, float a);

    #endregion

    #region QuadQuad

    /// <summary>
    /// Store 4x4 (16) pixels in z-curve order, these pixels will be contiguous in memory.<br/>
    /// x y must be a multiple of 4, otherwise it will be undefined behavior.
    /// <code>
    /// c2 c3   d2 d3    |    10 11   14 15
    /// c0 c1   d0 d1    |    08 09   12 13
    ///                  |                 
    /// a2 a3   b2 b3    |    02 03   06 07
    /// a0 a1   b0 b1    |    00 01   04 05
    /// </code>
    /// </summary>
    public abstract void QuadQuadStore(uint x, uint y, float4_mt color);

    public abstract void QuadQuadStore(uint z_index, float4_mt color);

    /// <summary>
    /// Load 4x4 (16) pixels in z-curve order, these pixels will be contiguous in memory.<br/>
    /// x y must be a multiple of 4, otherwise it will be undefined behavior.
    /// <code>
    /// c2 c3   d2 d3    |    10 11   14 15
    /// c0 c1   d0 d1    |    08 09   12 13
    ///                  |                 
    /// a2 a3   b2 b3    |    02 03   06 07
    /// a0 a1   b0 b1    |    00 01   04 05
    /// </code>
    /// </summary>
    public abstract float4_mt QuadQuadLoad(uint x, uint y);

    public abstract float4_mt QuadQuadLoad(uint z_index);

    #endregion

    #region Read

    public abstract void ReadRowUNorm8(int row, Span<byte> target, AJobScheduler? scheduler = null);

    #endregion

    #region Impl

    internal sealed class Impl_R8_G8_B8_A8_UNorm : SoftTexture
    {
        #region Blobs

        internal readonly byte[] m_blob0;
        internal readonly byte[] m_blob1;
        internal readonly byte[] m_blob2;
        internal readonly byte[] m_blob3;

        #endregion

        #region Ctor

        public Impl_R8_G8_B8_A8_UNorm(uint width, uint height, bool zeroed) : base(width, height, SoftTextureFormat.R8_G8_B8_A8_UNorm)
        {
            HasColor = true;
            var max_size = math.max(width, height).up2pow2();
            var arr_size = max_size * max_size + 63;
            if (zeroed)
            {
                m_blob0 = new byte[arr_size];
                m_blob1 = new byte[arr_size];
                m_blob2 = new byte[arr_size];
                m_blob3 = new byte[arr_size];
            }
            else
            {
                m_blob0 = GC.AllocateUninitializedArray<byte>((int)arr_size);
                m_blob1 = GC.AllocateUninitializedArray<byte>((int)arr_size);
                m_blob2 = GC.AllocateUninitializedArray<byte>((int)arr_size);
                m_blob3 = GC.AllocateUninitializedArray<byte>((int)arr_size);
            }
        }

        #endregion

        #region NotSupported

        public override float_mt SampleDepth(ref readonly SoftLaneContext ctx, in SoftSamplerState state, float2_mt uv)
            => throw new NotSupportedException();
        public override (float_mt depth, uint_mt stencil) LoadDepthStencil(ref readonly SoftLaneContext ctx, uint2_mt uv)
            => throw new NotSupportedException();
        public override void StoreDepthStencil(ref readonly SoftLaneContext ctx, uint2_mt uv, float_mt depth, uint_mt stencil)
            => throw new NotSupportedException();

        #endregion

        #region Sample

        public override float4_mt Sample(ref readonly SoftLaneContext ctx, in SoftSamplerState state, float2_mt uv, out float4_mt color)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Load

        public override float4_mt Load(ref readonly SoftLaneContext ctx, uint2_mt uv)
        {
            var p = uv.min(m_size_cache);
            var offset = SoftGraphicsUtils.EncodeZOrder(p).asi();
            var r = SoftGraphicsUtils.GatherUNorm(ref m_blob0[0], offset, ctx.ActiveLanes);
            var g = SoftGraphicsUtils.GatherUNorm(ref m_blob1[0], offset, ctx.ActiveLanes);
            var b = SoftGraphicsUtils.GatherUNorm(ref m_blob2[0], offset, ctx.ActiveLanes);
            var a = SoftGraphicsUtils.GatherUNorm(ref m_blob3[0], offset, ctx.ActiveLanes);
            return new(r, g, b, a);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override (float r, float g, float b, float a) Load(uint u, uint v)
        {
            var x = math.min(u, Width);
            var y = math.min(v, Height);
            var offset = SoftGraphicsUtils.EncodeZOrder(x, y);
            var r = Unsafe.Add(ref m_blob0[0], offset) * (1f / 255f);
            var g = Unsafe.Add(ref m_blob1[0], offset) * (1f / 255f);
            var b = Unsafe.Add(ref m_blob2[0], offset) * (1f / 255f);
            var a = Unsafe.Add(ref m_blob3[0], offset) * (1f / 255f);
            return (r, g, b, a);
        }

        #endregion

        #region Store

        public override void Store(ref readonly SoftLaneContext ctx, uint2_mt uv, float4_mt value)
        {
            var p = uv.min(m_size_cache);
            var offset = SoftGraphicsUtils.EncodeZOrder(p).asi();
            SoftGraphicsUtils.ScatterUNorm(value.x, ref m_blob0[0], offset, ctx.ActiveLanes);
            SoftGraphicsUtils.ScatterUNorm(value.y, ref m_blob1[0], offset, ctx.ActiveLanes);
            SoftGraphicsUtils.ScatterUNorm(value.z, ref m_blob2[0], offset, ctx.ActiveLanes);
            SoftGraphicsUtils.ScatterUNorm(value.w, ref m_blob3[0], offset, ctx.ActiveLanes);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override void Store(uint u, uint v, float r, float g, float b, float a)
        {
            var x = math.min(u, Width);
            var y = math.min(v, Height);
            var offset = SoftGraphicsUtils.EncodeZOrder(x, y);
            Unsafe.Add(ref m_blob0[0], offset) = (byte)Math.Round(Math.Clamp(r * 255f, 0f, 255f));
            Unsafe.Add(ref m_blob1[0], offset) = (byte)Math.Round(Math.Clamp(g * 255f, 0f, 255f));
            Unsafe.Add(ref m_blob2[0], offset) = (byte)Math.Round(Math.Clamp(b * 255f, 0f, 255f));
            Unsafe.Add(ref m_blob3[0], offset) = (byte)Math.Round(Math.Clamp(a * 255f, 0f, 255f));
        }

        #endregion

        #region Read

        public override unsafe void ReadRowUNorm8(int row, Span<byte> target, AJobScheduler? scheduler = null)
        {
            scheduler ??= SyncJobScheduler.Instance;

            var w = (Width + 15) / 16;
            uint_mt y = (uint)row;
            fixed (byte* p_target = target)
            {
                scheduler.Dispatch(w, 1, (self: this, (IntPtr)p_target, len: target.Length, y), static (ctx, x, _) =>
                {
                    var (self, p_target, target_len, y) = ctx;
                    var target = new Span<byte>((byte*)p_target, target_len);

                    var index = new uint_mt(x * 16) + SoftGraphicsUtils.IncMt;
                    var active = index < self.m_size_cache.x;
                    var offset = SoftGraphicsUtils.EncodeZOrder(new(index, y)).asi();

                    var r = SoftGraphicsUtils.GatherByte(ref self.m_blob0[0], offset.vector, active.vector);
                    var g = SoftGraphicsUtils.GatherByte(ref self.m_blob1[0], offset.vector, active.vector);
                    var b = SoftGraphicsUtils.GatherByte(ref self.m_blob2[0], offset.vector, active.vector);
                    var a = SoftGraphicsUtils.GatherByte(ref self.m_blob3[0], offset.vector, active.vector);

                    var rgba = Vector512.Create(
                        Vector256.Create(r, g),
                        Vector256.Create(b, a)
                    );
                    rgba = Vector512.Shuffle(
                        rgba,
                        Vector512.Create(
                            (byte) /**/ 00, 16, 32, 48, /**/ 01, 17, 33, 49,
                            /*       */ 02, 18, 34, 50, /**/ 03, 19, 35, 51,
                            /*       */ 04, 20, 36, 52, /**/ 05, 21, 37, 53,
                            /*       */ 06, 22, 38, 54, /**/ 07, 23, 39, 55,
                            /*                                            */
                            /*       */ 08, 24, 40, 56, /**/ 09, 25, 41, 57,
                            /*       */ 10, 26, 42, 58, /**/ 11, 27, 43, 59,
                            /*       */ 12, 28, 44, 60, /**/ 13, 29, 45, 61,
                            /*       */ 14, 30, 46, 62, /**/ 15, 31, 47, 63
                        )
                    );

                    if (x + 15 < self.Width)
                    {
                        rgba.StoreUnsafe(ref target[0], x * 64);
                    }
                    else
                    {
                        var len = self.Width - (x + 15);
                        Span<byte> tmp = stackalloc byte[64];
                        rgba.StoreUnsafe(ref tmp[0]);
                        tmp.CopyTo(target.Slice((int)x * 64, (int)len * 4));
                    }
                });
            }
        }

        #endregion

        #region QuadQuad

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override void QuadQuadStore(uint x, uint y, float4_mt color)
        {
            var index = SoftGraphicsUtils.EncodeZOrder(x, y) * 16;
            QuadQuadStore(index, color);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override void QuadQuadStore(uint z_index, float4_mt color)
        {
            var r512 = ((uint_mt)math_mt.clamp(color.r * 255f, 0f, 255f).round()).vector;
            var g512 = ((uint_mt)math_mt.clamp(color.g * 255f, 0f, 255f).round()).vector;
            var b512 = ((uint_mt)math_mt.clamp(color.b * 255f, 0f, 255f).round()).vector;
            var a512 = ((uint_mt)math_mt.clamp(color.a * 255f, 0f, 255f).round()).vector;

            var r256 = Vector256.Narrow(r512.GetLower(), r512.GetUpper());
            var g256 = Vector256.Narrow(g512.GetLower(), g512.GetUpper());
            var b256 = Vector256.Narrow(b512.GetLower(), b512.GetUpper());
            var a256 = Vector256.Narrow(a512.GetLower(), a512.GetUpper());

            var r128 = Vector128.Narrow(r256.GetLower(), r256.GetUpper());
            var g128 = Vector128.Narrow(g256.GetLower(), g256.GetUpper());
            var b128 = Vector128.Narrow(b256.GetLower(), b256.GetUpper());
            var a128 = Vector128.Narrow(a256.GetLower(), a256.GetUpper());

            r128.StoreUnsafe(ref Unsafe.Add(ref m_blob0[0], z_index));
            g128.StoreUnsafe(ref Unsafe.Add(ref m_blob1[0], z_index));
            b128.StoreUnsafe(ref Unsafe.Add(ref m_blob2[0], z_index));
            a128.StoreUnsafe(ref Unsafe.Add(ref m_blob3[0], z_index));
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override float4_mt QuadQuadLoad(uint x, uint y)
        {
            var index = SoftGraphicsUtils.EncodeZOrder(x, y) * 16;
            return QuadQuadLoad(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override float4_mt QuadQuadLoad(uint z_index)
        {
            var (r128a, r128b) = Vector128.Widen(Vector128.LoadUnsafe(ref Unsafe.Add(ref m_blob0[0], z_index)));
            var (g128a, g128b) = Vector128.Widen(Vector128.LoadUnsafe(ref Unsafe.Add(ref m_blob1[0], z_index)));
            var (b128a, b128b) = Vector128.Widen(Vector128.LoadUnsafe(ref Unsafe.Add(ref m_blob2[0], z_index)));
            var (a128a, a128b) = Vector128.Widen(Vector128.LoadUnsafe(ref Unsafe.Add(ref m_blob3[0], z_index)));

            var (r256a, r256b) = Vector256.Widen(Vector256.Create(r128a, r128b));
            var (g256a, g256b) = Vector256.Widen(Vector256.Create(g128a, g128b));
            var (b256a, b256b) = Vector256.Widen(Vector256.Create(b128a, b128b));
            var (a256a, a256b) = Vector256.Widen(Vector256.Create(a128a, a128b));

            var r = Vector512.ConvertToSingle(Vector512.Create(r256a, r256b)) * (1f / 255f);
            var g = Vector512.ConvertToSingle(Vector512.Create(g256a, g256b)) * (1f / 255f);
            var b = Vector512.ConvertToSingle(Vector512.Create(b256a, b256b)) * (1f / 255f);
            var a = Vector512.ConvertToSingle(Vector512.Create(a256a, a256b)) * (1f / 255f);

            return new(new(r), new(g), new(b), new(a));
        }

        #endregion
    }

    internal sealed class Impl_R32_G32_B32_A32_Float : SoftTexture
    {
        #region Blobs

        internal readonly float[] m_blob0;
        internal readonly float[] m_blob1;
        internal readonly float[] m_blob2;
        internal readonly float[] m_blob3;

        #endregion

        #region Ctor

        public Impl_R32_G32_B32_A32_Float(uint width, uint height, bool zeroed) : base(width, height, SoftTextureFormat.R32_G32_B32_A32_Float)
        {
            HasColor = true;
            var max_size = math.max(width, height).up2pow2();
            var arr_size = max_size * max_size + 15;
            if (zeroed)
            {
                m_blob0 = new float[arr_size];
                m_blob1 = new float[arr_size];
                m_blob2 = new float[arr_size];
                m_blob3 = new float[arr_size];
            }
            else
            {
                m_blob0 = GC.AllocateUninitializedArray<float>((int)arr_size);
                m_blob1 = GC.AllocateUninitializedArray<float>((int)arr_size);
                m_blob2 = GC.AllocateUninitializedArray<float>((int)arr_size);
                m_blob3 = GC.AllocateUninitializedArray<float>((int)arr_size);
            }
        }

        #endregion

        #region NotSupported

        public override float_mt SampleDepth(ref readonly SoftLaneContext ctx, in SoftSamplerState state, float2_mt uv)
            => throw new NotSupportedException();
        public override (float_mt depth, uint_mt stencil) LoadDepthStencil(ref readonly SoftLaneContext ctx, uint2_mt uv)
            => throw new NotSupportedException();
        public override void StoreDepthStencil(ref readonly SoftLaneContext ctx, uint2_mt uv, float_mt depth, uint_mt stencil)
            => throw new NotSupportedException();

        #endregion

        #region Sample

        public override float4_mt Sample(
            ref readonly SoftLaneContext ctx, in SoftSamplerState state, float2_mt uv, out float4_mt color
        )
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Load

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override float4_mt Load(
            ref readonly SoftLaneContext ctx,
            uint2_mt uv
        )
        {
            var p = uv.min(m_size_cache);
            var offset = SoftGraphicsUtils.EncodeZOrder(p).asi();
            var r = SoftGraphicsUtils.Gather(ref m_blob0[0], offset, ctx.ActiveLanes);
            var g = SoftGraphicsUtils.Gather(ref m_blob1[0], offset, ctx.ActiveLanes);
            var b = SoftGraphicsUtils.Gather(ref m_blob2[0], offset, ctx.ActiveLanes);
            var a = SoftGraphicsUtils.Gather(ref m_blob3[0], offset, ctx.ActiveLanes);
            return new(r, g, b, a);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override (float r, float g, float b, float a) Load(uint u, uint v)
        {
            var x = math.min(u, Width);
            var y = math.min(v, Height);
            var offset = SoftGraphicsUtils.EncodeZOrder(x, y);
            var r = Unsafe.Add(ref m_blob0[0], offset);
            var g = Unsafe.Add(ref m_blob1[0], offset);
            var b = Unsafe.Add(ref m_blob2[0], offset);
            var a = Unsafe.Add(ref m_blob3[0], offset);
            return (r, g, b, a);
        }

        #endregion

        #region Store

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override void Store(
            ref readonly SoftLaneContext ctx,
            uint2_mt uv,
            float4_mt value
        )
        {
            var p = uv.min(m_size_cache);
            var offset = SoftGraphicsUtils.EncodeZOrder(p).asi();
            SoftGraphicsUtils.Scatter(value.x, ref m_blob0[0], offset, ctx.ActiveLanes);
            SoftGraphicsUtils.Scatter(value.y, ref m_blob1[0], offset, ctx.ActiveLanes);
            SoftGraphicsUtils.Scatter(value.z, ref m_blob2[0], offset, ctx.ActiveLanes);
            SoftGraphicsUtils.Scatter(value.w, ref m_blob3[0], offset, ctx.ActiveLanes);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override void Store(uint u, uint v, float r, float g, float b, float a)
        {
            var x = math.min(u, Width);
            var y = math.min(v, Height);
            var offset = SoftGraphicsUtils.EncodeZOrder(x, y);
            Unsafe.Add(ref m_blob0[0], offset) = r;
            Unsafe.Add(ref m_blob1[0], offset) = g;
            Unsafe.Add(ref m_blob2[0], offset) = b;
            Unsafe.Add(ref m_blob3[0], offset) = a;
        }

        #endregion

        #region Read

        public override unsafe void ReadRowUNorm8(int row, Span<byte> target, AJobScheduler? scheduler = null)
        {
            scheduler ??= SyncJobScheduler.Instance;

            var w = (Width + 15) / 16;
            uint_mt y = (uint)row;
            fixed (byte* p_target = target)
            {
                scheduler.Dispatch(w, 1, (self: this, (IntPtr)p_target, len: target.Length, y), static (ctx, x, _) =>
                {
                    var (self, p_target, target_len, y) = ctx;
                    var target = new Span<byte>((byte*)p_target, target_len);

                    var index = new uint_mt(x * 16) + SoftGraphicsUtils.IncMt;
                    var active = index < self.m_size_cache.x;
                    var offset = SoftGraphicsUtils.EncodeZOrder(new(index, y)).asi();

                    var r = SoftGraphicsUtils.Gather(ref self.m_blob0[0], offset, active);
                    var g = SoftGraphicsUtils.Gather(ref self.m_blob1[0], offset, active);
                    var b = SoftGraphicsUtils.Gather(ref self.m_blob2[0], offset, active);
                    var a = SoftGraphicsUtils.Gather(ref self.m_blob3[0], offset, active);

                    var r512 = ((uint_mt)math_mt.clamp(r * 255f, 0f, 255f).round()).vector;
                    var g512 = ((uint_mt)math_mt.clamp(g * 255f, 0f, 255f).round()).vector;
                    var b512 = ((uint_mt)math_mt.clamp(b * 255f, 0f, 255f).round()).vector;
                    var a512 = ((uint_mt)math_mt.clamp(a * 255f, 0f, 255f).round()).vector;

                    var r256 = Vector256.Narrow(r512.GetLower(), r512.GetUpper());
                    var g256 = Vector256.Narrow(g512.GetLower(), g512.GetUpper());
                    var b256 = Vector256.Narrow(b512.GetLower(), b512.GetUpper());
                    var a256 = Vector256.Narrow(a512.GetLower(), a512.GetUpper());

                    var r128 = Vector128.Narrow(r256.GetLower(), r256.GetUpper());
                    var g128 = Vector128.Narrow(g256.GetLower(), g256.GetUpper());
                    var b128 = Vector128.Narrow(b256.GetLower(), b256.GetUpper());
                    var a128 = Vector128.Narrow(a256.GetLower(), a256.GetUpper());

                    var rgba = Vector512.Create(
                        Vector256.Create(r128, g128),
                        Vector256.Create(b128, a128)
                    );
                    rgba = Vector512.Shuffle(
                        rgba,
                        Vector512.Create(
                            (byte) /**/ 00, 16, 32, 48, /**/ 01, 17, 33, 49,
                            /*       */ 02, 18, 34, 50, /**/ 03, 19, 35, 51,
                            /*       */ 04, 20, 36, 52, /**/ 05, 21, 37, 53,
                            /*       */ 06, 22, 38, 54, /**/ 07, 23, 39, 55,
                            /*                                            */
                            /*       */ 08, 24, 40, 56, /**/ 09, 25, 41, 57,
                            /*       */ 10, 26, 42, 58, /**/ 11, 27, 43, 59,
                            /*       */ 12, 28, 44, 60, /**/ 13, 29, 45, 61,
                            /*       */ 14, 30, 46, 62, /**/ 15, 31, 47, 63
                        )
                    );

                    if (x + 15 < self.Width)
                    {
                        rgba.StoreUnsafe(ref target[0], x * 64);
                    }
                    else
                    {
                        var len = self.Width - (x + 15);
                        Span<byte> tmp = stackalloc byte[64];
                        rgba.StoreUnsafe(ref tmp[0]);
                        tmp.CopyTo(target.Slice((int)x * 64, (int)len * 4));
                    }
                });
            }
        }

        #endregion

        #region QuadQuad

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override void QuadQuadStore(uint x, uint y, float4_mt color)
        {
            var index = SoftGraphicsUtils.EncodeZOrder(x, y) * 16;
            QuadQuadStore(index, color);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override void QuadQuadStore(uint z_index, float4_mt color)
        {
            color.r.vector.StoreUnsafe(ref Unsafe.Add(ref m_blob0[0], z_index));
            color.g.vector.StoreUnsafe(ref Unsafe.Add(ref m_blob1[0], z_index));
            color.b.vector.StoreUnsafe(ref Unsafe.Add(ref m_blob2[0], z_index));
            color.a.vector.StoreUnsafe(ref Unsafe.Add(ref m_blob3[0], z_index));
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override float4_mt QuadQuadLoad(uint x, uint y)
        {
            var index = SoftGraphicsUtils.EncodeZOrder(x, y) * 16;
            return QuadQuadLoad(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override float4_mt QuadQuadLoad(uint z_index)
        {
            var r = Vector512.LoadUnsafe(ref Unsafe.Add(ref m_blob0[0], z_index));
            var g = Vector512.LoadUnsafe(ref Unsafe.Add(ref m_blob1[0], z_index));
            var b = Vector512.LoadUnsafe(ref Unsafe.Add(ref m_blob2[0], z_index));
            var a = Vector512.LoadUnsafe(ref Unsafe.Add(ref m_blob3[0], z_index));

            return new(new(r), new(g), new(b), new(a));
        }

        #endregion
    }

    #endregion
}
