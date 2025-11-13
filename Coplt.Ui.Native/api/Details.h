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
    struct IFontFallback;
    struct IFontFamily;
    struct IFontManager;
    struct ILayout;
    struct ILib;
    struct IStub;
    struct ITextData;
    struct ITextLayout;

} // namespace Coplt

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::IFont>
{
    VirtualTable<::Coplt::IUnknown> b;
    ::Coplt::NFontInfo const* (*const COPLT_CDECL f_get_Info)(const ::Coplt::IFont*) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_CreateFace)(const ::Coplt::IFont*, COPLT_OUT IFontFace** face) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFont
{
    extern "C" ::Coplt::NFontInfo const* COPLT_CDECL get_Info(const ::Coplt::IFont* self) noexcept;
    extern "C" ::Coplt::i32 COPLT_CDECL CreateFace(const ::Coplt::IFont* self, COPLT_OUT IFontFace** p0) noexcept;
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
            self->AddRef();
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
    template <class Impl>
    struct VirtualImpl
    {
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(const Interface* self) { return static_cast<const Impl*>(self); }
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(Interface* self) { return static_cast<Impl*>(self); }

        static ::Coplt::NFontInfo const* COPLT_CDECL f_get_Info(const ::Coplt::IFont* self) noexcept
        {
            return AsImpl(self)->Impl_get_Info();
        }

        static ::Coplt::i32 COPLT_CDECL f_CreateFace(const ::Coplt::IFont* self, COPLT_OUT IFontFace** p0) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_CreateFace(p0));
        }
    };

    template<class Impl>
    constexpr static VirtualTable s_vtb
    {
        .b = ComProxy<::Coplt::IUnknown>::s_vtb<Impl>,
        .f_get_Info = VirtualImpl<Impl>::f_get_Info,
        .f_CreateFace = VirtualImpl<Impl>::f_CreateFace,
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

    extern "C" inline ::Coplt::i32 COPLT_CDECL CreateFace(const ::Coplt::IFont* self, COPLT_OUT IFontFace** p0) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFont, CreateFace, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::IFont>(self)->Impl_CreateFace(p0));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFont, CreateFace, ::Coplt::i32)
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
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(IFont, self)->f_CreateFace(self, p0));
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
            self->AddRef();
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
    template <class Impl>
    struct VirtualImpl
    {
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(const Interface* self) { return static_cast<const Impl*>(self); }
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(Interface* self) { return static_cast<Impl*>(self); }

        static IFontFamily* const* COPLT_CDECL f_GetFamilies(const ::Coplt::IFontCollection* self, COPLT_OUT ::Coplt::u32* p0) noexcept
        {
            return AsImpl(self)->Impl_GetFamilies(p0);
        }

        static void COPLT_CDECL f_ClearNativeFamiliesCache(::Coplt::IFontCollection* self) noexcept
        {
            AsImpl(self)->Impl_ClearNativeFamiliesCache();
        }

        static ::Coplt::u32 COPLT_CDECL f_FindDefaultFamily(::Coplt::IFontCollection* self) noexcept
        {
            return AsImpl(self)->Impl_FindDefaultFamily();
        }
    };

    template<class Impl>
    constexpr static VirtualTable s_vtb
    {
        .b = ComProxy<::Coplt::IUnknown>::s_vtb<Impl>,
        .f_GetFamilies = VirtualImpl<Impl>::f_GetFamilies,
        .f_ClearNativeFamiliesCache = VirtualImpl<Impl>::f_ClearNativeFamiliesCache,
        .f_FindDefaultFamily = VirtualImpl<Impl>::f_FindDefaultFamily,
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
            self->AddRef();
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
    template <class Impl>
    struct VirtualImpl
    {
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(const Interface* self) { return static_cast<const Impl*>(self); }
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(Interface* self) { return static_cast<Impl*>(self); }
    };

    template<class Impl>
    constexpr static VirtualTable s_vtb
    {
        .b = ComProxy<::Coplt::IUnknown>::s_vtb<Impl>,
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
struct ::Coplt::Internal::VirtualTable<::Coplt::IFontFallback>
{
    VirtualTable<::Coplt::IUnknown> b;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontFallback
{
}

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::IFontFallback>
{
    using VirtualTable = VirtualTable<::Coplt::IFontFallback>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("b0dbb428-eca1-4784-b27f-629bddf93ea4");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::IFontFallback>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::IFontFallback*>(self)));
            self->AddRef();
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

    template <std::derived_from<::Coplt::IFontFallback> Base = ::Coplt::IFontFallback>
    struct Proxy : Impl, Base
    {
        explicit Proxy(const ::Coplt::Internal::VirtualTable<Base>* vtb) : Base(vtb) {}

        explicit Proxy() : Base(&GetVtb()) {}
    };
    template <class Impl>
    struct VirtualImpl
    {
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(const Interface* self) { return static_cast<const Impl*>(self); }
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(Interface* self) { return static_cast<Impl*>(self); }
    };

    template<class Impl>
    constexpr static VirtualTable s_vtb
    {
        .b = ComProxy<::Coplt::IUnknown>::s_vtb<Impl>,
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontFallback
{
}
#define COPLT_COM_INTERFACE_BODY_Coplt_IFontFallback\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::IFontFallback;\
\
    explicit IFontFallback(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::IFontFallback>
{
};

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::IFontFamily>
{
    VirtualTable<::Coplt::IUnknown> b;
    ::Coplt::Str16 const* (*const COPLT_CDECL f_GetLocalNames)(const ::Coplt::IFontFamily*, COPLT_OUT ::Coplt::u32* length) noexcept;
    ::Coplt::FontFamilyNameInfo const* (*const COPLT_CDECL f_GetNames)(const ::Coplt::IFontFamily*, COPLT_OUT ::Coplt::u32* length) noexcept;
    void (*const COPLT_CDECL f_ClearNativeNamesCache)(::Coplt::IFontFamily*) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_GetFonts)(::Coplt::IFontFamily*, COPLT_OUT ::Coplt::u32* length, COPLT_OUT ::Coplt::NFontPair const** pair) noexcept;
    void (*const COPLT_CDECL f_ClearNativeFontsCache)(::Coplt::IFontFamily*) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontFamily
{
    extern "C" ::Coplt::Str16 const* COPLT_CDECL GetLocalNames(const ::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept;
    extern "C" ::Coplt::FontFamilyNameInfo const* COPLT_CDECL GetNames(const ::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept;
    extern "C" void COPLT_CDECL ClearNativeNamesCache(::Coplt::IFontFamily* self) noexcept;
    extern "C" ::Coplt::i32 COPLT_CDECL GetFonts(::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0, COPLT_OUT ::Coplt::NFontPair const** p1) noexcept;
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
            self->AddRef();
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
        virtual ::Coplt::HResult Impl_GetFonts(COPLT_OUT ::Coplt::u32* length, COPLT_OUT ::Coplt::NFontPair const** pair) = 0;
        virtual void Impl_ClearNativeFontsCache() = 0;
    };

    template <std::derived_from<::Coplt::IFontFamily> Base = ::Coplt::IFontFamily>
    struct Proxy : Impl, Base
    {
        explicit Proxy(const ::Coplt::Internal::VirtualTable<Base>* vtb) : Base(vtb) {}

        explicit Proxy() : Base(&GetVtb()) {}
    };
    template <class Impl>
    struct VirtualImpl
    {
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(const Interface* self) { return static_cast<const Impl*>(self); }
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(Interface* self) { return static_cast<Impl*>(self); }

        static ::Coplt::Str16 const* COPLT_CDECL f_GetLocalNames(const ::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept
        {
            return AsImpl(self)->Impl_GetLocalNames(p0);
        }

        static ::Coplt::FontFamilyNameInfo const* COPLT_CDECL f_GetNames(const ::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept
        {
            return AsImpl(self)->Impl_GetNames(p0);
        }

        static void COPLT_CDECL f_ClearNativeNamesCache(::Coplt::IFontFamily* self) noexcept
        {
            AsImpl(self)->Impl_ClearNativeNamesCache();
        }

        static ::Coplt::i32 COPLT_CDECL f_GetFonts(::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0, COPLT_OUT ::Coplt::NFontPair const** p1) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_GetFonts(p0, p1));
        }

        static void COPLT_CDECL f_ClearNativeFontsCache(::Coplt::IFontFamily* self) noexcept
        {
            AsImpl(self)->Impl_ClearNativeFontsCache();
        }
    };

    template<class Impl>
    constexpr static VirtualTable s_vtb
    {
        .b = ComProxy<::Coplt::IUnknown>::s_vtb<Impl>,
        .f_GetLocalNames = VirtualImpl<Impl>::f_GetLocalNames,
        .f_GetNames = VirtualImpl<Impl>::f_GetNames,
        .f_ClearNativeNamesCache = VirtualImpl<Impl>::f_ClearNativeNamesCache,
        .f_GetFonts = VirtualImpl<Impl>::f_GetFonts,
        .f_ClearNativeFontsCache = VirtualImpl<Impl>::f_ClearNativeFontsCache,
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

    extern "C" inline ::Coplt::i32 COPLT_CDECL GetFonts(::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0, COPLT_OUT ::Coplt::NFontPair const** p1) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontFamily, GetFonts, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::IFontFamily>(self)->Impl_GetFonts(p0, p1));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontFamily, GetFonts, ::Coplt::i32)
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
    static COPLT_FORCE_INLINE ::Coplt::HResult GetFonts(::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0, COPLT_OUT ::Coplt::NFontPair const** p1) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(IFontFamily, self)->f_GetFonts(self, p0, p1));
    }
    static COPLT_FORCE_INLINE void ClearNativeFontsCache(::Coplt::IFontFamily* self) noexcept
    {
        COPLT_COM_PVTB(IFontFamily, self)->f_ClearNativeFontsCache(self);
    }
};

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::IFontManager>
{
    VirtualTable<::Coplt::IUnknown> b;
    ::Coplt::u64 (*const COPLT_CDECL f_SetAssocUpdate)(::Coplt::IFontManager*, void* Data, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* OnAdd, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* OnExpired) noexcept;
    void* (*const COPLT_CDECL f_RemoveAssocUpdate)(::Coplt::IFontManager*, ::Coplt::u64 AssocUpdateId) noexcept;
    void (*const COPLT_CDECL f_SetExpireFrame)(::Coplt::IFontManager*, ::Coplt::u64 FrameCount) noexcept;
    void (*const COPLT_CDECL f_SetExpireTime)(::Coplt::IFontManager*, ::Coplt::u64 TimeTicks) noexcept;
    ::Coplt::u64 (*const COPLT_CDECL f_GetCurrentFrame)(const ::Coplt::IFontManager*) noexcept;
    void (*const COPLT_CDECL f_Update)(::Coplt::IFontManager*, ::Coplt::u64 CurrentTime) noexcept;
    ::Coplt::u64 (*const COPLT_CDECL f_FontFaceToId)(::Coplt::IFontManager*, IFontFace* Face) noexcept;
    IFontFace* (*const COPLT_CDECL f_IdToFontFace)(::Coplt::IFontManager*, ::Coplt::u64 Id) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontManager
{
    extern "C" ::Coplt::u64 COPLT_CDECL SetAssocUpdate(::Coplt::IFontManager* self, void* p0, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* p1, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* p2) noexcept;
    extern "C" void* COPLT_CDECL RemoveAssocUpdate(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept;
    extern "C" void COPLT_CDECL SetExpireFrame(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept;
    extern "C" void COPLT_CDECL SetExpireTime(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept;
    extern "C" ::Coplt::u64 COPLT_CDECL GetCurrentFrame(const ::Coplt::IFontManager* self) noexcept;
    extern "C" void COPLT_CDECL Update(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept;
    extern "C" ::Coplt::u64 COPLT_CDECL FontFaceToId(::Coplt::IFontManager* self, IFontFace* p0) noexcept;
    extern "C" IFontFace* COPLT_CDECL IdToFontFace(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept;
}

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::IFontManager>
{
    using VirtualTable = VirtualTable<::Coplt::IFontManager>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("15a9651e-4fa2-48f3-9291-df0f9681a7d1");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::IFontManager>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::IFontManager*>(self)));
            self->AddRef();
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
            .f_SetAssocUpdate = VirtualImpl_Coplt_IFontManager::SetAssocUpdate,
            .f_RemoveAssocUpdate = VirtualImpl_Coplt_IFontManager::RemoveAssocUpdate,
            .f_SetExpireFrame = VirtualImpl_Coplt_IFontManager::SetExpireFrame,
            .f_SetExpireTime = VirtualImpl_Coplt_IFontManager::SetExpireTime,
            .f_GetCurrentFrame = VirtualImpl_Coplt_IFontManager::GetCurrentFrame,
            .f_Update = VirtualImpl_Coplt_IFontManager::Update,
            .f_FontFaceToId = VirtualImpl_Coplt_IFontManager::FontFaceToId,
            .f_IdToFontFace = VirtualImpl_Coplt_IFontManager::IdToFontFace,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual ::Coplt::u64 Impl_SetAssocUpdate(void* Data, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* OnAdd, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* OnExpired) = 0;
        virtual void* Impl_RemoveAssocUpdate(::Coplt::u64 AssocUpdateId) = 0;
        virtual void Impl_SetExpireFrame(::Coplt::u64 FrameCount) = 0;
        virtual void Impl_SetExpireTime(::Coplt::u64 TimeTicks) = 0;
        virtual ::Coplt::u64 Impl_GetCurrentFrame() const = 0;
        virtual void Impl_Update(::Coplt::u64 CurrentTime) = 0;
        virtual ::Coplt::u64 Impl_FontFaceToId(IFontFace* Face) = 0;
        virtual IFontFace* Impl_IdToFontFace(::Coplt::u64 Id) = 0;
    };

    template <std::derived_from<::Coplt::IFontManager> Base = ::Coplt::IFontManager>
    struct Proxy : Impl, Base
    {
        explicit Proxy(const ::Coplt::Internal::VirtualTable<Base>* vtb) : Base(vtb) {}

        explicit Proxy() : Base(&GetVtb()) {}
    };
    template <class Impl>
    struct VirtualImpl
    {
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(const Interface* self) { return static_cast<const Impl*>(self); }
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(Interface* self) { return static_cast<Impl*>(self); }

        static ::Coplt::u64 COPLT_CDECL f_SetAssocUpdate(::Coplt::IFontManager* self, void* p0, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* p1, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* p2) noexcept
        {
            return AsImpl(self)->Impl_SetAssocUpdate(p0, p1, p2);
        }

        static void* COPLT_CDECL f_RemoveAssocUpdate(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
        {
            return AsImpl(self)->Impl_RemoveAssocUpdate(p0);
        }

        static void COPLT_CDECL f_SetExpireFrame(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
        {
            AsImpl(self)->Impl_SetExpireFrame(p0);
        }

        static void COPLT_CDECL f_SetExpireTime(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
        {
            AsImpl(self)->Impl_SetExpireTime(p0);
        }

        static ::Coplt::u64 COPLT_CDECL f_GetCurrentFrame(const ::Coplt::IFontManager* self) noexcept
        {
            return AsImpl(self)->Impl_GetCurrentFrame();
        }

        static void COPLT_CDECL f_Update(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
        {
            AsImpl(self)->Impl_Update(p0);
        }

        static ::Coplt::u64 COPLT_CDECL f_FontFaceToId(::Coplt::IFontManager* self, IFontFace* p0) noexcept
        {
            return AsImpl(self)->Impl_FontFaceToId(p0);
        }

        static IFontFace* COPLT_CDECL f_IdToFontFace(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
        {
            return AsImpl(self)->Impl_IdToFontFace(p0);
        }
    };

    template<class Impl>
    constexpr static VirtualTable s_vtb
    {
        .b = ComProxy<::Coplt::IUnknown>::s_vtb<Impl>,
        .f_SetAssocUpdate = VirtualImpl<Impl>::f_SetAssocUpdate,
        .f_RemoveAssocUpdate = VirtualImpl<Impl>::f_RemoveAssocUpdate,
        .f_SetExpireFrame = VirtualImpl<Impl>::f_SetExpireFrame,
        .f_SetExpireTime = VirtualImpl<Impl>::f_SetExpireTime,
        .f_GetCurrentFrame = VirtualImpl<Impl>::f_GetCurrentFrame,
        .f_Update = VirtualImpl<Impl>::f_Update,
        .f_FontFaceToId = VirtualImpl<Impl>::f_FontFaceToId,
        .f_IdToFontFace = VirtualImpl<Impl>::f_IdToFontFace,
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontManager
{

    extern "C" inline ::Coplt::u64 COPLT_CDECL SetAssocUpdate(::Coplt::IFontManager* self, void* p0, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* p1, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* p2) noexcept
    {
        ::Coplt::u64 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontManager, SetAssocUpdate, ::Coplt::u64)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IFontManager>(self)->Impl_SetAssocUpdate(p0, p1, p2);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontManager, SetAssocUpdate, ::Coplt::u64)
        #endif
        return r;
    }

    extern "C" inline void* COPLT_CDECL RemoveAssocUpdate(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
    {
        void* r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontManager, RemoveAssocUpdate, void*)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IFontManager>(self)->Impl_RemoveAssocUpdate(p0);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontManager, RemoveAssocUpdate, void*)
        #endif
        return r;
    }

    extern "C" inline void COPLT_CDECL SetExpireFrame(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontManager, SetExpireFrame, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IFontManager>(self)->Impl_SetExpireFrame(p0);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontManager, SetExpireFrame, void)
        #endif
    }

    extern "C" inline void COPLT_CDECL SetExpireTime(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontManager, SetExpireTime, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IFontManager>(self)->Impl_SetExpireTime(p0);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontManager, SetExpireTime, void)
        #endif
    }

    extern "C" inline ::Coplt::u64 COPLT_CDECL GetCurrentFrame(const ::Coplt::IFontManager* self) noexcept
    {
        ::Coplt::u64 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontManager, GetCurrentFrame, ::Coplt::u64)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IFontManager>(self)->Impl_GetCurrentFrame();
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontManager, GetCurrentFrame, ::Coplt::u64)
        #endif
        return r;
    }

    extern "C" inline void COPLT_CDECL Update(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontManager, Update, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IFontManager>(self)->Impl_Update(p0);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontManager, Update, void)
        #endif
    }

    extern "C" inline ::Coplt::u64 COPLT_CDECL FontFaceToId(::Coplt::IFontManager* self, IFontFace* p0) noexcept
    {
        ::Coplt::u64 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontManager, FontFaceToId, ::Coplt::u64)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IFontManager>(self)->Impl_FontFaceToId(p0);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontManager, FontFaceToId, ::Coplt::u64)
        #endif
        return r;
    }

    extern "C" inline IFontFace* COPLT_CDECL IdToFontFace(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
    {
        IFontFace* r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontManager, IdToFontFace, IFontFace*)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IFontManager>(self)->Impl_IdToFontFace(p0);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontManager, IdToFontFace, IFontFace*)
        #endif
        return r;
    }
}
#define COPLT_COM_INTERFACE_BODY_Coplt_IFontManager\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::IFontManager;\
\
    explicit IFontManager(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::IFontManager>
{
    static COPLT_FORCE_INLINE ::Coplt::u64 SetAssocUpdate(::Coplt::IFontManager* self, void* p0, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* p1, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* p2) noexcept
    {
        return COPLT_COM_PVTB(IFontManager, self)->f_SetAssocUpdate(self, p0, p1, p2);
    }
    static COPLT_FORCE_INLINE void* RemoveAssocUpdate(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
    {
        return COPLT_COM_PVTB(IFontManager, self)->f_RemoveAssocUpdate(self, p0);
    }
    static COPLT_FORCE_INLINE void SetExpireFrame(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
    {
        COPLT_COM_PVTB(IFontManager, self)->f_SetExpireFrame(self, p0);
    }
    static COPLT_FORCE_INLINE void SetExpireTime(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
    {
        COPLT_COM_PVTB(IFontManager, self)->f_SetExpireTime(self, p0);
    }
    static COPLT_FORCE_INLINE ::Coplt::u64 GetCurrentFrame(const ::Coplt::IFontManager* self) noexcept
    {
        return COPLT_COM_PVTB(IFontManager, self)->f_GetCurrentFrame(self);
    }
    static COPLT_FORCE_INLINE void Update(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
    {
        COPLT_COM_PVTB(IFontManager, self)->f_Update(self, p0);
    }
    static COPLT_FORCE_INLINE ::Coplt::u64 FontFaceToId(::Coplt::IFontManager* self, IFontFace* p0) noexcept
    {
        return COPLT_COM_PVTB(IFontManager, self)->f_FontFaceToId(self, p0);
    }
    static COPLT_FORCE_INLINE IFontFace* IdToFontFace(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
    {
        return COPLT_COM_PVTB(IFontManager, self)->f_IdToFontFace(self, p0);
    }
};

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::ILayout>
{
    VirtualTable<::Coplt::IUnknown> b;
    ::Coplt::i32 (*const COPLT_CDECL f_Calc)(::Coplt::ILayout*, ::Coplt::NLayoutContext* ctx) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_ILayout
{
    extern "C" ::Coplt::i32 COPLT_CDECL Calc(::Coplt::ILayout* self, ::Coplt::NLayoutContext* p0) noexcept;
}

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::ILayout>
{
    using VirtualTable = VirtualTable<::Coplt::ILayout>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("f1e64bf0-ffb9-42ce-be78-31871d247883");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::ILayout>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::ILayout*>(self)));
            self->AddRef();
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
            .f_Calc = VirtualImpl_Coplt_ILayout::Calc,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual ::Coplt::HResult Impl_Calc(::Coplt::NLayoutContext* ctx) = 0;
    };

    template <std::derived_from<::Coplt::ILayout> Base = ::Coplt::ILayout>
    struct Proxy : Impl, Base
    {
        explicit Proxy(const ::Coplt::Internal::VirtualTable<Base>* vtb) : Base(vtb) {}

        explicit Proxy() : Base(&GetVtb()) {}
    };
    template <class Impl>
    struct VirtualImpl
    {
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(const Interface* self) { return static_cast<const Impl*>(self); }
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(Interface* self) { return static_cast<Impl*>(self); }

        static ::Coplt::i32 COPLT_CDECL f_Calc(::Coplt::ILayout* self, ::Coplt::NLayoutContext* p0) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_Calc(p0));
        }
    };

    template<class Impl>
    constexpr static VirtualTable s_vtb
    {
        .b = ComProxy<::Coplt::IUnknown>::s_vtb<Impl>,
        .f_Calc = VirtualImpl<Impl>::f_Calc,
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_ILayout
{

    extern "C" inline ::Coplt::i32 COPLT_CDECL Calc(::Coplt::ILayout* self, ::Coplt::NLayoutContext* p0) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILayout, Calc, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::ILayout>(self)->Impl_Calc(p0));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILayout, Calc, ::Coplt::i32)
        #endif
        return r;
    }
}
#define COPLT_COM_INTERFACE_BODY_Coplt_ILayout\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::ILayout;\
\
    explicit ILayout(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::ILayout>
{
    static COPLT_FORCE_INLINE ::Coplt::HResult Calc(::Coplt::ILayout* self, ::Coplt::NLayoutContext* p0) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(ILayout, self)->f_Calc(self, p0));
    }
};

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::ILib>
{
    VirtualTable<::Coplt::IUnknown> b;
    void (*const COPLT_CDECL f_SetLogger)(::Coplt::ILib*, void* obj, ::Coplt::Func<void, void*, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* logger, ::Coplt::Func<void, void*>* drop) noexcept;
    ::Coplt::Str8* (*const COPLT_CDECL f_GetCurrentErrorMessage)(::Coplt::ILib*, ::Coplt::Str8*) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_GetSystemFontCollection)(::Coplt::ILib*, IFontCollection** fc) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_GetSystemFontFallback)(::Coplt::ILib*, IFontFallback** ff) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_CreateLayout)(::Coplt::ILib*, ILayout** layout) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_SplitTexts)(::Coplt::ILib*, ::Coplt::NativeList<::Coplt::TextRange>* ranges, ::Coplt::char16 const* chars, ::Coplt::i32 len) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_ILib
{
    extern "C" void COPLT_CDECL SetLogger(::Coplt::ILib* self, void* p0, ::Coplt::Func<void, void*, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* p1, ::Coplt::Func<void, void*>* p2) noexcept;
    extern "C" ::Coplt::Str8* COPLT_CDECL GetCurrentErrorMessage(::Coplt::ILib* self, ::Coplt::Str8* r) noexcept;
    extern "C" ::Coplt::i32 COPLT_CDECL GetSystemFontCollection(::Coplt::ILib* self, IFontCollection** p0) noexcept;
    extern "C" ::Coplt::i32 COPLT_CDECL GetSystemFontFallback(::Coplt::ILib* self, IFontFallback** p0) noexcept;
    extern "C" ::Coplt::i32 COPLT_CDECL CreateLayout(::Coplt::ILib* self, ILayout** p0) noexcept;
    extern "C" ::Coplt::i32 COPLT_CDECL SplitTexts(::Coplt::ILib* self, ::Coplt::NativeList<::Coplt::TextRange>* p0, ::Coplt::char16 const* p1, ::Coplt::i32 p2) noexcept;
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
            self->AddRef();
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
            .f_GetSystemFontFallback = VirtualImpl_Coplt_ILib::GetSystemFontFallback,
            .f_CreateLayout = VirtualImpl_Coplt_ILib::CreateLayout,
            .f_SplitTexts = VirtualImpl_Coplt_ILib::SplitTexts,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual void Impl_SetLogger(void* obj, ::Coplt::Func<void, void*, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* logger, ::Coplt::Func<void, void*>* drop) = 0;
        virtual ::Coplt::Str8 Impl_GetCurrentErrorMessage() = 0;
        virtual ::Coplt::HResult Impl_GetSystemFontCollection(IFontCollection** fc) = 0;
        virtual ::Coplt::HResult Impl_GetSystemFontFallback(IFontFallback** ff) = 0;
        virtual ::Coplt::HResult Impl_CreateLayout(ILayout** layout) = 0;
        virtual ::Coplt::HResult Impl_SplitTexts(::Coplt::NativeList<::Coplt::TextRange>* ranges, ::Coplt::char16 const* chars, ::Coplt::i32 len) = 0;
    };

    template <std::derived_from<::Coplt::ILib> Base = ::Coplt::ILib>
    struct Proxy : Impl, Base
    {
        explicit Proxy(const ::Coplt::Internal::VirtualTable<Base>* vtb) : Base(vtb) {}

        explicit Proxy() : Base(&GetVtb()) {}
    };
    template <class Impl>
    struct VirtualImpl
    {
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(const Interface* self) { return static_cast<const Impl*>(self); }
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(Interface* self) { return static_cast<Impl*>(self); }

        static void COPLT_CDECL f_SetLogger(::Coplt::ILib* self, void* p0, ::Coplt::Func<void, void*, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* p1, ::Coplt::Func<void, void*>* p2) noexcept
        {
            AsImpl(self)->Impl_SetLogger(p0, p1, p2);
        }

        static ::Coplt::Str8* COPLT_CDECL f_GetCurrentErrorMessage(::Coplt::ILib* self, ::Coplt::Str8* r) noexcept
        {
            *r = AsImpl(self)->Impl_GetCurrentErrorMessage();
            return r;
        }

        static ::Coplt::i32 COPLT_CDECL f_GetSystemFontCollection(::Coplt::ILib* self, IFontCollection** p0) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_GetSystemFontCollection(p0));
        }

        static ::Coplt::i32 COPLT_CDECL f_GetSystemFontFallback(::Coplt::ILib* self, IFontFallback** p0) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_GetSystemFontFallback(p0));
        }

        static ::Coplt::i32 COPLT_CDECL f_CreateLayout(::Coplt::ILib* self, ILayout** p0) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_CreateLayout(p0));
        }

        static ::Coplt::i32 COPLT_CDECL f_SplitTexts(::Coplt::ILib* self, ::Coplt::NativeList<::Coplt::TextRange>* p0, ::Coplt::char16 const* p1, ::Coplt::i32 p2) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_SplitTexts(p0, p1, p2));
        }
    };

    template<class Impl>
    constexpr static VirtualTable s_vtb
    {
        .b = ComProxy<::Coplt::IUnknown>::s_vtb<Impl>,
        .f_SetLogger = VirtualImpl<Impl>::f_SetLogger,
        .f_GetCurrentErrorMessage = VirtualImpl<Impl>::f_GetCurrentErrorMessage,
        .f_GetSystemFontCollection = VirtualImpl<Impl>::f_GetSystemFontCollection,
        .f_GetSystemFontFallback = VirtualImpl<Impl>::f_GetSystemFontFallback,
        .f_CreateLayout = VirtualImpl<Impl>::f_CreateLayout,
        .f_SplitTexts = VirtualImpl<Impl>::f_SplitTexts,
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_ILib
{

    extern "C" inline void COPLT_CDECL SetLogger(::Coplt::ILib* self, void* p0, ::Coplt::Func<void, void*, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* p1, ::Coplt::Func<void, void*>* p2) noexcept
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

    extern "C" inline ::Coplt::i32 COPLT_CDECL GetSystemFontCollection(::Coplt::ILib* self, IFontCollection** p0) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILib, GetSystemFontCollection, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::ILib>(self)->Impl_GetSystemFontCollection(p0));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILib, GetSystemFontCollection, ::Coplt::i32)
        #endif
        return r;
    }

    extern "C" inline ::Coplt::i32 COPLT_CDECL GetSystemFontFallback(::Coplt::ILib* self, IFontFallback** p0) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILib, GetSystemFontFallback, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::ILib>(self)->Impl_GetSystemFontFallback(p0));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILib, GetSystemFontFallback, ::Coplt::i32)
        #endif
        return r;
    }

    extern "C" inline ::Coplt::i32 COPLT_CDECL CreateLayout(::Coplt::ILib* self, ILayout** p0) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILib, CreateLayout, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::ILib>(self)->Impl_CreateLayout(p0));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILib, CreateLayout, ::Coplt::i32)
        #endif
        return r;
    }

    extern "C" inline ::Coplt::i32 COPLT_CDECL SplitTexts(::Coplt::ILib* self, ::Coplt::NativeList<::Coplt::TextRange>* p0, ::Coplt::char16 const* p1, ::Coplt::i32 p2) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILib, SplitTexts, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::ILib>(self)->Impl_SplitTexts(p0, p1, p2));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILib, SplitTexts, ::Coplt::i32)
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
    static COPLT_FORCE_INLINE void SetLogger(::Coplt::ILib* self, void* p0, ::Coplt::Func<void, void*, ::Coplt::LogLevel, ::Coplt::i32, ::Coplt::char16*>* p1, ::Coplt::Func<void, void*>* p2) noexcept
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
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(ILib, self)->f_GetSystemFontCollection(self, p0));
    }
    static COPLT_FORCE_INLINE ::Coplt::HResult GetSystemFontFallback(::Coplt::ILib* self, IFontFallback** p0) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(ILib, self)->f_GetSystemFontFallback(self, p0));
    }
    static COPLT_FORCE_INLINE ::Coplt::HResult CreateLayout(::Coplt::ILib* self, ILayout** p0) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(ILib, self)->f_CreateLayout(self, p0));
    }
    static COPLT_FORCE_INLINE ::Coplt::HResult SplitTexts(::Coplt::ILib* self, ::Coplt::NativeList<::Coplt::TextRange>* p0, ::Coplt::char16 const* p1, ::Coplt::i32 p2) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(ILib, self)->f_SplitTexts(self, p0, p1, p2));
    }
};

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::IStub>
{
    VirtualTable<::Coplt::IUnknown> b;
    void (*const COPLT_CDECL f_Some)(::Coplt::IStub*, ::Coplt::NodeType a, ::Coplt::RootData* b, ::Coplt::NString* c) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IStub
{
    extern "C" void COPLT_CDECL Some(::Coplt::IStub* self, ::Coplt::NodeType p0, ::Coplt::RootData* p1, ::Coplt::NString* p2) noexcept;
}

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::IStub>
{
    using VirtualTable = VirtualTable<::Coplt::IStub>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("a998ec87-868d-4320-a30a-638c291f5562");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::IStub>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::IStub*>(self)));
            self->AddRef();
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
            .f_Some = VirtualImpl_Coplt_IStub::Some,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual void Impl_Some(::Coplt::NodeType a, ::Coplt::RootData* b, ::Coplt::NString* c) = 0;
    };

    template <std::derived_from<::Coplt::IStub> Base = ::Coplt::IStub>
    struct Proxy : Impl, Base
    {
        explicit Proxy(const ::Coplt::Internal::VirtualTable<Base>* vtb) : Base(vtb) {}

        explicit Proxy() : Base(&GetVtb()) {}
    };
    template <class Impl>
    struct VirtualImpl
    {
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(const Interface* self) { return static_cast<const Impl*>(self); }
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(Interface* self) { return static_cast<Impl*>(self); }

        static void COPLT_CDECL f_Some(::Coplt::IStub* self, ::Coplt::NodeType p0, ::Coplt::RootData* p1, ::Coplt::NString* p2) noexcept
        {
            AsImpl(self)->Impl_Some(p0, p1, p2);
        }
    };

    template<class Impl>
    constexpr static VirtualTable s_vtb
    {
        .b = ComProxy<::Coplt::IUnknown>::s_vtb<Impl>,
        .f_Some = VirtualImpl<Impl>::f_Some,
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_IStub
{

    extern "C" inline void COPLT_CDECL Some(::Coplt::IStub* self, ::Coplt::NodeType p0, ::Coplt::RootData* p1, ::Coplt::NString* p2) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IStub, Some, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IStub>(self)->Impl_Some(p0, p1, p2);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IStub, Some, void)
        #endif
    }
}
#define COPLT_COM_INTERFACE_BODY_Coplt_IStub\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::IStub;\
\
    explicit IStub(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::IStub>
{
    static COPLT_FORCE_INLINE void Some(::Coplt::IStub* self, ::Coplt::NodeType p0, ::Coplt::RootData* p1, ::Coplt::NString* p2) noexcept
    {
        COPLT_COM_PVTB(IStub, self)->f_Some(self, p0, p1, p2);
    }
};

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::ITextData>
{
    VirtualTable<::Coplt::IUnknown> b;
};
namespace Coplt::Internal::VirtualImpl_Coplt_ITextData
{
}

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::ITextData>
{
    using VirtualTable = VirtualTable<::Coplt::ITextData>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("bd0c7402-1de8-4547-860d-c78fd70ff203");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::ITextData>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::ITextData*>(self)));
            self->AddRef();
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

    template <std::derived_from<::Coplt::ITextData> Base = ::Coplt::ITextData>
    struct Proxy : Impl, Base
    {
        explicit Proxy(const ::Coplt::Internal::VirtualTable<Base>* vtb) : Base(vtb) {}

        explicit Proxy() : Base(&GetVtb()) {}
    };
    template <class Impl>
    struct VirtualImpl
    {
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(const Interface* self) { return static_cast<const Impl*>(self); }
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(Interface* self) { return static_cast<Impl*>(self); }
    };

    template<class Impl>
    constexpr static VirtualTable s_vtb
    {
        .b = ComProxy<::Coplt::IUnknown>::s_vtb<Impl>,
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_ITextData
{
}
#define COPLT_COM_INTERFACE_BODY_Coplt_ITextData\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::ITextData;\
\
    explicit ITextData(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::ITextData>
{
};

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::ITextLayout>
{
    VirtualTable<::Coplt::IUnknown> b;
};
namespace Coplt::Internal::VirtualImpl_Coplt_ITextLayout
{
}

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::ITextLayout>
{
    using VirtualTable = VirtualTable<::Coplt::ITextLayout>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("f558ba07-1f1d-4c32-8229-134271b17083");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::ITextLayout>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::ITextLayout*>(self)));
            self->AddRef();
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

    template <std::derived_from<::Coplt::ITextLayout> Base = ::Coplt::ITextLayout>
    struct Proxy : Impl, Base
    {
        explicit Proxy(const ::Coplt::Internal::VirtualTable<Base>* vtb) : Base(vtb) {}

        explicit Proxy() : Base(&GetVtb()) {}
    };
    template <class Impl>
    struct VirtualImpl
    {
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(const Interface* self) { return static_cast<const Impl*>(self); }
        template <class Interface>
        COPLT_FORCE_INLINE static auto AsImpl(Interface* self) { return static_cast<Impl*>(self); }
    };

    template<class Impl>
    constexpr static VirtualTable s_vtb
    {
        .b = ComProxy<::Coplt::IUnknown>::s_vtb<Impl>,
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_ITextLayout
{
}
#define COPLT_COM_INTERFACE_BODY_Coplt_ITextLayout\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::ITextLayout;\
\
    explicit ITextLayout(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::ITextLayout>
{
};

#endif //COPLT_UI_DETAILS_H
