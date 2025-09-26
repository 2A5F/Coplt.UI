#pragma once
#ifndef COPLT_UL_TEXT_LAYOUT_DETAILS_H
#define COPLT_UL_TEXT_LAYOUT_DETAILS_H

#include "CoCom.h"
#include "./Types.h"

namespace Coplt {

    using IUnknown = ::Coplt::IUnknown;
    using IWeak = ::Coplt::IWeak;

    struct IFontCollection;
    struct ILibTextLayout;

} // namespace Coplt

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::IFontCollection>
{
    VirtualTable<::Coplt::IUnknown> b;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontCollection
{
}

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::IFontCollection>
{
    using VirtualTable = VirtualTable<::Coplt::IFontCollection>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("e56d9271-e6fd-4def-b03a-570380e0d560");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::IFontCollection>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::IFontCollection*>(self)));
            return ::Coplt::HResultE::Ok;
        }
        return ComProxy<::Coplt::IUnknown>::QueryInterface(self, guid, object);
    }

    COPLT_FORCE_INLINE
    static const VirtualTable& GetVtb()
    {
        static VirtualTable vtb
        {
            .b = ComProxy<::Coplt::IUnknown>::GetVtb(),
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {
    };

    template <std::derived_from<::Coplt::IFontCollection> Base = ::Coplt::IFontCollection>
    struct Proxy : Impl, Base
    {
        explicit Proxy(const ::Coplt::Internal::VirtualTable<Base>* vtb) : Base(vtb) {}

        explicit Proxy() : Base(&GetVtb()) {}
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontCollection
{
}
#define COPLT_COM_INTERFACE_BODY_Coplt_IFontCollection\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::IFontCollection;\
\
    explicit IFontCollection(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::IFontCollection>
{
};

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::ILibTextLayout>
{
    VirtualTable<::Coplt::IUnknown> b;
    void (*const COPLT_CDECL f_SetLogger)(::Coplt::ILibTextLayout*, void* obj, ::Coplt::Func<void, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* logger, ::Coplt::Func<void, void*>* drop) noexcept;
    const::Coplt::u8* (*const COPLT_CDECL f_get_CurrentErrorMessage)(::Coplt::ILibTextLayout*) noexcept;
    ::Coplt::HResult* (*const COPLT_CDECL f_GetSystemFontCollection)(::Coplt::ILibTextLayout*, ::Coplt::HResult*, IFontCollection** fc) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_ILibTextLayout
{
    extern "C" void COPLT_CDECL SetLogger(::Coplt::ILibTextLayout* self, void* p0, ::Coplt::Func<void, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* p1, ::Coplt::Func<void, void*>* p2) noexcept;
    extern "C" const::Coplt::u8* COPLT_CDECL get_CurrentErrorMessage(::Coplt::ILibTextLayout* self) noexcept;
    extern "C" ::Coplt::HResult* COPLT_CDECL GetSystemFontCollection(::Coplt::ILibTextLayout* self, ::Coplt::HResult* r, IFontCollection** p0) noexcept;
}

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::ILibTextLayout>
{
    using VirtualTable = VirtualTable<::Coplt::ILibTextLayout>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("778be1fe-18f2-4aa5-8d1f-52d83b132cff");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::ILibTextLayout>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::ILibTextLayout*>(self)));
            return ::Coplt::HResultE::Ok;
        }
        return ComProxy<::Coplt::IUnknown>::QueryInterface(self, guid, object);
    }

    COPLT_FORCE_INLINE
    static const VirtualTable& GetVtb()
    {
        static VirtualTable vtb
        {
            .b = ComProxy<::Coplt::IUnknown>::GetVtb(),
            .f_SetLogger = VirtualImpl_Coplt_ILibTextLayout::SetLogger,
            .f_get_CurrentErrorMessage = VirtualImpl_Coplt_ILibTextLayout::get_CurrentErrorMessage,
            .f_GetSystemFontCollection = VirtualImpl_Coplt_ILibTextLayout::GetSystemFontCollection,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual void Impl_SetLogger(void* obj, ::Coplt::Func<void, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* logger, ::Coplt::Func<void, void*>* drop) = 0;
        virtual const::Coplt::u8* Impl_get_CurrentErrorMessage() = 0;
        virtual ::Coplt::HResult Impl_GetSystemFontCollection(IFontCollection** fc) = 0;
    };

    template <std::derived_from<::Coplt::ILibTextLayout> Base = ::Coplt::ILibTextLayout>
    struct Proxy : Impl, Base
    {
        explicit Proxy(const ::Coplt::Internal::VirtualTable<Base>* vtb) : Base(vtb) {}

        explicit Proxy() : Base(&GetVtb()) {}
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_ILibTextLayout
{

    extern "C" inline void COPLT_CDECL SetLogger(::Coplt::ILibTextLayout* self, void* p0, ::Coplt::Func<void, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* p1, ::Coplt::Func<void, void*>* p2) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILibTextLayout, SetLogger, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::ILibTextLayout>(self)->Impl_SetLogger(p0, p1, p2);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILibTextLayout, SetLogger, void)
        #endif
    }

    extern "C" inline const::Coplt::u8* COPLT_CDECL get_CurrentErrorMessage(::Coplt::ILibTextLayout* self) noexcept
    {
        const::Coplt::u8* r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILibTextLayout, get_CurrentErrorMessage, const::Coplt::u8*)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::ILibTextLayout>(self)->Impl_get_CurrentErrorMessage();
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILibTextLayout, get_CurrentErrorMessage, const::Coplt::u8*)
        #endif
        return r;
    }

    extern "C" inline ::Coplt::HResult* COPLT_CDECL GetSystemFontCollection(::Coplt::ILibTextLayout* self, ::Coplt::HResult* r, IFontCollection** p0) noexcept
    {
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILibTextLayout, GetSystemFontCollection, ::Coplt::HResult)
        #endif
        *r = ::Coplt::Internal::AsImpl<::Coplt::ILibTextLayout>(self)->Impl_GetSystemFontCollection(p0);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILibTextLayout, GetSystemFontCollection, ::Coplt::HResult)
        #endif
        return r;
    }
}
#define COPLT_COM_INTERFACE_BODY_Coplt_ILibTextLayout\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::ILibTextLayout;\
\
    explicit ILibTextLayout(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::ILibTextLayout>
{
    static COPLT_FORCE_INLINE void SetLogger(::Coplt::ILibTextLayout* self, void* p0, ::Coplt::Func<void, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* p1, ::Coplt::Func<void, void*>* p2) noexcept
    {
        COPLT_COM_PVTB(ILibTextLayout, self)->f_SetLogger(self, p0, p1, p2);
    }
    static COPLT_FORCE_INLINE const::Coplt::u8* get_CurrentErrorMessage(::Coplt::ILibTextLayout* self) noexcept
    {
        return COPLT_COM_PVTB(ILibTextLayout, self)->f_get_CurrentErrorMessage(self);
    }
    static COPLT_FORCE_INLINE ::Coplt::HResult GetSystemFontCollection(::Coplt::ILibTextLayout* self, IFontCollection** p0) noexcept
    {
        ::Coplt::HResult r{};
        return *COPLT_COM_PVTB(ILibTextLayout, self)->f_GetSystemFontCollection(self, &r, p0);
    }
};

#endif //COPLT_UL_TEXT_LAYOUT_DETAILS_H
