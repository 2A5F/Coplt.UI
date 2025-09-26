#pragma once

#include <dwrite.h>

#include "../Com.h"

namespace Coplt
{
    struct Backend;

    struct SystemFontCollection final : ComObject<IFontCollection>
    {
        Rc<IDWriteFactory> m_dw_factory;
        Rc<IDWriteFontCollection> m_collection;

        explicit SystemFontCollection(
            Rc<IDWriteFactory>&& dw_factory,
            Rc<IDWriteFontCollection>&& collection
        );

        static HResult Create(const Backend* backend, Rc<SystemFontCollection>& out);
    };
}
