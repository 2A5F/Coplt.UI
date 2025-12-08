using System.Runtime.CompilerServices;

namespace Coplt.UI.Trees;

public record struct NodeId(uint Index, uint Id, NodeType Type)
{
    public const uint TypeMask = 0xF;

    public uint Index = Index;
    private uint IdAndType = Id << 4 | (byte)Type & TypeMask;

    public uint Id
    {
        get => (IdAndType & ~TypeMask) >> 4;
        set => IdAndType = (IdAndType & TypeMask) | (value << 4);
    }
    public NodeType Type
    {
        get => (NodeType)(IdAndType & TypeMask);
        set => IdAndType = (IdAndType & ~TypeMask) | (byte)value;
    }

    public readonly bool Equals(NodeId other) => Id == other.Id && IdAndType == other.IdAndType;
    public readonly override int GetHashCode() => (int)(Index ^ IdAndType);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint NormalizeId(uint id) => id & 0x0FFF_FFFF;
}

public record struct ViewNode(uint Index)
{
    public uint Index = Index;

    public static implicit operator ViewNode(NodeId node) => new(node.Index);
}

public record struct TextNode(uint Index)
{
    public uint Index = Index;

    public static implicit operator TextNode(NodeId node) => new(node.Index);
}

public record struct ViewOrTextNode(uint Index)
{
    public uint Index = Index;

    public static implicit operator ViewOrTextNode(NodeId node) => new(node.Index);
    public static implicit operator ViewOrTextNode(ViewNode node) => new(node.Index);
    public static implicit operator ViewOrTextNode(TextNode node) => new(node.Index);
}
