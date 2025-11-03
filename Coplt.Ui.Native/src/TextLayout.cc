#include "TextLayout.h"
#include "TextLayout.h"

#include "Algorithm.h"

#include "Layout.h"

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
}

void BaseTextLayoutStorage::AddText(NodeId Parent, u32 Index, const u32 Length)
{
    if (Length == 0) return;
    if (m_paragraphs.empty() || m_paragraphs.back().Type != ParagraphType::Inline)
        m_paragraphs.push_back(Paragraph{m_items.size(), 0, 0, ParagraphType::Inline});
    const auto scope = m_scope_stack.empty() ? -1 : m_scope_stack.back();
    auto& paragraph = m_paragraphs.back();
    m_items.push_back(Item{
        .LogicTextStart = paragraph.LogicTextLength,
        .Length = Length,
        .Scope = scope,
        .TextIndex = Index,
        .NodeOrParent = Parent,
        .Type = ItemType::Text,
    });
    paragraph.ItemLength++;
    paragraph.LogicTextLength += Length;
}

void BaseTextLayoutStorage::AddInlineBlock(const NodeId Node)
{
    if (m_paragraphs.empty() || m_paragraphs.back().Type != ParagraphType::Inline)
        m_paragraphs.push_back(Paragraph{m_items.size(), 0, 0, ParagraphType::Inline});
    const auto scope = m_scope_stack.empty() ? -1 : m_scope_stack.back();
    auto& paragraph = m_paragraphs.back();
    m_items.push_back(Item{
        .LogicTextStart = paragraph.LogicTextLength,
        .Length = 1,
        .Scope = scope,
        .NodeOrParent = Node,
        .Type = ItemType::InlineBlock,
    });
    paragraph.ItemLength++;
    paragraph.LogicTextLength += 1;
}

void BaseTextLayoutStorage::AddBlock(const NodeId Node)
{
    if (m_paragraphs.empty() || m_paragraphs.back().Type != ParagraphType::Block)
        m_paragraphs.push_back(Paragraph{m_items.size(), 0, 0, ParagraphType::Block});
    const auto scope = m_scope_stack.empty() ? -1 : m_scope_stack.back();
    auto& paragraph = m_paragraphs.back();
    m_items.push_back(Item{
        .LogicTextStart = 0,
        .Length = 1,
        .Scope = scope,
        .NodeOrParent = Node,
        .Type = ItemType::Block,
    });
    paragraph.ItemLength++;
    paragraph.LogicTextLength += 1;
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

i32 BaseTextLayoutStorage::SearchItem(const u32 Paragraph, const u32 Position) const
{
    const auto& paragraph = m_paragraphs[Paragraph];
    if (paragraph.Type == ParagraphType::Block) return -1;
    if (Position >= paragraph.LogicTextLength) return -1;
    const auto index = Algorithm::BinarySearch(
        m_items.data(), static_cast<i32>(paragraph.ItemStart), static_cast<i32>(paragraph.ItemLength), Position,
        [](const Item& item, const u32 pos)
        {
            if (pos < item.LogicTextStart) return -1;
            if (pos >= item.LogicTextStart + item.Length) return 1;
            return 0;
        });
    if (index < 0) return -1;
    return index;
}

const char16* Coplt::GetText(NLayoutContext* ctx, const BaseTextLayoutStorage::Item* item)
{
    switch (item->Type)
    {
    case BaseTextLayoutStorage::ItemType::Text:
        {
            const auto node = LayoutCalc::CtxNodeRef(ctx, item->NodeOrParent);
            const auto text = node.GetText(item->TextIndex);
            return text.m_str;
        }
    case BaseTextLayoutStorage::ItemType::InlineBlock:
    case BaseTextLayoutStorage::ItemType::Block:
        return COPLT_STR16("￼");
    }
    return nullptr;
}
