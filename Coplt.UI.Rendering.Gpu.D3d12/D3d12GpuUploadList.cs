using Coplt.Dropping;
using Coplt.UI.Rendering.Gpu.D3d12.Utilities;
using Coplt.UI.Rendering.Gpu.Graphics;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Coplt.UI.Rendering.Gpu.D3d12;

[Dropping(Unmanaged = true)]
public sealed unsafe partial class D3d12GpuUploadList : GpuUploadList
{
    #region Fields

    public readonly D3d12GpuUploadListInner Inner;

    #endregion

    #region Props

    public ref readonly ComPtr<ID3D12Resource> Resource => ref Inner.Resource;
    public ref readonly ComPtr<ID3D12Resource2> Resource2 => ref Inner.Resource2;
    public override void* MappedPtr => Inner.MappedPtr;

    #endregion

    #region Ctor

    public D3d12GpuUploadList(GpuRendererBackendD3d12 Backend, uint Stride, uint Count)
    {
        Inner = new(Backend, Stride, Count);
    }

    #endregion

    #region MarkItemChanged

    public override void MarkItemChanged(uint index)
    {
        // nothing to do
        // todo upload buffer use copy
    }

    public override uint GpuDescId => Inner.SrvId.Id;

    #endregion
}

[Dropping(Unmanaged = true)]
public sealed unsafe partial class D3d12GpuUploadListInner
{
    #region Fields

    public readonly GpuRendererBackendD3d12 Backend;

    [Drop]
    internal ComPtr<ID3D12Resource> m_resource;
    [Drop]
    internal ComPtr<ID3D12Resource2> m_resource2;

    internal readonly void* m_mapped_ptr;

    private D3d12DescId m_srv_id;

    public ref readonly D3d12DescId SrvId => ref m_srv_id;

    #endregion

    #region Props

    public uint Stride { get; }
    public uint Count { get; }
    public ref readonly ComPtr<ID3D12Resource> Resource => ref m_resource;
    public ref readonly ComPtr<ID3D12Resource2> Resource2 => ref m_resource2;
    public ref readonly void* MappedPtr => ref m_mapped_ptr;

    #endregion

    #region Ctor

    public D3d12GpuUploadListInner(GpuRendererBackendD3d12 Backend, uint Stride, uint Count)
    {
        this.Backend = Backend;
        this.Stride = Stride;
        this.Count = Count;
        if (Backend.EnhancedBarriersSupported)
        {
            Backend.m_device10.Handle->CreateCommittedResource3(
                new HeapProperties
                {
                    Type = Backend.GPUUploadHeapSupported ? HeapType.GpuUpload : HeapType.Upload,
                },
                HeapFlags.None,
                new ResourceDesc1
                {
                    Dimension = ResourceDimension.Buffer,
                    Width = (uint)(Stride * Count),
                    Height = 1,
                    DepthOrArraySize = 1,
                    MipLevels = 1,
                    SampleDesc = new(1, 0),
                    Layout = TextureLayout.LayoutRowMajor,
                },
                BarrierLayout.Undefined,
                null, default(ComPtr<ID3D12ProtectedResourceSession>),
                0, null,
                out m_resource2
            ).TryThrowHResult();
            m_resource2.Handle->QueryInterface(out m_resource).TryThrowHResult();
        }
        else
        {
            Backend.m_device.Handle->CreateCommittedResource(
                new HeapProperties
                {
                    Type = Backend.GPUUploadHeapSupported ? HeapType.GpuUpload : HeapType.Upload,
                },
                HeapFlags.None,
                new ResourceDesc
                {
                    Dimension = ResourceDimension.Buffer,
                    Width = (uint)(Stride * Count),
                    Height = 1,
                    DepthOrArraySize = 1,
                    MipLevels = 1,
                    SampleDesc = new(1, 0),
                    Layout = TextureLayout.LayoutRowMajor,
                },
                ResourceStates.Common,
                null,
                out m_resource
            ).TryThrowHResult();
            m_resource.Handle->QueryInterface(out m_resource2);
        }
        m_resource.Handle->Map(0, null, ref m_mapped_ptr).TryThrowHResult();

        m_srv_id = Backend.CreateSrv(m_resource, new ShaderResourceViewDesc
        {
            Format = Format.FormatUnknown,
            ViewDimension = SrvDimension.Buffer,
            Shader4ComponentMapping = 0x1688,
            Buffer = new BufferSrv
            {
                FirstElement = 0,
                NumElements = Count,
                StructureByteStride = Stride,
                Flags = BufferSrvFlags.None,
            },
        });
    }

    #endregion

    #region Drop

    [Drop]
    private void Drop()
    {
        if (m_srv_id.Id == 0) return;
        Backend.ReturnDescId(m_srv_id);
        m_srv_id = default;
    }

    #endregion
}
