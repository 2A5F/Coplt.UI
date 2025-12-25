#pragma once
#ifndef COPLT_UI_INTERFACE_H
#define COPLT_UI_INTERFACE_H

#include "CoCom.h"
#include "./Types.h"
#include "./Details.h"

namespace Coplt {

    COPLT_COM_INTERFACE(IAtlasAllocator, "32b30623-411e-4fd5-a009-ae7e9ed88e78", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IAtlasAllocator

        COPLT_COM_METHOD(Clear, void, ());
        COPLT_COM_METHOD(get_IsEmpty, bool, ());
        COPLT_COM_METHOD(GetSize, void, (::Coplt::i32* out_width, ::Coplt::i32* out_height), out_width, out_height);
        COPLT_COM_METHOD(Allocate, bool, (::Coplt::i32 width, ::Coplt::i32 height, ::Coplt::u32* out_id, ::Coplt::AABB2DI* out_rect), width, height, out_id, out_rect);
        COPLT_COM_METHOD(Deallocate, void, (::Coplt::u32 id), id);
    };

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

        COPLT_COM_METHOD(SetManagedHandle, void, (void* Handle, ::Coplt::Func<void, void*>* OnDrop), Handle, OnDrop);
        COPLT_COM_METHOD(GetManagedHandle, void*, ());
        COPLT_COM_METHOD(get_Id, ::Coplt::u64, () const);
        COPLT_COM_METHOD(get_RefCount, ::Coplt::u32, () const);
        COPLT_COM_METHOD(get_FrameTime, ::Coplt::FrameTime const*, () const);
        COPLT_COM_METHOD(GetFrameSource, IFrameSource*, () const);
        COPLT_COM_METHOD(GetFontManager, IFontManager*, () const);
        COPLT_COM_METHOD(get_Info, ::Coplt::NFontInfo const*, () const);
        COPLT_COM_METHOD(GetData, void, (::Coplt::u8** p_data, ::Coplt::usize* size, ::Coplt::u32* index) const, p_data, size, index);
        COPLT_COM_METHOD(Equals, bool, (IFontFace* other) const, other);
        COPLT_COM_METHOD(HashCode, ::Coplt::i32, () const);
        COPLT_COM_METHOD(GetFamilyNames, ::Coplt::HResult, (void* ctx, ::Coplt::Func<void, void*, ::Coplt::char16*, ::Coplt::i32, ::Coplt::char16*, ::Coplt::i32>* add) const, ctx, add);
        COPLT_COM_METHOD(GetFaceNames, ::Coplt::HResult, (void* ctx, ::Coplt::Func<void, void*, ::Coplt::char16*, ::Coplt::i32, ::Coplt::char16*, ::Coplt::i32>* add) const, ctx, add);
    };

    COPLT_COM_INTERFACE(IFontFallback, "b0dbb428-eca1-4784-b27f-629bddf93ea4", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IFontFallback
    };

    COPLT_COM_INTERFACE(IFontFallbackBuilder, "9b4e9893-0ea4-456b-bf54-9563db70eff0", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IFontFallbackBuilder

        COPLT_COM_METHOD(Build, ::Coplt::HResult, (IFontFallback** ff), ff);
        COPLT_COM_METHOD(Add, ::Coplt::HResult, (::Coplt::char16 const* name, ::Coplt::i32 length, bool* exists), name, length, exists);
        COPLT_COM_METHOD(AddLocaled, ::Coplt::HResult, (::Coplt::LocaleId const* locale, ::Coplt::char16 const* name, ::Coplt::i32 name_length, bool* exists), locale, name, name_length, exists);
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

    COPLT_COM_INTERFACE(IFontManager, "15a9651e-4fa2-48f3-9291-df0f9681a7d1", ::Coplt::IWeak)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IFontManager

        COPLT_COM_METHOD(SetManagedHandle, void, (void* Handle, ::Coplt::Func<void, void*>* OnDrop), Handle, OnDrop);
        COPLT_COM_METHOD(GetManagedHandle, void*, ());
        COPLT_COM_METHOD(SetAssocUpdate, ::Coplt::u64, (void* Data, ::Coplt::Func<void, void*>* OnDrop, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* OnAdd, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* OnExpired), Data, OnDrop, OnAdd, OnExpired);
        COPLT_COM_METHOD(RemoveAssocUpdate, void, (::Coplt::u64 AssocUpdateId), AssocUpdateId);
        COPLT_COM_METHOD(GetFrameSource, IFrameSource*, ());
        COPLT_COM_METHOD(SetExpireFrame, void, (::Coplt::u64 FrameCount), FrameCount);
        COPLT_COM_METHOD(SetExpireTime, void, (::Coplt::u64 TimeTicks), TimeTicks);
        COPLT_COM_METHOD(Collect, void, ());
        COPLT_COM_METHOD(Add, void, (IFontFace* Face), Face);
        COPLT_COM_METHOD(GetOrAdd, IFontFace*, (::Coplt::u64 Id, void* Data, ::Coplt::Func<IFontFace*, void*, ::Coplt::u64>* OnAdd), Id, Data, OnAdd);
        COPLT_COM_METHOD(Get, IFontFace*, (::Coplt::u64 Id), Id);
    };

    COPLT_COM_INTERFACE(IFrameSource, "92a81f7e-98b1-4c83-b6ac-161fca9469d6", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IFrameSource

        COPLT_COM_METHOD(Get, void, (::Coplt::FrameTime* ft), ft);
        COPLT_COM_METHOD(Set, void, (::Coplt::FrameTime const* ft), ft);
    };

    COPLT_COM_INTERFACE(ILayout, "f1e64bf0-ffb9-42ce-be78-31871d247883", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_ILayout

        COPLT_COM_METHOD(Calc, ::Coplt::HResult, (::Coplt::NLayoutContext* ctx), ctx);
    };

    COPLT_COM_INTERFACE(ILib, "778be1fe-18f2-4aa5-8d1f-52d83b132cff", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_ILib

        COPLT_COM_METHOD(SetLogger, void, (void* obj, ::Coplt::Func<void, void*, ::Coplt::LogLevel, ::Coplt::StrKind, ::Coplt::i32, void*>* logger, ::Coplt::Func<::Coplt::u8, void*, ::Coplt::LogLevel>* is_enabled, ::Coplt::Func<void, void*>* drop), obj, logger, is_enabled, drop);
        COPLT_COM_METHOD(ClearLogger, void, ());
        COPLT_COM_METHOD(GetCurrentErrorMessage, ::Coplt::Str8, ());
        COPLT_COM_METHOD(CreateAtlasAllocator, ::Coplt::HResult, (::Coplt::AtlasAllocatorType Type, ::Coplt::i32 Width, ::Coplt::i32 Height, IAtlasAllocator** aa), Type, Width, Height, aa);
        COPLT_COM_METHOD(CreateFrameSource, ::Coplt::HResult, (IFrameSource** fs), fs);
        COPLT_COM_METHOD(CreateFontManager, ::Coplt::HResult, (IFrameSource* fs, IFontManager** fm), fs, fm);
        COPLT_COM_METHOD(GetSystemFontCollection, ::Coplt::HResult, (IFontCollection** fc), fc);
        COPLT_COM_METHOD(GetSystemFontFallback, ::Coplt::HResult, (IFontFallback** ff), ff);
        COPLT_COM_METHOD(CreateFontFallbackBuilder, ::Coplt::HResult, (IFontFallbackBuilder** ffb, ::Coplt::FontFallbackBuilderCreateInfo const* info), ffb, info);
        COPLT_COM_METHOD(CreateLayout, ::Coplt::HResult, (ILayout** layout), layout);
        COPLT_COM_METHOD(SplitTexts, ::Coplt::HResult, (::Coplt::NativeList<::Coplt::TextRange>* ranges, ::Coplt::char16 const* chars, ::Coplt::i32 len), ranges, chars, len);
    };

    COPLT_COM_INTERFACE(IPath, "dac7a459-b942-4a96-b7d6-ee5c74eca806", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IPath

        COPLT_COM_METHOD(CalcAABB, void, (::Coplt::AABB2DF* out_aabb), out_aabb);
    };

    COPLT_COM_INTERFACE(IPathBuilder, "ee1c5b1d-b22d-446a-9eef-128cec82e6c0", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IPathBuilder

        COPLT_COM_METHOD(Build, ::Coplt::HResult, (IPath** path), path);
        COPLT_COM_METHOD(Reserve, void, (::Coplt::i32 Endpoints, ::Coplt::i32 CtrlPoints), Endpoints, CtrlPoints);
        COPLT_COM_METHOD(Batch, void, (::Coplt::PathBuilderCmd const* cmds, ::Coplt::i32 num_cmds), cmds, num_cmds);
        COPLT_COM_METHOD(Close, void, ());
        COPLT_COM_METHOD(MoveTo, void, (::Coplt::f32 x, ::Coplt::f32 y), x, y);
        COPLT_COM_METHOD(LineTo, void, (::Coplt::f32 x, ::Coplt::f32 y), x, y);
        COPLT_COM_METHOD(QuadraticBezierTo, void, (::Coplt::f32 ctrl_x, ::Coplt::f32 ctrl_y, ::Coplt::f32 to_x, ::Coplt::f32 to_y), ctrl_x, ctrl_y, to_x, to_y);
        COPLT_COM_METHOD(CubicBezierTo, void, (::Coplt::f32 ctrl0_x, ::Coplt::f32 ctrl0_y, ::Coplt::f32 ctrl1_x, ::Coplt::f32 ctrl1_y, ::Coplt::f32 to_x, ::Coplt::f32 to_y), ctrl0_x, ctrl0_y, ctrl1_x, ctrl1_y, to_x, to_y);
        COPLT_COM_METHOD(Arc, void, (::Coplt::f32 center_x, ::Coplt::f32 center_y, ::Coplt::f32 radii_x, ::Coplt::f32 radii_y, ::Coplt::f32 sweep_angle, ::Coplt::f32 x_rotation), center_x, center_y, radii_x, radii_y, sweep_angle, x_rotation);
    };

    COPLT_COM_INTERFACE(IStub, "a998ec87-868d-4320-a30a-638c291f5562", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_IStub

        COPLT_COM_METHOD(Some, void, (::Coplt::NodeType a, ::Coplt::RootData* b, ::Coplt::NString* c), a, b, c);
    };

    COPLT_COM_INTERFACE(ITessellator, "acf5d52e-a656-4c00-a528-09aa4d86b2b2", ::Coplt::IUnknown)
    {
        COPLT_COM_INTERFACE_BODY_Coplt_ITessellator

        COPLT_COM_METHOD(Fill, ::Coplt::HResult, (IPath* path, ::Coplt::TessFillOptions* options), path, options);
        COPLT_COM_METHOD(Stroke, ::Coplt::HResult, (IPath* path, ::Coplt::TessStrokeOptions* options), path, options);
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
