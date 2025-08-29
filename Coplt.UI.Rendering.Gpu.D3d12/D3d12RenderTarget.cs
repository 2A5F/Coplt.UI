using Coplt.Dropping;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Coplt.UI.Rendering.Gpu.D3d12;

[Dropping]
public abstract unsafe partial class D3d12RenderTarget
{
    public abstract CpuDescriptorHandle Rtv { get; }

    public abstract ref readonly ComPtr<ID3D12Resource> Resource { get; }

    public abstract Format Format { get; }

    public abstract uint Width { get; }
    public abstract uint Height { get; }
}
