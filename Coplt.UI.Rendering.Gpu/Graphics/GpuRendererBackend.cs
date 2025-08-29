using Coplt.Dropping;
using Coplt.Mathematics;

namespace Coplt.UI.Rendering.Gpu.Graphics;

[Dropping]
public abstract partial class GpuRendererBackend
{
    public abstract bool BindLess { get; }
    public abstract uint MaxNumImagesInBatch { get; }

    public abstract GpuUploadList AllocUploadList(int Stride, int Count);

    public abstract void BeginFrame();

    public abstract void EndFrame();

    public abstract void SetViewPort(uint Left, uint Top, uint Width, uint Height);

    public abstract void DrawBox(
        in float4x4 VP
        // todo
    );
}
