#include "Layout.h"

#include "../FFIUtils.h"
#include "../lib.h"
#include "Error.h"
#include "TextLayout.h"

using namespace Coplt;

extern "C" int32_t coplt_ui_layout_calc(Layout* self, NLayoutContext* ctx);

Layout::Layout(Rc<LibUi> lib, Rc<IDWriteTextAnalyzer1>& text_analyzer)
    : m_lib(std::move(lib)),
      m_text_analyzer(std::move(text_analyzer))
{
}

Rc<Layout> Layout::Create(Rc<LibUi> lib)
{
    Rc<IDWriteTextAnalyzer> analyzer;
    if (const auto hr = lib->m_backend->m_dw_factory->CreateTextAnalyzer(analyzer.put()); FAILED(hr))
        throw ComException(hr, "Failed to create text analyzer");
    Rc<IDWriteTextAnalyzer1> analyzer1;
    if (const auto hr = analyzer->QueryInterface(analyzer1.put()); FAILED(hr))
        throw ComException(hr, "Failed to create text analyzer");

    return Rc(new Layout(std::move(lib), analyzer1));
}

HResult Layout::Impl_Calc(NLayoutContext* ctx)
{
    return Internal::BitCast<HResult>(coplt_ui_layout_calc(this, ctx));
}

namespace Coplt::LayoutCalc
{
    COPLT_FORCE_INLINE
    ChildsData* GetChildsData(NLayoutContext* ctx, i32 index, NodeType type)
    {
        switch (type)
        {
        case NodeType::View:
            return &ctx->view_childs_data[index];
        case NodeType::Text:
            std::unreachable();
        case NodeType::Root:
            return &ctx->root_childs_data[index];
        default:
            std::unreachable();
        }
    }

    COPLT_FORCE_INLINE
    ContainerStyleData* GetContainerStyleData(NLayoutContext* ctx, i32 index, NodeType type)
    {
        switch (type)
        {
        case NodeType::View:
            return &ctx->view_container_style_data[index];
        case NodeType::Text:
            std::unreachable();
        case NodeType::Root:
            return &ctx->root_container_style_data[index];
        default:
            std::unreachable();
        }
    }

    COPLT_FORCE_INLINE
    ContainerLayoutData* GetContainerLayoutData(NLayoutContext* ctx, i32 index, NodeType type)
    {
        switch (type)
        {
        case NodeType::View:
            return &ctx->view_container_layout_data[index];
        case NodeType::Text:
            std::unreachable();
        case NodeType::Root:
            return &ctx->root_container_layout_data[index];
        default:
            std::unreachable();
        }
    }

    COPLT_FORCE_INLINE
    TextData* GetTextData(NLayoutContext* ctx, i32 index, NodeType type)
    {
        switch (type)
        {
        case NodeType::View:
            std::unreachable();
        case NodeType::Text:
            &ctx->text_data[index];
        case NodeType::Root:
            std::unreachable();
        default:
            std::unreachable();
        }
    }
}

namespace Coplt::LayoutCalc::Texts
{
    extern "C" void coplt_ui_layout_analyze_text(
        Layout* self, NLayoutContext* ctx,
        i32 node_index, NodeType node_type
    )
    {
        auto& childs = *GetChildsData(ctx, node_index, node_type);
        auto& container_layout = *GetContainerLayoutData(ctx, node_index, node_type);
        const auto& container_style = *GetContainerStyleData(ctx, node_index, node_type);

        if (container_layout.TextLayoutObject == nullptr)
        {
            container_layout.TextLayoutObject = new TextLayout();
        }

        for (auto it = iter(childs.m_childs); it.MoveNext();)
        {
        }
        // todo
    }
}
