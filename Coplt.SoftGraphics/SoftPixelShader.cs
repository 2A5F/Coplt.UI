using Coplt.Mathematics.Simt;

namespace Coplt.SoftGraphics;

public delegate TOutput SoftPixelShader<TPipeline, in TInput, out TOutput>(
    ref TPipeline pipeline, SoftLaneContext ctx, scoped TInput input
)
    where TPipeline : allows ref struct
    where TInput : allows ref struct
    where TOutput : allows ref struct;

public struct SoftLaneContext
{
    public int_mt Index;
    public b32_mt ActiveLanes;
    public uint_mt QuadMask;
}

public interface ISoftPixelShader<TMesh, out TInput, TOutput>
    where TMesh : ISoftMeshData, allows ref struct
    where TInput : allows ref struct
    where TOutput : allows ref struct
{
    public static abstract bool HasDepth { get; }
    public static abstract bool HasStencil { get; }

    public TInput CreateInput(
        ref TMesh mesh,
        in InterpolateContext ctx,
        in PixelBasicData data
    );

    public float4_mt GetColor(in TOutput output);
    public float_mt GetDepth(in TOutput output);
    public uint_mt GetStencil(in TOutput output);
}
