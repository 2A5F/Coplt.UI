#include "Font.h"

#include "Error.h"
#include "FontFace.h"
#include "FontFamily.h"

using namespace Coplt;

Font::Font(Rc<IDWriteFont3>& font)
    : m_font(std::move(font))
{
    DWRITE_FONT_METRICS metrics;
    m_font->GetMetrics(&metrics);

    m_info.Metrics.Ascent = static_cast<float>(metrics.ascent);
    m_info.Metrics.Descent = static_cast<float>(metrics.descent);
    m_info.Metrics.Leading = static_cast<float>(metrics.lineGap);
    m_info.Metrics.LineHeight = static_cast<float>(metrics.ascent + metrics.descent + metrics.lineGap);
    m_info.Metrics.UnitsPerEm = metrics.designUnitsPerEm;

    switch (m_font->GetStretch())
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

    m_info.Weight = static_cast<FontWeight>(m_font->GetWeight());

    if (m_font->IsColorFont())
    {
        m_info.Flags |= FontFlags::Color;
    }

    if (m_font->IsMonospacedFont())
    {
        m_info.Flags |= FontFlags::Monospaced;
    }
}

NFontInfo const* Font::Impl_get_Info() const
{
    return &m_info;
}

HResult Font::Impl_CreateFace(IFontFace** face, IFontManager* manager) const
{
    return feb(
        [&]
        {
            return CreateFace(manager, face);
        }
    );
}

extern "C" HResultE coplt_ui_dwrite_create_font_face(
    const Rc<IDWriteFontFace5>* face,
    IFontManager* managerm,
    IFontFace** out
);

HResultE Font::CreateFace(IFontManager* manager, IFontFace** out) const
{
    Rc<IDWriteFontFace3> face3{};
    Rc<IDWriteFontFace5> face5{};
    if (const auto hr = m_font->CreateFontFace(face3.put()); FAILED(hr))
        return static_cast<HResultE>(hr);
    if (const auto hr = face3->QueryInterface(face5.put()); FAILED(hr))
        return static_cast<HResultE>(hr);
    const auto hr = coplt_ui_dwrite_create_font_face(&face5, manager, out);
    if (SUCCEEDED(hr))
    {
        manager->Add(*out);
    }
    return hr;
}
