using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Core.Geometry;
using Coplt.UI.Native;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public partial record struct TextSpanData
{
    [Drop]
    public NativeList<AABB2DF> BoundingBoxes;

    /// <summary>
    /// nth in <see cref="ChildsData.m_texts"/>
    /// </summary>
    public uint TextIndex;
    /// <summary>
    /// span start in <see cref="ChildsData.m_texts"/> :: <see cref="NString"/>
    /// </summary>
    public uint TextStart;
    /// <summary>
    /// span length in <see cref="ChildsData.m_texts"/> :: <see cref="NString"/>
    /// </summary>
    public uint TextLength;
}
