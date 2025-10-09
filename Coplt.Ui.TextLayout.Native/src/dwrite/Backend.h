#pragma once

#include <dwrite_3.h>
#include "../Com.h"

namespace Coplt
{
    struct Backend : ComObject<IUnknown>
    {
        Rc<IDWriteFactory7> m_dw_factory;

        explicit Backend(Rc<IDWriteFactory7>&& m_dw_factory);

        static HResult Create(Rc<Backend>& out);

        HResult GetSystemFontCollection(Rc<IFontCollection>& out) const;
    };
}
