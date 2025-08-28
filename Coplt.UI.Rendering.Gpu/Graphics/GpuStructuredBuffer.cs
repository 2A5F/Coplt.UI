using Coplt.Dropping;

namespace Coplt.UI.Rendering.Gpu.Graphics;

[Dropping]
public abstract partial class GpuStructuredBuffer
{
    public int Stride { get; protected set; }
    public int Count { get; protected set; }

    public abstract unsafe void* MappedPtr { get; }

    public abstract void MarkItemChanged(int index);
}
