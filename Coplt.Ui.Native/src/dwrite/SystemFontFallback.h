#pragma once

#include <dwrite_3.h>

#include "../Com.h"
#include "Backend.h"
#include "BaseFontFallback.h"

namespace Coplt
{
    struct SystemFontFallback final : BaseFontFallback
    {
        explicit SystemFontFallback(
            Rc<IDWriteFactory7> dw_factory,
            Rc<IDWriteFontFallback1>& fallback
        );

        static Rc<IFontFallback> Create(const TextBackend* backend);
    };
} // namespace Coplt
