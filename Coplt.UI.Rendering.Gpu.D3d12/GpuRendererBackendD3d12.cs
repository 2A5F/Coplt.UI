using Coplt.Dropping;
using Coplt.UI.Rendering.Gpu.D3d12.Utilities;
using Coplt.UI.Rendering.Gpu.Graphics;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Coplt.UI.Rendering.Gpu.D3d12;

[Dropping(Unmanaged = true)]
public unsafe partial class GpuRendererBackendD3d12 : GpuRendererBackend
{
    #region Fields

    public D3d12GpuContext? Context { get; }

    [Drop]
    internal ComPtr<ID3D12Device1> m_device;
    [Drop]
    internal ComPtr<ID3D12Device10> m_device10;
    [Drop]
    internal ComPtr<ID3D12CommandQueue> m_queue;
    [Drop]
    internal ComPtr<ID3D12GraphicsCommandList> m_command_list;

    public bool UMA { get; }
    public bool CacheCoherentUMA { get; }

    public RenderPassTier RenderPassesTier { get; }

    public MeshShaderTier MeshShaderTier { get; }

    public bool EnhancedBarriersSupported { get; }

    public bool GPUUploadHeapSupported { get; }

    #endregion

    #region Ctor

    public GpuRendererBackendD3d12(D3d12GpuContext Context)
        : this(Context.m_device, Context.m_queue, Context.m_command_list)
    {
        this.Context = Context;
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

        m_device.Handle->QueryInterface(out m_device10);

        FeatureDataArchitecture architecture;
        if (m_device.Handle->CheckFeatureSupport(Feature.Architecture, &architecture, (uint)sizeof(FeatureDataArchitecture))
            .AsHResult().IsSuccess)
        {
            UMA = architecture.UMA;
            CacheCoherentUMA = architecture.CacheCoherentUMA;
        }

        FeatureDataD3D12Options5 options5;
        if (m_device.Handle->CheckFeatureSupport(Feature.D3D12Options5, &options5, (uint)sizeof(FeatureDataD3D12Options5))
            .AsHResult().IsSuccess)
        {
            RenderPassesTier = options5.RenderPassesTier;
        }

        FeatureDataD3D12Options7 options7;
        if (m_device.Handle->CheckFeatureSupport(Feature.D3D12Options5, &options7, (uint)sizeof(FeatureDataD3D12Options7))
            .AsHResult().IsSuccess)
        {
            MeshShaderTier = options7.MeshShaderTier;
        }

        FeatureDataD3D12Options12 options12;
        if (m_device.Handle->CheckFeatureSupport(Feature.D3D12Options16, &options12, (uint)sizeof(FeatureDataD3D12Options12))
            .AsHResult().IsSuccess)
        {
            EnhancedBarriersSupported = options12.EnhancedBarriersSupported;
        }

        FeatureDataD3D12Options16 options16;
        if (m_device.Handle->CheckFeatureSupport(Feature.D3D12Options16, &options16, (uint)sizeof(FeatureDataD3D12Options16))
            .AsHResult().IsSuccess)
        {
            GPUUploadHeapSupported = options16.GPUUploadHeapSupported;
        }
    }

    #endregion

    #region Backend

    public override bool BindLess => true;
    public override uint MaxNumImagesInBatch => uint.MaxValue;

    #endregion

    #region Buffer Pool

    public override GpuBuffer RentBuffer(uint Size)
    {
        throw new NotImplementedException();
    }

    public override void ReturnBuffer(GpuBuffer Buffer)
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
