#include "FontFace.h"

#include "Error.h"

using namespace Coplt;

DWriteFontFace::DWriteFontFace(const Rc<IDWriteFont3>& font) : m_font(font)
{
    if (const auto hr = font->CreateFontFace(m_face.put()); FAILED(hr))
        throw ComException(hr, "Failed to create font face");
    m_hb_face = hb_directwrite_face_create(m_face.get());
    if (m_hb_face == nullptr)
        throw std::exception("Failed to create hb face");
}
