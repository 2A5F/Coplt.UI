#include "FontFace.h"

#include "Error.h"

using namespace Coplt;

DWriteFontFace::DWriteFontFace(Rc<IDWriteFontFace5>&& face)
    : m_face(std::forward<Rc<IDWriteFontFace5>>(face))
{
}

DWriteFontFace::DWriteFontFace(const Rc<IDWriteFontFace5>& face)
    : m_face(face)
{
}

DWriteFontFace::DWriteFontFace(const Rc<IDWriteFont3>& font)
{
    Rc<IDWriteFontFace3> face3{};
    if (const auto hr = font->CreateFontFace(face3.put()); FAILED(hr))
        throw ComException(hr, "Failed to create font face");
    if (const auto hr = face3->QueryInterface(m_face.put()); FAILED(hr))
        throw ComException(hr, "Failed to create font face");
}

bool DWriteFontFace::Impl_Equals(IFontFace* other) const
{
    return Equals(other);
}

i32 DWriteFontFace::Impl_HashCode() const
{
    return GetHashCode();
}

bool DWriteFontFace::Equals(IFontFace* other) const
{
    const auto o = static_cast<DWriteFontFace*>(other);
    if (m_face == nullptr) return o->m_face == nullptr;
    return m_face->Equals(o->m_face.get());
}

i32 DWriteFontFace::GetHashCode() const
{
    IDWriteFontFace* face = m_face.get();
    return reinterpret_cast<i32>(face) ^ static_cast<i32>(reinterpret_cast<u64>(face) >> 32);
}
