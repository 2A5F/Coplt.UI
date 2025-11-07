#pragma once

#include "Com.h"
#include "Assert.h"
#include "FFI.h"
#include "Map.h"
#include "TextLayout.h"
#include "Utils.h"

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

        CtxNodeRef() : ctx(nullptr), id()
        {
        }

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

        const NString& GetText(const NodeId text) const
        {
            const auto& childs = ChildsData();
            auto& texts = *ffi_map<u32, NString>(&childs.m_texts);
            return *texts.UnsafeAt(text.Index);
        }

        const NString& GetText(const u32 index) const
        {
            const auto& childs = ChildsData();
            auto& texts = *ffi_map<u32, NString>(&childs.m_texts);
            return *texts.UnsafeAt(index);
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
            auto& texts = *ffi_map<u32, NString>(&parent_childs->m_texts);
            auto& text = *texts.UnsafeAt(node.id.Index);
            if (text.m_len == 0) return;
            cur_text_layout->AddText(*parent_id, node.id.Index, text.m_len);
        }
        else
        {
            auto& data = node.CommonData();
            const auto& style = node.StyleData();
            auto& childs = node.ChildsData();

            const auto is_layout_dirty = data.LastLayoutVersion != data.LayoutVersion;
            const auto is_text_dirty = data.LastTextLayoutVersion != data.TextLayoutVersion;

            COPLT_DEBUG_ASSERT(
                is_text_dirty ? is_layout_dirty : true,
                "If is_text_dirty is true, then is_layout_dirty must also be true."
            );

            bool need_end_scope = false, need_finish_layout_build = false;
            if (cur_text_layout == nullptr)
            {
                if (!is_layout_dirty) return;
                data.TextLayoutBelongTo = nullptr; // todo remove , use node id not layout ptr
                if (style.Container == Container::Text)
                {
                    if (data.TextLayoutObject == nullptr)
                    {
                        data.TextLayoutObject = cur_text_layout = new TextLayout();
                        need_finish_layout_build = true;
                    }
                    else if (is_text_dirty)
                    {
                        cur_text_layout = static_cast<TextLayout*>(data.TextLayoutObject);
                        cur_text_layout->ClearCache();
                        need_finish_layout_build = true;
                    }
                    else
                    {
                        cur_text_layout = nullptr;
                    }
                }
            }
            else
            {
                data.TextLayoutBelongTo = cur_text_layout;
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
                    if (
                        style.Container == Container::Text
                        && style.Width == LengthType::Auto
                        && style.Height == LengthType::Auto
                        && style.MinWidth == LengthType::Auto
                        && style.MinHeight == LengthType::Auto
                        && style.MaxWidth == LengthType::Auto
                        && style.MaxHeight == LengthType::Auto
                        && !style.HasAspectRatio
                        && IsZeroLength(style.InsertTop, style.InsertTopValue)
                        && IsZeroLength(style.InsertRight, style.InsertRightValue)
                        && IsZeroLength(style.InsertBottom, style.InsertBottomValue)
                        && IsZeroLength(style.InsertLeft, style.InsertLeftValue)
                        && IsZeroLength(style.MarginTop, style.MarginTopValue)
                        && IsZeroLength(style.MarginRight, style.MarginRightValue)
                        && IsZeroLength(style.MarginBottom, style.MarginBottomValue)
                        && IsZeroLength(style.MarginLeft, style.MarginLeftValue)
                        && IsZeroLength(style.PaddingTop, style.PaddingTopValue)
                        && IsZeroLength(style.PaddingRight, style.PaddingRightValue)
                        && IsZeroLength(style.PaddingBottom, style.PaddingBottomValue)
                        && IsZeroLength(style.PaddingLeft, style.PaddingLeftValue)
                        && IsZeroLength(style.BorderTop, style.BorderTopValue)
                        && IsZeroLength(style.BorderRight, style.BorderRightValue)
                        && IsZeroLength(style.BorderBottom, style.BorderBottomValue)
                        && IsZeroLength(style.BorderLeft, style.BorderLeftValue)
                        && style.OverflowX == Overflow::Visible
                        && style.OverflowY == Overflow::Visible
                    )
                    {
                        need_end_scope = true;
                        cur_text_layout->StartScope(node.id);
                    }
                    else
                    {
                        cur_text_layout->AddInlineBlock(node.id);
                        cur_text_layout = nullptr;
                    }
                }
                else
                {
                    cur_text_layout->AddBlock(node.id);
                    cur_text_layout = nullptr;
                }
            }

            // process childs
            for (auto e = FFIUtils::GetEnumerator<NodeId>(&childs.m_childs); e.MoveNext();)
            {
                auto child = CtxNodeRef(node.ctx, *e.Current());
                Phase1(child, &node.id, &childs, cur_text_layout);
            }

            if (need_end_scope)
            {
                COPLT_DEBUG_ASSERT(cur_text_layout);
                cur_text_layout->EndScope();
            }
            if (need_finish_layout_build)
            {
                COPLT_DEBUG_ASSERT(cur_text_layout);
                cur_text_layout->FinishBuild();
            }
        }
    }

    enum class LayoutRunMode : u8
    {
        PerformLayout,
        ComputeSize,
        PerformHiddenLayout,
    };

    namespace Texts
    {
        struct LayoutInputs
        {
            f32 KnownWidth;
            f32 KnownHeight;
            f32 ParentWidth;
            f32 ParentHeight;
            f32 AvailableSpaceWidthValue;
            f32 AvailableSpaceHeightValue;
            bool HasKnownWidth;
            bool HasKnownHeight;
            bool HasParentWidth;
            bool HasParentHeight;
            AvailableSpaceType AvailableSpaceWidth;
            AvailableSpaceType AvailableSpaceHeight;
            LayoutRunMode RunMode;
        };

        struct LayoutOutputs
        {
            f32 Width;
            f32 Height;
            f32 ContentWidth;
            f32 ContentHeight;
            f32 FirstBaselinesX;
            f32 FirstBaselinesY;
            f32 TopMarginPositive;
            f32 TopMarginNegative;
            f32 BottomMarginPositive;
            f32 BottomMarginNegative;
            bool HasFirstBaselinesX;
            bool HasFirstBaselinesT;
            bool MarginsCanCollapseThrough;
        };

        extern "C" HResultE coplt_ui_layout_touch_text(
            void* self, NLayoutContext* ctx, const NodeId& node
        );
    }
}
