#pragma once

#include <hb.h>

namespace Coplt
{
    struct FontFace
    {
        hb_face_t* m_hb_face;

        ~FontFace();
    };
}
