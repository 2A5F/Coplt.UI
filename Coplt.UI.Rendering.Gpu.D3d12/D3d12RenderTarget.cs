using Coplt.Dropping;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Coplt.UI.Rendering.Gpu.D3d12;

[Dropping]
public abstract partial class D3d12RenderTarget
{
    public abstract CpuDescriptorHandle Rtv { get; }

    public abstract Format Format { get; }
}
