using System.Diagnostics;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Native;
using Coplt.UI.Trees.Datas;

namespace Coplt.UI.Trees.Modules;

[Dropping]
public sealed unsafe partial class LayoutModule : Document.IModule
{
    public static Document.IModule Create(Document document) => new LayoutModule(document);

    [Drop]
    private Rc<ILayout> m_layout;

    private readonly Document m_document;
    
    private readonly Document.Arche m_ar_Root;
    private readonly Document.Arche m_ar_View;
    private readonly Document.Arche m_ar_Text;
    
    private readonly Document.PinnedStorage<RootData> m_st_Root_RootData;
    private readonly Document.PinnedStorage<CommonStyleData> m_st_Root_CommonStyleData;
    private readonly Document.PinnedStorage<ChildsData> m_st_Root_ChildsData;
    private readonly Document.PinnedStorage<ViewStyleData> m_st_Root_ViewStyleData;
    private readonly Document.PinnedStorage<ViewLayoutData> m_st_Root_ViewLayoutData;
    
    private readonly Document.PinnedStorage<CommonStyleData> m_st_View_CommonStyleData;
    private readonly Document.PinnedStorage<ChildsData> m_st_View_ChildsData;
    private readonly Document.PinnedStorage<ViewStyleData> m_st_View_ViewStyleData;
    private readonly Document.PinnedStorage<ViewLayoutData> m_st_View_ViewLayoutData;
    
    private readonly Document.PinnedStorage<CommonStyleData> m_st_Text_CommonStyleData;
    private readonly Document.PinnedStorage<TextStyleData> m_st_Text_TextStyleData;

    public LayoutModule(Document document)
    {
        m_document = document;
        m_layout = NativeLib.Instance.m_layout.Clone();
        
        m_ar_Root = document.ArcheOf(NodeType.Root);
        m_st_Root_RootData = m_ar_Root.StorageOf<RootData>().AsPinned();
        m_st_Root_CommonStyleData = m_ar_Root.StorageOf<CommonStyleData>().AsPinned();
        m_st_Root_ChildsData = m_ar_Root.StorageOf<ChildsData>().AsPinned();
        m_st_Root_ViewStyleData = m_ar_Root.StorageOf<ViewStyleData>().AsPinned();
        m_st_Root_ViewLayoutData = m_ar_Root.StorageOf<ViewLayoutData>().AsPinned();
        
        m_ar_View = document.ArcheOf(NodeType.View);
        m_st_View_CommonStyleData = m_ar_View.StorageOf<CommonStyleData>().AsPinned();
        m_st_View_ChildsData = m_ar_View.StorageOf<ChildsData>().AsPinned();
        m_st_View_ViewStyleData = m_ar_View.StorageOf<ViewStyleData>().AsPinned();
        m_st_View_ViewLayoutData = m_ar_View.StorageOf<ViewLayoutData>().AsPinned();
        
        m_ar_Text = document.ArcheOf(NodeType.Text);
        m_st_Text_CommonStyleData = m_ar_Text.StorageOf<CommonStyleData>().AsPinned();
        m_st_Text_TextStyleData = m_ar_Text.StorageOf<TextStyleData>().AsPinned();
    }

    public void Update()
    {
        Debug.Assert(m_document.m_roots.Count == m_ar_Root.m_ctrl.m_count);
        var ctx = new NLayoutContext
        {
            root_count = m_document.m_roots.Count,
            view_count = m_ar_View.m_ctrl.m_count,
            text_count = m_ar_Text.m_ctrl.m_count,
            
            roots = m_document.m_roots.Raw,

            root_buckets = m_ar_Root.m_ctrl.m_buckets,
            root_ctrl = (NNodeIdCtrl*)m_ar_Root.m_ctrl.m_ctrls.m_items,
            root_root_data = m_st_Root_RootData.m_data.m_items,
            root_common_style_data = m_st_Root_CommonStyleData.m_data.m_items,
            root_childs_data = m_st_Root_ChildsData.m_data.m_items,
            root_style_data = m_st_Root_ViewStyleData.m_data.m_items,
            root_layout_data = m_st_Root_ViewLayoutData.m_data.m_items,

            view_buckets = m_ar_View.m_ctrl.m_buckets,
            view_ctrl = (NNodeIdCtrl*)m_ar_View.m_ctrl.m_ctrls.m_items,
            view_common_style_data = m_st_View_CommonStyleData.m_data.m_items,
            view_childs_data = m_st_View_ChildsData.m_data.m_items,
            view_style_data = m_st_View_ViewStyleData.m_data.m_items,
            view_layout_data = m_st_View_ViewLayoutData.m_data.m_items,

            text_buckets = m_ar_Text.m_ctrl.m_buckets,
            text_ctrl = (NNodeIdCtrl*)m_ar_Text.m_ctrl.m_ctrls.m_items,
            text_common_style_data = m_st_Text_CommonStyleData.m_data.m_items,
            text_style_data = m_st_Text_TextStyleData.m_data.m_items,
            
            rounding = true,
        };
        m_layout.Calc(&ctx).TryThrowWithMsg();
    }
}
