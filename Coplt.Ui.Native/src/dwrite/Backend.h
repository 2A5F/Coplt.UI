#pragma once

#include <dwrite_3.h>
#include "../Com.h"

namespace Coplt
{
    struct TextBackend : RefCount<TextBackend>
    {
        Rc<IDWriteFactory7> m_dw_factory;
        Rc<IFontManager> m_font_manager;

        explicit TextBackend(Rc<IDWriteFactory7>&& dw_factory);

        static Rc<TextBackend> Create(void* dw_factory);

        Rc<IFontCollection> GetSystemFontCollection() const;

        Rc<IFontFallback> GetSystemFontFallback() const;

        Rc<IFontFallbackBuilder> CreateFontFallbackBuilder(const FontFallbackBuilderCreateInfo& info) const;

        HResultE CreateLayout(ILayout** out) const;
    };
}
