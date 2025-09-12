using Coplt.Dropping;
using Coplt.UI.Rendering.Gpu.D3d12.Utilities;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Coplt.UI.Rendering.Gpu.D3d12;

[Dropping(Unmanaged = true)]
public sealed unsafe partial class D3d12GpuUploadBuffer
{
    #region Fields

    public readonly D3d12RendererBackend Backend;

    [Drop]
    internal ComPtr<ID3D12Resource> m_resource;
    [Drop]
    internal ComPtr<ID3D12Resource2> m_resource2;

    internal readonly void* m_mapped_ptr;

    internal readonly ulong m_gpu_v_ptr;

    #endregion

    #region Props

    public ref readonly ComPtr<ID3D12Resource> Resource => ref m_resource;
    public ref readonly ComPtr<ID3D12Resource2> Resource2 => ref m_resource2;
    public ref readonly void* MappedPtr => ref m_mapped_ptr;
    public ref readonly ulong GpuVPtr => ref m_gpu_v_ptr;

    public ulong Size { get; }

    #endregion

    #region Ctor

    public D3d12GpuUploadBuffer(D3d12RendererBackend Backend, ulong Size, bool AllowGpuUpload = true)
    {
        this.Backend = Backend;
        this.Size = Size;
        if (Backend.EnhancedBarriersSupported)
        {
            Backend.m_device10.Handle->CreateCommittedResource3(
                new HeapProperties
                {
                    Type = Backend.GPUUploadHeapSupported && AllowGpuUpload ? HeapType.GpuUpload : HeapType.Upload,
                },
                HeapFlags.None,
                new ResourceDesc1
                {
                    Dimension = ResourceDimension.Buffer,
                    Width = Size,
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
                    Type = Backend.GPUUploadHeapSupported && AllowGpuUpload ? HeapType.GpuUpload : HeapType.Upload,
                },
                HeapFlags.None,
                new ResourceDesc
                {
                    Dimension = ResourceDimension.Buffer,
                    Width = Size,
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
        m_gpu_v_ptr = m_resource.Handle->GetGPUVirtualAddress();
    }

    #endregion
}
