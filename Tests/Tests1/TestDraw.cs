using System.Diagnostics;
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

    public struct Pipeline : ISoftGraphicPipelineState, ISoftPixelShader<SoftSimpleRefMesh>
    {
        private static readonly SoftGraphicPipelineState s_state = new()
        {
            BlendEnable = true,
            SrcBlend = SoftBlend.SrcAlpha,
            DstBlend = SoftBlend.InvSrcAlpha,
            BlendOp = SoftBlendOp.Add,
        };
        public ref readonly SoftGraphicPipelineState State => ref s_state;

        [MethodImpl(256 | 512)]
        public void Invoke(
            ref SoftSimpleRefMesh mesh, in InterpolateContext ic, in SoftLaneContext lc, in PixelBasicData data,
            ref float4_mt output_color, ref float_mt output_depth, ref uint_mt output_stencil
        )
        {
            var color = ic.PerspectiveInterpolate(
                new float3_mt(0.9f, 0.2f, 0.2f),
                new float3_mt(0.2f, 0.9f, 0.2f),
                new float3_mt(0.2f, 0.2f, 0.9f)
            );
            output_color = new float4_mt(color, 0.75f);
        }
    }

    [Test]
    public void Test_Draw()
    {
        var ctx = new SoftGraphicsContext();
        ctx.SetJobScheduler(SyncJobScheduler.Instance);
        var rt = SoftTexture.Create(SoftTextureFormat.R8_G8_B8_A8_UNorm, 1024, 1024);
        ctx.SetRenderTarget(rt);
        ctx.ClearRenderTarget(new float4(0.89f, 0.56f, 0.24f, 1f));
        ctx.SetViewport(new(0, 0, 1024, 1024, 0, 1));
        ctx.SetScissorRect(new(0, 0, 1024, 1024));
        ctx.Draw(
            new SoftSimpleRefMesh(
                [0, 2],
                [1, 1],
                [2, 3],
                [-0.5f, 0.5f, -0.5f, 0.5f],
                [-0.5f, -0.5f, 0.5f, 0.5f],
                [1, 1, 1, 1],
                [1, 1, 1, 1]
            ),
            new Pipeline()
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
