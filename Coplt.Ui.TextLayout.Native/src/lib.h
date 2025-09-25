#pragma once
#include "Com.h"
#include "Defines.h"

namespace Coplt
{
    struct LibTextLayout final : ComObject<ILibTextLayout>
    {
    protected:
        IFace* Impl_CreateFace() override;
    };

    extern "C" COPLT_EXPORT ILibTextLayout* Coplt_CreateLibTextLayout();
}
