using Coplt.Mathematics.Simt;

namespace Coplt.SoftGraphics;

// /// <summary>
// /// 4 pixels per operation
// /// </summary>
// public delegate void SoftPixelShader<in VertexData, in PixelData>(
//     SoftLaneContext ctx, VertexData input, PixelData output
// )
//     where VertexData : IVertexData, allows ref struct
//     where PixelData : IPixelData, allows ref struct;

/// <summary>
/// Wave line context
/// </summary>
public struct SoftLaneContext
{
    public int_mt Index;
    public b32_mt ActiveLanes;
    public uint_mt QuadMask;
}

// public interface IVertexData
// {
//     public float4_mt Gather_Position_ClipSpace(
//         int_mt index, b32_mt active_lanes
//     );
//
//     // public static abstract void Interpolation(out T r, in T a, in T b, float_mt t);
// }

public interface IPixelData
{
    // public static abstract ref readonly float4_mt Get_Color0(in T input);
    //
    // public static abstract ref readonly float_mt Get_Depth(in T input);
    //
    // public static abstract ref readonly uint_mt Get_Stencil(in T input);
    //
    // public static abstract bool HasDepth { get; }
    // public static abstract bool HasStencil { get; }
}
