#pragma once

#include <dwrite_3.h>

#include "../Com.h"
#include "../../../ThirdParty/emhash/hash_table8.hpp"

#include "Font.h"

namespace Coplt
{
    struct FontFamily final : ComImpl<FontFamily, IFontFamily>
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

        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        Str16 const* Impl_GetLocalNames(COPLT_OUT u32* length) const;

        COPLT_FORCE_INLINE
        FontFamilyNameInfo const* Impl_GetNames(COPLT_OUT u32* length) const;

        COPLT_FORCE_INLINE
        void Impl_ClearNativeNamesCache();

        COPLT_FORCE_INLINE
        NFontPair const* Impl_GetFonts(COPLT_OUT u32* length);

        COPLT_FORCE_INLINE
        void Impl_ClearNativeFontsCache();

        COPLT_IMPL_END
    };
}
