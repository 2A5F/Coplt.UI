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
        COPLT_COM_METHOD(CreateFace, ::Coplt::HResult, (COPLT_OUT IFontFace** face, IFontManager* manager) const, face, manager);
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

        COPLT_COM_METHOD(get_Info, ::Coplt::NFontInfo const*, () const);
        COPLT_COM_METHOD(Equals, bool, (IFontFace* other) const, other);
        COPLT_COM_METHOD(HashCode, ::Coplt::i32, () const);
        COPLT_COM_METHOD(GetFamilyNames, ::Coplt::HResult, (void* ctx, ::Coplt::Func<void, void*, ::Coplt::char16*, ::Coplt::i32, ::Coplt::char16*, ::Coplt::i32>* add) const, ctx, add);
        COPLT_COM_METHOD(GetFaceNames, ::Coplt::HResult, (void* ctx, ::Coplt::Func<void, void*, ::Coplt::char16*, ::Coplt::i32, ::Coplt::char16*, ::Coplt::i32>* add) const, ctx, add);
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
        COPLT_COM_METHOD(GetFonts, ::Coplt::HResult, (COPLT_OUT ::Coplt::u32* length, COPLT_OUT ::Coplt::NFontPair const** pair), length, pair);
        COPLT_COM_METHOD(ClearNativeFontsCache, void, ());
    };

    COPLT_COM_INTERFACE(IFontManager, "15a9651e-4fa2-48f3-9291-df0f9681a7d1", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IFontManager

        COPLT_COM_METHOD(SetAssocUpdate, ::Coplt::u64, (void* Data, ::Coplt::Func<void, void*>* OnDrop, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* OnAdd, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* OnExpired), Data, OnDrop, OnAdd, OnExpired);
        COPLT_COM_METHOD(RemoveAssocUpdate, void, (::Coplt::u64 AssocUpdateId), AssocUpdateId);
        COPLT_COM_METHOD(SetExpireFrame, void, (::Coplt::u64 FrameCount), FrameCount);
        COPLT_COM_METHOD(SetExpireTime, void, (::Coplt::u64 TimeTicks), TimeTicks);
        COPLT_COM_METHOD(GetCurrentFrame, ::Coplt::u64, () const);
        COPLT_COM_METHOD(Update, void, (::Coplt::u64 CurrentTime), CurrentTime);
        COPLT_COM_METHOD(FontFaceToId, ::Coplt::u64, (IFontFace* Face), Face);
        COPLT_COM_METHOD(IdToFontFace, IFontFace*, (::Coplt::u64 Id), Id);
    };

    COPLT_COM_INTERFACE(ILayout, "f1e64bf0-ffb9-42ce-be78-31871d247883", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_ILayout

        COPLT_COM_METHOD(Calc, ::Coplt::HResult, (::Coplt::NLayoutContext* ctx), ctx);
    };

    COPLT_COM_INTERFACE(ILib, "778be1fe-18f2-4aa5-8d1f-52d83b132cff", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_ILib

        COPLT_COM_METHOD(SetLogger, void, (void* obj, ::Coplt::Func<void, void*, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* logger, ::Coplt::Func<void, void*>* drop), obj, logger, drop);
        COPLT_COM_METHOD(GetCurrentErrorMessage, ::Coplt::Str8, ());
        COPLT_COM_METHOD(CreateFontManager, ::Coplt::HResult, (IFontManager** fm), fm);
        COPLT_COM_METHOD(GetSystemFontCollection, ::Coplt::HResult, (IFontCollection** fc), fc);
        COPLT_COM_METHOD(GetSystemFontFallback, ::Coplt::HResult, (IFontFallback** ff), ff);
        COPLT_COM_METHOD(CreateLayout, ::Coplt::HResult, (ILayout** layout), layout);
        COPLT_COM_METHOD(SplitTexts, ::Coplt::HResult, (::Coplt::NativeList<::Coplt::TextRange>* ranges, ::Coplt::char16 const* chars, ::Coplt::i32 len), ranges, chars, len);
    };

    COPLT_COM_INTERFACE(IStub, "a998ec87-868d-4320-a30a-638c291f5562", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IStub

        COPLT_COM_METHOD(Some, void, (::Coplt::NodeType a, ::Coplt::RootData* b, ::Coplt::NString* c), a, b, c);
    };

    COPLT_COM_INTERFACE(ITextData, "bd0c7402-1de8-4547-860d-c78fd70ff203", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_ITextData
    };

    COPLT_COM_INTERFACE(ITextLayout, "f558ba07-1f1d-4c32-8229-134271b17083", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_ITextLayout
    };

} // namespace Coplt

#endif //COPLT_UI_INTERFACE_H
