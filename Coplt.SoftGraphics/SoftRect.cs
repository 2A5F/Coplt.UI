namespace Coplt.SoftGraphics;

public record struct SoftRect(uint Left, uint Top, uint Right, uint Bottom)
{
    public uint Left = Left;
    public uint Top = Top;
    public uint Right = Right;
    public uint Bottom = Bottom;

    public uint Width => Right - Left;
    public uint Height => Bottom - Top;
}
