using System.Runtime.CompilerServices;
using Coplt.UI.Collections;
using Coplt.UI.Native;
using Coplt.UI.Styles;
using Coplt.UI.Trees.Datas;

namespace Coplt.UI.Trees.Modules;

public sealed unsafe class LayoutModule : Document.IModule
{
    public static Document.IModule Create(Document document) => new LayoutModule();

    public void Update(Document document)
    {
        var ctx = new Ctx(document);
        ctx.ConsumeLayoutDirty();
        ctx.NativePart();
    }

    private ref struct Ctx(Document document)
    {
        private readonly ref HierarchyData hierarchy_datas = ref document.StorageOf<HierarchyData>().AsCommon().GetDataRef();
        private readonly CommonData* common_datas = document.StorageOf<CommonData>().AsPinned().GetDataPtr();
        private readonly ChildsData* childs_datas = document.StorageOf<ChildsData>().AsPinned().GetDataPtr();
        private readonly StyleData* style_datas = document.StorageOf<StyleData>().AsPinned().GetDataPtr();

        private ref HierarchyData HierarchyDataAt(NodeId id) => ref Unsafe.Add(ref hierarchy_datas, id.Index);
        private CommonData* CommonDataAt(NodeId id) => &common_datas[id.Index];
        private ChildsData* ChildsDataAt(NodeId id) => &childs_datas[id.Index];
        private StyleData* StyleDataAt(NodeId id) => &style_datas[id.Index];

        public void ConsumeLayoutDirty()
        {
            foreach (var kv in document.m_roots)
            {
                ConsumeLayoutDirty(kv.Key, new(uint.MaxValue), null);
            }
        }

        private void ConsumeLayoutDirty(NodeId node, ViewNode parent, NativeBox<TextData>* p_text_data)
        {
            var common = CommonDataAt(node);
            var style = StyleDataAt(node);
            if (common->LastLayoutVersion == common->LayoutVersion && p_text_data == null) return;
            common->LastLayoutVersion = common->LayoutVersion;
            common->LayoutCache.Flags = LayoutCacheFlags.Empty;
            if (style->Container == Container.Text)
            {
                ref var hierarchy = ref HierarchyDataAt(node);
                var childs = ChildsDataAt(node);
                var end_scope = false;
                var text_root = false;
                if (p_text_data != null)
                {
                    if (style->Position == Position.Absolute)
                    {
                        p_text_data->m_ptr->AddAbsoluteBlock(ref hierarchy_datas, parent, node);
                    }
                    else if (style->TextMode == TextMode.Inline)
                    {
                        p_text_data->m_ptr->StartScope(ref hierarchy_datas, node);
                        end_scope = true;
                    }
                    else
                    {
                        p_text_data->m_ptr->AddBlock(ref hierarchy_datas, parent, node);
                    }
                }
                ref var next_p_text_data = ref p_text_data;
                if (p_text_data == null || style->Position == Position.Absolute || style->TextMode != TextMode.Inline)
                {
                    text_root = true;
                    if (childs->m_text_data)
                    {
                        next_p_text_data = &childs->m_text_data;
                    }
                    else
                    {
                        childs->m_text_data = NativeBox<TextData>.NewZeroed();
                        next_p_text_data = &childs->m_text_data;
                        next_p_text_data->m_ptr->m_text_root = node;
                    }
                    next_p_text_data->m_ptr->Clear(ref hierarchy_datas);
                }
                else
                {
                    childs->m_text_data.Dispose();
                }
                foreach (var child in *childs)
                {
                    if (child.Type == NodeType.Text)
                    {
                        var text = hierarchy.m_texts[child.Index];
                        next_p_text_data->m_ptr->AddText(ref hierarchy_datas, node, child, (uint)text.Length);
                    }
                    else
                    {
                        ConsumeLayoutDirty(child, node, next_p_text_data);
                    }
                }
                if (end_scope) p_text_data->m_ptr->EndScope(ref hierarchy_datas);
                if (text_root) next_p_text_data->m_ptr->FinishBuild(ref hierarchy_datas);
            }
            else
            {
                var childs = ChildsDataAt(node);
                if (childs->m_text_data) childs->m_text_data.Dispose();
                if (p_text_data != null)
                {
                    if (style->Position == Position.Absolute)
                    {
                        p_text_data->m_ptr->AddAbsoluteBlock(ref hierarchy_datas, parent, node);
                    }
                    else if (style->TextMode == TextMode.Inline)
                    {
                        p_text_data->m_ptr->AddInlineBlock(ref hierarchy_datas, parent, node);
                    }
                    else
                    {
                        p_text_data->m_ptr->AddBlock(ref hierarchy_datas, parent, node);
                    }
                }
                foreach (var child in *childs)
                {
                    ConsumeLayoutDirty(child, node, null);
                }
            }
        }

        public void NativePart()
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
                    node_common_data = common_datas,
                    node_childs_data = childs_datas,
                    node_style_data = style_datas,

                    node_count = document.m_arche.m_ctrl.m_count,

                    rounding = true,
                };
                layout.Calc(&ctx).TryThrowWithMsg();
            }
        }
    }
}
