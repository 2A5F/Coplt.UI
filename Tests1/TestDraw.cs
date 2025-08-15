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

    public ref struct PixelInput
    {
        public float4_mt Position_CS;
        public float3_mt Color;
    }

    public struct Pipeline : ISoftGraphicPipelineState, ISoftPixelShader<SoftSimpleRefMesh, PixelInput, float4_mt>
    {
        private static readonly SoftGraphicPipelineState s_state = new()
        {
            BlendEnable = true,
            SrcBlend = SoftBlend.SrcAlpha,
            DstBlend = SoftBlend.InvSrcAlpha,
            BlendOp = SoftBlendOp.Add,
        };
        public ref readonly SoftGraphicPipelineState State => ref s_state;

        public static bool HasDepth => false;
        public static bool HasStencil => false;

        public PixelInput CreateInput(ref SoftSimpleRefMesh mesh, in InterpolateContext ctx, in PixelBasicData data) => new()
        {
            Position_CS = ctx.InterpolateClipSpace(data.cs_a, data.cs_a, data.cs_c),
            Color = ctx.PerspectiveInterpolate(
                new float3_mt(0.9f, 0.2f, 0.2f),
                new float3_mt(0.2f, 0.9f, 0.2f),
                new float3_mt(0.2f, 0.2f, 0.9f)
            ),
        };

        public float4_mt GetColor(in float4_mt output) => output;
        public float_mt GetDepth(in float4_mt output) => throw new NotSupportedException();
        public uint_mt GetStencil(in float4_mt output) => throw new NotSupportedException();
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
            new Pipeline(),
            static (ref Pipeline _, SoftLaneContext ctx, scoped PixelInput input) =>
            {
                var color = input.Color;
                return new float4_mt(color, 0.75f);
            }
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

    [Test, Repeat(100)]
    public void Test_Draw2()
    {
        var ctx = new SoftGraphicsContext();
        var rt = SoftTexture.Create(SoftTextureFormat.R8_G8_B8_A8_UNorm, 1024, 1024);
        ctx.SetRenderTarget(rt);
        ctx.ClearRenderTarget(new float4(0.89f, 0.56f, 0.24f, 1f));
        ctx.SetViewport(new(0, 0, 1024, 1024, 0, 1));
        ctx.SetScissorRect(new(0, 0, 1024, 1024));
    
        var start = Stopwatch.GetTimestamp();
    
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
            new Pipeline(),
            static (ref Pipeline _, SoftLaneContext ctx, scoped PixelInput input) =>
            {
                var color = input.Color;
                return new float4_mt(color, 0.75f);
            }
        );
    
        var end = Stopwatch.GetTimestamp();
        var el = Stopwatch.GetElapsedTime(start, end);
        Console.WriteLine($"{el} ; {el.TotalMilliseconds}ms ; {el.TotalMicroseconds}us");
    }
}
