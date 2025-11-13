#pragma once

#include <dwrite_3.h>
#include "../Com.h"
#include "../Harfpp.h"

namespace Coplt
{
    struct DWriteFontFace final : ComImpl<DWriteFontFace, IFontFace>
    {
        Rc<IDWriteFont3> m_font;
        Rc<IDWriteFontFace3> m_face;
        Harf::HFont m_hb_font;

        explicit DWriteFontFace(const Rc<IDWriteFont3>& font);

        COPLT_IMPL_START
        COPLT_IMPL_END
    };
}
