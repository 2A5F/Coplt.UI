using Coplt.Mathematics;

namespace Coplt.UI.Rendering.Gpu.D3d12;

internal record struct ViewData
{
    public float4 ViewSize;
    public float4x4 VP;

    public ViewData(uint Width, uint Height, float MaxZ)
    {
        var far = Math.Max(1, MaxZ + 1);
        var vp = float4x4.Ortho(Width, Height, far, float.Epsilon);
        vp = math.mul(vp, float4x4.Translate(new float3(-Width, -Height, 0) * 0.5f));
        ViewSize = new(Width, Height, 1f / Width, 1f / Height);
        VP = vp;
    }
}
