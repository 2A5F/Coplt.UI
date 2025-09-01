using System.Runtime.CompilerServices;

namespace Coplt.UI.Rendering.Gpu.D3d12;

internal static class Meshes
{
    public static float i(uint i) => Unsafe.BitCast<uint, float>(i);

    public static readonly Mesh Box = new()
    {
        Stirde = sizeof(float) * 5,
        Count = 12,
        Vertices =
        [
            /* pos              uv           border color */
            0.5f, 0.5f, /*   */ 0.5f, 0.5f, /*      */ i(0),
            0.0f, 0.0f, /*   */ 0.0f, 0.0f, /*      */ i(0),
            1.0f, 0.0f, /*   */ 1.0f, 0.0f, /*      */ i(0),
            /*                                            */
            0.5f, 0.5f, /*   */ 0.5f, 0.5f, /*      */ i(1),
            1.0f, 0.0f, /*   */ 1.0f, 0.0f, /*      */ i(1),
            1.0f, 1.0f, /*   */ 1.0f, 1.0f, /*      */ i(1),
            /*                                            */
            0.5f, 0.5f, /*   */ 0.5f, 0.5f, /*      */ i(2),
            1.0f, 1.0f, /*   */ 1.0f, 1.0f, /*      */ i(2),
            0.0f, 1.0f, /*   */ 0.0f, 1.0f, /*      */ i(2),
            /*                                            */
            0.5f, 0.5f, /*   */ 0.5f, 0.5f, /*      */ i(3),
            0.0f, 1.0f, /*   */ 0.0f, 1.0f, /*      */ i(3),
            0.0f, 0.0f, /*   */ 0.0f, 0.0f, /*      */ i(3),
        ]
    };
}

internal struct Mesh
{
    public uint Stirde;
    public uint Count;
    public float[] Vertices;
}
