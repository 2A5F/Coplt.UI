namespace Coplt.SoftGraphics;

/// <summary>
/// An immediate context, operations are synchronously blocked
/// </summary>
public sealed class SoftGraphicsContext
{
    public void SetRenderTarget(SoftTexture Color, SoftTexture? DepthStencil)
    {
        // todo
    }

    /// <summary>
    /// Dispatching pixel shader<br/>
    /// <b>Not an async operation, will block until complete</b>
    /// </summary>
    public void Draw<VertexData, PixelData>(
        SoftPrimitiveType Primitive,
        ReadOnlySpan<uint> Indices,
        ReadOnlySpan<VertexData> Vertices,
        SoftPixelShader<VertexData, PixelData> PixelShader
    )
        where VertexData : unmanaged, IVertexData<VertexData>
        where PixelData : unmanaged, IPixelData<PixelData>
    {
        // todo
    }
}
