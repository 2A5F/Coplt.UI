#pragma once

#include <vector>

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

        struct Item
        {
            u32 Start;
            u32 Length;
            u32 Scope;
            NodeId NodeOrParent;
            ItemType Type;
        };

        struct Paragraph
        {
            u32 Start;
            u32 Length;
            u32 LogicTextLength;
        };

        std::vector<Paragraph> m_paragraphs{};
        std::vector<Item> m_items{};
        std::vector<NodeId> m_scopes{};
        std::vector<u32> m_scope_stack{};

        BaseTextLayoutStorage();

        void ClearCache();
        void AddText(NodeId Parent, u32 Length);
        void AddInlineBlock(NodeId Node);
        void AddBlock(NodeId Node);
        void StartScope(NodeId Node);
        void EndScope();

        // return null if not find
        const Item* SearchItem(u32 Paragraph, u32 Position) const;
    };

    template <class Self>
    struct BaseTextLayout : ComImpl<Self, ITextLayout>, BaseTextLayoutStorage
    {
    };
}
