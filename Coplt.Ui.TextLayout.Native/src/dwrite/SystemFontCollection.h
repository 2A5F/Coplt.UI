#pragma once

#include <dwrite.h>

#include "../Com.h"
#include "FontFamily.h"

namespace Coplt
{
    struct Backend;

    struct SystemFontCollection final : ComObject<IFontCollection>
    {
        Rc<IDWriteFactory> m_dw_factory;
        Rc<IDWriteFontCollection> m_collection;
        std::vector<Rc<FontFamily>> m_families;
        std::vector<IFontFamily*> m_p_families;

        explicit SystemFontCollection(
            Rc<IDWriteFactory> dw_factory,
            Rc<IDWriteFontCollection>& collection,
            std::vector<Rc<FontFamily>>& families,
            std::vector<IFontFamily*>& p_families
        );

        static Rc<SystemFontCollection> Create(const Backend* backend);

        IFontFamily* const* Impl_GetFamilies(u32* count) const override;
        void Impl_ClearNativeFamiliesCache() override;
    };
}
