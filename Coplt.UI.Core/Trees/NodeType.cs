using System.Numerics;

namespace Coplt.UI.Trees;

public enum NodeType : byte
{
    View = 0,
    Text = 1,
    Root = 2,
}

[Flags]
public enum NodeTypes : uint
{
    None = 0,
    View = 1 << 0,
    Text = 1 << 1,
    Root = 1 << 2,

    AllView = View | Root,
    All = View | Text | Root,
}

public struct NodeTypesEnumerator(NodeTypes types)
{
    private NodeTypes types = types;

    public bool MoveNext()
    {
        if (types == default) return false;
        Current = (NodeType)BitOperations.TrailingZeroCount((uint)types);
        types &= types - 1;
        return true;
    }

    public NodeType Current { get; private set; }
}

public static class NodeTypeEx
{
    extension(NodeType)
    {
        public static int Length => 3;
    }
    
    extension(NodeTypes types)
    {
        public NodeTypesEnumerator GetEnumerator() => new(types);
    }
}
