namespace Coplt.UI.Rendering.Gpu.Graphics;

public abstract class GpuStructuredBuffer
{
    public int Stride { get; protected set; }
    public int Count { get; protected set; }
}
