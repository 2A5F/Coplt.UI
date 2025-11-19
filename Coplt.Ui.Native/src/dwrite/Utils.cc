#include "Utils.h"

using namespace Coplt;

std::wstring Coplt::GetFontFamilyName(const Rc<IDWriteFontFamily>& family)
{
    Rc<IDWriteLocalizedStrings> strings{};
    if (const auto hr = family->GetFamilyNames(strings.put()); FAILED(hr))
        throw ComException(hr, "Failed to get family names");

    std::wstring name{};
    const auto count = strings->GetCount();
    for (int i = 0; i < count; i++)
    {
        u32 len{};
        if (const auto hr = strings->GetLocaleNameLength(i, &len); FAILED(hr))
            throw ComException(hr, "Failed to get locale name length");
        std::wstring locale(len + 1, 0);
        if (const auto hr = strings->GetLocaleName(i, locale.data(), locale.size()); FAILED(hr))
            throw ComException(hr, "Failed to get locale name");

        if (const auto hr = strings->GetStringLength(i, &len); FAILED(hr))
            throw ComException(hr, "Failed to get string length");
        std::wstring str(len + 1, 0);
        if (const auto hr = strings->GetString(i, str.data(), str.size()); FAILED(hr))
            throw ComException(hr, "Failed to get string");

        if (i != 0) name.append(L", ");
        name.append(fmt::format(L"{}: {}", locale.c_str(), str.c_str()));
    }

    return name;
}

std::wstring Coplt::GetFontFaceName(const Rc<IDWriteFont>& font)
{
    Rc<IDWriteLocalizedStrings> strings{};
    if (const auto hr = font->GetFaceNames(strings.put()); FAILED(hr))
        throw ComException(hr, "Failed to get family names");

    std::wstring name{};
    const auto count = strings->GetCount();
    for (int i = 0; i < count; i++)
    {
        u32 len{};
        if (const auto hr = strings->GetLocaleNameLength(i, &len); FAILED(hr))
            throw ComException(hr, "Failed to get locale name length");
        std::wstring locale(len + 1, 0);
        if (const auto hr = strings->GetLocaleName(i, locale.data(), locale.size()); FAILED(hr))
            throw ComException(hr, "Failed to get locale name");

        if (const auto hr = strings->GetStringLength(i, &len); FAILED(hr))
            throw ComException(hr, "Failed to get string length");
        std::wstring str(len + 1, 0);
        if (const auto hr = strings->GetString(i, str.data(), str.size()); FAILED(hr))
            throw ComException(hr, "Failed to get string");

        if (i != 0) name.append(L", ");
        name.append(fmt::format(L"{}: {}", locale.c_str(), str.c_str()));
    }

    return name;
}

std::wstring Coplt::GetFontFamilyName(const Rc<IDWriteFont>& font)
{
    Rc<IDWriteFontFamily> family{};
    if (const auto hr = font->GetFontFamily(family.put()); FAILED(hr))
        throw ComException(hr, "Failed to get font family");

    return GetFontFamilyName(family);
}

std::wstring Coplt::GetFontName(const Rc<IDWriteFont>& font)
{
    std::wstring name{};

    name.append(L"[");
    name.append(GetFontFamilyName(font));
    name.append(L"] => [");
    name.append(GetFontFaceName(font));
    name.append(L"]");

    return name;
}

std::wstring Coplt::GetFamilyNames(const Rc<IDWriteFontFace5>& font)
{
    Rc<IDWriteLocalizedStrings> strings{};
    if (const auto hr = font->GetFamilyNames(strings.put()); FAILED(hr))
        throw ComException(hr, "Failed to get family names");

    std::wstring name{};
    const auto count = strings->GetCount();
    for (int i = 0; i < count; i++)
    {
        u32 len{};
        if (const auto hr = strings->GetLocaleNameLength(i, &len); FAILED(hr))
            throw ComException(hr, "Failed to get locale name length");
        std::wstring locale(len + 1, 0);
        if (const auto hr = strings->GetLocaleName(i, locale.data(), locale.size()); FAILED(hr))
            throw ComException(hr, "Failed to get locale name");

        if (const auto hr = strings->GetStringLength(i, &len); FAILED(hr))
            throw ComException(hr, "Failed to get string length");
        std::wstring str(len + 1, 0);
        if (const auto hr = strings->GetString(i, str.data(), str.size()); FAILED(hr))
            throw ComException(hr, "Failed to get string");

        if (i != 0) name.append(L", ");
        name.append(fmt::format(L"{}: {}", locale.c_str(), str.c_str()));
    }

    return name;
}

std::wstring Coplt::GetFaceNames(const Rc<IDWriteFontFace5>& font)
{
    Rc<IDWriteLocalizedStrings> strings{};
    if (const auto hr = font->GetFaceNames(strings.put()); FAILED(hr))
        throw ComException(hr, "Failed to get family names");

    std::wstring name{};
    const auto count = strings->GetCount();
    for (int i = 0; i < count; i++)
    {
        u32 len{};
        if (const auto hr = strings->GetLocaleNameLength(i, &len); FAILED(hr))
            throw ComException(hr, "Failed to get locale name length");
        std::wstring locale(len + 1, 0);
        if (const auto hr = strings->GetLocaleName(i, locale.data(), locale.size()); FAILED(hr))
            throw ComException(hr, "Failed to get locale name");

        if (const auto hr = strings->GetStringLength(i, &len); FAILED(hr))
            throw ComException(hr, "Failed to get string length");
        std::wstring str(len + 1, 0);
        if (const auto hr = strings->GetString(i, str.data(), str.size()); FAILED(hr))
            throw ComException(hr, "Failed to get string");

        if (i != 0) name.append(L", ");
        name.append(fmt::format(L"{}: {}", locale.c_str(), str.c_str()));
    }

    return name;
}
