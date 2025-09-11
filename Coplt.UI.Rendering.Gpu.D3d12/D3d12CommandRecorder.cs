using Coplt.Dropping;
using Coplt.UI.Rendering.Gpu.D3d12.Utilities;
using Coplt.UI.Rendering.Gpu.Graphics;
using Coplt.UI.Rendering.Gpu.Utilities;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.Maths;

namespace Coplt.UI.Rendering.Gpu.D3d12;

[Dropping(Unmanaged = true)]
public sealed unsafe partial class D3d12CommandRecorder : GpuCommandRecorder
{
    #region Fields

    public GpuRendererBackendD3d12 Backend { get; }

    [Drop]
    internal ComPtr<ID3D12GraphicsCommandList> m_command_list;
    [Drop]
    internal ComPtr<ID3D12GraphicsCommandList7> m_command_list7;

    [Drop]
    internal FixedArray3<ComPtr<ID3D12CommandAllocator>> m_cmd_allocator;

    private bool m_first = true;
    private int m_frame_count = 0;

    #endregion

    #region Props

    public ReadOnlySpan<ComPtr<ID3D12CommandAllocator>> CommandAllocator => m_cmd_allocator;
    public ref readonly ComPtr<ID3D12GraphicsCommandList> CommandList => ref m_command_list;
    public ref readonly ComPtr<ID3D12GraphicsCommandList7> CommandList7 => ref m_command_list7;

    #endregion

    #region Ctor

    public D3d12CommandRecorder(GpuRendererBackendD3d12 Backend)
    {
        this.Backend = Backend;
        for (var i = 0; i < GpuRendererBackend.FrameCount; i++)
        {
            ref var ca = ref m_cmd_allocator[i];
            Backend.m_device.Handle->CreateCommandAllocator(CommandListType.Bundle, out ca).TryThrowHResult();
        }
        Backend.m_device.Handle->CreateCommandList(0, CommandListType.Bundle, m_cmd_allocator[0], default(ComPtr<ID3D12PipelineState>), out m_command_list)
            .TryThrowHResult();
        m_command_list.Handle->QueryInterface(out m_command_list7);
    }

    #endregion

    #region Recorder

    public override void Renew()
    {
        if (m_first)
        {
            m_first = false;
            return;
        }
        m_frame_count++;
        if (m_frame_count >= GpuRendererBackend.FrameCount) m_frame_count = 0;
        var frame = m_frame_count;
        m_command_list.Handle->Reset(m_cmd_allocator[frame], null).TryThrowHResult();
    }

    public override void Finish()
    {
        m_command_list.Handle->Close().TryThrowHResult();
    }

    #endregion

    #region Commands

    public override void SetScissorRect(uint Left, uint Top, uint Width, uint Height)
    {
        m_command_list.Handle->RSSetScissorRects(1, new Box2D<int>((int)Left, (int)Top, (int)(Width + Left), (int)(Height + Top)));
    }

    #endregion
}
