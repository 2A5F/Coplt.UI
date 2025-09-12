using Coplt.Dropping;
using Coplt.Mathematics;
using Coplt.UI.Rendering.Gpu.D3d12.Utilities;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Coplt.UI.Rendering.Gpu.D3d12;

[Dropping]
public sealed unsafe partial class D3d12GpuRenderTarget : D3d12RenderTarget
{
    #region Fields

    public D3d12RendererBackend Backend { get; }

    [Drop]
    internal ComPtr<ID3D12Resource> m_resource;
    [Drop]
    internal ComPtr<ID3D12Resource2> m_resource2;
    [Drop]
    internal ComPtr<ID3D12DescriptorHeap> m_rtv_heap;

    #endregion

    #region Props

    public override ref readonly ComPtr<ID3D12Resource> Resource => ref m_resource;
    public ref readonly ComPtr<ID3D12Resource2> Resource2 => ref m_resource2;

    public override Format Format { get; }

    public override uint Width { get; }
    public override uint Height { get; }

    public Color? ClearBackgroundColor { get; }

    #endregion

    #region Ctor

    public D3d12GpuRenderTarget(
        D3d12RendererBackend Backend,
        Format Format, uint Width, uint Height, uint SampleCount = 1, uint SampleQuality = 0, Color? ClearBackgroundColor = null
    )
    {
        this.Backend = Backend;
        this.Format = Format;
        this.Width = Width;
        this.Height = Height;
        this.ClearBackgroundColor = ClearBackgroundColor;

        Backend.m_device.Handle->CreateDescriptorHeap(new DescriptorHeapDesc
        {
            Type = DescriptorHeapType.Rtv,
            NumDescriptors = 1,
            Flags = DescriptorHeapFlags.None,
        }, out m_rtv_heap).TryThrowHResult();
        Rtv = m_rtv_heap.Handle->GetCPUDescriptorHandleForHeapStart();

        ClearValue clear_value;
        if (ClearBackgroundColor.HasValue)
        {
            clear_value = new()
            {
                Format = Format,
            };
            (*(Color*)clear_value.Anonymous.Color) = ClearBackgroundColor.Value;
        }
        if (Backend.EnhancedBarriersSupported)
        {
            Backend.m_device10.Handle->CreateCommittedResource3(
                new HeapProperties { Type = HeapType.Default, },
                HeapFlags.None,
                new ResourceDesc1
                {
                    Dimension = ResourceDimension.Texture2D,
                    Format = Format,
                    Width = Width,
                    Height = Height,
                    DepthOrArraySize = 1,
                    MipLevels = 1,
                    SampleDesc = new(SampleCount, SampleQuality),
                    Layout = TextureLayout.LayoutUnknown,
                    Flags = ResourceFlags.AllowRenderTarget,
                },
                BarrierLayout.RenderTarget,
                ClearBackgroundColor.HasValue ? &clear_value : null,
                default(ComPtr<ID3D12ProtectedResourceSession>),
                0, null,
                out m_resource2
            ).TryThrowHResult();
            m_resource2.Handle->QueryInterface(out m_resource).TryThrowHResult();
        }
        else
        {
            Backend.m_device.Handle->CreateCommittedResource(
                new HeapProperties { Type = HeapType.Default, },
                HeapFlags.None,
                new ResourceDesc
                {
                    Dimension = ResourceDimension.Texture2D,
                    Format = Format,
                    Width = Width,
                    Height = Height,
                    DepthOrArraySize = 1,
                    MipLevels = 1,
                    SampleDesc = new(SampleCount, SampleQuality),
                    Layout = TextureLayout.LayoutUnknown,
                    Flags = ResourceFlags.AllowRenderTarget,
                },
                ResourceStates.RenderTarget,
                ClearBackgroundColor.HasValue ? &clear_value : null,
                out m_resource
            ).TryThrowHResult();
            m_resource.Handle->QueryInterface(out m_resource2);
        }

        Backend.m_device.Handle->CreateRenderTargetView(m_resource, null, Rtv);
    }

    #endregion

    #region Rtv

    public override CpuDescriptorHandle Rtv { get; }

    #endregion
}
