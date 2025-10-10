#pragma once

#include <dwrite_3.h>

#include "../Com.h"
#include "../../../ThirdParty/emhash/hash_table8.hpp"

#include "Font.h"

namespace Coplt
{
    struct FontFamily final : ComObject<IFontFamily>
    {
        Rc<IDWriteFontFamily2> m_family;

        std::vector<Rc<Font>> m_fonts;
        std::vector<NFontPair> m_p_fonts;
        bool m_has_fonts{false};

        std::vector<std::pair<std::wstring, u32>> m_names;
        std::vector<FontFamilyNameInfo> m_str_names;
        std::vector<std::wstring> m_local_names;
        emhash8::HashMap<std::wstring, u32> m_local_name_mapper;
        std::vector<Str16> m_str_local_names;

        explicit FontFamily(
            Rc<IDWriteFontFamily2>& family
        );

        const Str16* Impl_GetLocalNames(u32* length) const override;
        const FontFamilyNameInfo* Impl_GetNames(u32* length) const override;
        void Impl_ClearNativeNamesCache() override;
        NFontPair const* Impl_GetFonts(u32* length) override;
        void Impl_ClearNativeFontsCache() override;
    };
}
