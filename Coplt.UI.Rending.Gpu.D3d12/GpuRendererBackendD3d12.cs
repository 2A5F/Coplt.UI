using Coplt.Dropping;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Coplt.UI.Rending.Gpu.D3d12;

[Dropping(Unmanaged = true)]
public partial class GpuRendererBackendD3d12 : GpuRendererBackend
{
    #region Fields

    [Drop]
    internal readonly D3d12GpuContext? m_context;
    [Drop]
    internal ComPtr<ID3D12Device1> m_device;
    [Drop]
    internal ComPtr<ID3D12CommandQueue> m_queue;
    [Drop]
    internal ComPtr<ID3D12GraphicsCommandList> m_command_list;

    #endregion

    #region Ctor

    public GpuRendererBackendD3d12(D3d12GpuContext Context)
        : this(Context.m_device, Context.m_queue, Context.m_command_list)
    {
        m_context = Context;
    }

    /// <summary>
    /// Will call AddRef
    /// </summary>
    public GpuRendererBackendD3d12(
        ComPtr<ID3D12Device1> Device,
        ComPtr<ID3D12CommandQueue> Queue,
        ComPtr<ID3D12GraphicsCommandList> List
    )
    {
        m_device = Device;
        m_queue = Queue;
        m_command_list = List;

        m_device.AddRef();
        m_queue.AddRef();
        m_command_list.AddRef();
    }

    #endregion
}
