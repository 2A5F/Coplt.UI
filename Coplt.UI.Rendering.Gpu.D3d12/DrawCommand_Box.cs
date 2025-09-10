using Silk.NET.Direct3D12;

namespace Coplt.UI.Rendering.Gpu.D3d12;

public struct DrawCommand_Box
{
    public ConstantBufferViewDesc ViewData;
    public ulong Batches;
    public DrawArguments Draw;
}
