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

public readonly record struct ViewNode(uint Index)
{
    public readonly uint Index = Index;

    public static explicit operator ViewNode(NodeId node) =>
        node.Type is not NodeType.View ? throw new InvalidCastException() : new(node.Index);
}

public readonly record struct TextParagraphNode(uint Index)
{
    public readonly uint Index = Index;

    public static explicit operator TextParagraphNode(NodeId node) =>
        node.Type is not NodeType.TextParagraph ? throw new InvalidCastException() : new(node.Index);
}

public readonly record struct TextSpanNode(uint Index)
{
    public readonly uint Index = Index;

    public static explicit operator TextSpanNode(NodeId node) =>
        node.Type is not NodeType.TextSpan ? throw new InvalidCastException() : new(node.Index);
}
