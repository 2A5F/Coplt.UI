using System.Diagnostics.CodeAnalysis;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Native;
using Coplt.UI.Native.Collections;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public partial record struct ChildsData
{
    [Drop]
    [ComType<FFIOrderedSet<NodeId>>]
    private NOrderedSet<NodeId> m_childs;
    [Drop]
    private NativeList<NString> m_texts; // todo dict
    private uint m_text_id_inc;

    internal ulong m_version;
    internal ulong m_last_version;

    [UnscopedRef]
    public NOrderedSet<NodeId>.Enumerator GetEnumerator() => m_childs.GetEnumerator();

    public NodeId AddText(string text) => AddText(NString.Create(text));
    public NodeId AddText(NString text)
    {
        var id = m_text_id_inc++;
        var index = m_texts.Count;
        m_texts.Add(text);
        var node = new NodeId((uint)index, id, NodeType.Text);
        UnsafeAdd(node);
        return node;
    }

    private void RemoveText(NodeId node)
    {
        if (node.Type != NodeType.Text) throw new ArgumentException("node must be of type Text", nameof(node));
        UnsafeRemove(node);
        // todo remove text
    }

    public bool UnsafeAdd(NodeId locate)
    {
        var r = m_childs.Add(locate);
        if (r && m_version == m_last_version) m_version++;
        return r;
    }

    public bool UnsafeRemove(NodeId locate)
    {
        var r = m_childs.Remove(locate);
        if (r && m_version == m_last_version) m_version++;
        return r;
    }
}

public record struct ParentData
{
    private NodeId m_parent;
    private bool m_has_parent;

    public bool HasParent => m_has_parent;

    public NodeId? Parent => m_has_parent ? m_parent : null;

    public void UnsafeSetParent(NodeId locate)
    {
        m_has_parent = true;
        m_parent = locate;
    }

    public void UnsafeRemoveParent()
    {
        m_has_parent = false;
        m_parent = default;
    }
}
