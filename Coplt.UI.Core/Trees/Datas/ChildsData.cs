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
    [ComType<FFIOrderedSet>]
    public NOrderedSet<NodeId> m_childs;
    [Drop]
    public NativeList<NString> m_texts;

    [UnscopedRef]
    public NOrderedSet<NodeId>.Enumerator GetEnumerator() => m_childs.GetEnumerator();
}
