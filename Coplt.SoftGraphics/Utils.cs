using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Coplt.Mathematics.Simt;

namespace Coplt.SoftGraphics;

public static partial class Utils
{
    #region ZOrder

    private static readonly ushort[] s_z_order_encode_table =
    [
        0, 1, 4, 5, 16, 17, 20, 21, 64, 65, 68, 69, 80, 81, 84, 85, 256, 257, 260, 261, 272, 273, 276, 277, 320, 321, 324, 325, 336, 337, 340, 341, 1024, 1025,
        1028, 1029, 1040, 1041, 1044, 1045, 1088, 1089, 1092, 1093, 1104, 1105, 1108, 1109, 1280, 1281, 1284, 1285, 1296, 1297, 1300, 1301, 1344, 1345, 1348,
        1349, 1360, 1361, 1364, 1365, 4096, 4097, 4100, 4101, 4112, 4113, 4116, 4117, 4160, 4161, 4164, 4165, 4176, 4177, 4180, 4181, 4352, 4353, 4356, 4357,
        4368, 4369, 4372, 4373, 4416, 4417, 4420, 4421, 4432, 4433, 4436, 4437, 5120, 5121, 5124, 5125, 5136, 5137, 5140, 5141, 5184, 5185, 5188, 5189, 5200,
        5201, 5204, 5205, 5376, 5377, 5380, 5381, 5392, 5393, 5396, 5397, 5440, 5441, 5444, 5445, 5456, 5457, 5460, 5461, 16384, 16385, 16388, 16389, 16400,
        16401, 16404, 16405, 16448, 16449, 16452, 16453, 16464, 16465, 16468, 16469, 16640, 16641, 16644, 16645, 16656, 16657, 16660, 16661, 16704, 16705,
        16708, 16709, 16720, 16721, 16724, 16725, 17408, 17409, 17412, 17413, 17424, 17425, 17428, 17429, 17472, 17473, 17476, 17477, 17488, 17489, 17492,
        17493, 17664, 17665, 17668, 17669, 17680, 17681, 17684, 17685, 17728, 17729, 17732, 17733, 17744, 17745, 17748, 17749, 20480, 20481, 20484, 20485,
        20496, 20497, 20500, 20501, 20544, 20545, 20548, 20549, 20560, 20561, 20564, 20565, 20736, 20737, 20740, 20741, 20752, 20753, 20756, 20757, 20800,
        20801, 20804, 20805, 20816, 20817, 20820, 20821, 21504, 21505, 21508, 21509, 21520, 21521, 21524, 21525, 21568, 21569, 21572, 21573, 21584, 21585,
        21588, 21589, 21760, 21761, 21764, 21765, 21776, 21777, 21780, 21781, 21824, 21825, 21828, 21829, 21840, 21841, 21844, 21845
    ];

    [MethodImpl(256 | 512)]
    public static unsafe uint_mt16 EncodeZOrderGather(uint2_mt16 xy)
    {
        if (!Avx2.IsSupported) throw new NotSupportedException("Avx2 is not supported");

        var x = xy.x.vector;
        var y = xy.y.vector;
        var xl = x.GetLower();
        var xu = x.GetUpper();
        var yl = y.GetLower();
        var yu = y.GetUpper();

        fixed (ushort* ptr = s_z_order_encode_table)
        {
            var a_xl = Avx2.GatherVector256(
                (uint*)ptr, xl.AsInt32(), 2
            );
            var a_xu = Avx2.GatherVector256(
                (uint*)ptr, xu.AsInt32(), 2
            );
            var a_yl = Avx2.GatherVector256(
                (uint*)ptr, yl.AsInt32(), 2
            );
            var a_yu = Avx2.GatherVector256(
                (uint*)ptr, yu.AsInt32(), 2
            );

            var a_x = Vector512.Create(a_xl, a_xu) & Vector512.Create((uint)ushort.MaxValue);
            var a_y = Vector512.Create(a_yl, a_yu) & Vector512.Create((uint)ushort.MaxValue);

            a_x &= Vector512.Create((uint)ushort.MaxValue);
            a_y &= Vector512.Create((uint)ushort.MaxValue);

            return new(a_x | (a_y << 1));
        }
    }

    [MethodImpl(256 | 512)]
    public static uint_mt16 EncodeZOrderPclmulqdq(uint2_mt16 xy)
    {
        if (!Pclmulqdq.IsSupported) throw new NotSupportedException("Pclmulqdq is not supported");

        // wait .net 10, no 512 support is slow

        var x = xy.x.vector;
        var y = xy.y.vector;
        var xl = x.GetLower();
        var xu = x.GetUpper();
        var yl = y.GetLower();
        var yu = y.GetUpper();
        var xll = xl.GetLower();
        var xlu = xl.GetUpper();
        var xul = xu.GetLower();
        var xuu = xu.GetUpper();
        var yll = yl.GetLower();
        var ylu = yl.GetUpper();
        var yul = yu.GetLower();
        var yuu = yu.GetUpper();

        var (xll_a, xll_b) = Vector128.Widen(xll);
        var (xlu_a, xlu_b) = Vector128.Widen(xlu);
        var (xul_a, xul_b) = Vector128.Widen(xul);
        var (xuu_a, xuu_b) = Vector128.Widen(xuu);
        var (yll_a, yll_b) = Vector128.Widen(yll);
        var (ylu_a, ylu_b) = Vector128.Widen(ylu);
        var (yul_a, yul_b) = Vector128.Widen(yul);
        var (yuu_a, yuu_b) = Vector128.Widen(yuu);

        var r_xll_a = Pclmulqdq.CarrylessMultiply(xll_a, xll_a, 0);
        var r_xll_b = Pclmulqdq.CarrylessMultiply(xll_b, xll_b, 0);
        var r_xlu_a = Pclmulqdq.CarrylessMultiply(xlu_a, xlu_a, 0);
        var r_xlu_b = Pclmulqdq.CarrylessMultiply(xlu_b, xlu_b, 0);
        var r_xul_a = Pclmulqdq.CarrylessMultiply(xul_a, xul_a, 0);
        var r_xul_b = Pclmulqdq.CarrylessMultiply(xul_b, xul_b, 0);
        var r_xuu_a = Pclmulqdq.CarrylessMultiply(xuu_a, xuu_a, 0);
        var r_xuu_b = Pclmulqdq.CarrylessMultiply(xuu_b, xuu_b, 0);
        var r_yll_a = Pclmulqdq.CarrylessMultiply(yll_a, yll_a, 0);
        var r_yll_b = Pclmulqdq.CarrylessMultiply(yll_b, yll_b, 0);
        var r_ylu_a = Pclmulqdq.CarrylessMultiply(ylu_a, ylu_a, 0);
        var r_ylu_b = Pclmulqdq.CarrylessMultiply(ylu_b, ylu_b, 0);
        var r_yul_a = Pclmulqdq.CarrylessMultiply(yul_a, yul_a, 0);
        var r_yul_b = Pclmulqdq.CarrylessMultiply(yul_b, yul_b, 0);
        var r_yuu_a = Pclmulqdq.CarrylessMultiply(yuu_a, yuu_a, 0);
        var r_yuu_b = Pclmulqdq.CarrylessMultiply(yuu_b, yuu_b, 0);

        var r_xll = Vector128.Narrow(r_xll_a, r_xll_b);
        var r_xlu = Vector128.Narrow(r_xlu_a, r_xlu_b);
        var r_xul = Vector128.Narrow(r_xul_a, r_xul_b);
        var r_xuu = Vector128.Narrow(r_xuu_a, r_xuu_b);
        var r_yll = Vector128.Narrow(r_yll_a, r_yll_b);
        var r_ylu = Vector128.Narrow(r_ylu_a, r_ylu_b);
        var r_yul = Vector128.Narrow(r_yul_a, r_yul_b);
        var r_yuu = Vector128.Narrow(r_yuu_a, r_yuu_b);

        var r_xl = Vector256.Create(r_xll, r_xlu);
        var r_xu = Vector256.Create(r_xul, r_xuu);
        var r_yl = Vector256.Create(r_yll, r_ylu);
        var r_yu = Vector256.Create(r_yul, r_yuu);

        var r_x = Vector512.Create(r_xl, r_xu);
        var r_y = Vector512.Create(r_yl, r_yu);

        return new(r_x | (r_y << 1));
    }

    [MethodImpl(256 | 512)]
    public static uint_mt16 EncodeZOrderSoft(uint2_mt16 xy)
    {
        var x = xy.x.vector;
        var y = xy.y.vector;

        x &= Vector512.Create(0x0000FFFFu);
        y &= Vector512.Create(0x0000FFFFu);
        x = (x | (x << 8)) & Vector512.Create(0x00FF00FFu);
        y = (y | (y << 8)) & Vector512.Create(0x00FF00FFu);
        x = (x | (x << 4)) & Vector512.Create(0x0F0F0F0Fu);
        y = (y | (y << 4)) & Vector512.Create(0x0F0F0F0Fu);
        x = (x | (x << 2)) & Vector512.Create(0x33333333u);
        y = (y | (y << 2)) & Vector512.Create(0x33333333u);
        x = (x | (x << 1)) & Vector512.Create(0x55555555u);
        y = (y | (y << 1)) & Vector512.Create(0x55555555u);

        return new(x | (y << 1));

        // return (Encode(xy.y) << 1) | Encode(xy.x);
        //
        // [MethodImpl(256 | 512)]
        // static uint_mt16 Encode(uint_mt16 n)
        // {
        //     n &= 0x0000FFFF;
        //     n = (n | (n << 8)) & 0x00FF00FF;
        //     n = (n | (n << 4)) & 0x0F0F0F0F;
        //     n = (n | (n << 2)) & 0x33333333;
        //     n = (n | (n << 1)) & 0x55555555;
        //     return n;
        // }
    }

    [MethodImpl(256 | 512)]
    public static uint_mt16 EncodeZOrder(uint2_mt16 xy)
    {
        if (Avx512F.IsSupported) return EncodeZOrderSoft(xy);
        if (Avx2.IsSupported) return EncodeZOrderGather(xy);
        // else if (Pclmulqdq.IsSupported) return EncodeZOrderPclmulqdq(xy);
        else return EncodeZOrderSoft(xy);
    }

    [MethodImpl(256 | 512)]
    public static uint2_mt16 DecodeZOrderSoft(uint_mt16 code)
    {
        var x = code.vector;
        var y = x >> 1;

        x &= Vector512.Create(0x55555555u);
        y &= Vector512.Create(0x55555555u);
        x = (x | (x >> 1)) & Vector512.Create(0x33333333u);
        y = (y | (y >> 1)) & Vector512.Create(0x33333333u);
        x = (x | (x >> 2)) & Vector512.Create(0x0F0F0F0Fu);
        y = (y | (y >> 2)) & Vector512.Create(0x0F0F0F0Fu);
        x = (x | (x >> 4)) & Vector512.Create(0x00FF00FFu);
        y = (y | (y >> 4)) & Vector512.Create(0x00FF00FFu);
        x = (x | (x >> 8)) & Vector512.Create(0x0000FFFFu);
        y = (y | (y >> 8)) & Vector512.Create(0x0000FFFFu);

        return new(new(x), new(y));

        // return new(Decode(code), Decode(code >> 1));
        //
        // [MethodImpl(256 | 512)]
        // static uint_mt16 Decode(uint_mt16 n)
        // {
        //     n &= 0x55555555;
        //     n = (n | (n >> 1)) & 0x33333333;
        //     n = (n | (n >> 2)) & 0x0F0F0F0F;
        //     n = (n | (n >> 4)) & 0x00FF00FF;
        //     n = (n | (n >> 8)) & 0x0000FFFF;
        //     return n;
        // }
    }

    [MethodImpl(256 | 512)]
    public static uint2_mt16 DecodeZOrder(uint_mt16 code) => DecodeZOrderSoft(code);

    [MethodImpl(256 | 512)]
    public static uint EncodeZOrderPclmulqdq(uint x, uint y)
    {
        if (!Pclmulqdq.IsSupported) throw new NotSupportedException("Pclmulqdq is not supported");

        var v = Vector128.Create(x, y);
        var a = Pclmulqdq.CarrylessMultiply(v, v, 0);
        var r = a | (Vector128.Shuffle(a, Vector128.Create(1, 0)) << 1);
        return (uint)r[0];
    }

    [MethodImpl(256 | 512)]
    public static uint EncodeZOrderBmi2(uint x, uint y)
    {
        if (!Bmi2.IsSupported) throw new NotSupportedException("Bmi2 is not supported");
        return Bmi2.ParallelBitDeposit(x, 0x55555555u) | Bmi2.ParallelBitDeposit(y, 0xAAAAAAAAu);
    }

    [MethodImpl(256 | 512)]
    public static (uint x, uint y) DecodeZOrderBmi2(uint code)
    {
        if (!Bmi2.IsSupported) throw new NotSupportedException("Bmi2 is not supported");
        return (Bmi2.ParallelBitExtract(code, 0x55555555u), Bmi2.ParallelBitExtract(code, 0xAAAAAAAAu));
    }

    [MethodImpl(256 | 512)]
    public static uint EncodeZOrderSoft(uint x, uint y)
    {
        if (Vector64.IsHardwareAccelerated)
        {
            var n = Vector64.Create(x).WithElement(1, y);
            n &= Vector64.Create(0x0000FFFFu);
            n = (n | (n << 8)) & Vector64.Create(0x00FF00FFu);
            n = (n | (n << 4)) & Vector64.Create(0x0F0F0F0Fu);
            n = (n | (n << 2)) & Vector64.Create(0x33333333u);
            n = (n | (n << 1)) & Vector64.Create(0x55555555u);
            var r = n | (Vector64.Shuffle(n, Vector64.Create(1u, 0)) << 1);
            return r[0];
        }
        else if (Vector128.IsHardwareAccelerated)
        {
            var n = Vector128.Create(x).WithElement(1, y);
            n &= Vector128.Create(0x0000FFFFu);
            n = (n | (n << 8)) & Vector128.Create(0x00FF00FFu);
            n = (n | (n << 4)) & Vector128.Create(0x0F0F0F0Fu);
            n = (n | (n << 2)) & Vector128.Create(0x33333333u);
            n = (n | (n << 1)) & Vector128.Create(0x55555555u);
            var r = n | (Vector128.Shuffle(n, Vector128.Create(1u, 0, 0, 0)) << 1);
            return r[0];
        }
        else
        {
            return (Encode(y) << 1) | Encode(x);

            [MethodImpl(256 | 512)]
            static uint Encode(uint n)
            {
                n &= 0x0000FFFF;
                n = (n | (n << 8)) & 0x00FF00FF;
                n = (n | (n << 4)) & 0x0F0F0F0F;
                n = (n | (n << 2)) & 0x33333333;
                n = (n | (n << 1)) & 0x55555555;
                return n;
            }
        }
    }

    [MethodImpl(256 | 512)]
    public static (uint x, uint y) DecodeZOrderSoft(uint code)
    {
        if (Vector64.IsHardwareAccelerated)
        {
            var n = (Vector64.Create(code) >> 1).WithElement(0, code);
            n &= Vector64.Create(0x55555555u);
            n = (n | (n >> 1)) & Vector64.Create(0x33333333u);
            n = (n | (n >> 2)) & Vector64.Create(0x0F0F0F0Fu);
            n = (n | (n >> 4)) & Vector64.Create(0x00FF00FFu);
            n = (n | (n >> 8)) & Vector64.Create(0x0000FFFFu);
            return (n[0], n[1]);
        }
        else if (Vector128.IsHardwareAccelerated)
        {
            var n = (Vector128.Create(code) >> 1).WithElement(0, code);
            n &= Vector128.Create(0x55555555u);
            n = (n | (n >> 1)) & Vector128.Create(0x33333333u);
            n = (n | (n >> 2)) & Vector128.Create(0x0F0F0F0Fu);
            n = (n | (n >> 4)) & Vector128.Create(0x00FF00FFu);
            n = (n | (n >> 8)) & Vector128.Create(0x0000FFFFu);
            return (n[0], n[1]);
        }
        else
        {
            return new(Decode(code), Decode(code >> 1));

            [MethodImpl(256 | 512)]
            static uint Decode(uint n)
            {
                n &= 0x55555555;
                n = (n | (n >> 1)) & 0x33333333;
                n = (n | (n >> 2)) & 0x0F0F0F0F;
                n = (n | (n >> 4)) & 0x00FF00FF;
                n = (n | (n >> 8)) & 0x0000FFFF;
                return n;
            }
        }
    }

    [MethodImpl(256 | 512)]
    public static uint EncodeZOrder(uint x, uint y)
    {
        if (Bmi2.IsSupported) return EncodeZOrderBmi2(x, y);
        else return EncodeZOrderSoft(x, y);
    }

    [MethodImpl(256 | 512)]
    public static (uint x, uint y) DecodeZOrder(uint code)
    {
        if (Bmi2.IsSupported) return DecodeZOrderBmi2(code);
        return DecodeZOrderSoft(code);
    }

    #endregion

    #region Gather Unsafe

    [MethodImpl(256 | 512)]
    public static unsafe Vector64<float> Gather(float* addr, Vector64<int> offset, Vector64<uint> active_lanes)
    {
        if (Avx2.IsSupported)
            return Gather(
                addr,
                Vector128.Create(offset, offset),
                Vector128.Create(active_lanes, default)
            ).GetLower();
        return Vector64.Create(
            active_lanes[0] != 0 ? addr[offset[0]] : 0,
            active_lanes[0] != 0 ? addr[offset[1]] : 0
        );
    }

    [MethodImpl(256 | 512)]
    public static unsafe Vector128<float> Gather(float* addr, Vector128<int> offset, Vector128<uint> active_lanes)
    {
        if (Avx2.IsSupported)
            return Avx2.GatherMaskVector128(
                Vector128<float>.Zero, addr, offset, active_lanes.AsSingle(), 4
            );
        return Vector128.Create(
            Gather(addr, offset.GetLower(), active_lanes.GetLower()),
            Gather(addr, offset.GetUpper(), active_lanes.GetUpper())
        );
    }

    [MethodImpl(256 | 512)]
    public static unsafe Vector256<float> Gather(float* addr, Vector256<int> offset, Vector256<uint> active_lanes)
    {
        if (Avx2.IsSupported)
            return Avx2.GatherMaskVector256(
                Vector256<float>.Zero, addr, offset, active_lanes.AsSingle(), 4
            );
        return Vector256.Create(
            Gather(addr, offset.GetLower(), active_lanes.GetLower()),
            Gather(addr, offset.GetUpper(), active_lanes.GetUpper())
        );
    }

    [MethodImpl(256 | 512)]
    public static unsafe Vector512<float> Gather(float* addr, Vector512<int> offset, Vector512<uint> active_lanes)
    {
        return Vector512.Create(
            Gather(addr, offset.GetLower(), active_lanes.GetLower()),
            Gather(addr, offset.GetUpper(), active_lanes.GetUpper())
        );
    }

    #endregion

    #region GatherUNorm Unsafe

    [MethodImpl(256 | 512)]
    public static unsafe Vector64<float> GatherUNorm(byte* addr, Vector64<int> offset, Vector64<uint> active_lanes)
    {
        if (Avx2.IsSupported)
            return GatherUNorm(
                addr,
                Vector128.Create(offset, offset),
                Vector128.Create(active_lanes, default)
            ).GetLower();
        return Vector64.Create(
            active_lanes[0] != 0 ? addr[offset[0]] * (1f / 255f) : 0,
            active_lanes[1] != 0 ? addr[offset[1]] * (1f / 255f) : 0
        );
    }

    [MethodImpl(256 | 512)]
    public static unsafe Vector128<float> GatherUNorm(byte* addr, Vector128<int> offset, Vector128<uint> active_lanes)
    {
        if (Avx2.IsSupported)
        {
            var a = Avx2.GatherMaskVector128(
                Vector128<uint>.Zero, (uint*)addr, offset, active_lanes, 1
            ) & Vector128.Create((uint)byte.MaxValue);
            return Vector128.ConvertToSingle(a) * (1f / 255f);
        }
        return Vector128.Create(
            GatherUNorm(addr, offset.GetLower(), active_lanes.GetLower()),
            GatherUNorm(addr, offset.GetUpper(), active_lanes.GetUpper())
        );
    }

    [MethodImpl(256 | 512)]
    public static unsafe Vector256<float> GatherUNorm(byte* addr, Vector256<int> offset, Vector256<uint> active_lanes)
    {
        if (Avx2.IsSupported)
        {
            var a = Avx2.GatherMaskVector256(
                Vector256<uint>.Zero, (uint*)addr, offset, active_lanes, 1
            ) & Vector256.Create((uint)byte.MaxValue);
            return Vector256.ConvertToSingle(a) * (1f / 255f);
        }
        return Vector256.Create(
            GatherUNorm(addr, offset.GetLower(), active_lanes.GetLower()),
            GatherUNorm(addr, offset.GetUpper(), active_lanes.GetUpper())
        );
    }

    [MethodImpl(256 | 512)]
    public static unsafe Vector512<float> GatherUNorm(byte* addr, Vector512<int> offset, Vector512<uint> active_lanes)
    {
        return Vector512.Create(
            GatherUNorm(addr, offset.GetLower(), active_lanes.GetLower()),
            GatherUNorm(addr, offset.GetUpper(), active_lanes.GetUpper())
        );
    }

    #endregion

    #region Gather

    [MethodImpl(256 | 512)]
    public static unsafe float_mt16 Gather(ref float addr, int_mt16 offset, b32_mt16 active_lanes)
    {
        fixed (float* ptr = &addr)
        {
            return new(Gather(ptr, offset.vector, active_lanes.vector));
        }
    }

    [MethodImpl(256 | 512)]
    public static unsafe float_mt16 GatherUNorm(ref byte addr, int_mt16 offset, b32_mt16 active_lanes)
    {
        fixed (byte* ptr = &addr)
        {
            return new(GatherUNorm(ptr, offset.vector, active_lanes.vector));
        }
    }

    #endregion

    #region Scatter Unsafe

    [MethodImpl(256 | 512)]
    public static unsafe void Scatter(Vector64<float> value, float* addr, Vector64<int> offset, Vector64<uint> active_lanes)
    {
        if (active_lanes[0] != 0) addr[offset[0]] = value[0];
        if (active_lanes[1] != 0) addr[offset[1]] = value[1];
    }

    [MethodImpl(256 | 512)]
    public static unsafe void Scatter(Vector128<float> value, float* addr, Vector128<int> offset, Vector128<uint> active_lanes)
    {
        Scatter(value.GetLower(), addr, offset.GetLower(), active_lanes.GetLower());
        Scatter(value.GetUpper(), addr, offset.GetUpper(), active_lanes.GetUpper());
    }

    [MethodImpl(256 | 512)]
    public static unsafe void Scatter(Vector256<float> value, float* addr, Vector256<int> offset, Vector256<uint> active_lanes)
    {
        Scatter(value.GetLower(), addr, offset.GetLower(), active_lanes.GetLower());
        Scatter(value.GetUpper(), addr, offset.GetUpper(), active_lanes.GetUpper());
    }

    [MethodImpl(256 | 512)]
    public static unsafe void Scatter(Vector512<float> value, float* addr, Vector512<int> offset, Vector512<uint> active_lanes)
    {
        Scatter(value.GetLower(), addr, offset.GetLower(), active_lanes.GetLower());
        Scatter(value.GetUpper(), addr, offset.GetUpper(), active_lanes.GetUpper());
    }

    #endregion

    #region ScatterUNorm Unsafe

    [MethodImpl(256 | 512)]
    public static unsafe void ScatterUNorm(Vector64<float> value, byte* addr, Vector64<int> offset, Vector64<uint> active_lanes)
    {
        if (active_lanes[0] != 0) addr[offset[0]] = (byte)Math.Round(Math.Clamp(value[0] * 255f, 0f, 255f));
        if (active_lanes[1] != 0) addr[offset[1]] = (byte)Math.Round(Math.Clamp(value[1] * 255f, 0f, 255f));
    }

    [MethodImpl(256 | 512)]
    public static unsafe void ScatterUNorm(Vector128<float> value, byte* addr, Vector128<int> offset, Vector128<uint> active_lanes)
    {
        ScatterUNorm(value.GetLower(), addr, offset.GetLower(), active_lanes.GetLower());
        ScatterUNorm(value.GetUpper(), addr, offset.GetUpper(), active_lanes.GetUpper());
    }

    [MethodImpl(256 | 512)]
    public static unsafe void ScatterUNorm(Vector256<float> value, byte* addr, Vector256<int> offset, Vector256<uint> active_lanes)
    {
        ScatterUNorm(value.GetLower(), addr, offset.GetLower(), active_lanes.GetLower());
        ScatterUNorm(value.GetUpper(), addr, offset.GetUpper(), active_lanes.GetUpper());
    }

    [MethodImpl(256 | 512)]
    public static unsafe void ScatterUNorm(Vector512<float> value, byte* addr, Vector512<int> offset, Vector512<uint> active_lanes)
    {
        ScatterUNorm(value.GetLower(), addr, offset.GetLower(), active_lanes.GetLower());
        ScatterUNorm(value.GetUpper(), addr, offset.GetUpper(), active_lanes.GetUpper());
    }

    #endregion

    #region Scatter

    [MethodImpl(256 | 512)]
    public static unsafe void Scatter(float_mt16 value, ref float addr, int_mt16 offset, b32_mt16 active_lanes)
    {
        fixed (float* ptr = &addr)
        {
            Scatter(value.vector, ptr, offset.vector, active_lanes.vector);
        }
    }

    [MethodImpl(256 | 512)]
    public static unsafe void ScatterUNorm(float_mt16 value, ref byte addr, int_mt16 offset, b32_mt16 active_lanes)
    {
        fixed (byte* ptr = &addr)
        {
            ScatterUNorm(value.vector, ptr, offset.vector, active_lanes.vector);
        }
    }

    #endregion
}
