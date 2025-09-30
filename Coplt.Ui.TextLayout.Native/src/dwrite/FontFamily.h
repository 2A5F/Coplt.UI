#pragma once

#include <dwrite.h>

#include "../Com.h"

namespace Coplt
{
    struct FontFamily final : ComObject<IFontFamily>
    {
        Rc<IDWriteFontFamily> m_family;
        std::vector<std::wstring> m_names;
        std::vector<Str16> m_str_names;

        explicit FontFamily(
            Rc<IDWriteFontFamily>& family,
            std::vector<std::wstring>& names,
            std::vector<Str16> str_names
        );

        static Rc<FontFamily> Create(
            Rc<IDWriteFontFamily>& family
        );

        const Str16* Impl_GetNames(u32* length) const override;
        void Impl_ClearNativeNamesCache() override;
    };
}
