#pragma once
#ifndef COPLT_UL_TEXT_LAYOUT_DETAILS_H
#define COPLT_UL_TEXT_LAYOUT_DETAILS_H

#include "CoCom.h"
#include "./Types.h"

namespace Coplt {

    using IUnknown = ::Coplt::IUnknown;
    using IWeak = ::Coplt::IWeak;

    struct IFontCollection;
    struct IFontFamily;
    struct ILibTextLayout;

} // namespace Coplt

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::IFontCollection>
{
    VirtualTable<::Coplt::IUnknown> b;
    IFontFamily* const* (*const COPLT_CDECL f_GetFamilies)(const ::Coplt::IFontCollection*, COPLT_OUT ::Coplt::u32* count) noexcept;
    void (*const COPLT_CDECL f_ClearNativeFamiliesCache)(::Coplt::IFontCollection*) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontCollection
{
    extern "C" IFontFamily* const* COPLT_CDECL GetFamilies(const ::Coplt::IFontCollection* self, COPLT_OUT ::Coplt::u32* p0) noexcept;
    extern "C" void COPLT_CDECL ClearNativeFamiliesCache(::Coplt::IFontCollection* self) noexcept;
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
            .f_GetFamilies = VirtualImpl_Coplt_IFontCollection::GetFamilies,
            .f_ClearNativeFamiliesCache = VirtualImpl_Coplt_IFontCollection::ClearNativeFamiliesCache,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual IFontFamily* const* Impl_GetFamilies(COPLT_OUT ::Coplt::u32* count) const = 0;
        virtual void Impl_ClearNativeFamiliesCache() = 0;
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

    extern "C" inline IFontFamily* const* COPLT_CDECL GetFamilies(const ::Coplt::IFontCollection* self, COPLT_OUT ::Coplt::u32* p0) noexcept
    {
        IFontFamily* const* r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontCollection, GetFamilies, IFontFamily* const*)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IFontCollection>(self)->Impl_GetFamilies(p0);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontCollection, GetFamilies, IFontFamily* const*)
        #endif
        return r;
    }

    extern "C" inline void COPLT_CDECL ClearNativeFamiliesCache(::Coplt::IFontCollection* self) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontCollection, ClearNativeFamiliesCache, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IFontCollection>(self)->Impl_ClearNativeFamiliesCache();
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontCollection, ClearNativeFamiliesCache, void)
        #endif
    }
}
#define COPLT_COM_INTERFACE_BODY_Coplt_IFontCollection\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::IFontCollection;\
\
    explicit IFontCollection(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::IFontCollection>
{
    static COPLT_FORCE_INLINE IFontFamily* const* GetFamilies(const ::Coplt::IFontCollection* self, COPLT_OUT ::Coplt::u32* p0) noexcept
    {
        return COPLT_COM_PVTB(IFontCollection, self)->f_GetFamilies(self, p0);
    }
    static COPLT_FORCE_INLINE void ClearNativeFamiliesCache(::Coplt::IFontCollection* self) noexcept
    {
        COPLT_COM_PVTB(IFontCollection, self)->f_ClearNativeFamiliesCache(self);
    }
};

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::IFontFamily>
{
    VirtualTable<::Coplt::IUnknown> b;
    ::Coplt::Str16 const* (*const COPLT_CDECL f_GetLocalNames)(const ::Coplt::IFontFamily*, COPLT_OUT ::Coplt::u32* length) noexcept;
    ::Coplt::FontFamilyNameInfo const* (*const COPLT_CDECL f_GetNames)(const ::Coplt::IFontFamily*, COPLT_OUT ::Coplt::u32* length) noexcept;
    void (*const COPLT_CDECL f_ClearNativeNamesCache)(::Coplt::IFontFamily*) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontFamily
{
    extern "C" ::Coplt::Str16 const* COPLT_CDECL GetLocalNames(const ::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept;
    extern "C" ::Coplt::FontFamilyNameInfo const* COPLT_CDECL GetNames(const ::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept;
    extern "C" void COPLT_CDECL ClearNativeNamesCache(::Coplt::IFontFamily* self) noexcept;
}

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::IFontFamily>
{
    using VirtualTable = VirtualTable<::Coplt::IFontFamily>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("f8009d34-9417-4b87-b23b-b7885d27aeab");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::IFontFamily>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::IFontFamily*>(self)));
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
            .f_GetLocalNames = VirtualImpl_Coplt_IFontFamily::GetLocalNames,
            .f_GetNames = VirtualImpl_Coplt_IFontFamily::GetNames,
            .f_ClearNativeNamesCache = VirtualImpl_Coplt_IFontFamily::ClearNativeNamesCache,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual ::Coplt::Str16 const* Impl_GetLocalNames(COPLT_OUT ::Coplt::u32* length) const = 0;
        virtual ::Coplt::FontFamilyNameInfo const* Impl_GetNames(COPLT_OUT ::Coplt::u32* length) const = 0;
        virtual void Impl_ClearNativeNamesCache() = 0;
    };

    template <std::derived_from<::Coplt::IFontFamily> Base = ::Coplt::IFontFamily>
    struct Proxy : Impl, Base
    {
        explicit Proxy(const ::Coplt::Internal::VirtualTable<Base>* vtb) : Base(vtb) {}

        explicit Proxy() : Base(&GetVtb()) {}
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontFamily
{

    extern "C" inline ::Coplt::Str16 const* COPLT_CDECL GetLocalNames(const ::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept
    {
        ::Coplt::Str16 const* r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontFamily, GetLocalNames, ::Coplt::Str16 const*)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IFontFamily>(self)->Impl_GetLocalNames(p0);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontFamily, GetLocalNames, ::Coplt::Str16 const*)
        #endif
        return r;
    }

    extern "C" inline ::Coplt::FontFamilyNameInfo const* COPLT_CDECL GetNames(const ::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept
    {
        ::Coplt::FontFamilyNameInfo const* r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontFamily, GetNames, ::Coplt::FontFamilyNameInfo const*)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IFontFamily>(self)->Impl_GetNames(p0);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontFamily, GetNames, ::Coplt::FontFamilyNameInfo const*)
        #endif
        return r;
    }

    extern "C" inline void COPLT_CDECL ClearNativeNamesCache(::Coplt::IFontFamily* self) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontFamily, ClearNativeNamesCache, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IFontFamily>(self)->Impl_ClearNativeNamesCache();
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontFamily, ClearNativeNamesCache, void)
        #endif
    }
}
#define COPLT_COM_INTERFACE_BODY_Coplt_IFontFamily\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::IFontFamily;\
\
    explicit IFontFamily(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::IFontFamily>
{
    static COPLT_FORCE_INLINE ::Coplt::Str16 const* GetLocalNames(const ::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept
    {
        return COPLT_COM_PVTB(IFontFamily, self)->f_GetLocalNames(self, p0);
    }
    static COPLT_FORCE_INLINE ::Coplt::FontFamilyNameInfo const* GetNames(const ::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept
    {
        return COPLT_COM_PVTB(IFontFamily, self)->f_GetNames(self, p0);
    }
    static COPLT_FORCE_INLINE void ClearNativeNamesCache(::Coplt::IFontFamily* self) noexcept
    {
        COPLT_COM_PVTB(IFontFamily, self)->f_ClearNativeNamesCache(self);
    }
};

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::ILibTextLayout>
{
    VirtualTable<::Coplt::IUnknown> b;
    void (*const COPLT_CDECL f_SetLogger)(::Coplt::ILibTextLayout*, void* obj, ::Coplt::Func<void, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* logger, ::Coplt::Func<void, void*>* drop) noexcept;
    ::Coplt::Str8* (*const COPLT_CDECL f_GetCurrentErrorMessage)(::Coplt::ILibTextLayout*, ::Coplt::Str8*) noexcept;
    ::Coplt::HResult* (*const COPLT_CDECL f_GetSystemFontCollection)(::Coplt::ILibTextLayout*, ::Coplt::HResult*, IFontCollection** fc) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_ILibTextLayout
{
    extern "C" void COPLT_CDECL SetLogger(::Coplt::ILibTextLayout* self, void* p0, ::Coplt::Func<void, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* p1, ::Coplt::Func<void, void*>* p2) noexcept;
    extern "C" ::Coplt::Str8* COPLT_CDECL GetCurrentErrorMessage(::Coplt::ILibTextLayout* self, ::Coplt::Str8* r) noexcept;
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
            .f_GetCurrentErrorMessage = VirtualImpl_Coplt_ILibTextLayout::GetCurrentErrorMessage,
            .f_GetSystemFontCollection = VirtualImpl_Coplt_ILibTextLayout::GetSystemFontCollection,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual void Impl_SetLogger(void* obj, ::Coplt::Func<void, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* logger, ::Coplt::Func<void, void*>* drop) = 0;
        virtual ::Coplt::Str8 Impl_GetCurrentErrorMessage() = 0;
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

    extern "C" inline ::Coplt::Str8* COPLT_CDECL GetCurrentErrorMessage(::Coplt::ILibTextLayout* self, ::Coplt::Str8* r) noexcept
    {
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILibTextLayout, GetCurrentErrorMessage, ::Coplt::Str8)
        #endif
        *r = ::Coplt::Internal::AsImpl<::Coplt::ILibTextLayout>(self)->Impl_GetCurrentErrorMessage();
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILibTextLayout, GetCurrentErrorMessage, ::Coplt::Str8)
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
    static COPLT_FORCE_INLINE ::Coplt::Str8 GetCurrentErrorMessage(::Coplt::ILibTextLayout* self) noexcept
    {
        ::Coplt::Str8 r{};
        return *COPLT_COM_PVTB(ILibTextLayout, self)->f_GetCurrentErrorMessage(self, &r);
    }
    static COPLT_FORCE_INLINE ::Coplt::HResult GetSystemFontCollection(::Coplt::ILibTextLayout* self, IFontCollection** p0) noexcept
    {
        ::Coplt::HResult r{};
        return *COPLT_COM_PVTB(ILibTextLayout, self)->f_GetSystemFontCollection(self, &r, p0);
    }
};

#endif //COPLT_UL_TEXT_LAYOUT_DETAILS_H
