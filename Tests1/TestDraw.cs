using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Mathematics;
using Coplt.Mathematics.Simt;
using Coplt.SoftGraphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Tests1;

public class TestDraw
{
    [Test]
    public void Test_ClearColor_1024x1024_R32_G32_B32_A32_Float()
    {
        var ctx = new SoftGraphicsContext();
        var rt = SoftTexture.Create(SoftTextureFormat.R32_G32_B32_A32_Float, 1024, 1024);
        ctx.SetRenderTarget(rt);
        ctx.ClearRenderTarget(new float4(0.89f, 0.56f, 0.24f, 1f));

        using Image<Rgba32> img = new(1024, 1024);
        var buf = img.Frames[0].PixelBuffer;
        for (var y = 0; y < img.Height; y++)
        {
            var span = buf.DangerousGetRowSpan(y);
            rt.ReadRowUNorm8(y, MemoryMarshal.AsBytes(span), ParallelJobScheduler.Instance);
        }
        img.SaveAsPng("./Test_ClearColor_1024x1024_R32_G32_B32_A32_Float.png");

        Assert.Multiple(() =>
        {
            ParallelJobScheduler.Instance.Dispatch((uint)img.Width, (uint)img.Height, buf, static (buf, x, y) =>
            {
                var span = buf.DangerousGetRowSpan((int)y);
                var pixel = span[(int)x];
                Assert.That(pixel, Is.EqualTo(new Rgba32(227, 143, 61, 255)));
            });
        });
    }

    [Test]
    public void Test_ClearColor_1024x1024_R8_G8_B8_A8_UNorm()
    {
        var ctx = new SoftGraphicsContext();
        var rt = SoftTexture.Create(SoftTextureFormat.R8_G8_B8_A8_UNorm, 1024, 1024);
        ctx.SetRenderTarget(rt);
        ctx.ClearRenderTarget(new float4(0.89f, 0.56f, 0.24f, 1f));

        using Image<Rgba32> img = new(1024, 1024);
        var buf = img.Frames[0].PixelBuffer;
        for (var y = 0; y < img.Height; y++)
        {
            var span = buf.DangerousGetRowSpan(y);
            rt.ReadRowUNorm8(y, MemoryMarshal.AsBytes(span), ParallelJobScheduler.Instance);
        }
        img.SaveAsPng("./Test_ClearColor_1024x1024_R8_G8_B8_A8_UNorm.png");

        Assert.Multiple(() =>
        {
            ParallelJobScheduler.Instance.Dispatch((uint)img.Width, (uint)img.Height, buf, static (buf, x, y) =>
            {
                var span = buf.DangerousGetRowSpan((int)y);
                var pixel = span[(int)x];
                Assert.That(pixel, Is.EqualTo(new Rgba32(227, 143, 61, 255)));
            });
        });
    }

    public ref struct Mesh1(
        ReadOnlySpan<int> Indices_A,
        ReadOnlySpan<int> Indices_B,
        ReadOnlySpan<int> Indices_C,
        ReadOnlySpan<float> Position_ClipSpace_X,
        ReadOnlySpan<float> Position_ClipSpace_Y,
        ReadOnlySpan<float> Position_ClipSpace_Z,
        ReadOnlySpan<float> Position_ClipSpace_W
    ) : ISoftMeshData
    {
        public ReadOnlySpan<int> Indices_A = Indices_A;
        public ReadOnlySpan<int> Indices_B = Indices_B;
        public ReadOnlySpan<int> Indices_C = Indices_C;

        public ReadOnlySpan<float> Position_ClipSpace_X = Position_ClipSpace_X;
        public ReadOnlySpan<float> Position_ClipSpace_Y = Position_ClipSpace_Y;
        public ReadOnlySpan<float> Position_ClipSpace_Z = Position_ClipSpace_Z;
        public ReadOnlySpan<float> Position_ClipSpace_W = Position_ClipSpace_W;

        #region Interface

        public float4_mt Gather_Position_ClipSpace(int_mt index, b32_mt active_lanes)
        {
            var x = SoftGraphicsUtils.Gather(in Position_ClipSpace_X[0], index, active_lanes);
            var y = SoftGraphicsUtils.Gather(in Position_ClipSpace_Y[0], index, active_lanes);
            var z = SoftGraphicsUtils.Gather(in Position_ClipSpace_Z[0], index, active_lanes);
            var w = SoftGraphicsUtils.Gather(in Position_ClipSpace_W[0], index, active_lanes);
            return new(x, y, z, w);
        }


        public uint NumClusters => 1;
        public uint NumPrimitives(uint Cluster) => (uint)Indices_A.Length;

        [MethodImpl(256 | 512)]
        public void Load(uint Cluster, uint IndexStep16, out float4_mt a, out float4_mt b, out float4_mt c, out uint_mt index, out b32_mt active_lanes)
        {
            var index_ = IndexStep16 + SoftGraphicsUtils.IncMt;
            var active_lanes_ = index_ < (uint)Indices_A.Length;

            var index_a = Indices_A[(int)IndexStep16];
            var index_b = Indices_B[(int)IndexStep16];
            var index_c = Indices_C[(int)IndexStep16];

            a = Gather_Position_ClipSpace(index_a.asi(), active_lanes_);
            b = Gather_Position_ClipSpace(index_b.asi(), active_lanes_);
            c = Gather_Position_ClipSpace(index_c.asi(), active_lanes_);

            index = index_;
            active_lanes = active_lanes_;
        }

        #endregion
    }

    public struct Pixel1 : IPixelData { }

    [Test]
    public void Test_Draw()
    {
        var ctx = new SoftGraphicsContext();
        ctx.SetJobScheduler(SyncJobScheduler.Instance);
        var rt = SoftTexture.Create(SoftTextureFormat.R8_G8_B8_A8_UNorm, 1024, 1024);
        ctx.SetRenderTarget(rt);
        ctx.SetViewport(new(0, 0, 1024, 1024, 0, 1));
        ctx.SetScissorRect(new(0, 0, 1024, 1024));
        ctx.Draw(
            new Mesh1(
                [0],
                [1],
                [2],
                [-0.5f, 0, 0.5f],
                [-0.5f, 0.5f, -0.5f],
                [1, 1, 1],
                [1, 1, 1]
            )
        );

        using Image<Rgba32> img = new(1024, 1024);
        var buf = img.Frames[0].PixelBuffer;
        for (var y = 0; y < img.Height; y++)
        {
            var span = buf.DangerousGetRowSpan(y);
            rt.ReadRowUNorm8(y, MemoryMarshal.AsBytes(span), ParallelJobScheduler.Instance);
        }
        img.SaveAsPng("./Test_Draw.png");
    }
}
