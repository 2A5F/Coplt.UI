#pragma once
#ifndef COPLT_UI_DETAILS_H
#define COPLT_UI_DETAILS_H

#include "CoCom.h"
#include "./Types.h"

namespace Coplt {

    using IUnknown = ::Coplt::IUnknown;
    using IWeak = ::Coplt::IWeak;

    struct IFont;
    struct IFontCollection;
    struct IFontFace;
    struct IFontFamily;
    struct ILib;

} // namespace Coplt

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::IFont>
{
    VirtualTable<::Coplt::IUnknown> b;
    ::Coplt::NFontInfo const* (*const COPLT_CDECL f_get_Info)(const ::Coplt::IFont*) noexcept;
    ::Coplt::HResult* (*const COPLT_CDECL f_CreateFace)(const ::Coplt::IFont*, ::Coplt::HResult*, COPLT_OUT IFontFace** face) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFont
{
    extern "C" ::Coplt::NFontInfo const* COPLT_CDECL get_Info(const ::Coplt::IFont* self) noexcept;
    extern "C" ::Coplt::HResult* COPLT_CDECL CreateFace(const ::Coplt::IFont* self, ::Coplt::HResult* r, COPLT_OUT IFontFace** p0) noexcept;
}

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::IFont>
{
    using VirtualTable = VirtualTable<::Coplt::IFont>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("09c443bc-9736-4aac-8117-6890555005ff");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::IFont>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::IFont*>(self)));
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
            .f_get_Info = VirtualImpl_Coplt_IFont::get_Info,
            .f_CreateFace = VirtualImpl_Coplt_IFont::CreateFace,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual ::Coplt::NFontInfo const* Impl_get_Info() const = 0;
        virtual ::Coplt::HResult Impl_CreateFace(COPLT_OUT IFontFace** face) const = 0;
    };

    template <std::derived_from<::Coplt::IFont> Base = ::Coplt::IFont>
    struct Proxy : Impl, Base
    {
        explicit Proxy(const ::Coplt::Internal::VirtualTable<Base>* vtb) : Base(vtb) {}

        explicit Proxy() : Base(&GetVtb()) {}
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFont
{

    extern "C" inline ::Coplt::NFontInfo const* COPLT_CDECL get_Info(const ::Coplt::IFont* self) noexcept
    {
        ::Coplt::NFontInfo const* r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFont, get_Info, ::Coplt::NFontInfo const*)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IFont>(self)->Impl_get_Info();
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFont, get_Info, ::Coplt::NFontInfo const*)
        #endif
        return r;
    }

    extern "C" inline ::Coplt::HResult* COPLT_CDECL CreateFace(const ::Coplt::IFont* self, ::Coplt::HResult* r, COPLT_OUT IFontFace** p0) noexcept
    {
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFont, CreateFace, ::Coplt::HResult)
        #endif
        *r = ::Coplt::Internal::AsImpl<::Coplt::IFont>(self)->Impl_CreateFace(p0);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFont, CreateFace, ::Coplt::HResult)
        #endif
        return r;
    }
}
#define COPLT_COM_INTERFACE_BODY_Coplt_IFont\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::IFont;\
\
    explicit IFont(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::IFont>
{
    static COPLT_FORCE_INLINE ::Coplt::NFontInfo const* get_Info(const ::Coplt::IFont* self) noexcept
    {
        return COPLT_COM_PVTB(IFont, self)->f_get_Info(self);
    }
    static COPLT_FORCE_INLINE ::Coplt::HResult CreateFace(const ::Coplt::IFont* self, COPLT_OUT IFontFace** p0) noexcept
    {
        ::Coplt::HResult r{};
        return *COPLT_COM_PVTB(IFont, self)->f_CreateFace(self, &r, p0);
    }
};

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::IFontCollection>
{
    VirtualTable<::Coplt::IUnknown> b;
    IFontFamily* const* (*const COPLT_CDECL f_GetFamilies)(const ::Coplt::IFontCollection*, COPLT_OUT ::Coplt::u32* count) noexcept;
    void (*const COPLT_CDECL f_ClearNativeFamiliesCache)(::Coplt::IFontCollection*) noexcept;
    ::Coplt::u32 (*const COPLT_CDECL f_FindDefaultFamily)(::Coplt::IFontCollection*) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontCollection
{
    extern "C" IFontFamily* const* COPLT_CDECL GetFamilies(const ::Coplt::IFontCollection* self, COPLT_OUT ::Coplt::u32* p0) noexcept;
    extern "C" void COPLT_CDECL ClearNativeFamiliesCache(::Coplt::IFontCollection* self) noexcept;
    extern "C" ::Coplt::u32 COPLT_CDECL FindDefaultFamily(::Coplt::IFontCollection* self) noexcept;
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
            .f_FindDefaultFamily = VirtualImpl_Coplt_IFontCollection::FindDefaultFamily,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual IFontFamily* const* Impl_GetFamilies(COPLT_OUT ::Coplt::u32* count) const = 0;
        virtual void Impl_ClearNativeFamiliesCache() = 0;
        virtual ::Coplt::u32 Impl_FindDefaultFamily() = 0;
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

    extern "C" inline ::Coplt::u32 COPLT_CDECL FindDefaultFamily(::Coplt::IFontCollection* self) noexcept
    {
        ::Coplt::u32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontCollection, FindDefaultFamily, ::Coplt::u32)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IFontCollection>(self)->Impl_FindDefaultFamily();
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontCollection, FindDefaultFamily, ::Coplt::u32)
        #endif
        return r;
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
    static COPLT_FORCE_INLINE ::Coplt::u32 FindDefaultFamily(::Coplt::IFontCollection* self) noexcept
    {
        return COPLT_COM_PVTB(IFontCollection, self)->f_FindDefaultFamily(self);
    }
};

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::IFontFace>
{
    VirtualTable<::Coplt::IUnknown> b;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontFace
{
}

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::IFontFace>
{
    using VirtualTable = VirtualTable<::Coplt::IFontFace>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("09c443bc-9736-4aac-8117-6890555005ff");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::IFontFace>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::IFontFace*>(self)));
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

    template <std::derived_from<::Coplt::IFontFace> Base = ::Coplt::IFontFace>
    struct Proxy : Impl, Base
    {
        explicit Proxy(const ::Coplt::Internal::VirtualTable<Base>* vtb) : Base(vtb) {}

        explicit Proxy() : Base(&GetVtb()) {}
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontFace
{
}
#define COPLT_COM_INTERFACE_BODY_Coplt_IFontFace\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::IFontFace;\
\
    explicit IFontFace(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::IFontFace>
{
};

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::IFontFamily>
{
    VirtualTable<::Coplt::IUnknown> b;
    ::Coplt::Str16 const* (*const COPLT_CDECL f_GetLocalNames)(const ::Coplt::IFontFamily*, COPLT_OUT ::Coplt::u32* length) noexcept;
    ::Coplt::FontFamilyNameInfo const* (*const COPLT_CDECL f_GetNames)(const ::Coplt::IFontFamily*, COPLT_OUT ::Coplt::u32* length) noexcept;
    void (*const COPLT_CDECL f_ClearNativeNamesCache)(::Coplt::IFontFamily*) noexcept;
    ::Coplt::NFontPair const* (*const COPLT_CDECL f_GetFonts)(::Coplt::IFontFamily*, COPLT_OUT ::Coplt::u32* length) noexcept;
    void (*const COPLT_CDECL f_ClearNativeFontsCache)(::Coplt::IFontFamily*) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontFamily
{
    extern "C" ::Coplt::Str16 const* COPLT_CDECL GetLocalNames(const ::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept;
    extern "C" ::Coplt::FontFamilyNameInfo const* COPLT_CDECL GetNames(const ::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept;
    extern "C" void COPLT_CDECL ClearNativeNamesCache(::Coplt::IFontFamily* self) noexcept;
    extern "C" ::Coplt::NFontPair const* COPLT_CDECL GetFonts(::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept;
    extern "C" void COPLT_CDECL ClearNativeFontsCache(::Coplt::IFontFamily* self) noexcept;
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
            .f_GetFonts = VirtualImpl_Coplt_IFontFamily::GetFonts,
            .f_ClearNativeFontsCache = VirtualImpl_Coplt_IFontFamily::ClearNativeFontsCache,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual ::Coplt::Str16 const* Impl_GetLocalNames(COPLT_OUT ::Coplt::u32* length) const = 0;
        virtual ::Coplt::FontFamilyNameInfo const* Impl_GetNames(COPLT_OUT ::Coplt::u32* length) const = 0;
        virtual void Impl_ClearNativeNamesCache() = 0;
        virtual ::Coplt::NFontPair const* Impl_GetFonts(COPLT_OUT ::Coplt::u32* length) = 0;
        virtual void Impl_ClearNativeFontsCache() = 0;
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

    extern "C" inline ::Coplt::NFontPair const* COPLT_CDECL GetFonts(::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept
    {
        ::Coplt::NFontPair const* r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontFamily, GetFonts, ::Coplt::NFontPair const*)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IFontFamily>(self)->Impl_GetFonts(p0);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontFamily, GetFonts, ::Coplt::NFontPair const*)
        #endif
        return r;
    }

    extern "C" inline void COPLT_CDECL ClearNativeFontsCache(::Coplt::IFontFamily* self) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontFamily, ClearNativeFontsCache, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IFontFamily>(self)->Impl_ClearNativeFontsCache();
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontFamily, ClearNativeFontsCache, void)
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
    static COPLT_FORCE_INLINE ::Coplt::NFontPair const* GetFonts(::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept
    {
        return COPLT_COM_PVTB(IFontFamily, self)->f_GetFonts(self, p0);
    }
    static COPLT_FORCE_INLINE void ClearNativeFontsCache(::Coplt::IFontFamily* self) noexcept
    {
        COPLT_COM_PVTB(IFontFamily, self)->f_ClearNativeFontsCache(self);
    }
};

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::ILib>
{
    VirtualTable<::Coplt::IUnknown> b;
    void (*const COPLT_CDECL f_SetLogger)(::Coplt::ILib*, void* obj, ::Coplt::Func<void, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* logger, ::Coplt::Func<void, void*>* drop) noexcept;
    ::Coplt::Str8* (*const COPLT_CDECL f_GetCurrentErrorMessage)(::Coplt::ILib*, ::Coplt::Str8*) noexcept;
    ::Coplt::HResult* (*const COPLT_CDECL f_GetSystemFontCollection)(::Coplt::ILib*, ::Coplt::HResult*, IFontCollection** fc) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_ILib
{
    extern "C" void COPLT_CDECL SetLogger(::Coplt::ILib* self, void* p0, ::Coplt::Func<void, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* p1, ::Coplt::Func<void, void*>* p2) noexcept;
    extern "C" ::Coplt::Str8* COPLT_CDECL GetCurrentErrorMessage(::Coplt::ILib* self, ::Coplt::Str8* r) noexcept;
    extern "C" ::Coplt::HResult* COPLT_CDECL GetSystemFontCollection(::Coplt::ILib* self, ::Coplt::HResult* r, IFontCollection** p0) noexcept;
}

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::ILib>
{
    using VirtualTable = VirtualTable<::Coplt::ILib>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("778be1fe-18f2-4aa5-8d1f-52d83b132cff");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::ILib>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::ILib*>(self)));
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
            .f_SetLogger = VirtualImpl_Coplt_ILib::SetLogger,
            .f_GetCurrentErrorMessage = VirtualImpl_Coplt_ILib::GetCurrentErrorMessage,
            .f_GetSystemFontCollection = VirtualImpl_Coplt_ILib::GetSystemFontCollection,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual void Impl_SetLogger(void* obj, ::Coplt::Func<void, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* logger, ::Coplt::Func<void, void*>* drop) = 0;
        virtual ::Coplt::Str8 Impl_GetCurrentErrorMessage() = 0;
        virtual ::Coplt::HResult Impl_GetSystemFontCollection(IFontCollection** fc) = 0;
    };

    template <std::derived_from<::Coplt::ILib> Base = ::Coplt::ILib>
    struct Proxy : Impl, Base
    {
        explicit Proxy(const ::Coplt::Internal::VirtualTable<Base>* vtb) : Base(vtb) {}

        explicit Proxy() : Base(&GetVtb()) {}
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_ILib
{

    extern "C" inline void COPLT_CDECL SetLogger(::Coplt::ILib* self, void* p0, ::Coplt::Func<void, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* p1, ::Coplt::Func<void, void*>* p2) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILib, SetLogger, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::ILib>(self)->Impl_SetLogger(p0, p1, p2);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILib, SetLogger, void)
        #endif
    }

    extern "C" inline ::Coplt::Str8* COPLT_CDECL GetCurrentErrorMessage(::Coplt::ILib* self, ::Coplt::Str8* r) noexcept
    {
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILib, GetCurrentErrorMessage, ::Coplt::Str8)
        #endif
        *r = ::Coplt::Internal::AsImpl<::Coplt::ILib>(self)->Impl_GetCurrentErrorMessage();
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILib, GetCurrentErrorMessage, ::Coplt::Str8)
        #endif
        return r;
    }

    extern "C" inline ::Coplt::HResult* COPLT_CDECL GetSystemFontCollection(::Coplt::ILib* self, ::Coplt::HResult* r, IFontCollection** p0) noexcept
    {
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILib, GetSystemFontCollection, ::Coplt::HResult)
        #endif
        *r = ::Coplt::Internal::AsImpl<::Coplt::ILib>(self)->Impl_GetSystemFontCollection(p0);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILib, GetSystemFontCollection, ::Coplt::HResult)
        #endif
        return r;
    }
}
#define COPLT_COM_INTERFACE_BODY_Coplt_ILib\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::ILib;\
\
    explicit ILib(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::ILib>
{
    static COPLT_FORCE_INLINE void SetLogger(::Coplt::ILib* self, void* p0, ::Coplt::Func<void, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* p1, ::Coplt::Func<void, void*>* p2) noexcept
    {
        COPLT_COM_PVTB(ILib, self)->f_SetLogger(self, p0, p1, p2);
    }
    static COPLT_FORCE_INLINE ::Coplt::Str8 GetCurrentErrorMessage(::Coplt::ILib* self) noexcept
    {
        ::Coplt::Str8 r{};
        return *COPLT_COM_PVTB(ILib, self)->f_GetCurrentErrorMessage(self, &r);
    }
    static COPLT_FORCE_INLINE ::Coplt::HResult GetSystemFontCollection(::Coplt::ILib* self, IFontCollection** p0) noexcept
    {
        ::Coplt::HResult r{};
        return *COPLT_COM_PVTB(ILib, self)->f_GetSystemFontCollection(self, &r, p0);
    }
};

#endif //COPLT_UI_DETAILS_H
