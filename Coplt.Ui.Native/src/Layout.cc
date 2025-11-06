#include "Layout.h"

using namespace Coplt;

namespace Coplt::LayoutCalc
{
    void Impl_Phase0(
        const CtxNodeRef node,
        CommonData* cur_data = nullptr
    )
    {
        if (node.Type() == NodeType::Text) return;
        auto& data = node.CommonData();
        const auto& style = node.StyleData();
        auto& childs = node.ChildsData();

        const auto is_layout_dirty = data.LastLayoutVersion != data.LayoutVersion;
        const auto is_text_dirty = data.LastTextLayoutVersion != data.TextLayoutVersion;

        COPLT_DEBUG_ASSERT(
            is_text_dirty ? is_layout_dirty : true,
            "If is_text_dirty is true, then is_layout_dirty must also be true."
        );

        if (!is_layout_dirty) return;

        data.LayoutCache.HasFinalLayoutEntry = false;
        data.LayoutCache.HasMeasureEntries0 = false;
        data.LayoutCache.HasMeasureEntries1 = false;
        data.LayoutCache.HasMeasureEntries2 = false;
        data.LayoutCache.HasMeasureEntries3 = false;
        data.LayoutCache.HasMeasureEntries4 = false;
        data.LayoutCache.HasMeasureEntries5 = false;
        data.LayoutCache.HasMeasureEntries6 = false;
        data.LayoutCache.HasMeasureEntries7 = false;
        data.LayoutCache.HasMeasureEntries8 = false;
        data.LayoutCache.IsEmpty = true;

        // propagation text layout dirty
        if (cur_data == nullptr)
        {
            if (style.Container == Container::Text)
            {
                cur_data = &data;
            }
        }
        else
        {
            if (style.Container == Container::Text)
            {
                if (is_text_dirty && cur_data->LastTextLayoutVersion != cur_data->TextLayoutVersion)
                {
                    cur_data->TextLayoutVersion++;
                    data.LastTextLayoutVersion = data.TextLayoutVersion;
                }
            }
            else
            {
                cur_data = nullptr;
            }
        }

        // process childs
        for (auto e = FFIUtils::GetEnumerator<NodeId>(&childs.m_childs); e.MoveNext();)
        {
            auto child = CtxNodeRef(node.ctx, *e.Current());
            Impl_Phase0(child, cur_data);
        }
    }

    void Phase0(const CtxNodeRef node)
    {
        Impl_Phase0(node);
    }
}
