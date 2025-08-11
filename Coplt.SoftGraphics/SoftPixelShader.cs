using Coplt.Mathematics.Simt;

namespace Coplt.SoftGraphics;

/// <summary>
/// 4 pixels per operation
/// </summary>
public delegate void SoftPixelShader<in VertexData, in PixelData>(
    SoftLaneContext ctx, VertexData input, PixelData output
)
    where VertexData : IVertexData, allows ref struct
    where PixelData : IPixelData, allows ref struct;

/// <summary>
/// Wave line context
/// </summary>
public struct SoftLaneContext
{
    public int_mt16 Index;
    public b32_mt16 ActiveLanes;
    public uint_mt16 QuadMask;
}

public interface IVertexData
{
    public float4_mt16 Gather_Position_ClipSpace(
        int_mt16 index, b32_mt16 active_lanes
    );

    // public static abstract void Interpolation(out T r, in T a, in T b, float_mt16 t);
}

public interface IPixelData
{
    // public static abstract ref readonly float4_mt16 Get_Color0(in T input);
    //
    // public static abstract ref readonly float_mt16 Get_Depth(in T input);
    //
    // public static abstract ref readonly uint_mt16 Get_Stencil(in T input);
    //
    // public static abstract bool HasDepth { get; }
    // public static abstract bool HasStencil { get; }
}
