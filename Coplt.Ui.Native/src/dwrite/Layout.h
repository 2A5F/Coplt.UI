#pragma once

#include <dwrite_3.h>
#include "../Com.h"
#include "../Layout.h"
#include "../Map.h"

namespace Coplt
{
    struct LibUi;

    struct Layout final : ComImpl<Layout, ILayout>
    {
        Rc<LibUi> m_lib;
        Rc<IDWriteTextAnalyzer1> m_text_analyzer;
        Rc<IDWriteFontFallback1> m_system_font_fallback;

        explicit Layout(Rc<LibUi> lib, Rc<IDWriteTextAnalyzer1>& analyzer, Rc<IDWriteFontFallback1>& font_fallback);

        static Rc<Layout> Create(Rc<LibUi> lib);

        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        HResult Impl_Calc(NLayoutContext* ctx);

        COPLT_IMPL_END

        HResult Calc(NLayoutContext* ctx);
    };
} // namespace Coplt
