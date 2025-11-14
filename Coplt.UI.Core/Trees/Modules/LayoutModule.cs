using System.Diagnostics;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Collections;
using Coplt.UI.Native;
using Coplt.UI.Trees.Datas;

namespace Coplt.UI.Trees.Modules;

public sealed unsafe class LayoutModule : Document.IModule
{
    public static Document.IModule Create(Document document) => new LayoutModule();

    public void Update(Document document)
    {
        ref var layout = ref NativeLib.Instance.m_layout;
        fixed (NativeMap<NodeId, RootData>* p_roots = &document.m_roots)
        {
            var ctx = new NLayoutContext
            {
                font_manager = document.m_font_manager.m_inner,

                roots = p_roots,

                node_buckets = document.m_arche.m_ctrl.m_buckets,
                node_ctrl = document.m_arche.m_ctrl.m_ctrls.m_items,
                node_common_data = document.m_arche.StorageOf<CommonData>().AsPinned().m_data.m_items,
                node_childs_data = document.m_arche.StorageOf<ChildsData>().AsPinned().m_data.m_items,
                node_style_data = document.m_arche.StorageOf<StyleData>().AsPinned().m_data.m_items,

                node_count = document.m_arche.m_ctrl.m_count,

                rounding = true,
            };
            layout.Calc(&ctx).TryThrowWithMsg();
        }
    }
}
