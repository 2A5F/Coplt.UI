namespace Coplt.SoftGraphics;

/// <summary>
/// 4 pixels per operation
/// </summary>
public delegate void SoftPixelShader<VertexData, PixelData>(
    SoftQuad<SoftLineContext> ctx, SoftQuad<VertexData> input, SoftQuad<PixelData> output
)
    where VertexData : unmanaged, IVertexData<VertexData>
    where PixelData : unmanaged, IPixelData<PixelData>;

/// <summary>
/// 4 pixels per operation
/// </summary>
public ref struct SoftQuad<T>
{
    /// <summary>
    /// <code>0, 0</code>
    /// <code>
    /// - -
    /// X -
    /// </code>
    /// </summary>
    public ref T A;
    /// <summary>
    /// <code>1, 0</code>
    /// <code>
    /// - -
    /// - X
    /// </code>
    /// </summary>
    public ref T B;
    /// <summary>
    /// <code>0, 1</code>
    /// <code>
    /// X -
    /// - -
    /// </code>
    /// </summary>
    public ref T C;
    /// <summary>
    /// <code>1, 1</code>
    /// <code>
    /// - X
    /// - -
    /// </code>
    /// </summary>
    public ref T D;
}

/// <summary>
/// Wave line context
/// </summary>
public struct SoftLineContext
{
    /// <summary>
    /// Is this line active
    /// </summary>
    public bool Active;
}

public interface IVertexData<T> where T : unmanaged, IVertexData<T>
{
    public float Position_ClipSpace_X { get; }
    public float Position_ClipSpace_Y { get; }
    public float Position_ClipSpace_Z { get; }
    public float Position_ClipSpace_W { get; }

    public static abstract T Interpolation(in T a, in T b, float t);
}

public interface IPixelData<T> where T : unmanaged, IPixelData<T>
{
    public float Color0_R { get; }
    public float Color0_G { get; }
    public float Color0_B { get; }
    public float Color0_A { get; }
    
    public float Depth { get; }
    public byte Stencil { get; }
    
    public static abstract bool HasDepth { get; }
    public static abstract bool HasStencil { get; }
}
