#pragma once

#include <dwrite_3.h>

#include "../Com.h"

namespace Coplt
{
    struct BaseFontFallback : ComImpl<BaseFontFallback, IFontFallback>
    {
        Rc<IDWriteFactory7> m_dw_factory;
        Rc<IDWriteFontFallback> m_fallback;

        explicit BaseFontFallback(
            Rc<IDWriteFactory7>& dw_factory,
            Rc<IDWriteFontFallback>& fallback
        ) : m_dw_factory(std::move(dw_factory)), m_fallback(std::move(fallback))
        {
        }

        COPLT_IMPL_START
        COPLT_IMPL_END
    };
}
