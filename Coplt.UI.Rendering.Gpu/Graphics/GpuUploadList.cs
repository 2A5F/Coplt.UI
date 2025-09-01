using Coplt.Dropping;

namespace Coplt.UI.Rendering.Gpu.Graphics;

[Dropping]
public abstract partial class GpuUploadList
{
    public int Stride { get; protected set; }
    public int Count { get; protected set; }

    public abstract unsafe void* MappedPtr { get; }

    public abstract void MarkItemChanged(uint index);
    
    public abstract uint GpuDescId { get; }
}
