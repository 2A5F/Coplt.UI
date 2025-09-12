using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Dropping;
using Coplt.Mathematics;
using Coplt.UI.Rendering.Gpu.Utilities;
using Coplt.UI.Rendering.Gpu.D3d12.Utilities;
using Coplt.UI.Rendering.Gpu.Graphics;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using Feature = Silk.NET.Direct3D12.Feature;

namespace Coplt.UI.Rendering.Gpu.D3d12;

[Dropping(Unmanaged = true)]
public unsafe partial class D3d12GpuContext : ID3d12RecyclablePoolSource
{
    #region Consts

    public const uint FrameCount = GpuRendererBackend.FrameCount;

    #endregion

    #region Logger

    public static Action<MessageCategory, MessageSeverity, MessageID, IntPtr, IntPtr>? LoggerFunc { get; set; }

    #endregion

    #region Silk

#pragma warning disable CS0618
    public DXGI Dxgi { get; } = DXGI.GetApi();
    public D3D12 D3d12 { get; } = D3D12.GetApi();
#pragma warning restore CS0618

    #endregion

    #region Fields

    [Drop]
    internal ComPtr<ID3D12Debug> m_debug_controller;
    [Drop]
    internal ComPtr<IDXGIFactory6> m_factory;
    [Drop]
    internal ComPtr<IDXGIAdapter1> m_adapter;
    [Drop]
    internal ComPtr<ID3D12Device1> m_device;
    [Drop]
    internal ComPtr<ID3D12InfoQueue1> m_info_queue;
    [Drop]
    internal ComPtr<ID3D12CommandQueue> m_queue;
    [Drop]
    internal ComPtr<ID3D12Fence> m_fence;

    [Drop]
    internal InlineArray3<ComPtr<ID3D12CommandAllocator>> m_cmd_allocator;
    [Drop]
    internal ComPtr<ID3D12GraphicsCommandList> m_command_list;
    [Drop]
    internal ComPtr<ID3D12GraphicsCommandList7> m_command_list7;

    internal EventWaitHandle m_event;
    internal InlineArray3<ulong> m_fence_values = default;
    internal ulong fence_value;
    internal int m_cur_frame;
    internal uint m_callback_cookie;

    private readonly Queue<ID3d12Recyclable> m_recycle_queue = new();
    private readonly Lock m_recycle_lock = new();

    #endregion

    #region Props

    public bool DebugEnabled { get; }

    public ref readonly ComPtr<IDXGIFactory6> Factory => ref m_factory;
    public ref readonly ComPtr<IDXGIAdapter1> Adapter => ref m_adapter;
    public ref readonly ComPtr<ID3D12Device1> Device => ref m_device;

    public ref readonly ComPtr<ID3D12CommandQueue> Queue => ref m_queue;
    public ref readonly ComPtr<ID3D12Fence> Fence => ref m_fence;

    public ReadOnlySpan<ComPtr<ID3D12CommandAllocator>> CommandAllocator => m_cmd_allocator;
    public ref readonly ComPtr<ID3D12GraphicsCommandList> CommandList => ref m_command_list;
    public ref readonly ComPtr<ID3D12GraphicsCommandList7> CommandList7 => ref m_command_list7;

    public int CurrentFrame => m_cur_frame;

    public bool EnhancedBarriersSupported { get; }

    #endregion

    #region Ctor

    public D3d12GpuContext(bool debug = false)
    {
        #region create event

        m_event = new(false, EventResetMode.AutoReset);

        #endregion

        #region create dx12

        var dxgi_flags = 0u;
        if (debug)
        {
            if (((HResult)D3d12.GetDebugInterface(out m_debug_controller)).IsSuccess)
            {
                m_debug_controller.EnableDebugLayer();
                dxgi_flags |= DXGI.CreateFactoryDebug;
                DebugEnabled = true;
            }

            if (((HResult)m_debug_controller.QueryInterface(out ComPtr<ID3D12Debug5> debug5)).IsSuccess)
            {
                debug5.EnableDebugLayer();
                debug5.Dispose();
            }
        }

        Dxgi.CreateDXGIFactory2(dxgi_flags, out m_factory).TryThrowHResult();
        m_factory.EnumAdapterByGpuPreference(0, GpuPreference.HighPerformance, out m_adapter).TryThrowHResult();

        D3d12.CreateDevice(m_adapter, D3DFeatureLevel.Level121, out m_device).TryThrowHResult();
        if (DebugEnabled)
        {
            m_device.SetName(in "Main Device".AsSpan()[0]).TryThrowHResult();
            if (m_device.QueryInterface(out m_info_queue).AsHResult().IsSuccess)
            {
                uint cookie = 0;
                if (m_info_queue.Handle->RegisterMessageCallback(
                        (delegate* unmanaged[Cdecl]<MessageCategory, MessageSeverity, MessageID, byte*, void*, void>)&DebugCallback,
                        MessageCallbackFlags.FlagNone,
                        null,
                        &cookie
                    ).AsHResult().IsSuccess)
                {
                    m_callback_cookie = cookie;
                }
            }
        }

        #endregion

        #region create queue

        CommandQueueDesc queue_desc = new()
        {
            Type = CommandListType.Direct,
            Priority = 0,
            Flags = CommandQueueFlags.None,
            NodeMask = 0
        };
        m_device.CreateCommandQueue(&queue_desc, out m_queue).TryThrowHResult();
        if (DebugEnabled) m_queue.SetName(in "Main Queue".AsSpan()[0]).TryThrowHResult();

        #endregion

        #region create fence

        m_device.CreateFence(0u, FenceFlags.None, out m_fence).TryThrowHResult();
        if (DebugEnabled) m_fence.SetName(in "Main Fence".AsSpan()[0]).TryThrowHResult();

        #endregion

        #region create command

        for (var i = 0; i < FrameCount; i++)
        {
            ref var ca = ref m_cmd_allocator[i];
            m_device.Handle->CreateCommandAllocator(CommandListType.Direct, out ca).TryThrowHResult();
        }

        m_device.Handle->CreateCommandList(0, CommandListType.Direct, m_cmd_allocator[0], default(ComPtr<ID3D12PipelineState>), out m_command_list)
            .TryThrowHResult();
        m_command_list.Handle->QueryInterface(out m_command_list7);

        #endregion

        #region Query Features

        FeatureDataD3D12Options12 options12;
        if (m_device.Handle->CheckFeatureSupport(Feature.D3D12Options16, &options12, (uint)sizeof(FeatureDataD3D12Options12))
            .AsHResult().IsSuccess)
        {
            EnhancedBarriersSupported = options12.EnhancedBarriersSupported;
        }

        #endregion
    }

    #endregion

    #region DebugCallback

    [Drop(Order = -1)]
    private void UnRegDebugCallback()
    {
        if (m_info_queue.Handle == null) return;
        m_info_queue.Handle->UnregisterMessageCallback(m_callback_cookie);
        m_callback_cookie = 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void DebugCallback(MessageCategory Category, MessageSeverity Severity, MessageID id, byte* pDescription, void* pContext)
    {
        try
        {
            LoggerFunc?.Invoke(Category, Severity, id, (IntPtr)pDescription, (IntPtr)pContext);
        }
        catch
        {
            // ignored
        }
    }

    #endregion

    #region Drop

    [Drop(Order = -1000)]
    private void WaitAll()
    {
        Wait(Signal());
    }

    #endregion

    #region Fence

    public ulong AllocSignal() => Interlocked.Increment(ref fence_value);

    public ulong Signal()
    {
        if (m_fence.Handle == null) return 0;
        var value = AllocSignal();
        m_queue.Signal(m_fence.Handle, value).TryThrowHResult();
        return value;
    }

    /// <summary>
    /// Wait On Gpu
    /// </summary>
    public void Wait(ulong value) => m_queue.Wait(m_fence.Handle, value).TryThrowHResult();

    /// <summary>
    /// Wait On Cpu
    /// </summary>
    public void Wait(ulong value, EventWaitHandle handle) => m_fence.Wait(value, handle);

    #endregion

    #region Submit

    public void Submit()
    {
        SubmitNotEnd();
        EndFrame();
    }

    public void SubmitNotEnd()
    {
        m_command_list.Handle->Close();
        var list = (ID3D12CommandList*)m_command_list.Handle;
        m_queue.Handle->ExecuteCommandLists(1, &list);
    }

    public void EndFrame()
    {
        m_fence_values[m_cur_frame] = Signal();
    }

    public void ReadyNextFrame()
    {
        m_cur_frame++;
        if (m_cur_frame >= FrameCount) m_cur_frame = 0;
        var value = m_fence_values[m_cur_frame];
        Wait(value, m_event);
        m_cmd_allocator[m_cur_frame].Handle->Reset().TryThrowHResult();
        m_command_list.Handle->Reset(m_cmd_allocator[m_cur_frame].Handle, null).TryThrowHResult();
        Recycle();
    }

    public void ReadyNextFrameNoWait()
    {
        m_cur_frame++;
        if (m_cur_frame >= FrameCount) m_cur_frame = 0;
        m_cmd_allocator[m_cur_frame].Handle->Reset().TryThrowHResult();
        m_command_list.Handle->Reset(m_cmd_allocator[m_cur_frame].Handle, null).TryThrowHResult();
        Recycle();
    }

    #endregion

    #region Recycle

    public void RegRecycle(ID3d12Recyclable item)
    {
        using var _ = m_recycle_lock.EnterScope();
        m_recycle_queue.Enqueue(item);
    }

    private void Recycle()
    {
        using var _ = m_recycle_lock.EnterScope();
        re:
        if (m_recycle_queue.TryPeek(out var item))
        {
            if (item.CurrentFrame != m_cur_frame) return;
            item.Recycle();
            m_recycle_queue.Dequeue();
            goto re;
        }
    }

    #endregion

    #region Commands

    public void ClearRenderTargetView(CpuDescriptorHandle RenderTargetView, float4 Color)
    {
        m_command_list.Handle->ClearRenderTargetView(RenderTargetView, (float*)&Color, 0, null);
    }

    #endregion
}
