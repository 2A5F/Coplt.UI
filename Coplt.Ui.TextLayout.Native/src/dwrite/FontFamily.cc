#include "FontFamily.h"

#include <system_error>
#include <fmt/format.h>

#include "Error.h"

using namespace Coplt;

FontFamily::FontFamily(
    Rc<IDWriteFontFamily>& family,
    std::vector<std::wstring>& names,
    std::vector<Str16> str_names
) :
    m_family(std::move(family)),
    m_names(std::move(names)),
    m_str_names(std::move(str_names))
{
}

Rc<FontFamily> FontFamily::Create(Rc<IDWriteFontFamily>& family)
{
    Rc<IDWriteLocalizedStrings> names;
    if (const auto hr = family->GetFamilyNames(names.put()); FAILED(hr))
        throw ComException(hr, "Failed to get family names");
    const auto num_names = names->GetCount();
    std::vector<std::wstring> w_names;
    std::vector<Str16> str_names;
    w_names.reserve(num_names);
    str_names.reserve(num_names);
    for (u32 i = 0; i < num_names; ++i)
    {
        u32 len;
        if (const auto hr = names->GetStringLength(i, &len); FAILED(hr))
            throw ComException(hr, "Failed to get string length");
        std::wstring str(len, L'\0');
        if (const auto hr = names->GetString(i, str.data(), len + 1); FAILED(hr))
            throw ComException(hr, "Failed to get string");
        w_names.push_back(std::move(str));
    }
    for (u32 i = 0; i < num_names; ++i)
    {
        str_names.push_back(Str16(w_names[i].data(), w_names[i].length()));
    }
    return Rc(new FontFamily(family, w_names, str_names));
}

const Str16* FontFamily::Impl_GetNames(u32* length) const
{
    *length = m_str_names.size();
    return m_str_names.data();
}

void FontFamily::Impl_ClearNativeNamesCache()
{
    m_str_names = {};
    m_names = {};
}
