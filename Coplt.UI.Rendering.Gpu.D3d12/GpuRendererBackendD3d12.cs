using System.Collections.Concurrent;
using Coplt.Dropping;
using Coplt.UI.Rendering.Gpu.D3d12.Utilities;
using Coplt.UI.Rendering.Gpu.Graphics;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using Feature = Silk.NET.Direct3D12.Feature;

namespace Coplt.UI.Rendering.Gpu.D3d12;

[Dropping(Unmanaged = true)]
public unsafe partial class GpuRendererBackendD3d12 : GpuRendererBackend
{
    #region Silk

#pragma warning disable CS0618
    public D3D12 D3d12 { get; } = D3D12.GetApi();
#pragma warning restore CS0618

    #endregion

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

    [Drop]
    internal ComPtr<ID3D12DescriptorHeap> m_res_heap;

    internal ConcurrentQueue<int> m_freed_res_id = new();
    // internal int m_res_id_inc; // todo

    #endregion

    #region Query

    public bool UMA { get; }
    public bool CacheCoherentUMA { get; }

    public RenderPassTier RenderPassesTier { get; }

    public MeshShaderTier MeshShaderTier { get; }

    public bool EnhancedBarriersSupported { get; }

    public bool GPUUploadHeapSupported { get; }

    #endregion

    #region Shader

    public D3d12RootSignature RootSignature_Box { get; }
    public D3d12GraphicsPipeline Pipeline_Box_NoDepth { get; }
    public D3d12GraphicsPipeline Pipeline_Box_Depth { get; }

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
        #region Init

        m_device = Device;
        m_queue = Queue;
        m_command_list = List;

        m_device.AddRef();
        m_queue.AddRef();
        m_command_list.AddRef();

        m_device.Handle->QueryInterface(out m_device10);

        m_device.Handle->CreateDescriptorHeap(new DescriptorHeapDesc
        {
            Type = DescriptorHeapType.CbvSrvUav,
            NumDescriptors = 1_000_000,
            Flags = DescriptorHeapFlags.ShaderVisible,
        }, out m_res_heap).TryThrowHResult();

        #endregion

        #region Query

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

        #endregion

        #region Shader

        var asm = typeof(GpuRendererBackendD3d12).Assembly;

        {
            var box_vertex = asm.GetManifestResourceSpan(".shaders/Box.Vertex.dxil");
            var box_pixel = asm.GetManifestResourceSpan(".shaders/Box.Pixel.dxil");

            RootSignature_Box = new D3d12RootSignature(this, [
                // ViewData
                new RootParameter
                {
                    ParameterType = RootParameterType.Type32BitConstants,
                    ShaderVisibility = ShaderVisibility.All,
                    Constants = new RootConstants
                    {
                        ShaderRegister = 0,
                        RegisterSpace = 0,
                        Num32BitValues = 5,
                    },
                },
                // BoxDatas
                new RootParameter
                {
                    ParameterType = RootParameterType.TypeSrv,
                    ShaderVisibility = ShaderVisibility.All,
                    Descriptor = new()
                    {
                        ShaderRegister = 0,
                        RegisterSpace = 0,
                    },
                }
            ]);

            Pipeline_Box_NoDepth = new D3d12GraphicsPipeline(
                this, RootSignature_Box,
                box_vertex, box_pixel,
                Format.FormatR8G8B8A8Unorm,
                Format.FormatUnknown,
                new()
                {
                    BlendEnable = true,
                },
                new()
                {
                    PInputElementDescs = null,
                    NumElements = 0,
                }
            );

            Pipeline_Box_Depth = new D3d12GraphicsPipeline(
                this, RootSignature_Box,
                box_vertex, box_pixel,
                Format.FormatR8G8B8A8Unorm,
                Format.FormatD24UnormS8Uint,
                new()
                {
                    BlendEnable = true,
                    DepthEnable = true,
                    DepthWrite = true,
                },
                new()
                {
                    PInputElementDescs = null,
                    NumElements = 0,
                }
            );
        }

        #endregion
    }

    #endregion

    #region Backend

    public override bool BindLess => true;
    public override uint MaxNumImagesInBatch => uint.MaxValue;

    #endregion

    #region Buffer

    public override GpuStructuredBuffer AllocStructuredBuffer(int Stride, int Count) =>
        new D3d12GpuStructuredBuffer(this, Stride, Count);

    #endregion

    #region DrawBatch

    public override void DrawBatch(uint NumBatches, GpuStructuredBuffer BatchBuffer, ReadOnlySpan<GpuImage> Images)
    {
        throw new NotImplementedException();
    }

    #endregion
}
