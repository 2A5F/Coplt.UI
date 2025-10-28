#pragma once

#include <dwrite_3.h>
#include "../Com.h"

namespace Coplt
{
    struct TextBackend : RefCount<TextBackend>
    {
        Rc<IDWriteFactory7> m_dw_factory;

        explicit TextBackend(Rc<IDWriteFactory7>&& m_dw_factory);

        static HResult Create(Rc<TextBackend>& out);

        Rc<IFontCollection> GetSystemFontCollection() const;

        Rc<IFontFallback> GetSystemFontFallback() const;
    };
}
