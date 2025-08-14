using System.Runtime.CompilerServices;
using Coplt.Mathematics;
using Coplt.Mathematics.Simt;

namespace Coplt.SoftGraphics;

public ref struct SoftRefMesh : ISoftMeshData
{
    public ReadOnlySpan<int> Indices_A;
    public ReadOnlySpan<int> Indices_B;
    public ReadOnlySpan<int> Indices_C;

    public ReadOnlySpan<float> Position_ClipSpace_X;
    public ReadOnlySpan<float> Position_ClipSpace_Y;
    public ReadOnlySpan<float> Position_ClipSpace_Z;
    public ReadOnlySpan<float> Position_ClipSpace_W;

    public SoftRefMesh(
        ReadOnlySpan<int> Indices_A,
        ReadOnlySpan<int> Indices_B,
        ReadOnlySpan<int> Indices_C,
        ReadOnlySpan<float> Position_ClipSpace_X,
        ReadOnlySpan<float> Position_ClipSpace_Y,
        ReadOnlySpan<float> Position_ClipSpace_Z,
        ReadOnlySpan<float> Position_ClipSpace_W
    )
    {
        if (Indices_A.Length != Indices_B.Length || Indices_A.Length != Indices_C.Length)
            throw new ArgumentException("Indices length not same");
        if (
            Position_ClipSpace_X.Length != Position_ClipSpace_Y.Length
            || Position_ClipSpace_X.Length != Position_ClipSpace_Z.Length
            || Position_ClipSpace_X.Length != Position_ClipSpace_W.Length
        ) throw new ArgumentException("Position ClipSpace length not same");

        this.Indices_A = Indices_A;
        this.Indices_B = Indices_B;
        this.Indices_C = Indices_C;
        this.Position_ClipSpace_X = Position_ClipSpace_X;
        this.Position_ClipSpace_Y = Position_ClipSpace_Y;
        this.Position_ClipSpace_Z = Position_ClipSpace_Z;
        this.Position_ClipSpace_W = Position_ClipSpace_W;
    }

    [MethodImpl(256 | 512)]
    private static unsafe int_mt LoadIndex(int index, ReadOnlySpan<int> span)
    {
        if (index + 16 <= span.Length)
        {
            return Unsafe.As<int, int_mt>(ref Unsafe.AsRef(in span[index]));
        }
        else
        {
            int_mt r;
            span[index..].CopyTo(new Span<int>(&r, 16));
            return r;
        }
    }

    [MethodImpl(256 | 512)]
    private float4_mt Gather_Position_ClipSpace(int_mt index, b32_mt active_lanes)
    {
        var x = SoftGraphicsUtils.Gather(in Position_ClipSpace_X[0], index, active_lanes);
        var y = SoftGraphicsUtils.Gather(in Position_ClipSpace_Y[0], index, active_lanes);
        var z = SoftGraphicsUtils.Gather(in Position_ClipSpace_Z[0], index, active_lanes);
        var w = SoftGraphicsUtils.Gather(in Position_ClipSpace_W[0], index, active_lanes);
        return new(x, y, z, w);
    }

    public uint NumClusters
    {
        [MethodImpl(256 | 512)]
        get => 1;
    }

    [MethodImpl(256 | 512)]
    public uint NumPrimitives(uint Cluster) => (uint)Indices_A.Length;

    [MethodImpl(256 | 512)]
    public void Load(uint Cluster, uint IndexStep16, out float4_mt a, out float4_mt b, out float4_mt c, out uint_mt index, out b32_mt active_lanes)
    {
        var index_ = IndexStep16 + SoftGraphicsUtils.IncMt;
        var active_lanes_ = index_ < (uint)Indices_A.Length;

        var index_a = LoadIndex((int)IndexStep16, Indices_A);
        var index_b = LoadIndex((int)IndexStep16, Indices_B);
        var index_c = LoadIndex((int)IndexStep16, Indices_C);

        a = Gather_Position_ClipSpace(index_a, active_lanes_);
        b = Gather_Position_ClipSpace(index_b, active_lanes_);
        c = Gather_Position_ClipSpace(index_c, active_lanes_);

        index = index_;
        active_lanes = active_lanes_;
    }
}
