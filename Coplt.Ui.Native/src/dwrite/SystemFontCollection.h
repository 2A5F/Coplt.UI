#pragma once

#include <dwrite_3.h>

#include "../Com.h"
#include "FontFamily.h"

namespace Coplt
{
    struct Backend;

    struct SystemFontCollection final : ComObject<IFontCollection>
    {
        Rc<IDWriteFactory7> m_dw_factory;
        Rc<IDWriteFontCollection3> m_collection;
        std::vector<Rc<FontFamily>> m_families;
        std::vector<IFontFamily*> m_p_families;

        explicit SystemFontCollection(
            Rc<IDWriteFactory7> dw_factory,
            Rc<IDWriteFontCollection3>& collection,
            std::vector<Rc<FontFamily>>& families,
            std::vector<IFontFamily*>& p_families
        );

        static Rc<SystemFontCollection> Create(const Backend* backend);

        IFontFamily* const* Impl_GetFamilies(u32* count) const override;
        void Impl_ClearNativeFamiliesCache() override;

        u32 Impl_FindDefaultFamily() override;
    };
}
