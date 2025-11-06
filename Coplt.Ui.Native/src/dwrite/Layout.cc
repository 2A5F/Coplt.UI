#include "Layout.h"

#include <print>
#include "fmt/xchar.h"

#include "../lib.h"
#include "Error.h"
#include "TextLayout.h"

using namespace Coplt;

extern "C" int32_t coplt_ui_layout_calc(Layout* self, NLayoutContext* ctx);

Layout::Layout(Rc<LibUi> lib, Rc<IDWriteTextAnalyzer1>& text_analyzer, Rc<IDWriteFontFallback1>& font_fallback)
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

    Rc<IDWriteFontFallback1> font_fallback1;
    if (const auto hr = font_fallback->QueryInterface(font_fallback1.put()); FAILED(hr))
        throw ComException(hr, "Failed to get system font fallback");

    return Rc(new Layout(std::move(lib), analyzer1, font_fallback1));
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
    void do_coplt_ui_layout_touch_text(
        Layout* self, CtxNodeRef node
    );

    extern "C" HResultE coplt_ui_layout_touch_text(
        void* self, NLayoutContext* ctx, const NodeId& node
    )
    {
        return feb([&]
        {
            do_coplt_ui_layout_touch_text(
                static_cast<Layout*>(self), CtxNodeRef(ctx, node)
            );
            return HResultE::Ok;
        });
    }

    void do_coplt_ui_layout_touch_text(
        Layout* self, CtxNodeRef node
    )
    {
        auto& data = node.CommonData();
        auto& style = node.StyleData();
        if (style.Container != Container::Text || data.TextLayoutObject == nullptr) return;

        const auto is_text_dirty = data.LastTextLayoutVersion != data.TextLayoutVersion;

        if (data.TextLayoutObject == nullptr) throw NullPointerError();
        auto text_layout = static_cast<TextLayout*>(data.TextLayoutObject);

        if (is_text_dirty)
        {
            text_layout->ReBuild(self, node);
            data.LastTextLayoutVersion &= data.TextLayoutVersion;
        }
    }
}
