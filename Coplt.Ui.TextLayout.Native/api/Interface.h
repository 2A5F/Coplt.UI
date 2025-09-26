#pragma once
#ifndef COPLT_UL_TEXT_LAYOUT_INTERFACE_H
#define COPLT_UL_TEXT_LAYOUT_INTERFACE_H

#include "CoCom.h"
#include "./Types.h"
#include "./Details.h"

namespace Coplt {

    COPLT_COM_INTERFACE(IFontCollection, "e56d9271-e6fd-4def-b03a-570380e0d560", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IFontCollection
    };

    COPLT_COM_INTERFACE(ILibTextLayout, "778be1fe-18f2-4aa5-8d1f-52d83b132cff", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_ILibTextLayout

        COPLT_COM_METHOD(SetLogger, void, (void* obj, ::Coplt::Func<void, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* logger, ::Coplt::Func<void, void*>* drop), obj, logger, drop);
        COPLT_COM_METHOD(get_CurrentErrorMessage, const::Coplt::u8*, ());
        COPLT_COM_METHOD(GetSystemFontCollection, ::Coplt::HResult, (IFontCollection** fc), fc);
    };

} // namespace Coplt

#endif //COPLT_UL_TEXT_LAYOUT_INTERFACE_H
