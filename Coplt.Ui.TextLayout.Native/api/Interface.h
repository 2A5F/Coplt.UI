#pragma once
#ifndef COPLT_UL_TEXT_LAYOUT_INTERFACE_H
#define COPLT_UL_TEXT_LAYOUT_INTERFACE_H

#include "CoCom.h"
#include "./Types.h"
#include "./Details.h"

namespace Coplt {

    COPLT_COM_INTERFACE(IFace, "805e2d1f-6be2-4ebd-ac64-60c6f5f73d63", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IFace
    };

    COPLT_COM_INTERFACE(ILibTextLayout, "778be1fe-18f2-4aa5-8d1f-52d83b132cff", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_ILibTextLayout

        COPLT_COM_METHOD(CreateFace, IFace*, ())
    };

} // namespace Coplt

#endif //COPLT_UL_TEXT_LAYOUT_INTERFACE_H
