#pragma once

#include <dwrite_3.h>
#include "../Com.h"

namespace Coplt
{
    struct DWriteFontFace final : ComImpl<DWriteFontFace, IFontFace>
    {
        Rc<IDWriteFontFace5> m_face;

        explicit DWriteFontFace(Rc<IDWriteFontFace5>&& face);
        explicit DWriteFontFace(const Rc<IDWriteFontFace5>& face);
        explicit DWriteFontFace(const Rc<IDWriteFont3>& font);

        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        bool Impl_Equals(IFontFace* other) const;

        COPLT_FORCE_INLINE
        i32 Impl_HashCode() const;

        COPLT_IMPL_END

        bool Equals(IFontFace* other) const;

        COPLT_FORCE_INLINE
        i32 GetHashCode() const;
    };
}
