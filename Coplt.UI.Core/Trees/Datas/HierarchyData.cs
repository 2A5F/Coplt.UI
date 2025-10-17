using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Native.Collections;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public partial record struct ChildsData
{
    [Drop]
    [ComType<Ptr<FFIOrderedSet<NodeId>>>]
    public NOrderedSet<NodeId> m_childs;
}

public record struct ParentData
{
    public NodeId m_parent;
    public bool m_has_parent;
}
