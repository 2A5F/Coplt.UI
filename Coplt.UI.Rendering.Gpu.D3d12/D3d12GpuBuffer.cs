using Coplt.Dropping;
using Coplt.UI.Rendering.Gpu.D3d12.Utilities;
using Coplt.UI.Rendering.Gpu.Graphics;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Coplt.UI.Rendering.Gpu.D3d12;

[Dropping(Unmanaged = true)]
public sealed unsafe partial class D3d12GpuBuffer : GpuBuffer
{
    #region Fields

    public readonly GpuRendererBackendD3d12 Backend;

    [Drop]
    internal ComPtr<ID3D12Resource> m_resource;
    [Drop]
    internal ComPtr<ID3D12Resource2> m_resource2;

    internal readonly void* m_mapped_ptr;

    #endregion

    #region Props

    public ref readonly ComPtr<ID3D12Resource> Resource => ref m_resource;
    public ref readonly ComPtr<ID3D12Resource2> Resource2 => ref m_resource2;
    public ref readonly void* MappedPtr => ref m_mapped_ptr;

    #endregion

    #region Ctor

    public D3d12GpuBuffer(GpuRendererBackendD3d12 Backend, ulong Size)
    {
        this.Backend = Backend;
        if (Backend.EnhancedBarriersSupported)
        {
            Backend.m_device10.Handle->CreateCommittedResource3(
                new HeapProperties
                {
                    Type = Backend.GPUUploadHeapSupported ? HeapType.GpuUpload : HeapType.Upload,
                },
                HeapFlags.AllowOnlyBuffers,
                new ResourceDesc1
                {
                    Dimension = ResourceDimension.Buffer,
                    Width = Size,
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
                }, HeapFlags.AllowOnlyBuffers, new ResourceDesc
                {
                    Dimension = ResourceDimension.Buffer,
                    Width = Size,
                }, ResourceStates.Common, null,
                out m_resource
            ).TryThrowHResult();
            m_resource.Handle->QueryInterface(out m_resource2);
        }
        m_resource.Handle->Map(0, null, ref m_mapped_ptr).TryThrowHResult();
    }

    #endregion
}
