namespace Coplt.SoftGraphics;

public record struct SoftViewport(float TopLeftX, float TopLeftY, float Width, float Height, float MinDepth, float MaxDepth)
{
    public float TopLeftX = TopLeftX;
    public float TopLeftY = TopLeftY;
    public float Width = Width;
    public float Height = Height;
    public float MinDepth = MinDepth;
    public float MaxDepth = MaxDepth;
}
