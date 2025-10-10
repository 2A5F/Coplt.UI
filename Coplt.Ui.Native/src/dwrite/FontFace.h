#pragma once

#include "../FontFace.h"

#include <dwrite_3.h>
#include <hb-directwrite.h>

namespace Coplt
{
    struct DWriteFontFace final : FontFace
    {
        Rc<IDWriteFont3> m_font;
        Rc<IDWriteFontFace3> m_face;

        explicit DWriteFontFace(const Rc<IDWriteFont3>& font);
    };
}
