#pragma once

#include <vector>
#include <span>

#include "Com.h"
#include "LayoutCommon.h"

namespace Coplt::LayoutCalc::Texts
{
    enum class TextItemType
    {
        Text,
        InlineBlock,
        Block,
    };

    struct TextScopeRange
    {
        u32 ItemStart;
        u32 ItemLength;
        u32 Scope;
        u32 LogicTextStart;
        u32 LogicTextLength;
    };

    enum class TextParagraphType : u8
    {
        Inline,
        Block,
    };

    struct Paragraph
    {
        std::vector<TextScopeRange> ScopeRanges{};
        u32 ItemStart;
        u32 ItemLength;
        u32 LogicTextLength;
        TextParagraphType Type;
    };

    struct BaseTextLayoutStorage
    {
        std::vector<Paragraph> m_paragraphs{};
        // std::vector<TextItem> m_items{};
        std::vector<NodeId> m_scopes{};
        std::vector<u32> m_scope_stack{};

        BaseTextLayoutStorage();

        // return null if not find
        i32 SearchItem(u32 Paragraph, u32 Position) const;
    };

    template <class Self>
    struct BaseTextLayout : ComImpl<Self, ITextLayout>, BaseTextLayoutStorage
    {
    };

    // const char16* GetText(NLayoutContext* ctx, const TextItem* item);

    extern "C" HResultE coplt_ui_layout_text_compute(
        void* sub_doc, ITextLayout* layout, NLayoutContext* ctx, const NodeId& node,
        const LayoutInputs* inputs, LayoutOutput* outputs
    );

    extern "C" HResultE coplt_ui_layout_compute_child_layout(
        void* sub_doc, const NodeId& node,
        const LayoutInputs* inputs, LayoutOutput* outputs
    );

    inline void ComputeChildLayout(
        void* sub_doc, const NodeId& node,
        const LayoutInputs* inputs, LayoutOutput* outputs
    )
    {
        const auto r = coplt_ui_layout_compute_child_layout(sub_doc, node, inputs, outputs);
        if (r != HResultE::Ok) throw r;
    }
}
