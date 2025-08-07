using System.Runtime.InteropServices;
using Coplt.Mathematics;
using Coplt.SoftGraphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Tests1;

public class TestDraw
{
    [Test]
    public void Test1()
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
        img.SaveAsPng("./test_draw_test1.png");
    }
}
