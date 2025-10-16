using Coplt.Dropping;
using Coplt.UI.Collections;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public partial record struct ChildsData
{
    // public OrderedSet<NodeId> m_childs; // todo native
}

public record struct ParentData
{
    public NodeId m_parent;
    public bool m_has_parent;
}
