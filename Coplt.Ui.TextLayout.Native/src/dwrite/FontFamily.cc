#include "FontFamily.h"

#include <system_error>
#include <fmt/format.h>

#include "Error.h"

using namespace Coplt;

FontFamily::FontFamily(
    Rc<IDWriteFontFamily>& family,
    std::vector<std::pair<std::wstring, u32>>& names,
    std::vector<FontFamilyNameInfo>& str_names,
    std::vector<std::wstring>& local_names,
    emhash8::HashMap<std::wstring, u32>& local_name_mapper,
    std::vector<Str16>& str_local_names
) :
    m_family(std::move(family)),
    m_names(std::move(names)),
    m_str_names(std::move(str_names)),
    m_local_names(std::move(local_names)),
    m_local_name_mapper(std::move(local_name_mapper)),
    m_str_local_names(std::move(str_local_names))
{
}

Rc<FontFamily> FontFamily::Create(Rc<IDWriteFontFamily>& family)
{
    Rc<IDWriteLocalizedStrings> names;
    if (const auto hr = family->GetFamilyNames(names.put()); FAILED(hr))
        throw ComException(hr, "Failed to get family names");
    const auto num_names = names->GetCount();
    std::vector<std::wstring> local_names;
    emhash8::HashMap<std::wstring, u32> local_name_mapper;
    std::vector<Str16> str_local_names;
    std::vector<std::pair<std::wstring, u32>> w_names;
    std::vector<FontFamilyNameInfo> str_names;
    w_names.reserve(num_names);
    str_names.reserve(num_names);
    for (u32 i = 0; i < num_names; ++i)
    {
        u32 len;
        if (const auto hr = names->GetLocaleNameLength(i, &len); FAILED(hr))
            throw ComException(hr, "Failed to get locale name length");
        std::wstring local(len, L'\0');
        if (const auto hr = names->GetLocaleName(i, local.data(), len + 1); FAILED(hr))
            throw ComException(hr, "Failed to get locale name");
        auto [it, inserted] = local_name_mapper.try_emplace(local, local_names.size());
        if (inserted) local_names.push_back(std::move(local));
        const auto local_index = it->second;
        if (const auto hr = names->GetStringLength(i, &len); FAILED(hr))
            throw ComException(hr, "Failed to get string length");
        std::wstring str(len, L'\0');
        if (const auto hr = names->GetString(i, str.data(), len + 1); FAILED(hr))
            throw ComException(hr, "Failed to get string");
        w_names.emplace_back(std::move(str), local_index);
    }
    const auto num_local_names = local_names.size();
    str_local_names.reserve(num_local_names);
    for (u32 i = 0; i < num_local_names; ++i)
    {
        auto& name = local_names[i];
        str_local_names.push_back(Str16(name.data(), name.length()));
    }
    for (u32 i = 0; i < num_names; ++i)
    {
        auto& [name, local_index] = w_names[i];
        str_names.push_back({
            Str16(name.data(), name.length()),
            local_index,
        });
    }
    return Rc(new FontFamily(family, w_names, str_names, local_names, local_name_mapper, str_local_names));
}

IFont* const* FontFamily::Impl_GetFonts(u32* length) const
{
    // todo
    return nullptr;
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
