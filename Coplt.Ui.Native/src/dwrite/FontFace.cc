#include "FontFace.h"

#include "Error.h"

using namespace Coplt;

void Coplt::coplt_ui_dwrite_get_font_face_info(IDWriteFontFace5* face, NFontInfo* info)
{
    switch (face->GetStretch())
    {
    case DWRITE_FONT_STRETCH_UNDEFINED:
        info->Width.Width = 1.0;
        break;
    case DWRITE_FONT_STRETCH_ULTRA_CONDENSED:
        info->Width.Width = 0.5;
        break;
    case DWRITE_FONT_STRETCH_EXTRA_CONDENSED:
        info->Width.Width = 0.625;
        break;
    case DWRITE_FONT_STRETCH_CONDENSED:
        info->Width.Width = 0.75;
        break;
    case DWRITE_FONT_STRETCH_SEMI_CONDENSED:
        info->Width.Width = 0.775;
        break;
    case DWRITE_FONT_STRETCH_NORMAL:
        info->Width.Width = 1.0;
        break;
    case DWRITE_FONT_STRETCH_SEMI_EXPANDED:
        info->Width.Width = 1.125;
        break;
    case DWRITE_FONT_STRETCH_EXPANDED:
        info->Width.Width = 1.25;
        break;
    case DWRITE_FONT_STRETCH_EXTRA_EXPANDED:
        info->Width.Width = 1.5;
        break;
    case DWRITE_FONT_STRETCH_ULTRA_EXPANDED:
        info->Width.Width = 2.0;
        break;
    default:
        info->Width.Width = 1.0;
    }

    info->Weight = static_cast<FontWeight>(face->GetWeight());

    info->Flags = FontFlags::None;
    if (face->IsColorFont())
    {
        info->Flags |= FontFlags::Color;
    }

    if (face->IsMonospacedFont())
    {
        info->Flags |= FontFlags::Monospaced;
    }
}
