using System.Runtime.CompilerServices;
using Coplt.Dropping;
using Coplt.Mathematics;
using Coplt.UI.Rendering.Gpu.Utilities;
using Coplt.UI.Rendering.Gpu.D3d12.Utilities;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Coplt.UI.Rendering.Gpu.D3d12;

[Dropping(Unmanaged = true)]
public sealed unsafe partial class HwndSwapChain : D3d12RenderTarget
{
    #region Consts

    public const int FrameCount = 3;

    #endregion

    #region Filds

    [Drop]
    internal ComPtr<ID3D12Device1> m_device;
    
    [Drop]
    internal ComPtr<IDXGISwapChain3> m_swap_chain;
    [Drop]
    internal ComPtr<ID3D12DescriptorHeap> m_rtv_heap;

    [Drop]
    internal FixedArray3<ComPtr<ID3D12Resource>> m_buffers;
    internal FixedArray3<ulong> m_fence_values;

    internal int m_frame_index;
    internal readonly uint rtv_descriptor_size;

    internal uint2 m_cur_size;
    internal uint2 m_new_size;

    internal EventWaitHandle m_event;
    internal Lock m_lock = new();

    #endregion

    #region Props

    public D3d12GpuContext Context { get; }

    public override Format Format { get; } = Format.FormatB8G8R8A8Unorm;
    public bool VSync { get; set; } = false;

    public uint2 Size => m_cur_size;

    public ref readonly ComPtr<IDXGISwapChain3> SwapChain3 => ref m_swap_chain;
    public ref readonly ComPtr<ID3D12DescriptorHeap> RtvHeap => ref m_rtv_heap;

    private ResourceDesc m_desc;

    #endregion

    #region Ctor

    public HwndSwapChain(D3d12GpuContext Context, IntPtr Hwnd, uint2 Size)
    {
        this.Context = Context;

        Context.m_device.Handle->AddRef();
        m_device = Context.m_device;

        m_new_size = m_cur_size = Size;

        m_desc = new()
        {
            Dimension = ResourceDimension.Texture2D,
            Alignment = 0,
            Width = m_cur_size.x,
            Height = m_cur_size.y,
            DepthOrArraySize = 1,
            MipLevels = 1,
            Format = Format,
            SampleDesc = new(1, 0),
            Layout = TextureLayout.LayoutUnknown,
            Flags = ResourceFlags.None
        };

        SwapChainDesc1 desc = new()
        {
            Width = m_cur_size.x,
            Height = m_cur_size.y,
            Format = Format,
            Stereo = false,
            SampleDesc = { Count = 1, Quality = 0 },
            BufferUsage = DXGI.UsageRenderTargetOutput | DXGI.UsageShaderInput,
            BufferCount = FrameCount,
            Scaling = Scaling.Stretch,
            SwapEffect = SwapEffect.FlipDiscard,
            AlphaMode = AlphaMode.Unspecified,
            Flags = 0
        };

        using ComPtr<IDXGISwapChain1> swap_chain = default;
        Context.Factory.CreateSwapChainForHwnd((IUnknown*)Context.Queue.Handle, Hwnd, &desc, (SwapChainFullscreenDesc*)null,
            (IDXGIOutput*)null, &swap_chain.Handle).TryThrowHResult();
        swap_chain.QueryInterface(out m_swap_chain).TryThrowHResult();

        m_frame_index = (int)m_swap_chain.GetCurrentBackBufferIndex();

        DescriptorHeapDesc rtv_heap_desc = new()
        {
            Type = DescriptorHeapType.Rtv,
            NumDescriptors = FrameCount,
            Flags = DescriptorHeapFlags.None,
            NodeMask = 0
        };
        Context.Device.CreateDescriptorHeap(&rtv_heap_desc, out m_rtv_heap).TryThrowHResult();
        rtv_descriptor_size = Context.Device.GetDescriptorHandleIncrementSize(DescriptorHeapType.Rtv);

        CreateRts();

        m_event = new(false, EventResetMode.AutoReset);
    }

    #endregion

    #region Create Rts

    private void CreateRts()
    {
        var handle = m_rtv_heap.GetCPUDescriptorHandleForHeapStart();
        for (var i = 0; i < FrameCount; i++)
        {
            m_swap_chain.GetBuffer((uint)i, out m_buffers[i]).TryThrowHResult();
            if (Context.DebugEnabled) m_buffers[i].SetName(in $"SwapChain Frame {i}".AsSpan()[0]).TryThrowHResult();
            RenderTargetViewDesc desc = new()
            {
                Format = Format,
                ViewDimension = RtvDimension.Texture2D,
                Texture2D = new()
                {
                    MipSlice = 0,
                    PlaneSlice = 0,
                }
            };
            Context.Device.CreateRenderTargetView(m_buffers[i].Handle, &desc, handle);
            handle = new(handle.Ptr + rtv_descriptor_size);
        }
    }

    #endregion

    #region Render

    public override CpuDescriptorHandle Rtv
    {
        get
        {
            var handle = m_rtv_heap.GetCPUDescriptorHandleForHeapStart();
            return new(handle.Ptr + (nuint)m_frame_index * rtv_descriptor_size);
        }
    }

    public ref readonly ComPtr<ID3D12Resource> Resource => ref m_buffers[m_frame_index];

    public ID3D12Resource* Raw => Resource.Handle;
    public ID3D12Resource* Ptr => Resource.Handle;
    public ref readonly ResourceDesc Desc => ref m_desc;

    #endregion

    #region Ctrl

    public void OnResize(uint2 size)
    {
        Interlocked.Exchange(
            ref Unsafe.As<uint2, ulong>(ref m_new_size),
            Unsafe.BitCast<uint2, ulong>(size)
        );
    }

    public void Present()
    {
        using var _ = m_lock.EnterScope();
        PresentNoWait_InLock();
        WaitFrameReady_InLock();
    }

    public void PresentNoWait()
    {
        using var _ = m_lock.EnterScope();
        PresentNoWait_InLock();
    }

    public void WaitFrameReady()
    {
        using var _ = m_lock.EnterScope();
        WaitFrameReady_InLock();
    }

    private void PresentNoWait_InLock()
    {
        m_swap_chain.Present(VSync ? 1u : 0u, 0u);
        m_fence_values[m_frame_index] = Context.Signal();
    }

    private void WaitFrameReady_InLock()
    {
        var cur_size = m_cur_size;
        var new_size = Unsafe.BitCast<ulong, uint2>(Interlocked.Read(ref Unsafe.As<uint2, ulong>(ref m_new_size)));
        if (!cur_size.Equals(new_size))
        {
            DoResize_InLock(new_size);
        }
        var min = ulong.MaxValue;
        for (var i = 0; i < FrameCount; i++)
        {
            var v = m_fence_values[i];
            if (v < min) min = v;
        }
        Context.Wait(min, m_event);
        m_frame_index = (int)m_swap_chain.GetCurrentBackBufferIndex();
    }

    private void DoResize_InLock(uint2 size)
    {
        WaitAll_InLock();
        for (var i = 0; i < FrameCount; i++)
        {
            m_buffers[i].Dispose();
            m_buffers[i] = default;
        }
        m_swap_chain.ResizeBuffers(FrameCount, size.x, size.y, Format, 0).TryThrowHResult();
        CreateRts();
        m_frame_index = (int)m_swap_chain.GetCurrentBackBufferIndex();
        m_cur_size = size;
        m_desc.Width = m_cur_size.x;
        m_desc.Height = m_cur_size.y;
    }

    private void WaitAll_InLock()
    {
        ulong max = 0;
        for (var i = 0; i < FrameCount; i++)
        {
            var v = m_fence_values[i];
            if (v > max) max = v;
        }
        Context.Wait(max, m_event);
    }


    [Drop(Order = -1)]
    public void WaitAll()
    {
        using var _ = m_lock.EnterScope();
        WaitAll_InLock();
    }

    #endregion

    #region Barrier

    public void BarrierToRenderTarget()
    {
        if (Context.EnhancedBarriersSupported)
        {
            TextureBarrier barrier = new()
            {
                SyncBefore = BarrierSync.None,
                SyncAfter = BarrierSync.RenderTarget,
                AccessBefore = BarrierAccess.NoAccess,
                AccessAfter = BarrierAccess.RenderTarget,
                LayoutBefore = BarrierLayout.Present,
                LayoutAfter = BarrierLayout.RenderTarget,
                PResource = Resource,
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
            };
            Context.m_command_list7.Handle->Barrier(1, new BarrierGroup
            {
                Type = BarrierType.Texture,
                NumBarriers = 1,
                PTextureBarriers = &barrier,
            });
        }
        else
        {
            ResourceBarrier barrier = new()
            {
                Type = ResourceBarrierType.Transition,
                Flags = ResourceBarrierFlags.None,
                Transition = new()
                {
                    PResource = Resource,
                    Subresource = 0,
                    StateBefore = ResourceStates.Present,
                    StateAfter = ResourceStates.RenderTarget,
                },
            };
            Context.m_command_list.Handle->ResourceBarrier(1, barrier);
        }
    }

    public void BarrierToPresent()
    {
        if (Context.EnhancedBarriersSupported)
        {
            TextureBarrier barrier = new()
            {
                SyncBefore = BarrierSync.RenderTarget,
                SyncAfter = BarrierSync.None,
                AccessBefore = BarrierAccess.RenderTarget,
                AccessAfter = BarrierAccess.NoAccess,
                LayoutBefore = BarrierLayout.RenderTarget,
                LayoutAfter = BarrierLayout.Present,
                PResource = Resource,
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
            };
            Context.m_command_list7.Handle->Barrier(1, new BarrierGroup
            {
                Type = BarrierType.Texture,
                NumBarriers = 1,
                PTextureBarriers = &barrier,
            });
        }
        else
        {
            ResourceBarrier barrier = new()
            {
                Type = ResourceBarrierType.Transition,
                Flags = ResourceBarrierFlags.None,
                Transition = new()
                {
                    PResource = Resource,
                    Subresource = 0,
                    StateBefore = ResourceStates.RenderTarget,
                    StateAfter = ResourceStates.Present,
                },
            };
            Context.m_command_list.Handle->ResourceBarrier(1, barrier);
        }
    }

    #endregion
}
