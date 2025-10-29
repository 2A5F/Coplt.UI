#include "FontFamily.h"

#include <system_error>
#include <fmt/format.h>

#include "Error.h"
#include "Font.h"

using namespace Coplt;

FontFamily::FontFamily(Rc<IDWriteFontFamily2>& family) : m_family(std::move(family))
{
    Rc<IDWriteLocalizedStrings> names;
    if (const auto hr = m_family->GetFamilyNames(names.put()); FAILED(hr))
        throw ComException(hr, "Failed to get family names");
    const auto num_names = names->GetCount();
    m_names.reserve(num_names);
    m_str_names.reserve(num_names);
    for (u32 i = 0; i < num_names; ++i)
    {
        u32 len;
        if (const auto hr = names->GetLocaleNameLength(i, &len); FAILED(hr))
            throw ComException(hr, "Failed to get locale name length");
        std::wstring local(len, L'\0');
        if (const auto hr = names->GetLocaleName(i, local.data(), len + 1); FAILED(hr))
            throw ComException(hr, "Failed to get locale name");
        auto [it, inserted] = m_local_name_mapper.try_emplace(local, m_local_names.size());
        if (inserted) m_local_names.push_back(std::move(local));
        const auto local_index = it->second;
        if (const auto hr = names->GetStringLength(i, &len); FAILED(hr))
            throw ComException(hr, "Failed to get string length");
        std::wstring str(len, L'\0');
        if (const auto hr = names->GetString(i, str.data(), len + 1); FAILED(hr))
            throw ComException(hr, "Failed to get string");
        m_names.emplace_back(std::move(str), local_index);
    }
    const auto num_local_names = m_local_names.size();
    m_str_local_names.reserve(num_local_names);
    for (u32 i = 0; i < num_local_names; ++i)
    {
        auto& name = m_local_names[i];
        m_str_local_names.push_back(Str16(name.data(), name.length()));
    }
    for (u32 i = 0; i < num_names; ++i)
    {
        auto& [name, local_index] = m_names[i];
        m_str_names.push_back({
            Str16(name.data(), name.length()),
            local_index,
        });
    }
}

const Str16* FontFamily::Impl_GetLocalNames(u32* length) const
{
    *length = m_str_local_names.size();
    return m_str_local_names.data();
}

const FontFamilyNameInfo* FontFamily::Impl_GetNames(u32* length) const
{
    *length = m_str_names.size();
    return m_str_names.data();
}

void FontFamily::Impl_ClearNativeNamesCache()
{
    m_str_names = {};
    m_names = {};
    m_local_names = {};
    m_local_name_mapper = {};
    m_str_local_names = {};
}

HResult FontFamily::Impl_GetFonts(u32* length, NFontPair const** pair)
{
    if (!m_has_fonts)
    {
        return feb([&]
        {
            const auto num_fonts = m_family->GetFontCount();
            m_fonts.reserve(num_fonts);
            m_p_fonts.reserve(num_fonts);
            for (u32 i = 0; i < num_fonts; ++i)
            {
                Rc<IDWriteFont3> d_font;
                if (const auto r = m_family->GetFont(i, d_font.put()); FAILED(r))
                    throw ComException(r, "Failed to get font");
                Rc font(new Font(d_font));
                m_p_fonts.push_back(NFontPair{font.get(), &font->m_info});
                m_fonts.push_back(std::move(font));
            }
            m_has_fonts = true;
            *length = m_p_fonts.size();
            *pair = m_p_fonts.data();
            return HResultE::Ok;
        });
    }
    *length = m_p_fonts.size();
    *pair = m_p_fonts.data();
    return HResultE::Ok;
}

void FontFamily::Impl_ClearNativeFontsCache()
{
    m_p_fonts.clear();
    m_fonts.clear();
    m_has_fonts = false;
}
