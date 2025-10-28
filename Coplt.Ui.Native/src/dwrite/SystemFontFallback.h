#pragma once

#include <dwrite_3.h>

#include "../Com.h"
#include "Backend.h"

namespace Coplt
{
    struct SystemFontFallback final : ComImpl<SystemFontFallback, IFontFallback>
    {
        Rc<IDWriteFactory7> m_dw_factory;
        Rc<IDWriteFontFallback> m_fallback;

        explicit SystemFontFallback(
            Rc<IDWriteFactory7> dw_factory,
            Rc<IDWriteFontFallback>& fallback
        );

        static Rc<IFontFallback> Create(const TextBackend* backend);

        COPLT_IMPL_START
        COPLT_IMPL_END
    };
} // namespace Coplt
