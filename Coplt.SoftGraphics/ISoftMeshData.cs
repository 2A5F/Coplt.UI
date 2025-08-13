using Coplt.Mathematics.Simt;

namespace Coplt.SoftGraphics;

public interface ISoftMeshData
{
    public uint NumClusters { get; }

    public uint NumPrimitives(uint Cluster);

    public void Load(uint Cluster, uint IndexStep16, out float4_mt a, out float4_mt b, out float4_mt c, out uint_mt index, out b32_mt active_lanes);
}
