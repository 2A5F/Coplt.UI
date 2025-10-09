#pragma once

#include <hb.h>

#include "../api/Interface.h"

namespace Coplt
{
    struct FontFace : ComObject<IFontFace>
    {
        hb_face_t* m_hb_face{nullptr};

        ~FontFace() override;
    };
}
