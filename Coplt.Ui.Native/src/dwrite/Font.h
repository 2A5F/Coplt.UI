#pragma once

#include <dwrite_3.h>

#include "../Com.h"

namespace Coplt
{
    struct FontFamily;
    struct DWriteFontFace;

    struct Font final : ComImpl<Font, IFont>
    {
        NFontInfo m_info{};
        Rc<IDWriteFont3> m_font;

        explicit Font(Rc<IDWriteFont3>& font);

        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        NFontInfo const* Impl_get_Info() const;

        COPLT_FORCE_INLINE
        HResult Impl_CreateFace(COPLT_OUT IFontFace** face, IFontManager* manager) const;

        COPLT_IMPL_END

        HResultE CreateFace(IFontManager* manager, IFontFace** out) const;
    };
}
