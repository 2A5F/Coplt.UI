using Coplt.Mathematics.Simt;

namespace Coplt.SoftGraphics;

public struct SoftLaneContext
{
    public int_mt Index;
    public b32_mt ActiveLanes;
    public uint_mt QuadMask;
}

public interface ISoftPixelShader<TMesh>
    where TMesh : ISoftMeshData, allows ref struct
{
    public void Invoke(
        ref TMesh mesh,
        in InterpolateContext ic,
        in SoftLaneContext lc,
        in PixelBasicData data,
        ref float4_mt output_color,
        ref float_mt output_depth,
        ref uint_mt output_stencil
    );
}
