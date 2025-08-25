using Coplt.Dropping;
using Coplt.UI.Rendering.Gpu.Graphics;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Coplt.UI.Rendering.Gpu.D3d12;

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

    #region Backend

    public override bool BindLess => true;
    public override uint MaxNumImagesInBatch => uint.MaxValue;

    #endregion

    #region AllocBuffer

    public override GpuBuffer AllocBuffer(uint Size)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region DrawBatch

    public override void DrawBatch(uint NumBatches, GpuBuffer BatchBuffer, ReadOnlySpan<GpuImage> Images)
    {
        throw new NotImplementedException();
    }

    #endregion
}
