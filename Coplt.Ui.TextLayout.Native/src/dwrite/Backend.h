#pragma once

#include <dwrite.h>
#include "../Com.h"

namespace Coplt
{
    struct Backend : ComObject<IUnknown>
    {
        Rc<IDWriteFactory> m_dw_factory;

        explicit Backend(Rc<IDWriteFactory>&& m_dw_factory);

        static HResult Create(Rc<Backend>& out);

        HResult GetSystemFontCollection(Rc<IFontCollection>& out) const;
    };
}
