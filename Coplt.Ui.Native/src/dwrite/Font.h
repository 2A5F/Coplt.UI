#pragma once

#include <dwrite_3.h>
#include <hb-directwrite.h>

#include "../Com.h"

namespace Coplt
{
    struct FontFamily;

    struct Font final : ComObject<IFont>
    {
        NFontInfo m_info{};
        Rc<IDWriteFont3> m_font;

        explicit Font(Rc<IDWriteFont3>& font);

        NFontInfo const* Impl_get_Info() const override;

        HResult Impl_CreateFace(IFontFace** face) const override;
    };
}
