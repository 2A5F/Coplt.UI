#pragma once

#include <dwrite.h>
#include <hb-directwrite.h>

#include "../Com.h"

namespace Coplt
{
    struct FontFamily;

    struct Font final : ComObject<IFont>
    {
        NFontInfo m_info{};
        Rc<IDWriteFont> m_font;

        explicit Font(Rc<IDWriteFont>& font);

        NFontInfo* Impl_get_Info() override;
    };
}
