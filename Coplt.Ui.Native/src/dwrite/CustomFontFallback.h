#pragma once

#include <dwrite_3.h>

#include "../Com.h"
#include "Backend.h"
#include "BaseFontFallback.h"

namespace Coplt
{
    struct CustomFontFallback final : BaseFontFallback
    {
        explicit CustomFontFallback(
            Rc<IDWriteFactory7> dw_factory,
            Rc<IDWriteFontFallback1>& fallback
        );

        static Rc<CustomFontFallback> Create(const Rc<IDWriteFactory7>& factory, IDWriteFontFallbackBuilder* builder);
    };
}
