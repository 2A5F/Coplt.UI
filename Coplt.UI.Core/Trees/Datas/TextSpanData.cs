using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Core.Geometry;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public partial record struct TextSpanData
{
    [Drop]
    public NativeList<AABB2DF> BoundingBoxes;
    
    public uint TextStart;
    public uint TextLength;
}
