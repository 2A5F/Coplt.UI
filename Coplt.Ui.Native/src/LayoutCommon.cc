#include "Layout.h"
#include "LayoutCommon.h"

using namespace Coplt;

LayoutCalc::ParagraphSpanInlineBlockTmpData::ParagraphSpanInlineBlockTmpData(CtxNodeRef node, Size<std::optional<f32>> inner_size)
    : m_layout_output(), m_node(node.id), m_margin(), m_padding(), m_border()
{
    const auto& style = node.StyleData();

    // todo

    Rect padding = GetPadding(style)
        .ResolveOrZero(inner_size);

    m_size = GetSize(style)
        .TryResolve(inner_size)
        .TryApplyAspectRatio(GetAspectRatio(style));
}
