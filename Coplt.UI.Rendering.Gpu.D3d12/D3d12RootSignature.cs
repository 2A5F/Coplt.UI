using System.Runtime.InteropServices;
using System.Text;
using Coplt.Dropping;
using Coplt.UI.Rendering.Gpu.D3d12.Utilities;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Coplt.UI.Rendering.Gpu.D3d12;

[Dropping]
public sealed unsafe partial class D3d12RootSignature
{
    #region StaticSamplers

    internal static readonly StaticSamplerDesc[] s_static_samplers =
    [
        // LinearClamp
        new()
        {
            Filter = Filter.MinMagLinearMipPoint,
            AddressU = TextureAddressMode.Clamp,
            AddressV = TextureAddressMode.Clamp,
            AddressW = TextureAddressMode.Clamp,
            ShaderRegister = 0,
            RegisterSpace = 1,
            ShaderVisibility = ShaderVisibility.All,
        },
        // LinearWrap
        new()
        {
            Filter = Filter.MinMagLinearMipPoint,
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Wrap,
            AddressW = TextureAddressMode.Wrap,
            ShaderRegister = 1,
            RegisterSpace = 1,
            ShaderVisibility = ShaderVisibility.All,
        },
        // PointClamp
        new()
        {
            Filter = Filter.MinMagMipPoint,
            AddressU = TextureAddressMode.Clamp,
            AddressV = TextureAddressMode.Clamp,
            AddressW = TextureAddressMode.Clamp,
            ShaderRegister = 2,
            RegisterSpace = 1,
            ShaderVisibility = ShaderVisibility.All,
        },
        // PointWrap
        new()
        {
            Filter = Filter.MinMagMipPoint,
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Wrap,
            AddressW = TextureAddressMode.Wrap,
            ShaderRegister = 3,
            RegisterSpace = 1,
            ShaderVisibility = ShaderVisibility.All,
        },
    ];

    #endregion

    #region Fields

    public GpuRendererBackendD3d12 Backend { get; }

    [Drop]
    internal ComPtr<ID3D12RootSignature> m_root_signature;

    #endregion

    #region Props

    public ref readonly ComPtr<ID3D12RootSignature> RootSignature => ref m_root_signature;

    #endregion

    #region Ctor

    public D3d12RootSignature(
        GpuRendererBackendD3d12 Backend,
        ReadOnlySpan<RootParameter> Params
    )
    {
        this.Backend = Backend;
        fixed (StaticSamplerDesc* p_static_samplers = s_static_samplers)
        {
            var num_params = Params.Length + 1;
            var param_s = stackalloc RootParameter[num_params];
            Params.CopyTo(new Span<RootParameter>(param_s, num_params)[1..]);
            DescriptorRange range0 = new()
            {
                RangeType = DescriptorRangeType.Srv,
                NumDescriptors = uint.MaxValue,
                BaseShaderRegister = 0,
                RegisterSpace = 1,
                OffsetInDescriptorsFromTableStart = 0,
            };
            param_s[0] = new RootParameter
            {
                ParameterType = RootParameterType.TypeDescriptorTable,
                ShaderVisibility = ShaderVisibility.All,
                DescriptorTable = new RootDescriptorTable
                {
                    NumDescriptorRanges = 1,
                    PDescriptorRanges = &range0,
                },
            };
            RootSignatureDesc desc = new()
            {
                NumParameters = (uint)num_params,
                PParameters = param_s,
                NumStaticSamplers = (uint)s_static_samplers.Length,
                PStaticSamplers = p_static_samplers,
                Flags = RootSignatureFlags.AllowInputAssemblerInputLayout,
            };
            ComPtr<ID3D10Blob> blob_ = default;
            ComPtr<ID3D10Blob> err_blob_ = default;
            var r = Backend.D3d12.SerializeRootSignature(&desc, D3DRootSignatureVersion.Version1, ref blob_, ref err_blob_);
            using var blob = blob_;
            using var err_blob = err_blob_;
            if (err_blob_.Handle != null)
            {
                var msg = Encoding.UTF8.GetString(err_blob_.Handle->Buffer);
                throw new COMException(msg, Marshal.GetExceptionForHR(r));
            }
            r.TryThrowHResult();
            Backend.m_device.Handle->CreateRootSignature(
                0, blob_.Handle->GetBufferPointer(), blob_.Handle->GetBufferSize(),
                out m_root_signature
            ).TryThrowHResult();
        }
    }

    #endregion
}
