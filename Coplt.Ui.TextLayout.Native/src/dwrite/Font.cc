#include "Font.h"

#include "Error.h"
#include "FontFamily.h"

using namespace Coplt;

Font::Font(Rc<IDWriteFont>& font) : m_font(std::move(font))
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

    switch (m_font->GetStyle())
    {
    case DWRITE_FONT_STYLE_NORMAL:
        m_info.Style = FontStyle::Normal;
        break;
    case DWRITE_FONT_STYLE_OBLIQUE:
        m_info.Style = FontStyle::Oblique;
        break;
    case DWRITE_FONT_STYLE_ITALIC:
        m_info.Style = FontStyle::Italic;
        break;
    default:
        m_info.Style = FontStyle::Normal;
    }

    m_info.Width = static_cast<FontWidth>(m_font->GetWeight());
}

NFontInfo* Font::Impl_get_Info()
{
    return &m_info;
}
