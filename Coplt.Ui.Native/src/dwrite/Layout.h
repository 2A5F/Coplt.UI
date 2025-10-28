#pragma once

#include <dwrite_3.h>
#include "../Com.h"

namespace Coplt
{
    struct LibUi;

    struct Layout final : ComImpl<Layout, ILayout>
    {
        Rc<LibUi> m_lib;
        Rc<IDWriteTextAnalyzer1> m_text_analyzer;

        explicit Layout(Rc<LibUi> lib, Rc<IDWriteTextAnalyzer1>& analyzer);

        static Rc<Layout> Create(Rc<LibUi> lib);

        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        HResult Impl_Calc(NLayoutContext* ctx);

        COPLT_IMPL_END
    };
} // namespace Coplt
