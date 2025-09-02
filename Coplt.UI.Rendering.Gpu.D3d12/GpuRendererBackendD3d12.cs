using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Dropping;
using Coplt.Mathematics;
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

    [Drop(Order = -1)]
    public D3d12RenderTarget RenderTarget { get; }

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
    internal GpuDescriptorHandle m_res_heap_start_G;
    internal CpuDescriptorHandle m_res_heap_start_C;
    internal uint m_res_heap_stride;

    internal ConcurrentQueue<D3d12DescId> m_freed_res_id = new();
    internal uint m_res_id_inc; // never zero

    [Drop]
    internal readonly D3d12RecyclablePool<RecyclablePack> m_pack_pool;
    [Drop]
    internal RecyclablePack? m_pack;

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

    [Dropping(Unmanaged = true)]
    internal sealed partial class RecyclablePack(D3d12RecyclablePool<RecyclablePack> Pool, GpuRendererBackendD3d12 Backend)
        : AD3d12Recyclable<RecyclablePack>(Pool)
    {
        [Drop]
        public D3d12FrameUploadPool FrameUploadBuffer { get; } = new(Backend);
        [Drop]
        public D3d12GpuRenderTarget? MsaaRt { get; set; }

        protected override RecyclablePack Recycle()
        {
            FrameUploadBuffer.Reset();
            return this;
        }

        public D3d12GpuRenderTarget GetMsaaRt()
        {
            var rt = Backend.RenderTarget;
            var width = rt.Width;
            var height = rt.Height;
            if (MsaaRt == null) goto Create;
            if (
                MsaaRt.Width != width
                || MsaaRt.Height != height
                || MsaaRt.ClearBackgroundColor.HasValue != Backend.ClearBackgroundColor.HasValue
                || !MsaaRt.ClearBackgroundColor.GetValueOrDefault().rgba.Equals(Backend.ClearBackgroundColor.GetValueOrDefault().rgba)
            )
            {
                MsaaRt.Dispose();
                goto Create;
            }
            return MsaaRt;
            Create:
            return MsaaRt = new D3d12GpuRenderTarget(Backend, rt.Format, width, height, 4, 0, Backend.ClearBackgroundColor);
        }
    }

    #endregion

    #region BoxMesh

    [Drop(Order = 3)]
    internal D3d12GpuBuffer m_mesh_box;

    #endregion

    #region Shader

    public D3d12RootSignature RootSignature_Box { get; }
    public D3d12GraphicsPipeline Pipeline_Box_NoDepth { get; }
    public D3d12GraphicsPipeline Pipeline_Box_Depth { get; }

    #endregion

    #region Ctor

    /// <param name="Context">Context's dispose will be called</param>
    /// <param name="RenderTarget">RenderTarget's dispose will be called</param>
    public GpuRendererBackendD3d12(D3d12GpuContext Context, D3d12RenderTarget RenderTarget)
        : this(Context.m_device, Context.m_queue, Context.m_command_list, Context, RenderTarget, Context.DebugEnabled)
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
        D3d12RenderTarget RenderTarget,
        bool DebugEnabled = false
    )
    {
        #region Init

        this.RecyclablePoolSource = RecyclablePoolSource;
        this.RenderTarget = RenderTarget;
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
        m_res_heap_start_G = m_res_heap.Handle->GetGPUDescriptorHandleForHeapStart();
        m_res_heap_start_C = m_res_heap.Handle->GetCPUDescriptorHandleForHeapStart();
        m_res_heap_stride = m_device.Handle->GetDescriptorHandleIncrementSize(DescriptorHeapType.CbvSrvUav);

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

        m_mesh_box = D3d12GpuBuffer.Upload(this, MemoryMarshal.AsBytes(Meshes.Box.Vertices));

        if (!GPUUploadHeapSupported)
        {
            D3d12GpuBuffer.BatchBarrier_CopyDst_To_Mesh(this,
                m_mesh_box
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
                    ParameterType = RootParameterType.TypeCbv,
                    ShaderVisibility = ShaderVisibility.All,
                    Descriptor = new()
                    {
                        ShaderRegister = 0,
                        RegisterSpace = 0,
                    },
                },
                // Batches
                new RootParameter
                {
                    ParameterType = RootParameterType.TypeSrv,
                    ShaderVisibility = ShaderVisibility.All,
                    Descriptor = new()
                    {
                        ShaderRegister = 0,
                        RegisterSpace = 10,
                    },
                },
            ]);

            var input_element = stackalloc InputElementDesc[3]
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
                new()
                {
                    SemanticName = (byte*)Unsafe.AsPointer(in "BorderColor"u8[0]),
                    SemanticIndex = 0,
                    Format = Format.FormatR32Uint,
                    InputSlot = 0,
                    AlignedByteOffset = uint.MaxValue,
                    InputSlotClass = InputClassification.PerVertexData,
                    InstanceDataStepRate = 0,
                },
            };
            var input_layout = new InputLayoutDesc
            {
                PInputElementDescs = input_element,
                NumElements = 3,
            };

            Pipeline_Box_NoDepth = new D3d12GraphicsPipeline(
                this, RootSignature_Box,
                box_vertex, box_pixel,
                RenderTarget.Format,
                Format.FormatUnknown,
                new()
                {
                    MultisampleEnable = true,
                    SampleDesc = new(4, 0),
                    BlendEnable = true,
                },
                input_layout
            );

            Pipeline_Box_Depth = new D3d12GraphicsPipeline(
                this, RootSignature_Box,
                box_vertex, box_pixel,
                RenderTarget.Format,
                Format.FormatD24UnormS8Uint,
                new()
                {
                    MultisampleEnable = true,
                    SampleDesc = new(4, 0),
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

    #region DescHeap

    private D3d12DescId RentDescId()
    {
        if (m_freed_res_id.TryDequeue(out var id)) return id;
        return new(Interlocked.Increment(ref m_res_id_inc)); // never zero
    }

    public void ReturnDescId(D3d12DescId Id)
    {
        m_freed_res_id.Enqueue(Id);
    }

    public D3d12DescId CreateSrv(ID3D12Resource* Resource, in ShaderResourceViewDesc Desc)
    {
        var id = RentDescId();
        m_device.Handle->CreateShaderResourceView(Resource, Desc, new(m_res_heap_start_C.Ptr + id.Id * m_res_heap_stride));
        return id;
    }

    #endregion

    #region Backend

    public override bool BindLess => true;
    public override uint MaxNumImagesInBatch => uint.MaxValue;

    public bool DebugEnabled { get; }

    #endregion

    #region Buffer

    public override GpuUploadList AllocUploadList(uint Stride, uint Count) =>
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
        Debug.Assert(m_pack != null);

        if (m_pack.MsaaRt != null)
        {
            var rt = m_pack.MsaaRt;
            var dst = RenderTarget;

            #region Barrier Before Resolve

            {
                if (EnhancedBarriersSupported)
                {
                    var barriers = stackalloc TextureBarrier[2]
                    {
                        new()
                        {
                            SyncBefore = BarrierSync.None,
                            SyncAfter = BarrierSync.Resolve,
                            AccessBefore = BarrierAccess.NoAccess,
                            AccessAfter = BarrierAccess.ResolveDest,
                            LayoutBefore = BarrierLayout.Present,
                            LayoutAfter = BarrierLayout.ResolveDest,
                            PResource = dst.Resource,
                            Subresources = new()
                            {
                                IndexOrFirstMipLevel = 0,
                                NumMipLevels = 1,
                                FirstArraySlice = 0,
                                NumArraySlices = 1,
                                FirstPlane = 0,
                                NumPlanes = 1,
                            },
                            Flags = TextureBarrierFlags.None,
                        },
                        new()
                        {
                            SyncBefore = BarrierSync.RenderTarget,
                            SyncAfter = BarrierSync.Resolve,
                            AccessBefore = BarrierAccess.RenderTarget,
                            AccessAfter = BarrierAccess.ResolveSource,
                            LayoutBefore = BarrierLayout.RenderTarget,
                            LayoutAfter = BarrierLayout.ResolveSource,
                            PResource = rt.Resource,
                            Subresources = new()
                            {
                                IndexOrFirstMipLevel = 0,
                                NumMipLevels = 1,
                                FirstArraySlice = 0,
                                NumArraySlices = 1,
                                FirstPlane = 0,
                                NumPlanes = 1,
                            },
                            Flags = TextureBarrierFlags.None,
                        },
                    };
                    m_command_list7.Handle->Barrier(1, new BarrierGroup
                    {
                        Type = BarrierType.Texture,
                        NumBarriers = 2,
                        PTextureBarriers = barriers,
                    });
                }
                else
                {
                    var barriers = stackalloc ResourceBarrier[2]
                    {
                        new()
                        {
                            Type = ResourceBarrierType.Transition,
                            Flags = ResourceBarrierFlags.None,
                            Transition = new()
                            {
                                PResource = dst.Resource,
                                Subresource = 0,
                                StateBefore = ResourceStates.Present,
                                StateAfter = ResourceStates.ResolveDest,
                            },
                        },
                        new()
                        {
                            Type = ResourceBarrierType.Transition,
                            Flags = ResourceBarrierFlags.None,
                            Transition = new()
                            {
                                PResource = rt.Resource,
                                Subresource = 0,
                                StateBefore = ResourceStates.RenderTarget,
                                StateAfter = ResourceStates.ResolveSource,
                            },
                        }
                    };
                    m_command_list.Handle->ResourceBarrier(2, barriers);
                }
            }

            #endregion

            m_command_list.Handle->ResolveSubresource(dst.Resource, 0, rt.Resource, 0, dst.Format);

            #region Barrier After Resolve

            {
                if (EnhancedBarriersSupported)
                {
                    var barriers = stackalloc TextureBarrier[2]
                    {
                        new()
                        {
                            SyncBefore = BarrierSync.Resolve,
                            SyncAfter = BarrierSync.None,
                            AccessBefore = BarrierAccess.ResolveDest,
                            AccessAfter = BarrierAccess.NoAccess,
                            LayoutBefore = BarrierLayout.ResolveDest,
                            LayoutAfter = BarrierLayout.Present,
                            PResource = dst.Resource,
                            Subresources = new()
                            {
                                IndexOrFirstMipLevel = 0,
                                NumMipLevels = 1,
                                FirstArraySlice = 0,
                                NumArraySlices = 1,
                                FirstPlane = 0,
                                NumPlanes = 1,
                            },
                            Flags = TextureBarrierFlags.None,
                        },
                        new()
                        {
                            SyncBefore = BarrierSync.Resolve,
                            SyncAfter = BarrierSync.None,
                            AccessBefore = BarrierAccess.ResolveSource,
                            AccessAfter = BarrierAccess.NoAccess,
                            LayoutBefore = BarrierLayout.ResolveSource,
                            LayoutAfter = BarrierLayout.RenderTarget,
                            PResource = rt.Resource,
                            Subresources = new()
                            {
                                IndexOrFirstMipLevel = 0,
                                NumMipLevels = 1,
                                FirstArraySlice = 0,
                                NumArraySlices = 1,
                                FirstPlane = 0,
                                NumPlanes = 1,
                            },
                            Flags = TextureBarrierFlags.None,
                        },
                    };
                    m_command_list7.Handle->Barrier(1, new BarrierGroup
                    {
                        Type = BarrierType.Texture,
                        NumBarriers = 2,
                        PTextureBarriers = barriers,
                    });
                }
                else
                {
                    var barriers = stackalloc ResourceBarrier[2]
                    {
                        new()
                        {
                            Type = ResourceBarrierType.Transition,
                            Flags = ResourceBarrierFlags.None,
                            Transition = new()
                            {
                                PResource = dst.Resource,
                                Subresource = 0,
                                StateBefore = ResourceStates.ResolveDest,
                                StateAfter = ResourceStates.Present,
                            },
                        },
                        new()
                        {
                            Type = ResourceBarrierType.Transition,
                            Flags = ResourceBarrierFlags.None,
                            Transition = new()
                            {
                                PResource = rt.Resource,
                                Subresource = 0,
                                StateBefore = ResourceStates.ResolveSource,
                                StateAfter = ResourceStates.RenderTarget,
                            },
                        }
                    };
                    m_command_list.Handle->ResourceBarrier(2, barriers);
                }
            }

            #endregion
        }

        m_pack_pool.Return(m_pack);
        m_pack = null!;
    }

    #endregion

    #region ClearBackground

    public override void ClearBackground(Color color)
    {
        Debug.Assert(m_pack != null);
        var rt = m_pack.GetMsaaRt();
        m_command_list.Handle->ClearRenderTargetView(rt.Rtv, (float*)&color, 0, null);
    }

    #endregion

    #region SetViewPort

    private record struct ViewData
    {
        public float4 ViewSize;
        public float4x4 VP;
    }

    private UploadRange<ViewData> m_cur_view_data;

    public override void SetViewPort(uint Left, uint Top, uint Width, uint Height, float MaxZ)
    {
        Debug.Assert(m_pack != null);

        m_command_list.Handle->RSSetViewports(1, new Viewport(Left, Top + Height, Width, -Height, 0, 1));
        m_command_list.Handle->RSSetScissorRects(1, new Box2D<int>((int)Left, (int)Top, (int)(Width + Left), (int)(Height + Top)));
        var far = Math.Max(1, MaxZ + 1);
        var vp = float4x4.Ortho(Width, Height, far, float.Epsilon);
        vp = math.mul(vp, float4x4.Translate(new float3(-Width, -Height, 0) * 0.5f));
        m_cur_view_data = m_pack.FrameUploadBuffer.Alloc(new ViewData
        {
            ViewSize = new(Width, Height, 1f / Width, 1f / Height),
            VP = vp,
        });
    }

    #endregion

    #region Draw

    public override void DrawBox(ReadOnlySpan<BatchData> Batches)
    {
        Debug.Assert(m_pack != null);
        var rt = m_pack.GetMsaaRt();

        var batches = m_pack.FrameUploadBuffer.Alloc(Batches);

        m_command_list.Handle->SetGraphicsRootSignature(RootSignature_Box.m_root_signature);
        m_command_list.Handle->SetGraphicsRootDescriptorTable(0, m_res_heap_start_G);
        m_command_list.Handle->SetPipelineState(Pipeline_Box_NoDepth.m_pipeline);
        m_command_list.Handle->SetGraphicsRootConstantBufferView(1, m_cur_view_data.GpuVPtr);
        m_command_list.Handle->SetGraphicsRootShaderResourceView(2, batches.GpuVPtr);
        m_command_list.Handle->IASetPrimitiveTopology(D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist);
        m_command_list.Handle->OMSetRenderTargets(1, rt.Rtv, false, null);
        m_command_list.Handle->DrawInstanced(18, 1, 0, 0);
    }

    #endregion
}
