using Coplt.Dropping;
using Coplt.Mathematics;

namespace Coplt.UI.Rendering.Gpu.Graphics;

[Dropping]
public abstract partial class GpuRendererBackend
{
    public Color? ClearBackgroundColor { get; set; }

    public abstract bool BindLess { get; }
    public abstract uint MaxNumImagesInBatch { get; }

    public abstract GpuUploadList AllocUploadList(uint Stride, uint Count);

    public abstract void BeginFrame();

    public abstract void EndFrame();

    public abstract void ClearBackground(Color color);

    public abstract void SetViewPort(uint Left, uint Top, uint Width, uint Height, float MaxZ);

    public abstract void DrawBox(ReadOnlySpan<BatchData> Batches);
}
