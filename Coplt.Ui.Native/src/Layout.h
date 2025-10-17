#pragma once

#include "../api/Interface.h"

namespace Coplt
{
    struct Layout final : ComObject<ILayout>
    {
        HResult Impl_Calc(NLayoutContext* ctx) override;
    };
}
