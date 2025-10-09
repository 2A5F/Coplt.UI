#pragma once
#ifndef COPLT_UL_TEXT_LAYOUT_INTERFACE_H
#define COPLT_UL_TEXT_LAYOUT_INTERFACE_H

#include "CoCom.h"
#include "./Types.h"
#include "./Details.h"

namespace Coplt {

    COPLT_COM_INTERFACE(IFont, "09c443bc-9736-4aac-8117-6890555005ff", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IFont

        COPLT_COM_METHOD(get_Info, ::Coplt::NFontInfo const*, () const);
    };

    COPLT_COM_INTERFACE(IFontCollection, "e56d9271-e6fd-4def-b03a-570380e0d560", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IFontCollection

        COPLT_COM_METHOD(GetFamilies, IFontFamily* const*, (COPLT_OUT ::Coplt::u32* count) const, count);
        COPLT_COM_METHOD(ClearNativeFamiliesCache, void, ());
        COPLT_COM_METHOD(FindDefaultFamily, ::Coplt::u32, ());
    };

    COPLT_COM_INTERFACE(IFontFamily, "f8009d34-9417-4b87-b23b-b7885d27aeab", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IFontFamily

        COPLT_COM_METHOD(GetLocalNames, ::Coplt::Str16 const*, (COPLT_OUT ::Coplt::u32* length) const, length);
        COPLT_COM_METHOD(GetNames, ::Coplt::FontFamilyNameInfo const*, (COPLT_OUT ::Coplt::u32* length) const, length);
        COPLT_COM_METHOD(ClearNativeNamesCache, void, ());
        COPLT_COM_METHOD(GetFonts, ::Coplt::NFontPair const*, (COPLT_OUT ::Coplt::u32* length), length);
        COPLT_COM_METHOD(ClearNativeFontsCache, void, ());
    };

    COPLT_COM_INTERFACE(ILibTextLayout, "778be1fe-18f2-4aa5-8d1f-52d83b132cff", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_ILibTextLayout

        COPLT_COM_METHOD(SetLogger, void, (void* obj, ::Coplt::Func<void, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* logger, ::Coplt::Func<void, void*>* drop), obj, logger, drop);
        COPLT_COM_METHOD(GetCurrentErrorMessage, ::Coplt::Str8, ());
        COPLT_COM_METHOD(GetSystemFontCollection, ::Coplt::HResult, (IFontCollection** fc), fc);
    };

} // namespace Coplt

#endif //COPLT_UL_TEXT_LAYOUT_INTERFACE_H
