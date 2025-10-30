using System.Diagnostics;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Native;
using Coplt.UI.Trees.Datas;

namespace Coplt.UI.Trees.Modules;

public sealed unsafe class LayoutModule : Document.IModule
{
    public static Document.IModule Create(Document document) => new LayoutModule();

    public void Update(Document document)
    {
        ref var layout = ref NativeLib.Instance.m_layout;
        var ctx = new NLayoutContext
        {
            roots = document.m_roots.Raw,

            node_buckets = document.m_arche.m_ctrl.m_buckets,
            node_ctrl = document.m_arche.m_ctrl.m_ctrls.m_items,
            node_common_data = document.m_arche.StorageOf<CommonData>().AsPinned().m_data.m_items,
            node_childs_data = document.m_arche.StorageOf<ChildsData>().AsPinned().m_data.m_items,
            node_style_data = document.m_arche.StorageOf<StyleData>().AsPinned().m_data.m_items,

            root_count = document.m_roots.Count,
            node_count = document.m_arche.m_ctrl.m_count,

            rounding = true,
        };
        layout.Calc(&ctx).TryThrowWithMsg();
    }
}
