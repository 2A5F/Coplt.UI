using Coplt.Dropping;
using Coplt.UI.Rendering.Gpu.D3d12.Utilities;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Coplt.UI.Rendering.Gpu.D3d12;

[Dropping(Unmanaged = true)]
public sealed unsafe partial class D3d12GpuBuffer
{
    #region Fields

    public readonly D3d12RendererBackend Backend;

    [Drop]
    internal ComPtr<ID3D12Resource> m_resource;
    [Drop]
    internal ComPtr<ID3D12Resource2> m_resource2;

    internal readonly ulong m_gpu_v_ptr;

    #endregion

    #region Props

    public ref readonly ComPtr<ID3D12Resource> Resource => ref m_resource;
    public ref readonly ComPtr<ID3D12Resource2> Resource2 => ref m_resource2;

    internal readonly void* m_mapped_ptr;

    public ref readonly ulong GpuVPtr => ref m_gpu_v_ptr;
    public ulong Size { get; }

    #endregion

    #region Ctor

    public D3d12GpuBuffer(D3d12RendererBackend Backend, ulong Size, bool UseGpuUpload = false)
    {
        this.Backend = Backend;
        this.Size = Size;
        this.Backend = Backend;
        this.Size = Size;
        if (Backend.EnhancedBarriersSupported)
        {
            Backend.m_device10.Handle->CreateCommittedResource3(
                new HeapProperties
                {
                    Type = Backend.GPUUploadHeapSupported && UseGpuUpload ? HeapType.GpuUpload : HeapType.Default,
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
                    Type = Backend.GPUUploadHeapSupported && UseGpuUpload ? HeapType.GpuUpload : HeapType.Default,
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
        if (Backend.GPUUploadHeapSupported && UseGpuUpload)
        {
            m_resource.Handle->Map(0, null, ref m_mapped_ptr).TryThrowHResult();
        }
        m_gpu_v_ptr = m_resource.Handle->GetGPUVirtualAddress();
    }

    #endregion

    #region Upload

    public static D3d12GpuBuffer Upload(D3d12RendererBackend Backend, ReadOnlySpan<byte> data)
    {
        if (Backend.GPUUploadHeapSupported)
        {
            D3d12GpuBuffer buffer = new(Backend, (uint)data.Length, UseGpuUpload: true);
            var target = new Span<byte>(buffer.m_mapped_ptr, data.Length);
            data.CopyTo(target);
            return buffer;
        }
        else
        {
            D3d12GpuBuffer buffer = new(Backend, (uint)data.Length, UseGpuUpload: false);
            var tmp = Backend.m_pack!.FrameUploadBuffer.Alloc(data, Align: 512);
            Backend.m_command_list.Handle->CopyBufferRegion(buffer.m_resource, 0, tmp.Resource, 0, (uint)data.Length);
            return buffer;
        }
    }

    #endregion

    #region Barrier

    public ResourceBarrier ResourceBarrier_CopyDst_To_Mesh() => new()
    {
        Type = ResourceBarrierType.Transition,
        Flags = ResourceBarrierFlags.None,
        Transition = new ResourceTransitionBarrier
        {
            PResource = m_resource,
            Subresource = 0,
            StateBefore = ResourceStates.CopyDest,
            StateAfter = ResourceStates.VertexAndConstantBuffer | ResourceStates.IndexBuffer,
        },
    };

    public BufferBarrier BufferBarrier_CopyDst_To_Mesh() => new()
    {
        SyncBefore = BarrierSync.Copy,
        SyncAfter = BarrierSync.VertexShading,
        AccessBefore = BarrierAccess.CopyDest,
        AccessAfter = BarrierAccess.VertexBuffer | BarrierAccess.IndexBuffer,
        PResource = m_resource,
        Offset = 0,
        Size = Size,
    };

    public static void BatchBarrier_CopyDst_To_Mesh(D3d12RendererBackend Backend, params ReadOnlySpan<D3d12GpuBuffer> buffers)
    {
        if (Backend.EnhancedBarriersSupported)
        {
            var barriers = stackalloc ResourceBarrier[buffers.Length];
            for (int i = 0; i < buffers.Length; i++)
            {
                barriers[i] = buffers[i].ResourceBarrier_CopyDst_To_Mesh();
            }
            Backend.m_command_list.Handle->ResourceBarrier((uint)buffers.Length, barriers);
        }
        else
        {
            var barriers = stackalloc BufferBarrier[buffers.Length];
            for (int i = 0; i < buffers.Length; i++)
            {
                barriers[i] = buffers[i].BufferBarrier_CopyDst_To_Mesh();
            }
            Backend.m_command_list7.Handle->Barrier(1, new BarrierGroup
            {
                Type = BarrierType.Buffer,
                NumBarriers = (uint)buffers.Length,
                PBufferBarriers = barriers,
            });
        }
    }

    #endregion
}
