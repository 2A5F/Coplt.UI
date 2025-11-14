#include "FontFace.h"

#include "Error.h"

using namespace Coplt;

DWriteFontFace::DWriteFontFace(Rc<IDWriteFontFace5>&& face)
    : m_face(std::forward<Rc<IDWriteFontFace5>>(face))
{
    InitInfo();
}

DWriteFontFace::DWriteFontFace(const Rc<IDWriteFontFace5>& face)
    : m_face(face)
{
    InitInfo();
}

void DWriteFontFace::InitInfo()
{
    DWRITE_FONT_METRICS1 metrics;
    m_face->GetMetrics(&metrics);

    m_info.Metrics.Ascent = static_cast<float>(metrics.ascent);
    m_info.Metrics.Descent = static_cast<float>(metrics.descent);
    m_info.Metrics.Leading = static_cast<float>(metrics.lineGap);
    m_info.Metrics.LineHeight = static_cast<float>(metrics.ascent + metrics.descent + metrics.lineGap);
    m_info.Metrics.UnitsPerEm = metrics.designUnitsPerEm;

    switch (m_face->GetStretch())
    {
    case DWRITE_FONT_STRETCH_UNDEFINED:
        m_info.Width.Width = 1.0;
        break;
    case DWRITE_FONT_STRETCH_ULTRA_CONDENSED:
        m_info.Width.Width = 0.5;
        break;
    case DWRITE_FONT_STRETCH_EXTRA_CONDENSED:
        m_info.Width.Width = 0.625;
        break;
    case DWRITE_FONT_STRETCH_CONDENSED:
        m_info.Width.Width = 0.75;
        break;
    case DWRITE_FONT_STRETCH_SEMI_CONDENSED:
        m_info.Width.Width = 0.775;
        break;
    case DWRITE_FONT_STRETCH_NORMAL:
        m_info.Width.Width = 1.0;
        break;
    case DWRITE_FONT_STRETCH_SEMI_EXPANDED:
        m_info.Width.Width = 1.125;
        break;
    case DWRITE_FONT_STRETCH_EXPANDED:
        m_info.Width.Width = 1.25;
        break;
    case DWRITE_FONT_STRETCH_EXTRA_EXPANDED:
        m_info.Width.Width = 1.5;
        break;
    case DWRITE_FONT_STRETCH_ULTRA_EXPANDED:
        m_info.Width.Width = 2.0;
        break;
    default:
        m_info.Width.Width = 1.0;
    }

    m_info.Weight = static_cast<FontWeight>(m_face->GetWeight());

    if (m_face->IsColorFont())
    {
        m_info.Flags |= FontFlags::Color;
    }

    if (m_face->IsMonospacedFont())
    {
        m_info.Flags |= FontFlags::Monospaced;
    }
}

NFontInfo const* DWriteFontFace::Impl_get_Info() const
{
    return &m_info;
}

bool DWriteFontFace::Impl_Equals(IFontFace* other) const
{
    return Equals(other);
}

i32 DWriteFontFace::Impl_HashCode() const
{
    return GetHashCode();
}

HResult DWriteFontFace::Impl_GetFamilyNames(void* ctx, Func<void, void*, char16*, i32, char16*, i32>* add) const
{
    return feb(
        [&] -> HResult
        {
            GetFamilyNames(ctx, add);
            return HResultE::Ok;
        }
    );
}

HResult DWriteFontFace::Impl_GetFaceNames(void* ctx, Func<void, void*, char16*, i32, char16*, i32>* add) const
{
    return feb(
        [&] -> HResult
        {
            GetFaceNames(ctx, add);
            return HResultE::Ok;
        }
    );
}

bool DWriteFontFace::Equals(IFontFace* other) const
{
    const auto o = static_cast<DWriteFontFace*>(other);
    if (m_face == nullptr) return o->m_face == nullptr;
    return m_face->Equals(o->m_face.get());
}

i32 DWriteFontFace::GetHashCode() const
{
    IDWriteFontFace* face = m_face.get();
    return reinterpret_cast<i32>(face) ^ static_cast<i32>(reinterpret_cast<u64>(face) >> 32);
}

void DWriteFontFace::GetFamilyNames(void* ctx, Func<void, void*, char16*, i32, char16*, i32>* add) const
{
    Rc<IDWriteLocalizedStrings> names{};
    if (const auto hr = m_face->GetFamilyNames(names.put()); FAILED(hr))
        throw ComException(hr, "Failed to get family names");
    GetNames(names.get(), ctx, add);
}

void DWriteFontFace::GetFaceNames(void* ctx, Func<void, void*, char16*, i32, char16*, i32>* add) const
{
    Rc<IDWriteLocalizedStrings> names{};
    if (const auto hr = m_face->GetFaceNames(names.put()); FAILED(hr))
        throw ComException(hr, "Failed to get face names");
    GetNames(names.get(), ctx, add);
}

void DWriteFontFace::GetNames(IDWriteLocalizedStrings* names, void* ctx, Func<void, void*, char16*, i32, char16*, i32>* add)
{
    const auto count = names->GetCount();
    std::vector<wchar_t> locale{};
    std::vector<wchar_t> string{};
    for (u32 i = 0; i < count; ++i)
    {
        u32 locale_len{};
        u32 string_len{};
        if (const auto hr = names->GetLocaleNameLength(i, &locale_len); FAILED(hr))
            throw ComException(hr, "Failed to get locale name length");
        locale.resize(locale_len + 1);
        if (const auto hr = names->GetLocaleName(i, locale.data(), locale_len + 1); FAILED(hr))
            throw ComException(hr, "Failed to get locale name");
        if (const auto hr = names->GetStringLength(i, &string_len); FAILED(hr))
            throw ComException(hr, "Failed to get string length");
        string.resize(string_len + 1);
        if (const auto hr = names->GetString(i, string.data(), string_len + 1); FAILED(hr))
            throw ComException(hr, "Failed to get string");
        add(ctx, locale.data(), locale_len, string.data(), string_len);
    }
}
