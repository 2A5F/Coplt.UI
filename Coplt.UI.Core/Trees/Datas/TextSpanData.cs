using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Core.Geometry;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public partial record struct TextSpanData
{
    [Drop]
    public NativeList<AABB2DF> m_bounding_boxes;
    
    public uint TextStart;
    public uint TextLength;

    public ReadOnlySpan<AABB2DF> BoundingBoxes => m_bounding_boxes.AsSpan;
}
