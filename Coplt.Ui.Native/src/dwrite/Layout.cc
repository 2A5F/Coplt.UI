#include "Layout.h"

#include <print>
#include "fmt/xchar.h"

#include "../FFIUtils.h"
#include "../lib.h"
#include "Error.h"
#include "TextLayout.h"
#include "TextData.h"

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
            return &ctx->text_data[index];
        case NodeType::Root:
            std::unreachable();
        default:
            std::unreachable();
        }
    }
}

namespace Coplt::LayoutCalc::Texts
{
    void do_coplt_ui_layout_analyze_text(
        Layout* self, NLayoutContext* ctx,
        i32 node_index, NodeType node_type
    );

    extern "C" HResultE coplt_ui_layout_analyze_text(
        Layout* self, NLayoutContext* ctx,
        i32 node_index, NodeType node_type
    )
    {
        return feb([&]
        {
            do_coplt_ui_layout_analyze_text(
                self, ctx, node_index, node_type
            );
            return HResultE::Ok;
        });
    }

    void do_coplt_ui_layout_analyze_text(
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
            const auto child_locate = it.Current();
            auto& text_data = *GetTextData(ctx, child_locate.Index, NodeType::Text);

            self->m_lib->m_logger.Log(LogLevel::Info, text_data.m_text.m_size, text_data.m_text.m_items);

            Rc<IDWriteFontFallback> fallback;
            if (const auto hr = self->m_lib->m_backend->m_dw_factory->GetSystemFontFallback(fallback.put()); FAILED(hr))
                throw ComException(hr, "Failed to get system font fallback");

            SliceTextAnalysisSource src(text_data.m_text.m_items, text_data.m_text.m_size);


            for (auto i = 0; i < text_data.m_text.m_size;)
            {
                u32 mapped_length = 0;
                Rc<IDWriteFont> font;
                float scale;

                if (const auto hr = fallback->MapCharacters(
                    &src, i, text_data.m_text.m_size - i, nullptr, nullptr, DWRITE_FONT_WEIGHT_NORMAL,
                    DWRITE_FONT_STYLE_NORMAL,
                    DWRITE_FONT_STRETCH_NORMAL, &mapped_length, font.put(), &scale); FAILED(hr))
                    throw ComException(hr, "Failed to map characters");
                const auto ci = i;
                i += mapped_length;

                Rc<IDWriteFontFamily> family;
                font->GetFontFamily(family.put());

                Rc<IDWriteLocalizedStrings> name;
                family->GetFamilyNames(name.put());

                const auto count = name->GetCount();
                for (u32 j = 0; j < count; ++j)
                {
                    u32 local_len, str_len;
                    name->GetLocaleNameLength(j, &local_len);
                    name->GetStringLength(j, &str_len);
                    std::wstring local(local_len + 1, 0);
                    std::wstring str(str_len + 1, 0);
                    name->GetLocaleName(j, local.data(), local_len + 1);
                    name->GetString(j, str.data(), str_len + 1);
                    self->m_lib->m_logger.Log(LogLevel::Info, fmt::format(L"{} .. {}; {} : {}", ci, i, local.c_str(), str.c_str()));
                }
            }
        }
        // todo
    }
}
