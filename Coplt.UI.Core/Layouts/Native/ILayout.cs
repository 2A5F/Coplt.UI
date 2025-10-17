using System.Runtime.InteropServices;
using Coplt.Com;
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
    public int view_count;
    public int text_count;
    
    public int* view_buckets;
    public NNodeIdCtrl* view_ctrl;
    public CommonStyleData* view_common_style_data;
    public ChildsData* view_childs_data;
    public ViewStyleData* view_style_data;
    public ViewLayoutData* view_layout_data;

    public int* text_buckets;
    public NNodeIdCtrl* text_ctrl;
    public CommonStyleData* text_common_style_data;
    public TextStyleData* text_style_data;

    public bool rounding;
}

[Interface, Guid("f1e64bf0-ffb9-42ce-be78-31871d247883")]
public unsafe partial struct ILayout
{
    public partial HResult Calc(NLayoutContext* ctx);
}
