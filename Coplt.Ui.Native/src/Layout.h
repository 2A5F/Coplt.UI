#pragma once

#include "Com.h"

namespace Coplt
{
    struct LibUi;

    struct Layout final : ComImpl<Layout, ILayout>
    {
        Rc<LibUi> m_lib;

        explicit Layout(Rc<LibUi> lib);

        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        HResult Impl_Calc(NLayoutContext* ctx);

        COPLT_IMPL_END
    };
} // namespace Coplt
