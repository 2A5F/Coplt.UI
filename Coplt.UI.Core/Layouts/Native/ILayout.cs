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
    public int* roots;

    public int* view_buckets;
    public int* text_buckets;
    public int* root_buckets;

    public NNodeIdCtrl* view_ctrl;
    public NNodeIdCtrl* text_ctrl;
    public NNodeIdCtrl* root_ctrl;

    public CommonStyleData* view_common_style_data;
    public CommonStyleData* text_common_style_data;
    public CommonStyleData* root_common_style_data;

    public ChildsData* view_childs_data;
    public void* _pad_0;
    public ChildsData* root_childs_data;

    public ViewStyleData* view_style_data;
    public void* _pad_1;
    public ViewStyleData* root_style_data;

    public ViewLayoutData* view_layout_data;
    public void* _pad_2;
    public ViewLayoutData* root_layout_data;

    public TextStyleData* text_style_data;

    public RootData* root_root_data;

    public int root_count;
    public int view_count;
    public int text_count;

    public bool rounding;
}

[Interface, Guid("f1e64bf0-ffb9-42ce-be78-31871d247883")]
public unsafe partial struct ILayout
{
    public partial HResult Calc(NLayoutContext* ctx);
}
