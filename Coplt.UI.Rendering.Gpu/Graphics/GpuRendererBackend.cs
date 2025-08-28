using Coplt.Dropping;

namespace Coplt.UI.Rendering.Gpu.Graphics;

[Dropping]
public abstract partial class GpuRendererBackend
{
    public abstract bool BindLess { get; }
    public abstract uint MaxNumImagesInBatch { get; }

    public abstract GpuStructuredBuffer AllocStructuredBuffer(int Stride, int Count);

    /// <param name="NumBatches"></param>
    /// <param name="BatchBuffer"></param>
    /// <param name="Images">Empty [] when <see cref="BindLess"/> is ture</param>
    public abstract void DrawBatch(
        uint NumBatches,
        GpuStructuredBuffer BatchBuffer,
        ReadOnlySpan<GpuImage> Images
    );
}
