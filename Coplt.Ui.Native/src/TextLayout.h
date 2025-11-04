#pragma once

#include <vector>
#include <span>

#include "Com.h"

namespace Coplt
{
    struct BaseTextLayoutStorage
    {
        enum class ItemType
        {
            Text,
            InlineBlock,
            Block,
        };

        enum class ParagraphType
        {
            Inline,
            Block,
        };

        struct Item
        {
            u32 LogicTextStart;
            u32 LogicTextLength;
            u32 Scope;
            u32 TextIndex;
            NodeId NodeOrParent;
            ItemType Type;
        };

        struct ScopeRange
        {
            u32 ItemStart;
            u32 ItemLength;
            u32 Scope;
            u32 LogicTextStart;
            u32 LogicTextLength;
        };

        struct Paragraph
        {
            std::vector<ScopeRange> ScopeRanges{};
            u32 ItemStart;
            u32 ItemLength;
            u32 LogicTextLength;
            ParagraphType Type;
        };

        std::vector<Paragraph> m_paragraphs{};
        std::vector<Item> m_items{};
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

    const char16* GetText(NLayoutContext* ctx, const BaseTextLayoutStorage::Item* item);
}
