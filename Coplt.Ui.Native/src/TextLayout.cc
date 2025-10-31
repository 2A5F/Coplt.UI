#include "TextLayout.h"

#include "Algorithm.h"

using namespace Coplt;

BaseTextLayoutStorage::BaseTextLayoutStorage()
{
    ClearCache();
}

void BaseTextLayoutStorage::ClearCache()
{
    m_paragraphs.clear();
    m_items.clear();
    m_scopes.clear();
    m_scope_stack.clear();
    m_paragraphs.push_back(Paragraph{0, 0, 0});
}

void BaseTextLayoutStorage::AddText(NodeId Parent, const u32 Length)
{
    if (Length == 0) return;
    const auto scope = m_scope_stack.empty() ? -1 : m_scope_stack.back();
    auto& paragraph = m_paragraphs.back();
    m_items.push_back(Item{
        .Start = paragraph.LogicTextLength,
        .Length = Length,
        .Scope = scope,
        .NodeOrParent = Parent,
        .Type = ItemType::Text,
    });
    paragraph.Length++;
    paragraph.LogicTextLength += Length;
}

void BaseTextLayoutStorage::AddInlineBlock(const NodeId Node)
{
    const auto scope = m_scope_stack.empty() ? -1 : m_scope_stack.back();
    auto& paragraph = m_paragraphs.back();
    m_items.push_back(Item{
        .Start = paragraph.LogicTextLength,
        .Length = 1,
        .Scope = scope,
        .NodeOrParent = Node,
        .Type = ItemType::InlineBlock,
    });
    paragraph.Length++;
    paragraph.LogicTextLength += 1;
}

void BaseTextLayoutStorage::AddBlock(const NodeId Node)
{
    m_paragraphs.push_back(Paragraph{m_items.size(), 1, 1});
    const auto scope = m_scope_stack.empty() ? -1 : m_scope_stack.back();
    m_items.push_back(Item{
        .Start = 0,
        .Length = 1,
        .Scope = scope,
        .NodeOrParent = Node,
        .Type = ItemType::Block,
    });
}

void BaseTextLayoutStorage::StartScope(const NodeId Node)
{
    m_scope_stack.push_back(static_cast<u32>(m_scopes.size()));
    m_scopes.push_back(Node);
}

void BaseTextLayoutStorage::EndScope()
{
    m_scope_stack.pop_back();
}

const BaseTextLayoutStorage::Item* BaseTextLayoutStorage::SearchItem(const u32 Paragraph, const u32 Position) const
{
    const auto& paragraph = m_paragraphs[Paragraph];
    const auto index = Algorithm::BinarySearch(
        m_items.data(), static_cast<i32>(paragraph.Start), static_cast<i32>(paragraph.Length), Position,
        [](const Item& item, const u32 pos)
        {
            if (pos < item.Start) return -1;
            if (pos >= item.Start + item.Length) return 1;
            return 0;
        });
    if (index < 0) return nullptr;
    return &m_items[index];
}
