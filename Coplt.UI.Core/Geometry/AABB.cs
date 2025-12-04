namespace Coplt.UI.Core.Geometry;

public record struct AABB2DU
{
    public uint MinX;
    public uint MinY;
    public uint MaxX;
    public uint MaxY;
}

public record struct AABB2DI
{
    public int MinX;
    public int MinY;
    public int MaxX;
    public int MaxY;
}

public record struct AABB2DF
{
    public float MinX;
    public float MinY;
    public float MaxX;
    public float MaxY;
}
