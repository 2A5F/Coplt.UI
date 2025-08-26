using Coplt.Dropping;

namespace Coplt.UI.Rendering.Gpu.Graphics;

[Dropping]
public abstract partial class GpuRendererBackend
{
    public abstract bool BindLess { get; }
    public abstract uint MaxNumImagesInBatch { get; }

    public abstract GpuBuffer RentBuffer(uint Size);

    public abstract void ReturnBuffer(GpuBuffer Buffer);

    /// <param name="NumBatches"></param>
    /// <param name="BatchBuffer"></param>
    /// <param name="Images">Empty [] when <see cref="BindLess"/> is ture</param>
    public abstract void DrawBatch(
        uint NumBatches,
        GpuBuffer BatchBuffer,
        ReadOnlySpan<GpuImage> Images
    );
}
