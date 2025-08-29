using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Dropping;
using Coplt.UI.Rendering.Gpu.D3d12.Utilities;
using Coplt.UI.Rendering.Gpu.Graphics;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using Silk.NET.Maths;
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

    [Drop(Order = 0)]
    public D3d12GpuContext? Context { get; }

    public ID3d12RecyclablePoolSource RecyclablePoolSource { get; }

    [Drop(Order = 3)]
    internal ComPtr<ID3D12Device1> m_device;
    [Drop(Order = 3)]
    internal ComPtr<ID3D12Device10> m_device10;
    [Drop(Order = 2)]
    internal ComPtr<ID3D12CommandQueue> m_queue;
    [Drop(Order = 0)]
    internal ComPtr<ID3D12GraphicsCommandList> m_command_list;
    [Drop(Order = 0)]
    internal ComPtr<ID3D12GraphicsCommandList7> m_command_list7;

    [Drop(Order = 1)]
    internal ComPtr<ID3D12DescriptorHeap> m_res_heap;

    internal ConcurrentQueue<int> m_freed_res_id = new();
    // internal int m_res_id_inc; // todo

    internal readonly D3d12RecyclablePool<RecyclablePack> m_pack_pool;
    internal RecyclablePack m_pack;

    #endregion

    #region Query

    public bool UMA { get; }
    public bool CacheCoherentUMA { get; }

    public RenderPassTier RenderPassesTier { get; }

    public MeshShaderTier MeshShaderTier { get; }

    public bool EnhancedBarriersSupported { get; }

    public bool GPUUploadHeapSupported { get; }

    #endregion

    #region RecyclablePack

    internal sealed class RecyclablePack(D3d12RecyclablePool<RecyclablePack> Pool, GpuRendererBackendD3d12 Backend)
        : AD3d12Recyclable<RecyclablePack>(Pool)
    {
        public D3d12FrameUploadPool FrameUploadBuffer { get; } = new(Backend);

        protected override unsafe RecyclablePack Recycle()
        {
            FrameUploadBuffer.Reset();
            return this;
        }
    }

    #endregion

    #region BoxMesh

    [Drop(Order = 3)]
    internal D3d12GpuBuffer m_box_mesh;

    #endregion

    #region Shader

    public D3d12RootSignature RootSignature_Box { get; }
    public D3d12GraphicsPipeline Pipeline_Box_NoDepth { get; }
    public D3d12GraphicsPipeline Pipeline_Box_Depth { get; }

    #endregion

    #region Ctor

    /// <param name="Context">Context's dispose will be called</param>
    public GpuRendererBackendD3d12(D3d12GpuContext Context)
        : this(Context.m_device, Context.m_queue, Context.m_command_list, Context, Context.DebugEnabled)
    {
        this.Context = Context;
    }

    /// <summary>
    /// Will call AddRef
    /// </summary>
    public GpuRendererBackendD3d12(
        ComPtr<ID3D12Device1> Device,
        ComPtr<ID3D12CommandQueue> Queue,
        ComPtr<ID3D12GraphicsCommandList> List,
        ID3d12RecyclablePoolSource RecyclablePoolSource,
        bool DebugEnabled = false
    )
    {
        #region Init

        this.RecyclablePoolSource = RecyclablePoolSource;
        this.DebugEnabled = DebugEnabled;

        Device.AddRef();
        m_device = Device;

        Queue.AddRef();
        m_queue = Queue;

        List.AddRef();
        m_command_list = List;
        List.Handle->QueryInterface(out m_command_list7);

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

        #region Pools

        m_pack_pool = new(RecyclablePoolSource);
        m_pack = new(m_pack_pool, this);

        #endregion

        #region Mesh

        m_box_mesh = D3d12GpuBuffer.Upload(this, MemoryMarshal.AsBytes(D3d12Meshes.Box));

        if (!GPUUploadHeapSupported)
        {
            D3d12GpuBuffer.BatchBarrier_CopyDst_To_Mesh(this,
                m_box_mesh
            );
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
                        Num32BitValues = 20,
                    },
                },
            ]);

            var input_element = stackalloc InputElementDesc[2]
            {
                new()
                {
                    SemanticName = (byte*)Unsafe.AsPointer(in "Position"u8[0]),
                    SemanticIndex = 0,
                    Format = Format.FormatR32G32Float,
                    InputSlot = 0,
                    AlignedByteOffset = uint.MaxValue,
                    InputSlotClass = InputClassification.PerVertexData,
                    InstanceDataStepRate = 0,
                },
                new()
                {
                    SemanticName = (byte*)Unsafe.AsPointer(in "UV"u8[0]),
                    SemanticIndex = 0,
                    Format = Format.FormatR32G32Float,
                    InputSlot = 0,
                    AlignedByteOffset = uint.MaxValue,
                    InputSlotClass = InputClassification.PerVertexData,
                    InstanceDataStepRate = 0,
                },
            };
            var input_layout = new InputLayoutDesc
            {
                PInputElementDescs = input_element,
                NumElements = 2,
            };

            Pipeline_Box_NoDepth = new D3d12GraphicsPipeline(
                this, RootSignature_Box,
                box_vertex, box_pixel,
                Format.FormatR8G8B8A8Unorm,
                Format.FormatUnknown,
                new()
                {
                    BlendEnable = true,
                },
                input_layout
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
                input_layout
            );
        }

        #endregion
    }

    #endregion

    #region Backend

    public override bool BindLess => true;
    public override uint MaxNumImagesInBatch => uint.MaxValue;

    public bool DebugEnabled { get; }

    #endregion

    #region Buffer

    public override GpuUploadList AllocUploadList(int Stride, int Count) =>
        new D3d12GpuUploadList(this, Stride, Count);

    #endregion

    #region Frame

    public override void BeginFrame()
    {
        m_pack = m_pack_pool.Rent() ?? new(m_pack_pool, this);
        m_command_list.Handle->SetDescriptorHeaps(1, m_res_heap);
    }

    public override void EndFrame()
    {
        m_pack_pool.Return(m_pack);
        m_pack = null!;
    }

    #endregion

    #region SetViewPort

    private Viewport m_cur_viewport;

    public override void SetViewPort(uint Left, uint Top, uint Width, uint Height)
    {
        m_command_list.Handle->RSSetViewports(1, m_cur_viewport = new Viewport(Left, Top, Width, Height, 0, 1));
        m_command_list.Handle->RSSetScissorRects(1, new Box2D<int>((int)Left, (int)Top, (int)(Width + Left), (int)(Height + Top)));
    }

    #endregion

    #region Draw

    public override void DrawBox()
    {
        m_command_list.Handle->SetGraphicsRootSignature(RootSignature_Box.m_root_signature);
        m_command_list.Handle->SetPipelineState(Pipeline_Box_NoDepth.m_pipeline);
        // m_command_list.Handle->Set

        // m_command_list.Handle
    }

    #endregion
}
