#include "TextLayout.h"
#include "TextLayout.h"

#include "Algorithm.h"

#include "Layout.h"

using namespace Coplt::LayoutCalc::Texts;

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
    if (m_paragraphs.empty() || m_paragraphs.back().Type != TextParagraphType::Inline)
        m_paragraphs.push_back(Paragraph{{}, m_items.size(), 0, 0, TextParagraphType::Inline});
    const auto scope = m_scope_stack.empty() ? -1 : m_scope_stack.back();
    auto& paragraph = m_paragraphs.back();
    m_items.push_back(TextItem{
        .LogicTextStart = paragraph.LogicTextLength,
        .LogicTextLength = Length,
        .Scope = scope,
        .TextIndex = Index,
        .NodeOrParent = Parent,
        .Type = TextItemType::Text,
    });
    paragraph.ItemLength++;
    paragraph.LogicTextLength += Length;
}

void BaseTextLayoutStorage::AddInlineBlock(const NodeId Node)
{
    if (m_paragraphs.empty() || m_paragraphs.back().Type != TextParagraphType::Inline)
        m_paragraphs.push_back(Paragraph{{}, m_items.size(), 0, 0, TextParagraphType::Inline});
    const auto scope = m_scope_stack.empty() ? -1 : m_scope_stack.back();
    auto& paragraph = m_paragraphs.back();
    m_items.push_back(TextItem{
        .LogicTextStart = paragraph.LogicTextLength,
        .LogicTextLength = 1,
        .Scope = scope,
        .NodeOrParent = Node,
        .Type = TextItemType::InlineBlock,
    });
    paragraph.ItemLength++;
    paragraph.LogicTextLength += 1;
}

void BaseTextLayoutStorage::AddBlock(const NodeId Node)
{
    if (m_paragraphs.empty() || m_paragraphs.back().Type != TextParagraphType::Block)
        m_paragraphs.push_back(Paragraph{{}, m_items.size(), 0, 0, TextParagraphType::Block});
    const auto scope = m_scope_stack.empty() ? -1 : m_scope_stack.back();
    auto& paragraph = m_paragraphs.back();
    m_items.push_back(TextItem{
        .LogicTextStart = 0,
        .LogicTextLength = 1,
        .Scope = scope,
        .NodeOrParent = Node,
        .Type = TextItemType::Block,
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

void BaseTextLayoutStorage::FinishBuild()
{
    for (auto& paragraph : m_paragraphs)
    {
        COPLT_DEBUG_ASSERT(paragraph.ItemLength != 0, "There should be no empty paragraphs.");

        const std::span items(m_items.data() + paragraph.ItemStart, paragraph.ItemLength);
        if (items.size() == 1)
        {
            const auto& item = items.front();
            paragraph.ScopeRanges.push_back(TextScopeRange{
                .ItemStart = paragraph.ItemStart,
                .ItemLength = 1,
                .Scope = item.Scope,
                .LogicTextStart = item.LogicTextStart,
                .LogicTextLength = item.LogicTextLength,
            });
            continue;
        }

        std::optional<u32> scope = std::nullopt;
        u32 logic_text_start = 0, logic_text_length = 0, last_item_index = 0;
        for (u32 i = 0; i < items.size(); ++i)
        {
            const auto& item = items[i];
            if (scope == item.Scope)
            {
                logic_text_length += item.LogicTextLength;
            }
            else
            {
                if (i != 0)
                {
                    COPLT_DEBUG_ASSERT(scope.has_value());
                    paragraph.ScopeRanges.push_back(TextScopeRange{
                        .ItemStart = paragraph.ItemStart + last_item_index,
                        .ItemLength = i - last_item_index,
                        .Scope = scope.value(),
                        .LogicTextStart = logic_text_start,
                        .LogicTextLength = logic_text_length,
                    });
                }
                scope = item.Scope;
                logic_text_length = item.LogicTextLength;
                logic_text_start = item.LogicTextStart;
                last_item_index = i;
            }
        }
        if (last_item_index < items.size())
        {
            COPLT_DEBUG_ASSERT(scope.has_value());
            paragraph.ScopeRanges.push_back(TextScopeRange{
                .ItemStart = paragraph.ItemStart + last_item_index,
                .ItemLength = static_cast<u32>(items.size()) - last_item_index,
                .Scope = scope.value(),
                .LogicTextStart = logic_text_start,
                .LogicTextLength = logic_text_length,
            });
        }
    }
}

i32 BaseTextLayoutStorage::SearchItem(const u32 Paragraph, const u32 Position) const
{
    const auto& paragraph = m_paragraphs[Paragraph];
    if (paragraph.Type == TextParagraphType::Block) return -1;
    if (Position >= paragraph.LogicTextLength) return -1;
    const auto index = Algorithm::BinarySearch(
        m_items.data(), static_cast<i32>(paragraph.ItemStart), static_cast<i32>(paragraph.ItemLength), Position,
        [](const TextItem& item, const u32 pos)
        {
            if (pos < item.LogicTextStart) return 1;
            if (pos >= item.LogicTextStart + item.LogicTextLength) return -1;
            return 0;
        });
    if (index < 0) return -1;
    return index;
}

const char16* LayoutCalc::Texts::GetText(NLayoutContext* ctx, const TextItem* item)
{
    switch (item->Type)
    {
    case TextItemType::Text:
        {
            const auto node = CtxNodeRef(ctx, item->NodeOrParent);
            const auto text = node.GetText(item->TextIndex);
            return text.m_str;
        }
    case TextItemType::InlineBlock:
    case TextItemType::Block:
        return COPLT_STR16("￼");
    }
    return nullptr;
}
