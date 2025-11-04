#include "Layout.h"

#include <print>
#include "fmt/xchar.h"

#include "../lib.h"
#include "Error.h"
#include "TextLayout.h"

using namespace Coplt;

extern "C" int32_t coplt_ui_layout_calc(Layout* self, NLayoutContext* ctx);

Layout::Layout(Rc<LibUi> lib, Rc<IDWriteTextAnalyzer1>& text_analyzer, Rc<IDWriteFontFallback>& font_fallback)
    : m_lib(std::move(lib)),
      m_text_analyzer(std::move(text_analyzer)),
      m_system_font_fallback(std::move(font_fallback))
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

    Rc<IDWriteFontFallback> font_fallback;
    if (const auto hr = lib->m_backend->m_dw_factory->GetSystemFontFallback(font_fallback.put()); FAILED(hr))
        throw ComException(hr, "Failed to get system font fallback");

    return Rc(new Layout(std::move(lib), analyzer1, font_fallback));
}

HResult Layout::Impl_Calc(NLayoutContext* ctx)
{
    return feb([&]
    {
        return Calc(ctx);
    });
}

HResult Layout::Calc(NLayoutContext* ctx)
{
    using namespace LayoutCalc;
    const auto roots = ffi_map<NodeId, RootData>(ctx->roots);
    for (auto e = roots->GetEnumerator(); e.MoveNext();)
    {
        const auto& root = *e.Current().second;
        const auto node = CtxNodeRef(ctx, root.Node);
        Phase0(node);
        Phase1<TextLayout>(node);
    }
    return Internal::BitCast<HResult>(coplt_ui_layout_calc(this, ctx));
}

namespace Coplt::LayoutCalc::Texts
{
    void do_coplt_ui_layout_analyze_text(Layout* self, CtxNodeRef node, const TextAnalyzeInputs& inputs);

    extern "C" HResultE coplt_ui_layout_analyze_text(
        void* self, NLayoutContext* ctx, const NodeId& node, const TextAnalyzeInputs& inputs
    )
    {
        return feb([&]
        {
            do_coplt_ui_layout_analyze_text(
                static_cast<Layout*>(self), CtxNodeRef(ctx, node), inputs
            );
            return HResultE::Ok;
        });
    }

    void do_coplt_ui_layout_analyze_text(Layout* self, CtxNodeRef node, const TextAnalyzeInputs& inputs)
    {
        auto& childs = node.ChildsData();
        auto& data = node.CommonData();
        const auto& style = node.StyleData();

        const auto is_text_dirty = HasFlags(data.DirtyFlags, DirtyFlags::TextLayout);

        if (data.TextLayoutObject == nullptr) throw NullPointerError();
        auto text_layout = static_cast<TextLayout*>(data.TextLayoutObject);

        if (is_text_dirty)
        {
            text_layout->ReBuild(self, node);
        }

        // todo
    }
}
