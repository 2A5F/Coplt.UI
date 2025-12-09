#include "TextLayout.h"
#include "TextLayout.h"

#include "Algorithm.h"

#include "Layout.h"

using namespace Coplt::LayoutCalc::Texts;

BaseTextLayoutStorage::BaseTextLayoutStorage()
{
}

i32 BaseTextLayoutStorage::SearchItem(const u32 Paragraph, const u32 Position) const
{
    return -1;
    // const auto& paragraph = m_paragraphs[Paragraph];
    // if (paragraph.Type == TextParagraphType::Block) return -1;
    // if (Position >= paragraph.LogicTextLength) return -1;
    // const auto index = Algorithm::BinarySearch(
    //     m_items.data(), static_cast<i32>(paragraph.ItemStart), static_cast<i32>(paragraph.ItemLength), Position,
    //     [](const TextItem& item, const u32 pos)
    //     {
    //         if (pos < item.LogicTextStart) return 1;
    //         if (pos >= item.LogicTextStart + item.LogicTextLength) return -1;
    //         return 0;
    //     });
    // if (index < 0) return -1;
    // return index;
}

// const char16* LayoutCalc::Texts::GetText(NLayoutContext* ctx, const TextItem* item)
// {
//     // switch (item->Type)
//     // {
//     // case TextItemType::Text:
//     //     {
//     //         // const auto node = CtxNodeRef(ctx, item->NodeOrParent);
//     //         // const auto text = node.GetText(item->TextIndex);
//     //         // return text.m_str;
//     //         return nullptr;
//     //     }
//     // case TextItemType::InlineBlock:
//     // case TextItemType::Block:
//     //     return COPLT_STR16("￼");
//     // }
//     return nullptr;
// }
