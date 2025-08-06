using Coplt.Mathematics.Simt;

namespace Coplt.SoftGraphics;

/// <summary>
/// 4 pixels per operation
/// </summary>
public delegate PixelData SoftPixelShader<in VertexData, out PixelData>(
    SoftLaneContext ctx, VertexData input
)
    where VertexData : unmanaged, IVertexData<VertexData>
    where PixelData : unmanaged, IPixelData<PixelData>;

/// <summary>
/// Wave line context
/// </summary>
public struct SoftLaneContext
{
    public b32_mt16 ActiveLanes;
    public uint_mt16 QuadMask;
}

public interface IVertexData<T> where T : unmanaged, IVertexData<T>
{
    public float4_mt16 Position_ClipSpace { get; }

    public static abstract T Interpolation(in T a, in T b, float_mt16 t);
}

public interface IPixelData<T> where T : unmanaged, IPixelData<T>
{
    public float4_mt16 Color0 { get; }
    
    public float_mt16 Depth { get; }
    public uint_mt16 Stencil { get; }
    
    public static abstract bool HasDepth { get; }
    public static abstract bool HasStencil { get; }
}
