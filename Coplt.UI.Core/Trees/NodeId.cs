namespace Coplt.UI.Trees;

public record struct NodeId(uint Id, uint Version, NodeType Type)
{
    public const uint TypeMask = 0b1111;

    public uint Id = Id;
    private uint VersionAndType = Version << 4 | (byte)Type & TypeMask;

    public uint Version
    {
        get => (VersionAndType & ~TypeMask) >> 4;
        set => VersionAndType = (VersionAndType & TypeMask) | (value << 4);
    }
    public NodeType Type
    {
        get => (NodeType)(VersionAndType & TypeMask);
        set => VersionAndType = (VersionAndType & ~TypeMask) | (byte)value;
    }

    public readonly bool Equals(NodeId other) => Id == other.Id && VersionAndType == other.VersionAndType;
    public readonly override int GetHashCode() => (int)(Id ^ VersionAndType);
}

public record struct NodeLocate(NodeId Id, int Index)
{
    public NodeId Id = Id;
    public int Index = Index;
}
