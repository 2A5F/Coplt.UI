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

    public ref struct Vertex1(
        ReadOnlySpan<float> Position_ClipSpace_X,
        ReadOnlySpan<float> Position_ClipSpace_Y,
        ReadOnlySpan<float> Position_ClipSpace_Z,
        ReadOnlySpan<float> Position_ClipSpace_W
    ) : IVertexData
    {
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
            3,
            [0],
            [1],
            [2],
            new(
                [-0.5f, 0, 0.5f],
                [0, 0.5f, 0],
                [1, 1, 1],
                [1, 1, 1]
            ),
            static (SoftLaneContext ctx, Vertex1 input, Pixel1 output) => { }
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
