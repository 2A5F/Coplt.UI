#include "BaseFontFallback.h"

using namespace Coplt;

extern "C" void coplt_ui_dwrite_get_font_fallback(IFontFallback* obj, IDWriteFontFallback1** out)
{
    const auto bf = static_cast<BaseFontFallback*>(obj);
    const auto dfb = bf->m_fallback.get();
    dfb->AddRef();
    *out = dfb;
}
