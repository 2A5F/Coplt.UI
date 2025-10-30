using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.UI.Collections;
using Coplt.UI.Trees;
using Coplt.UI.Trees.Datas;

namespace Coplt.UI.Native;

public struct NNodeIdCtrl
{
    public int HashCode;
    public int Next;
    public NodeId Key;
}

public unsafe struct NLayoutContext
{
    public RootData* roots;

    public int* node_buckets;
    [ComType<Ptr<NNodeIdCtrl>>]
    public NSplitMapCtrl<uint>.Ctrl* node_ctrl;
    public CommonData* node_common_data;
    public ChildsData* node_childs_data;
    public StyleData* node_style_data;

    public int root_count;
    public int node_count;

    public bool rounding;
}

[Interface, Guid("f1e64bf0-ffb9-42ce-be78-31871d247883")]
public unsafe partial struct ILayout
{
    public partial HResult Calc(NLayoutContext* ctx);
}
