#include "FontFallbackBuilder.h"

#include "CustomFontFallback.h"

using namespace Coplt;

FontFallbackBuilder::FontFallbackBuilder(
    const TextBackend* backend, const FontFallbackBuilderCreateInfo& info
)
    : m_factory(backend->m_dw_factory), m_use_system_fallback(!info.DisableSystemFallback)
{
    if (const auto hr = backend->m_dw_factory->GetSystemFontCollection(
        false,
        DWRITE_FONT_FAMILY_MODEL_TYPOGRAPHIC,
        m_system_font_collection.put()
    ); FAILED(hr))
        throw ComException(hr, "Failed to get system font collection");

    if (const auto hr = backend->m_dw_factory->CreateFontFallbackBuilder(m_builder.put()); FAILED(hr))
        throw ComException(hr, "Failed to create font fallback builder");
}

Rc<CustomFontFallback> FontFallbackBuilder::Build() const
{
    if (m_use_system_fallback)
    {
        Rc<IDWriteFontFallback> sys_fallback{};
        if (const auto hr = m_factory->GetSystemFontFallback(sys_fallback.put()); FAILED(hr))
            throw ComException(hr, "Failed to get system font fallback");

        if (const auto hr = m_builder->AddMappings(sys_fallback.get()); FAILED(hr))
            throw ComException(hr, "Failed to add system font fallback");
    }

    return CustomFontFallback::Create(m_factory, m_builder.get());
}

bool FontFallbackBuilder::Add(char16 const* name, const i32 length) const
{
    return Add(nullptr, name, length);
}

bool FontFallbackBuilder::Add(char16 const* locale, char16 const* name, const i32 length) const
{
    u32 index = -1;
    BOOL exists = false;
    if (const auto hr = m_system_font_collection->FindFamilyName(name, &index, &exists); FAILED(hr))
        throw ComException(hr, "Failed to find family name");
    if (!exists) return false;
    Rc<IDWriteFontFamily2> family{};
    if (const auto hr = m_system_font_collection->GetFontFamily(index, family.put()))
        throw ComException(hr, "Failed to get font family");

    Rc<IDWriteFont> font{};
    if (const auto hr = family->GetFirstMatchingFont(
        DWRITE_FONT_WEIGHT_REGULAR,
        DWRITE_FONT_STRETCH_NORMAL,
        DWRITE_FONT_STYLE_NORMAL,
        font.put()
    ); FAILED(hr))
        throw ComException(hr, "Failed to get first matching font");
    Rc<IDWriteFont1> font1{};
    if (const auto hr = font->QueryInterface(font1.put()); FAILED(hr))
        throw ComException(hr, "Failed to get font1");
    u32 count = 0;
    if (const auto hr = font1->GetUnicodeRanges(0, nullptr, &count); hr != E_NOT_SUFFICIENT_BUFFER && FAILED(hr))
        throw ComException(hr, "Failed to get unicode ranges");
    std::vector<DWRITE_UNICODE_RANGE> ranges(count);
    if (const auto hr = font1->GetUnicodeRanges(count, ranges.data(), &count); FAILED(hr))
        throw ComException(hr, "Failed to get unicode ranges");

    char16 const* names[] =
    {
        name,
    };

    if (const auto hr = m_builder->AddMapping(
        ranges.data(),
        ranges.size(),
        names,
        std::size(names),
        m_system_font_collection.get(),
        locale,
        nullptr,
        1
    ); FAILED(hr))
        throw ComException(hr, "Failed to add mapping");

    return true;
}

HResult FontFallbackBuilder::Impl_Build(IFontFallback** ff)
{
    return feb(
        [&]
        {
            auto out = Build();
            *ff = out.leak();
            return HResultE::Ok;
        }
    );
}

HResult FontFallbackBuilder::Impl_Add(char16 const* name, i32 length, bool* exists)
{
    return feb(
        [&]
        {
            *exists = Add(name, length);
            return HResultE::Ok;
        }
    );
}

HResult FontFallbackBuilder::Impl_AddLocaled(char16 const* locale, char16 const* name, i32 name_length, bool* exists)
{
    return feb(
        [&]
        {
            *exists = Add(locale, name, name_length);
            return HResultE::Ok;
        }
    );
}
