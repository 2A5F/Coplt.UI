using Coplt.Dropping;
using Coplt.UI.Rendering.Gpu.D3d12.Utilities;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Coplt.UI.Rendering.Gpu.D3d12;

public record struct D3d12GraphicsPipelineDesc()
{
    public bool AlphaToCoverage { get; set; } = false;
    public bool BlendEnable { get; set; } = false;
    public Blend SrcBlend { get; set; } = Blend.SrcAlpha;
    public Blend DstBlend { get; set; } = Blend.InvSrcAlpha;
    public BlendOp BlendOp { get; set; } = BlendOp.Add;
    public Blend AlphaSrcBlend { get; set; } = Blend.One;
    public Blend AlphaDstBlend { get; set; } = Blend.One;
    public BlendOp AlphaBlendOp { get; set; } = BlendOp.Max;
    public ColorWriteEnable RenderTargetWriteMask { get; set; } = ColorWriteEnable.Red | ColorWriteEnable.Green | ColorWriteEnable.Blue;
    public CullMode CullMode { get; set; } = CullMode.None;
    public bool DepthClipEnable { get; set; } = false;
    public bool MultisampleEnable { get; set; } = false;
    public bool DepthEnable { get; set; } = false;
    public bool DepthWrite { get; set; } = false;
    public ComparisonFunc DepthFunc { get; set; } = ComparisonFunc.GreaterEqual;
    public bool StencilEnable { get; set; } = false;
    public byte StencilReadMask { get; set; } = 0xFF;
    public byte StencilWriteMask { get; set; } = 0xFF;
    public StencilOp FrontFailOp { get; set; } = StencilOp.Keep;
    public StencilOp FrontDepthFailOp { get; set; } = StencilOp.Keep;
    public StencilOp FrontPassOp { get; set; } = StencilOp.Keep;
    public ComparisonFunc FrontFunc { get; set; } = ComparisonFunc.Always;
    public StencilOp BackFailOp { get; set; } = StencilOp.Keep;
    public StencilOp BackDepthFailOp { get; set; } = StencilOp.Keep;
    public StencilOp BackPassOp { get; set; } = StencilOp.Keep;
    public ComparisonFunc BackFunc { get; set; } = ComparisonFunc.Always;
}

[Dropping]
public sealed unsafe partial class D3d12GraphicsPipeline
{
    #region Fields

    public GpuRendererBackendD3d12 Backend { get; }
    public D3d12RootSignature RootSignature { get; }

    [Drop]
    internal ComPtr<ID3D12PipelineState> m_pipeline;

    #endregion

    #region Props

    public ref readonly ComPtr<ID3D12PipelineState> Pipeline => ref m_pipeline;

    #endregion

    #region Ctor

    public D3d12GraphicsPipeline(
        GpuRendererBackendD3d12 Backend,
        D3d12RootSignature RootSignature,
        ReadOnlySpan<byte> VertexShader,
        ReadOnlySpan<byte> PixelShader,
        Format RtvFormat,
        Format DsvFormat,
        D3d12GraphicsPipelineDesc Desc,
        InputLayoutDesc InputLayout
    )
    {
        this.Backend = Backend;
        this.RootSignature = RootSignature;
        fixed (byte* p_vertex = VertexShader)
        fixed (byte* p_pixel = PixelShader)
        {
            Backend.m_device.Handle->CreateGraphicsPipelineState(new GraphicsPipelineStateDesc
            {
                PRootSignature = RootSignature.RootSignature,
                VS = new()
                {
                    PShaderBytecode = p_vertex,
                    BytecodeLength = (uint)VertexShader.Length,
                },
                PS = new()
                {
                    PShaderBytecode = p_pixel,
                    BytecodeLength = (uint)PixelShader.Length,
                },
                BlendState = new()
                {
                    AlphaToCoverageEnable = Desc.AlphaToCoverage,
                    IndependentBlendEnable = false,
                    RenderTarget = new()
                    {
                        Element0 = new()
                        {
                            BlendEnable = Desc.BlendEnable,
                            LogicOpEnable = default,
                            SrcBlend = Desc.SrcBlend,
                            DestBlend = Desc.DstBlend,
                            BlendOp = Desc.BlendOp,
                            SrcBlendAlpha = Desc.AlphaSrcBlend,
                            DestBlendAlpha = Desc.AlphaDstBlend,
                            BlendOpAlpha = Desc.AlphaBlendOp,
                            LogicOp = LogicOp.Noop,
                            RenderTargetWriteMask = (byte)Desc.RenderTargetWriteMask,
                        },
                    }
                },
                SampleMask = uint.MaxValue,
                RasterizerState = new()
                {
                    FillMode = FillMode.Solid,
                    CullMode = Desc.CullMode,
                    FrontCounterClockwise = false,
                    DepthBias = 0,
                    DepthBiasClamp = 0,
                    SlopeScaledDepthBias = 0,
                    DepthClipEnable = Desc.DepthClipEnable,
                    MultisampleEnable = Desc.MultisampleEnable,
                    AntialiasedLineEnable = false,
                    ForcedSampleCount = 0,
                    ConservativeRaster = ConservativeRasterizationMode.Off,
                },
                DepthStencilState = new()
                {
                    DepthEnable = Desc.DepthEnable,
                    DepthWriteMask = Desc.DepthWrite ? DepthWriteMask.All : DepthWriteMask.Zero,
                    DepthFunc = Desc.DepthFunc,
                    StencilEnable = Desc.StencilEnable,
                    StencilReadMask = Desc.StencilReadMask,
                    StencilWriteMask = Desc.StencilWriteMask,
                    FrontFace = new()
                    {
                        StencilFailOp = Desc.FrontFailOp,
                        StencilDepthFailOp = Desc.FrontDepthFailOp,
                        StencilPassOp = Desc.FrontPassOp,
                        StencilFunc = Desc.FrontFunc
                    },
                    BackFace = new()
                    {
                        StencilFailOp = Desc.BackFailOp,
                        StencilDepthFailOp = Desc.BackDepthFailOp,
                        StencilPassOp = Desc.BackPassOp,
                        StencilFunc = Desc.BackFunc,
                    },
                },
                InputLayout = InputLayout,
                IBStripCutValue = IndexBufferStripCutValue.ValueDisabled,
                PrimitiveTopologyType = PrimitiveTopologyType.Triangle,
                NumRenderTargets = 1,
                RTVFormats = new()
                {
                    Element0 = RtvFormat,
                },
                DSVFormat = DsvFormat,
                SampleDesc = new(1, 0),
            }, out m_pipeline).TryThrowHResult();
        }
    }

    #endregion
}
