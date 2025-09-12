using Coplt.Dropping;
using Coplt.UI.Rendering.Gpu.D3d12.Utilities;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Coplt.UI.Rendering.Gpu.D3d12;

[Dropping]
public sealed unsafe partial class D3d12CommandSignature
{
    #region Fields

    public D3d12RendererBackend Backend { get; }
    public D3d12RootSignature RootSignature { get; }

    [Drop]
    internal ComPtr<ID3D12CommandSignature> m_command_signature;

    #endregion

    #region Props

    public ref readonly ComPtr<ID3D12CommandSignature> CommandSignature => ref m_command_signature;

    #endregion

    #region Ctor

    public D3d12CommandSignature(
        D3d12RendererBackend Backend, D3d12RootSignature RootSignature,
        uint ByteStride, ReadOnlySpan<IndirectArgumentDesc> args
    )
    {
        this.Backend = Backend;
        this.RootSignature = RootSignature;
        fixed (IndirectArgumentDesc* p_args = args)
        {
            Backend.m_device.Handle->CreateCommandSignature(new CommandSignatureDesc
            {
                ByteStride = ByteStride,
                NumArgumentDescs = (uint)args.Length,
                PArgumentDescs = p_args,
            }, RootSignature.RootSignature, out m_command_signature).TryThrowHResult();
        }
    }

    #endregion
}
