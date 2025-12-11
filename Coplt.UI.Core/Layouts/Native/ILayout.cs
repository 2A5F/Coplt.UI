using System.Runtime.InteropServices;
using Coplt.Com;
using Coplt.UI.Collections;
using Coplt.UI.Native.Collections;
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
    public IFontManager* font_manager;

    [ComType<Ptr<FFIMap>>]
    public NativeMap<NodeId, RootData>* roots;

    public int* view_buckets;
    [ComType<Ptr<NNodeIdCtrl>>]
    public NSplitMapCtrl<uint>.Ctrl* view_ctrl;
    public CommonData* view_common_data;
    public ChildsData* view_childs_data;
    public StyleData* view_style_data;
    
    public int* text_paragraph_buckets;
    [ComType<Ptr<NNodeIdCtrl>>]
    public NSplitMapCtrl<uint>.Ctrl* text_paragraph_ctrl;
    public CommonData* text_paragraph_common_data;
    public ChildsData* text_paragraph_childs_data;
    public TextParagraphData* text_paragraph_data;
    public TextStyleData* text_paragraph_style_data;
    
    public int* text_span_buckets;
    [ComType<Ptr<NNodeIdCtrl>>]
    public NSplitMapCtrl<uint>.Ctrl* text_span_ctrl;
    public CommonData* text_span_common_data;
    public TextSpanData* text_span_data;
    public TextStyleData* text_span_style_data;

    public int view_count;
    public int text_paragraph_count;
    public int text_span_count;

    public bool rounding;
}

[Interface, Guid("f1e64bf0-ffb9-42ce-be78-31871d247883")]
public unsafe partial struct ILayout
{
    public partial HResult Calc(NLayoutContext* ctx);
}
