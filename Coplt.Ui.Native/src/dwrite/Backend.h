#pragma once

#include <dwrite_3.h>
#include "../Com.h"
#include "FontManager.h"

namespace Coplt
{
    struct TextBackend : RefCount<TextBackend>
    {
        Rc<IDWriteFactory7> m_dw_factory;

        explicit TextBackend(Rc<IDWriteFactory7>&& dw_factory);

        static Rc<TextBackend> Create(void* dw_factory);

        Rc<DWriteFontManager> CreateFontManager() const;

        Rc<IFontCollection> GetSystemFontCollection() const;

        Rc<IFontFallback> GetSystemFontFallback() const;
    };
}
