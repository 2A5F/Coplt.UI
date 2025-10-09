#include "FontFace.h"

using namespace Coplt;

FontFace::~FontFace()
{
    if (m_hb_face != nullptr)
    {
        hb_face_destroy(m_hb_face);
        m_hb_face = nullptr;
    }
}

