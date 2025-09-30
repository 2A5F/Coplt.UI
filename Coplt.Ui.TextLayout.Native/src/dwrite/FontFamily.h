#pragma once

#include <dwrite.h>

#include "../Com.h"
#include "../../../ThirdParty/emhash/hash_table8.hpp"

namespace Coplt
{
    struct FontFamily final : ComObject<IFontFamily>
    {
        Rc<IDWriteFontFamily> m_family;

        std::vector<std::pair<std::wstring, u32>> m_names;
        std::vector<FontFamilyNameInfo> m_str_names;
        std::vector<std::wstring> m_local_names;
        emhash8::HashMap<std::wstring, u32> m_local_name_mapper;
        std::vector<Str16> m_str_local_names;

        explicit FontFamily(
            Rc<IDWriteFontFamily>& family,
            std::vector<std::pair<std::wstring, u32>>& names,
            std::vector<FontFamilyNameInfo>& str_names,
            std::vector<std::wstring>& local_names,
            emhash8::HashMap<std::wstring, u32>& local_name_mapper,
            std::vector<Str16>& str_local_names
        );

        static Rc<FontFamily> Create(
            Rc<IDWriteFontFamily>& family
        );

        IFont* const* Impl_GetFonts(u32* length) const override;
        const Str16* Impl_GetLocalNames(u32* length) const override;
        const FontFamilyNameInfo* Impl_GetNames(u32* length) const override;
        void Impl_ClearNativeNamesCache() override;
    };
}
