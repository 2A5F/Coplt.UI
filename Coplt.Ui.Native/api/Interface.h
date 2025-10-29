#pragma once
#ifndef COPLT_UI_INTERFACE_H
#define COPLT_UI_INTERFACE_H

#include "CoCom.h"
#include "./Types.h"
#include "./Details.h"

namespace Coplt {

    COPLT_COM_INTERFACE(IFont, "09c443bc-9736-4aac-8117-6890555005ff", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IFont

        COPLT_COM_METHOD(get_Info, ::Coplt::NFontInfo const*, () const);
        COPLT_COM_METHOD(CreateFace, ::Coplt::HResult, (COPLT_OUT IFontFace** face) const, face);
    };

    COPLT_COM_INTERFACE(IFontCollection, "e56d9271-e6fd-4def-b03a-570380e0d560", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IFontCollection

        COPLT_COM_METHOD(GetFamilies, IFontFamily* const*, (COPLT_OUT ::Coplt::u32* count) const, count);
        COPLT_COM_METHOD(ClearNativeFamiliesCache, void, ());
        COPLT_COM_METHOD(FindDefaultFamily, ::Coplt::u32, ());
    };

    COPLT_COM_INTERFACE(IFontFace, "09c443bc-9736-4aac-8117-6890555005ff", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IFontFace
    };

    COPLT_COM_INTERFACE(IFontFallback, "b0dbb428-eca1-4784-b27f-629bddf93ea4", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IFontFallback
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

    COPLT_COM_INTERFACE(ILayout, "f1e64bf0-ffb9-42ce-be78-31871d247883", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_ILayout

        COPLT_COM_METHOD(Calc, ::Coplt::HResult, (::Coplt::NLayoutContext* ctx), ctx);
    };

    COPLT_COM_INTERFACE(ILib, "778be1fe-18f2-4aa5-8d1f-52d83b132cff", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_ILib

        COPLT_COM_METHOD(SetLogger, void, (void* obj, ::Coplt::Func<void, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* logger, ::Coplt::Func<void, void*>* drop), obj, logger, drop);
        COPLT_COM_METHOD(GetCurrentErrorMessage, ::Coplt::Str8, ());
        COPLT_COM_METHOD(Alloc, void*, (::Coplt::i32 size, ::Coplt::i32 align) const, size, align);
        COPLT_COM_METHOD(Free, void, (void* ptr, ::Coplt::i32 align) const, ptr, align);
        COPLT_COM_METHOD(ZAlloc, void*, (::Coplt::i32 size, ::Coplt::i32 align) const, size, align);
        COPLT_COM_METHOD(ReAlloc, void*, (void* ptr, ::Coplt::i32 size, ::Coplt::i32 align) const, ptr, size, align);
        COPLT_COM_METHOD(GetSystemFontCollection, ::Coplt::HResult, (IFontCollection** fc), fc);
        COPLT_COM_METHOD(GetSystemFontFallback, ::Coplt::HResult, (IFontFallback** ff), ff);
        COPLT_COM_METHOD(CreateLayout, ::Coplt::HResult, (ILayout** layout), layout);
        COPLT_COM_METHOD(SplitTexts, ::Coplt::HResult, (::Coplt::NativeList<::Coplt::TextRange>* ranges, ::Coplt::char16 const* chars, ::Coplt::i32 len), ranges, chars, len);
    };

    COPLT_COM_INTERFACE(IStub, "a998ec87-868d-4320-a30a-638c291f5562", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IStub

        COPLT_COM_METHOD(Some, void, (::Coplt::NodeType a), a);
    };

    COPLT_COM_INTERFACE(ITextLayout, "f558ba07-1f1d-4c32-8229-134271b17083", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_ITextLayout
    };

} // namespace Coplt

#endif //COPLT_UI_INTERFACE_H
