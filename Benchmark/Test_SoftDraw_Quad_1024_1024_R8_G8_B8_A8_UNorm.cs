using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using Coplt.Mathematics;
using Coplt.Mathematics.Simt;
using Coplt.SoftGraphics;

namespace Benchmark;

[DisassemblyDiagnoser(maxDepth: 1000, syntax: DisassemblySyntax.Intel)]
public class Test_SoftDraw_Quad_1024_1024_R8_G8_B8_A8_UNorm
{
    public SoftGraphicsContext ctx= null!;
    public SoftTexture rt= null!;

    [GlobalSetup]
    public void Setup()
    {
        ctx = new SoftGraphicsContext();
        rt = SoftTexture.Create(SoftTextureFormat.R8_G8_B8_A8_UNorm, 1024, 1024);
        ctx.SetRenderTarget(rt);
        ctx.ClearRenderTarget(new float4(0.89f, 0.56f, 0.24f, 1f));
        ctx.SetViewport(new(0, 0, 1024, 1024, 0, 1));
        ctx.SetScissorRect(new(0, 0, 1024, 1024));
    }

    [Benchmark]
    public void Draw()
    {
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
}
