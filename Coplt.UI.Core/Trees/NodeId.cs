namespace Coplt.UI.Trees;

public record struct NodeId(uint Id, uint Version, NodeType Type)
{
    public const uint TypeMask = 0b1;

    public uint Id = Id;
    private uint VersionAndType = Version << 1 | (byte)Type & TypeMask;

    public uint Version
    {
        get => (VersionAndType & ~TypeMask) >> 1;
        set => VersionAndType = (VersionAndType & TypeMask) | (value << 1);
    }
    public NodeType Type
    {
        get => (NodeType)(Id & TypeMask);
        set => VersionAndType = (VersionAndType & ~TypeMask) | (byte)value & TypeMask;
    }

    public readonly bool Equals(NodeId other) => Id == other.Id && VersionAndType == other.VersionAndType;
    public readonly override int GetHashCode() => (int)(Id ^ VersionAndType);
}
