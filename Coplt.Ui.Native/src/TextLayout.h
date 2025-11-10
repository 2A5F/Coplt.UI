#pragma once

#include <vector>
#include <span>

#include "Com.h"

namespace Coplt::LayoutCalc
{
    enum class TextParagraphType
    {
        Inline,
        Block,
    };

    // opaque
    struct TextParagraph
    {
        TextParagraphType Type;
    };

    // opaque
    struct TextRun
    {
    };
}

namespace Coplt::LayoutCalc::Texts
{
    enum class TextItemType
    {
        Text,
        InlineBlock,
        Block,
    };

    struct TextItem
    {
        u32 LogicTextStart;
        u32 LogicTextLength;
        u32 Scope;
        u32 TextIndex;
        NodeId NodeOrParent;
        TextItemType Type;
    };

    struct TextScopeRange
    {
        u32 ItemStart;
        u32 ItemLength;
        u32 Scope;
        u32 LogicTextStart;
        u32 LogicTextLength;
    };

    struct TextParagraphImpl : TextParagraph
    {
        std::vector<TextScopeRange> ScopeRanges{};
        u32 ItemStart;
        u32 ItemLength;
        u32 LogicTextLength;
    };

    struct BaseTextLayoutStorage
    {
        std::vector<TextParagraphImpl> m_paragraphs{};
        std::vector<TextItem> m_items{};
        std::vector<NodeId> m_scopes{};
        std::vector<u32> m_scope_stack{};

        BaseTextLayoutStorage();

        void ClearCache();
        void AddText(NodeId Parent, u32 Index, u32 Length);
        void AddInlineBlock(NodeId Node);
        void AddBlock(NodeId Node);
        void StartScope(NodeId Node);
        void EndScope();
        void FinishBuild();

        // return null if not find
        i32 SearchItem(u32 Paragraph, u32 Position) const;
    };

    template <class Self>
    struct BaseTextLayout : ComImpl<Self, ITextLayout>, BaseTextLayoutStorage
    {
    };

    const char16* GetText(NLayoutContext* ctx, const TextItem* item);

    extern "C" void coplt_ui_layout_text_get_paragraphs(
        ITextLayout* layout, TextParagraph** out_paragraphs, u32* out_count, u32* out_stride
    );
}
