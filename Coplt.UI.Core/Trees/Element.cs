using Coplt.UI.Collections;

namespace Coplt.UI.Trees;

public sealed class Element(Document Document, NodeType Type)
{
    public Document Document { get; } = Document;
    public NodeType Type { get; } = Type;
    public Document.Arche Arche { get; } = Document.m_arches[(int)Type];

    internal EmbedSet<Element> m_childs_refs = new();
}
