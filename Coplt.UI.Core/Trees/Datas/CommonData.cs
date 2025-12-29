using System.Diagnostics.CodeAnalysis;
using Coplt.Dropping;
using Coplt.Mathematics;
using Coplt.UI.Miscellaneous;
using Coplt.UI.Native;

namespace Coplt.UI.Trees.Datas;

[Dropping]
public partial record struct CommonData()
{
    internal uint NodeId = uint.MaxValue;
    internal NodeId ParentValue;

    internal bool HasParent = false;

    public NodeId? Parent
    {
        get => HasParent ? ParentValue : null;
        set
        {
            if (value.HasValue)
            {
                ParentValue = value.Value;
                HasParent = true;
            }
            else
            {
                HasParent = false;
                ParentValue = default;
            }
        }
    }

}

public static class CommonDataEx
{
    extension(in CommonData data)
    {
        public uint NodeId => data.NodeId;
    }
}
