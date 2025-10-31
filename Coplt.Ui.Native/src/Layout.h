#pragma once

#include "Com.h"
#include "Assert.h"
#include "FFI.h"
#include "Map.h"
#include "TextLayout.h"

namespace Coplt::LayoutCalc
{
    COPLT_FORCE_INLINE
    ChildsData* GetChildsData(NLayoutContext* ctx, u32 index)
    {
        return &ctx->node_childs_data[index];
    }

    COPLT_FORCE_INLINE
    StyleData* GetStyleData(NLayoutContext* ctx, u32 index)
    {
        return &ctx->node_style_data[index];
    }

    COPLT_FORCE_INLINE
    CommonData* GetCommonData(NLayoutContext* ctx, u32 index)
    {
        return &ctx->node_common_data[index];
    }

    struct CtxNodeRef
    {
        NLayoutContext* ctx;
        NodeId id;

        explicit CtxNodeRef(
            NLayoutContext* ctx, const NodeId id
        ) : ctx(ctx), id(id)
        {
        }

        NodeType Type() const
        {
            return FFIUtils::GetType(id);
        }

        ChildsData& ChildsData() const
        {
            return *GetChildsData(ctx, id.Index);
        }

        StyleData& StyleData() const
        {
            return *GetStyleData(ctx, id.Index);
        }

        CommonData& CommonData() const
        {
            return *GetCommonData(ctx, id.Index);
        }
    };

    // collect and spread dirty
    void Phase0(CtxNodeRef node);

    // rebuild text layout
    template <class TextLayout> requires std::is_base_of_v<BaseTextLayout<TextLayout>, TextLayout>
    void Phase1(
        const CtxNodeRef node,
        const NodeId* parent_id = nullptr,
        ChildsData* parent_childs = nullptr,
        TextLayout* cur_text_layout = nullptr
    )
    {
        if (node.Type() == NodeType::Text)
        {
            if (!cur_text_layout) return;
            auto& texts = *ffi_map(&parent_childs->m_texts);
            auto& text = *texts.UnsafeAt(node.id.Index);
            if (text.m_len == 0) return;
            cur_text_layout->AddText(*parent_id, text.m_len);
        }
        else
        {
            auto& data = node.CommonData();
            const auto& style = node.StyleData();
            auto& childs = node.ChildsData();

            const auto is_layout_dirty = HasFlags(data.DirtyFlags, DirtyFlags::Layout);
            const auto is_text_dirty = HasFlags(data.DirtyFlags, DirtyFlags::TextLayout);

            COPLT_DEBUG_ASSERT(
                is_text_dirty ? is_layout_dirty : true,
                "If is_text_dirty is true, then is_layout_dirty must also be true."
            );

            if (!is_layout_dirty) return;

            bool need_end_scope = false;
            if (cur_text_layout == nullptr)
            {
                if (style.Container == Container::Text)
                {
                    if (data.TextLayoutObject == nullptr)
                    {
                        data.TextLayoutObject = cur_text_layout = new TextLayout();
                    }
                    else if (is_text_dirty)
                    {
                        cur_text_layout = static_cast<TextLayout*>(data.TextLayoutObject);
                        cur_text_layout->ClearCache();
                    }
                    else
                    {
                        cur_text_layout = nullptr;
                    }
                }
            }
            else
            {
                if (style.Container == Container::Text)
                {
                    if (data.TextLayoutObject != nullptr)
                    {
                        static_cast<TextLayout*>(data.TextLayoutObject)->ClearCache();
                        data.TextLayoutObject->Release();
                        data.TextLayoutObject = nullptr;
                    }
                }
                if (style.TextMode == TextMode::Inline)
                {
                    if (style.Container == Container::Text)
                    {
                        need_end_scope = true;
                        cur_text_layout->StartScope(node.id);
                    }
                    else
                    {
                        cur_text_layout->AddInlineBlock(node.id);
                    }
                }
                else
                {
                    cur_text_layout->AddBlock(node.id);
                }
            }

            if (cur_text_layout)
            {
                // process childs
                for (auto e = FFIUtils::GetEnumerator(&childs.m_childs); e.MoveNext();)
                {
                    auto child = CtxNodeRef(node.ctx, *e.Current());
                    Phase1(child, &node.id, &childs, cur_text_layout);
                }
            }

            if (need_end_scope)
            {
                cur_text_layout->EndScope(node.id);
            }
        }
    }
}
