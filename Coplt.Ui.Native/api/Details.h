#pragma once
#ifndef COPLT_UI_DETAILS_H
#define COPLT_UI_DETAILS_H

#include "CoCom.h"
#include "./Types.h"

namespace Coplt {

    using IUnknown = ::Coplt::IUnknown;
    using IWeak = ::Coplt::IWeak;

    struct IAtlasAllocator;
    struct IFont;
    struct IFontCollection;
    struct IFontFace;
    struct IFontFallback;
    struct IFontFallbackBuilder;
    struct IFontFamily;
    struct IFontManager;
    struct ILayout;
    struct ILib;
    struct IPath;
    struct IPathBuilder;
    struct IStub;
    struct ITessellator;
    struct ITextData;
    struct ITextLayout;

} // namespace Coplt

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::IAtlasAllocator>
{
    VirtualTable<::Coplt::IUnknown> b;
    void (*const COPLT_CDECL f_Clear)(::Coplt::IAtlasAllocator*) noexcept;
    bool (*const COPLT_CDECL f_get_IsEmpty)(::Coplt::IAtlasAllocator*) noexcept;
    void (*const COPLT_CDECL f_GetSize)(::Coplt::IAtlasAllocator*, ::Coplt::i32* out_width, ::Coplt::i32* out_height) noexcept;
    bool (*const COPLT_CDECL f_Allocate)(::Coplt::IAtlasAllocator*, ::Coplt::i32 width, ::Coplt::i32 height, ::Coplt::u32* out_id, ::Coplt::AABB2DI* out_rect) noexcept;
    void (*const COPLT_CDECL f_Deallocate)(::Coplt::IAtlasAllocator*, ::Coplt::u32 id) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IAtlasAllocator
{
    void COPLT_CDECL Clear(::Coplt::IAtlasAllocator* self) noexcept;
    bool COPLT_CDECL get_IsEmpty(::Coplt::IAtlasAllocator* self) noexcept;
    void COPLT_CDECL GetSize(::Coplt::IAtlasAllocator* self, ::Coplt::i32* p0, ::Coplt::i32* p1) noexcept;
    bool COPLT_CDECL Allocate(::Coplt::IAtlasAllocator* self, ::Coplt::i32 p0, ::Coplt::i32 p1, ::Coplt::u32* p2, ::Coplt::AABB2DI* p3) noexcept;
    void COPLT_CDECL Deallocate(::Coplt::IAtlasAllocator* self, ::Coplt::u32 p0) noexcept;
}

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::IAtlasAllocator>
{
    using VirtualTable = VirtualTable<::Coplt::IAtlasAllocator>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("32b30623-411e-4fd5-a009-ae7e9ed88e78");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::IAtlasAllocator>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::IAtlasAllocator*>(self)));
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
            .f_Clear = VirtualImpl_Coplt_IAtlasAllocator::Clear,
            .f_get_IsEmpty = VirtualImpl_Coplt_IAtlasAllocator::get_IsEmpty,
            .f_GetSize = VirtualImpl_Coplt_IAtlasAllocator::GetSize,
            .f_Allocate = VirtualImpl_Coplt_IAtlasAllocator::Allocate,
            .f_Deallocate = VirtualImpl_Coplt_IAtlasAllocator::Deallocate,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual void Impl_Clear() = 0;
        virtual bool Impl_get_IsEmpty() = 0;
        virtual void Impl_GetSize(::Coplt::i32* out_width, ::Coplt::i32* out_height) = 0;
        virtual bool Impl_Allocate(::Coplt::i32 width, ::Coplt::i32 height, ::Coplt::u32* out_id, ::Coplt::AABB2DI* out_rect) = 0;
        virtual void Impl_Deallocate(::Coplt::u32 id) = 0;
    };

    template <std::derived_from<::Coplt::IAtlasAllocator> Base = ::Coplt::IAtlasAllocator>
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

        static void COPLT_CDECL f_Clear(::Coplt::IAtlasAllocator* self) noexcept
        {
            AsImpl(self)->Impl_Clear();
        }

        static bool COPLT_CDECL f_get_IsEmpty(::Coplt::IAtlasAllocator* self) noexcept
        {
            return AsImpl(self)->Impl_get_IsEmpty();
        }

        static void COPLT_CDECL f_GetSize(::Coplt::IAtlasAllocator* self, ::Coplt::i32* p0, ::Coplt::i32* p1) noexcept
        {
            AsImpl(self)->Impl_GetSize(p0, p1);
        }

        static bool COPLT_CDECL f_Allocate(::Coplt::IAtlasAllocator* self, ::Coplt::i32 p0, ::Coplt::i32 p1, ::Coplt::u32* p2, ::Coplt::AABB2DI* p3) noexcept
        {
            return AsImpl(self)->Impl_Allocate(p0, p1, p2, p3);
        }

        static void COPLT_CDECL f_Deallocate(::Coplt::IAtlasAllocator* self, ::Coplt::u32 p0) noexcept
        {
            AsImpl(self)->Impl_Deallocate(p0);
        }
    };

    template<class Impl>
    constexpr static VirtualTable s_vtb
    {
        .b = ComProxy<::Coplt::IUnknown>::s_vtb<Impl>,
        .f_Clear = VirtualImpl<Impl>::f_Clear,
        .f_get_IsEmpty = VirtualImpl<Impl>::f_get_IsEmpty,
        .f_GetSize = VirtualImpl<Impl>::f_GetSize,
        .f_Allocate = VirtualImpl<Impl>::f_Allocate,
        .f_Deallocate = VirtualImpl<Impl>::f_Deallocate,
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_IAtlasAllocator
{

    inline void COPLT_CDECL Clear(::Coplt::IAtlasAllocator* self) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IAtlasAllocator, Clear, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IAtlasAllocator>(self)->Impl_Clear();
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IAtlasAllocator, Clear, void)
        #endif
    }

    inline bool COPLT_CDECL get_IsEmpty(::Coplt::IAtlasAllocator* self) noexcept
    {
        bool r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IAtlasAllocator, get_IsEmpty, bool)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IAtlasAllocator>(self)->Impl_get_IsEmpty();
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IAtlasAllocator, get_IsEmpty, bool)
        #endif
        return r;
    }

    inline void COPLT_CDECL GetSize(::Coplt::IAtlasAllocator* self, ::Coplt::i32* p0, ::Coplt::i32* p1) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IAtlasAllocator, GetSize, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IAtlasAllocator>(self)->Impl_GetSize(p0, p1);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IAtlasAllocator, GetSize, void)
        #endif
    }

    inline bool COPLT_CDECL Allocate(::Coplt::IAtlasAllocator* self, ::Coplt::i32 p0, ::Coplt::i32 p1, ::Coplt::u32* p2, ::Coplt::AABB2DI* p3) noexcept
    {
        bool r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IAtlasAllocator, Allocate, bool)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IAtlasAllocator>(self)->Impl_Allocate(p0, p1, p2, p3);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IAtlasAllocator, Allocate, bool)
        #endif
        return r;
    }

    inline void COPLT_CDECL Deallocate(::Coplt::IAtlasAllocator* self, ::Coplt::u32 p0) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IAtlasAllocator, Deallocate, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IAtlasAllocator>(self)->Impl_Deallocate(p0);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IAtlasAllocator, Deallocate, void)
        #endif
    }
}
#define COPLT_COM_INTERFACE_BODY_Coplt_IAtlasAllocator\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::IAtlasAllocator;\
\
    explicit IAtlasAllocator(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::IAtlasAllocator>
{
    static COPLT_FORCE_INLINE void Clear(::Coplt::IAtlasAllocator* self) noexcept
    {
        COPLT_COM_PVTB(IAtlasAllocator, self)->f_Clear(self);
    }
    static COPLT_FORCE_INLINE bool get_IsEmpty(::Coplt::IAtlasAllocator* self) noexcept
    {
        return COPLT_COM_PVTB(IAtlasAllocator, self)->f_get_IsEmpty(self);
    }
    static COPLT_FORCE_INLINE void GetSize(::Coplt::IAtlasAllocator* self, ::Coplt::i32* p0, ::Coplt::i32* p1) noexcept
    {
        COPLT_COM_PVTB(IAtlasAllocator, self)->f_GetSize(self, p0, p1);
    }
    static COPLT_FORCE_INLINE bool Allocate(::Coplt::IAtlasAllocator* self, ::Coplt::i32 p0, ::Coplt::i32 p1, ::Coplt::u32* p2, ::Coplt::AABB2DI* p3) noexcept
    {
        return COPLT_COM_PVTB(IAtlasAllocator, self)->f_Allocate(self, p0, p1, p2, p3);
    }
    static COPLT_FORCE_INLINE void Deallocate(::Coplt::IAtlasAllocator* self, ::Coplt::u32 p0) noexcept
    {
        COPLT_COM_PVTB(IAtlasAllocator, self)->f_Deallocate(self, p0);
    }
};

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::IFont>
{
    VirtualTable<::Coplt::IUnknown> b;
    ::Coplt::NFontInfo const* (*const COPLT_CDECL f_get_Info)(const ::Coplt::IFont*) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_CreateFace)(const ::Coplt::IFont*, COPLT_OUT IFontFace** face, IFontManager* manager) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFont
{
    ::Coplt::NFontInfo const* COPLT_CDECL get_Info(const ::Coplt::IFont* self) noexcept;
    ::Coplt::i32 COPLT_CDECL CreateFace(const ::Coplt::IFont* self, COPLT_OUT IFontFace** p0, IFontManager* p1) noexcept;
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
        virtual ::Coplt::HResult Impl_CreateFace(COPLT_OUT IFontFace** face, IFontManager* manager) const = 0;
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

        static ::Coplt::i32 COPLT_CDECL f_CreateFace(const ::Coplt::IFont* self, COPLT_OUT IFontFace** p0, IFontManager* p1) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_CreateFace(p0, p1));
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

    inline ::Coplt::NFontInfo const* COPLT_CDECL get_Info(const ::Coplt::IFont* self) noexcept
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

    inline ::Coplt::i32 COPLT_CDECL CreateFace(const ::Coplt::IFont* self, COPLT_OUT IFontFace** p0, IFontManager* p1) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFont, CreateFace, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::IFont>(self)->Impl_CreateFace(p0, p1));
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
    static COPLT_FORCE_INLINE ::Coplt::HResult CreateFace(const ::Coplt::IFont* self, COPLT_OUT IFontFace** p0, IFontManager* p1) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(IFont, self)->f_CreateFace(self, p0, p1));
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
    IFontFamily* const* COPLT_CDECL GetFamilies(const ::Coplt::IFontCollection* self, COPLT_OUT ::Coplt::u32* p0) noexcept;
    void COPLT_CDECL ClearNativeFamiliesCache(::Coplt::IFontCollection* self) noexcept;
    ::Coplt::u32 COPLT_CDECL FindDefaultFamily(::Coplt::IFontCollection* self) noexcept;
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

    inline IFontFamily* const* COPLT_CDECL GetFamilies(const ::Coplt::IFontCollection* self, COPLT_OUT ::Coplt::u32* p0) noexcept
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

    inline void COPLT_CDECL ClearNativeFamiliesCache(::Coplt::IFontCollection* self) noexcept
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

    inline ::Coplt::u32 COPLT_CDECL FindDefaultFamily(::Coplt::IFontCollection* self) noexcept
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
    ::Coplt::u64 (*const COPLT_CDECL f_get_Id)(const ::Coplt::IFontFace*) noexcept;
    ::Coplt::NFontInfo const* (*const COPLT_CDECL f_get_Info)(const ::Coplt::IFontFace*) noexcept;
    bool (*const COPLT_CDECL f_Equals)(const ::Coplt::IFontFace*, IFontFace* other) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_HashCode)(const ::Coplt::IFontFace*) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_GetFamilyNames)(const ::Coplt::IFontFace*, void* ctx, ::Coplt::Func<void, void*, ::Coplt::char16*, ::Coplt::i32, ::Coplt::char16*, ::Coplt::i32>* add) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_GetFaceNames)(const ::Coplt::IFontFace*, void* ctx, ::Coplt::Func<void, void*, ::Coplt::char16*, ::Coplt::i32, ::Coplt::char16*, ::Coplt::i32>* add) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontFace
{
    ::Coplt::u64 COPLT_CDECL get_Id(const ::Coplt::IFontFace* self) noexcept;
    ::Coplt::NFontInfo const* COPLT_CDECL get_Info(const ::Coplt::IFontFace* self) noexcept;
    bool COPLT_CDECL Equals(const ::Coplt::IFontFace* self, IFontFace* p0) noexcept;
    ::Coplt::i32 COPLT_CDECL HashCode(const ::Coplt::IFontFace* self) noexcept;
    ::Coplt::i32 COPLT_CDECL GetFamilyNames(const ::Coplt::IFontFace* self, void* p0, ::Coplt::Func<void, void*, ::Coplt::char16*, ::Coplt::i32, ::Coplt::char16*, ::Coplt::i32>* p1) noexcept;
    ::Coplt::i32 COPLT_CDECL GetFaceNames(const ::Coplt::IFontFace* self, void* p0, ::Coplt::Func<void, void*, ::Coplt::char16*, ::Coplt::i32, ::Coplt::char16*, ::Coplt::i32>* p1) noexcept;
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
            .f_get_Id = VirtualImpl_Coplt_IFontFace::get_Id,
            .f_get_Info = VirtualImpl_Coplt_IFontFace::get_Info,
            .f_Equals = VirtualImpl_Coplt_IFontFace::Equals,
            .f_HashCode = VirtualImpl_Coplt_IFontFace::HashCode,
            .f_GetFamilyNames = VirtualImpl_Coplt_IFontFace::GetFamilyNames,
            .f_GetFaceNames = VirtualImpl_Coplt_IFontFace::GetFaceNames,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual ::Coplt::u64 Impl_get_Id() const = 0;
        virtual ::Coplt::NFontInfo const* Impl_get_Info() const = 0;
        virtual bool Impl_Equals(IFontFace* other) const = 0;
        virtual ::Coplt::i32 Impl_HashCode() const = 0;
        virtual ::Coplt::HResult Impl_GetFamilyNames(void* ctx, ::Coplt::Func<void, void*, ::Coplt::char16*, ::Coplt::i32, ::Coplt::char16*, ::Coplt::i32>* add) const = 0;
        virtual ::Coplt::HResult Impl_GetFaceNames(void* ctx, ::Coplt::Func<void, void*, ::Coplt::char16*, ::Coplt::i32, ::Coplt::char16*, ::Coplt::i32>* add) const = 0;
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

        static ::Coplt::u64 COPLT_CDECL f_get_Id(const ::Coplt::IFontFace* self) noexcept
        {
            return AsImpl(self)->Impl_get_Id();
        }

        static ::Coplt::NFontInfo const* COPLT_CDECL f_get_Info(const ::Coplt::IFontFace* self) noexcept
        {
            return AsImpl(self)->Impl_get_Info();
        }

        static bool COPLT_CDECL f_Equals(const ::Coplt::IFontFace* self, IFontFace* p0) noexcept
        {
            return AsImpl(self)->Impl_Equals(p0);
        }

        static ::Coplt::i32 COPLT_CDECL f_HashCode(const ::Coplt::IFontFace* self) noexcept
        {
            return AsImpl(self)->Impl_HashCode();
        }

        static ::Coplt::i32 COPLT_CDECL f_GetFamilyNames(const ::Coplt::IFontFace* self, void* p0, ::Coplt::Func<void, void*, ::Coplt::char16*, ::Coplt::i32, ::Coplt::char16*, ::Coplt::i32>* p1) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_GetFamilyNames(p0, p1));
        }

        static ::Coplt::i32 COPLT_CDECL f_GetFaceNames(const ::Coplt::IFontFace* self, void* p0, ::Coplt::Func<void, void*, ::Coplt::char16*, ::Coplt::i32, ::Coplt::char16*, ::Coplt::i32>* p1) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_GetFaceNames(p0, p1));
        }
    };

    template<class Impl>
    constexpr static VirtualTable s_vtb
    {
        .b = ComProxy<::Coplt::IUnknown>::s_vtb<Impl>,
        .f_get_Id = VirtualImpl<Impl>::f_get_Id,
        .f_get_Info = VirtualImpl<Impl>::f_get_Info,
        .f_Equals = VirtualImpl<Impl>::f_Equals,
        .f_HashCode = VirtualImpl<Impl>::f_HashCode,
        .f_GetFamilyNames = VirtualImpl<Impl>::f_GetFamilyNames,
        .f_GetFaceNames = VirtualImpl<Impl>::f_GetFaceNames,
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontFace
{

    inline ::Coplt::u64 COPLT_CDECL get_Id(const ::Coplt::IFontFace* self) noexcept
    {
        ::Coplt::u64 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontFace, get_Id, ::Coplt::u64)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IFontFace>(self)->Impl_get_Id();
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontFace, get_Id, ::Coplt::u64)
        #endif
        return r;
    }

    inline ::Coplt::NFontInfo const* COPLT_CDECL get_Info(const ::Coplt::IFontFace* self) noexcept
    {
        ::Coplt::NFontInfo const* r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontFace, get_Info, ::Coplt::NFontInfo const*)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IFontFace>(self)->Impl_get_Info();
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontFace, get_Info, ::Coplt::NFontInfo const*)
        #endif
        return r;
    }

    inline bool COPLT_CDECL Equals(const ::Coplt::IFontFace* self, IFontFace* p0) noexcept
    {
        bool r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontFace, Equals, bool)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IFontFace>(self)->Impl_Equals(p0);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontFace, Equals, bool)
        #endif
        return r;
    }

    inline ::Coplt::i32 COPLT_CDECL HashCode(const ::Coplt::IFontFace* self) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontFace, HashCode, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IFontFace>(self)->Impl_HashCode();
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontFace, HashCode, ::Coplt::i32)
        #endif
        return r;
    }

    inline ::Coplt::i32 COPLT_CDECL GetFamilyNames(const ::Coplt::IFontFace* self, void* p0, ::Coplt::Func<void, void*, ::Coplt::char16*, ::Coplt::i32, ::Coplt::char16*, ::Coplt::i32>* p1) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontFace, GetFamilyNames, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::IFontFace>(self)->Impl_GetFamilyNames(p0, p1));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontFace, GetFamilyNames, ::Coplt::i32)
        #endif
        return r;
    }

    inline ::Coplt::i32 COPLT_CDECL GetFaceNames(const ::Coplt::IFontFace* self, void* p0, ::Coplt::Func<void, void*, ::Coplt::char16*, ::Coplt::i32, ::Coplt::char16*, ::Coplt::i32>* p1) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontFace, GetFaceNames, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::IFontFace>(self)->Impl_GetFaceNames(p0, p1));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontFace, GetFaceNames, ::Coplt::i32)
        #endif
        return r;
    }
}
#define COPLT_COM_INTERFACE_BODY_Coplt_IFontFace\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::IFontFace;\
\
    explicit IFontFace(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::IFontFace>
{
    static COPLT_FORCE_INLINE ::Coplt::u64 get_Id(const ::Coplt::IFontFace* self) noexcept
    {
        return COPLT_COM_PVTB(IFontFace, self)->f_get_Id(self);
    }
    static COPLT_FORCE_INLINE ::Coplt::NFontInfo const* get_Info(const ::Coplt::IFontFace* self) noexcept
    {
        return COPLT_COM_PVTB(IFontFace, self)->f_get_Info(self);
    }
    static COPLT_FORCE_INLINE bool Equals(const ::Coplt::IFontFace* self, IFontFace* p0) noexcept
    {
        return COPLT_COM_PVTB(IFontFace, self)->f_Equals(self, p0);
    }
    static COPLT_FORCE_INLINE ::Coplt::i32 HashCode(const ::Coplt::IFontFace* self) noexcept
    {
        return COPLT_COM_PVTB(IFontFace, self)->f_HashCode(self);
    }
    static COPLT_FORCE_INLINE ::Coplt::HResult GetFamilyNames(const ::Coplt::IFontFace* self, void* p0, ::Coplt::Func<void, void*, ::Coplt::char16*, ::Coplt::i32, ::Coplt::char16*, ::Coplt::i32>* p1) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(IFontFace, self)->f_GetFamilyNames(self, p0, p1));
    }
    static COPLT_FORCE_INLINE ::Coplt::HResult GetFaceNames(const ::Coplt::IFontFace* self, void* p0, ::Coplt::Func<void, void*, ::Coplt::char16*, ::Coplt::i32, ::Coplt::char16*, ::Coplt::i32>* p1) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(IFontFace, self)->f_GetFaceNames(self, p0, p1));
    }
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
struct ::Coplt::Internal::VirtualTable<::Coplt::IFontFallbackBuilder>
{
    VirtualTable<::Coplt::IUnknown> b;
    ::Coplt::i32 (*const COPLT_CDECL f_Build)(::Coplt::IFontFallbackBuilder*, IFontFallback** ff) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_Add)(::Coplt::IFontFallbackBuilder*, ::Coplt::char16 const* name, ::Coplt::i32 length, bool* exists) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_AddLocaled)(::Coplt::IFontFallbackBuilder*, ::Coplt::char16 const* locale, ::Coplt::char16 const* name, ::Coplt::i32 name_length, bool* exists) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontFallbackBuilder
{
    ::Coplt::i32 COPLT_CDECL Build(::Coplt::IFontFallbackBuilder* self, IFontFallback** p0) noexcept;
    ::Coplt::i32 COPLT_CDECL Add(::Coplt::IFontFallbackBuilder* self, ::Coplt::char16 const* p0, ::Coplt::i32 p1, bool* p2) noexcept;
    ::Coplt::i32 COPLT_CDECL AddLocaled(::Coplt::IFontFallbackBuilder* self, ::Coplt::char16 const* p0, ::Coplt::char16 const* p1, ::Coplt::i32 p2, bool* p3) noexcept;
}

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::IFontFallbackBuilder>
{
    using VirtualTable = VirtualTable<::Coplt::IFontFallbackBuilder>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("9b4e9893-0ea4-456b-bf54-9563db70eff0");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::IFontFallbackBuilder>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::IFontFallbackBuilder*>(self)));
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
            .f_Build = VirtualImpl_Coplt_IFontFallbackBuilder::Build,
            .f_Add = VirtualImpl_Coplt_IFontFallbackBuilder::Add,
            .f_AddLocaled = VirtualImpl_Coplt_IFontFallbackBuilder::AddLocaled,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual ::Coplt::HResult Impl_Build(IFontFallback** ff) = 0;
        virtual ::Coplt::HResult Impl_Add(::Coplt::char16 const* name, ::Coplt::i32 length, bool* exists) = 0;
        virtual ::Coplt::HResult Impl_AddLocaled(::Coplt::char16 const* locale, ::Coplt::char16 const* name, ::Coplt::i32 name_length, bool* exists) = 0;
    };

    template <std::derived_from<::Coplt::IFontFallbackBuilder> Base = ::Coplt::IFontFallbackBuilder>
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

        static ::Coplt::i32 COPLT_CDECL f_Build(::Coplt::IFontFallbackBuilder* self, IFontFallback** p0) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_Build(p0));
        }

        static ::Coplt::i32 COPLT_CDECL f_Add(::Coplt::IFontFallbackBuilder* self, ::Coplt::char16 const* p0, ::Coplt::i32 p1, bool* p2) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_Add(p0, p1, p2));
        }

        static ::Coplt::i32 COPLT_CDECL f_AddLocaled(::Coplt::IFontFallbackBuilder* self, ::Coplt::char16 const* p0, ::Coplt::char16 const* p1, ::Coplt::i32 p2, bool* p3) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_AddLocaled(p0, p1, p2, p3));
        }
    };

    template<class Impl>
    constexpr static VirtualTable s_vtb
    {
        .b = ComProxy<::Coplt::IUnknown>::s_vtb<Impl>,
        .f_Build = VirtualImpl<Impl>::f_Build,
        .f_Add = VirtualImpl<Impl>::f_Add,
        .f_AddLocaled = VirtualImpl<Impl>::f_AddLocaled,
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontFallbackBuilder
{

    inline ::Coplt::i32 COPLT_CDECL Build(::Coplt::IFontFallbackBuilder* self, IFontFallback** p0) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontFallbackBuilder, Build, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::IFontFallbackBuilder>(self)->Impl_Build(p0));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontFallbackBuilder, Build, ::Coplt::i32)
        #endif
        return r;
    }

    inline ::Coplt::i32 COPLT_CDECL Add(::Coplt::IFontFallbackBuilder* self, ::Coplt::char16 const* p0, ::Coplt::i32 p1, bool* p2) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontFallbackBuilder, Add, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::IFontFallbackBuilder>(self)->Impl_Add(p0, p1, p2));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontFallbackBuilder, Add, ::Coplt::i32)
        #endif
        return r;
    }

    inline ::Coplt::i32 COPLT_CDECL AddLocaled(::Coplt::IFontFallbackBuilder* self, ::Coplt::char16 const* p0, ::Coplt::char16 const* p1, ::Coplt::i32 p2, bool* p3) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontFallbackBuilder, AddLocaled, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::IFontFallbackBuilder>(self)->Impl_AddLocaled(p0, p1, p2, p3));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontFallbackBuilder, AddLocaled, ::Coplt::i32)
        #endif
        return r;
    }
}
#define COPLT_COM_INTERFACE_BODY_Coplt_IFontFallbackBuilder\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::IFontFallbackBuilder;\
\
    explicit IFontFallbackBuilder(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::IFontFallbackBuilder>
{
    static COPLT_FORCE_INLINE ::Coplt::HResult Build(::Coplt::IFontFallbackBuilder* self, IFontFallback** p0) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(IFontFallbackBuilder, self)->f_Build(self, p0));
    }
    static COPLT_FORCE_INLINE ::Coplt::HResult Add(::Coplt::IFontFallbackBuilder* self, ::Coplt::char16 const* p0, ::Coplt::i32 p1, bool* p2) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(IFontFallbackBuilder, self)->f_Add(self, p0, p1, p2));
    }
    static COPLT_FORCE_INLINE ::Coplt::HResult AddLocaled(::Coplt::IFontFallbackBuilder* self, ::Coplt::char16 const* p0, ::Coplt::char16 const* p1, ::Coplt::i32 p2, bool* p3) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(IFontFallbackBuilder, self)->f_AddLocaled(self, p0, p1, p2, p3));
    }
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
    ::Coplt::Str16 const* COPLT_CDECL GetLocalNames(const ::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept;
    ::Coplt::FontFamilyNameInfo const* COPLT_CDECL GetNames(const ::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept;
    void COPLT_CDECL ClearNativeNamesCache(::Coplt::IFontFamily* self) noexcept;
    ::Coplt::i32 COPLT_CDECL GetFonts(::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0, COPLT_OUT ::Coplt::NFontPair const** p1) noexcept;
    void COPLT_CDECL ClearNativeFontsCache(::Coplt::IFontFamily* self) noexcept;
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

    inline ::Coplt::Str16 const* COPLT_CDECL GetLocalNames(const ::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept
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

    inline ::Coplt::FontFamilyNameInfo const* COPLT_CDECL GetNames(const ::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0) noexcept
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

    inline void COPLT_CDECL ClearNativeNamesCache(::Coplt::IFontFamily* self) noexcept
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

    inline ::Coplt::i32 COPLT_CDECL GetFonts(::Coplt::IFontFamily* self, COPLT_OUT ::Coplt::u32* p0, COPLT_OUT ::Coplt::NFontPair const** p1) noexcept
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

    inline void COPLT_CDECL ClearNativeFontsCache(::Coplt::IFontFamily* self) noexcept
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
    VirtualTable<::Coplt::IWeak> b;
    ::Coplt::u64 (*const COPLT_CDECL f_SetAssocUpdate)(::Coplt::IFontManager*, void* Data, ::Coplt::Func<void, void*>* OnDrop, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* OnAdd, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* OnExpired) noexcept;
    void (*const COPLT_CDECL f_RemoveAssocUpdate)(::Coplt::IFontManager*, ::Coplt::u64 AssocUpdateId) noexcept;
    void (*const COPLT_CDECL f_SetExpireFrame)(::Coplt::IFontManager*, ::Coplt::u64 FrameCount) noexcept;
    void (*const COPLT_CDECL f_SetExpireTime)(::Coplt::IFontManager*, ::Coplt::u64 TimeTicks) noexcept;
    ::Coplt::u64 (*const COPLT_CDECL f_GetCurrentFrame)(const ::Coplt::IFontManager*) noexcept;
    void (*const COPLT_CDECL f_Update)(::Coplt::IFontManager*, ::Coplt::u64 CurrentTime) noexcept;
    ::Coplt::u64 (*const COPLT_CDECL f_FontFaceToId)(::Coplt::IFontManager*, IFontFace* Face) noexcept;
    IFontFace* (*const COPLT_CDECL f_IdToFontFace)(::Coplt::IFontManager*, ::Coplt::u64 Id) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IFontManager
{
    ::Coplt::u64 COPLT_CDECL SetAssocUpdate(::Coplt::IFontManager* self, void* p0, ::Coplt::Func<void, void*>* p1, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* p2, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* p3) noexcept;
    void COPLT_CDECL RemoveAssocUpdate(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept;
    void COPLT_CDECL SetExpireFrame(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept;
    void COPLT_CDECL SetExpireTime(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept;
    ::Coplt::u64 COPLT_CDECL GetCurrentFrame(const ::Coplt::IFontManager* self) noexcept;
    void COPLT_CDECL Update(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept;
    ::Coplt::u64 COPLT_CDECL FontFaceToId(::Coplt::IFontManager* self, IFontFace* p0) noexcept;
    IFontFace* COPLT_CDECL IdToFontFace(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept;
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
        return ComProxy<::Coplt::IWeak>::QueryInterface(self, guid, object);
    }

    COPLT_FORCE_INLINE
    static const VirtualTable& GetVtb()
    {
        static VirtualTable vtb
        {
            .b = ComProxy<::Coplt::IWeak>::GetVtb(),
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

    struct Impl : ComProxy<::Coplt::IWeak>::Impl
    {

        virtual ::Coplt::u64 Impl_SetAssocUpdate(void* Data, ::Coplt::Func<void, void*>* OnDrop, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* OnAdd, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* OnExpired) = 0;
        virtual void Impl_RemoveAssocUpdate(::Coplt::u64 AssocUpdateId) = 0;
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

        static ::Coplt::u64 COPLT_CDECL f_SetAssocUpdate(::Coplt::IFontManager* self, void* p0, ::Coplt::Func<void, void*>* p1, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* p2, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* p3) noexcept
        {
            return AsImpl(self)->Impl_SetAssocUpdate(p0, p1, p2, p3);
        }

        static void COPLT_CDECL f_RemoveAssocUpdate(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
        {
            AsImpl(self)->Impl_RemoveAssocUpdate(p0);
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
        .b = ComProxy<::Coplt::IWeak>::s_vtb<Impl>,
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

    inline ::Coplt::u64 COPLT_CDECL SetAssocUpdate(::Coplt::IFontManager* self, void* p0, ::Coplt::Func<void, void*>* p1, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* p2, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* p3) noexcept
    {
        ::Coplt::u64 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontManager, SetAssocUpdate, ::Coplt::u64)
        #endif
        r = ::Coplt::Internal::AsImpl<::Coplt::IFontManager>(self)->Impl_SetAssocUpdate(p0, p1, p2, p3);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontManager, SetAssocUpdate, ::Coplt::u64)
        #endif
        return r;
    }

    inline void COPLT_CDECL RemoveAssocUpdate(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IFontManager, RemoveAssocUpdate, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IFontManager>(self)->Impl_RemoveAssocUpdate(p0);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IFontManager, RemoveAssocUpdate, void)
        #endif
    }

    inline void COPLT_CDECL SetExpireFrame(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
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

    inline void COPLT_CDECL SetExpireTime(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
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

    inline ::Coplt::u64 COPLT_CDECL GetCurrentFrame(const ::Coplt::IFontManager* self) noexcept
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

    inline void COPLT_CDECL Update(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
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

    inline ::Coplt::u64 COPLT_CDECL FontFaceToId(::Coplt::IFontManager* self, IFontFace* p0) noexcept
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

    inline IFontFace* COPLT_CDECL IdToFontFace(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
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
    using Super = ::Coplt::IWeak;\
    using Self = ::Coplt::IFontManager;\
\
    explicit IFontManager(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::IFontManager>
{
    static COPLT_FORCE_INLINE ::Coplt::u64 SetAssocUpdate(::Coplt::IFontManager* self, void* p0, ::Coplt::Func<void, void*>* p1, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* p2, ::Coplt::Func<void, void*, IFontFace*, ::Coplt::u64>* p3) noexcept
    {
        return COPLT_COM_PVTB(IFontManager, self)->f_SetAssocUpdate(self, p0, p1, p2, p3);
    }
    static COPLT_FORCE_INLINE void RemoveAssocUpdate(::Coplt::IFontManager* self, ::Coplt::u64 p0) noexcept
    {
        COPLT_COM_PVTB(IFontManager, self)->f_RemoveAssocUpdate(self, p0);
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
    ::Coplt::i32 COPLT_CDECL Calc(::Coplt::ILayout* self, ::Coplt::NLayoutContext* p0) noexcept;
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

    inline ::Coplt::i32 COPLT_CDECL Calc(::Coplt::ILayout* self, ::Coplt::NLayoutContext* p0) noexcept
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
    void (*const COPLT_CDECL f_SetLogger)(::Coplt::ILib*, void* obj, ::Coplt::Func<void, void*, ::Coplt::LogLevel, ::Coplt::StrKind, ::Coplt::i32, void*>* logger, ::Coplt::Func<::Coplt::u8, void*, ::Coplt::LogLevel>* is_enabled, ::Coplt::Func<void, void*>* drop) noexcept;
    void (*const COPLT_CDECL f_ClearLogger)(::Coplt::ILib*) noexcept;
    ::Coplt::Str8* (*const COPLT_CDECL f_GetCurrentErrorMessage)(::Coplt::ILib*, ::Coplt::Str8*) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_CreateAtlasAllocator)(::Coplt::ILib*, ::Coplt::AtlasAllocatorType Type, ::Coplt::i32 Width, ::Coplt::i32 Height, IAtlasAllocator** aa) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_CreateFontManager)(::Coplt::ILib*, IFontManager** fm) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_GetSystemFontCollection)(::Coplt::ILib*, IFontCollection** fc) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_GetSystemFontFallback)(::Coplt::ILib*, IFontFallback** ff) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_CreateFontFallbackBuilder)(::Coplt::ILib*, IFontFallbackBuilder** ffb, ::Coplt::FontFallbackBuilderCreateInfo const* info) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_CreateLayout)(::Coplt::ILib*, ILayout** layout) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_SplitTexts)(::Coplt::ILib*, ::Coplt::NativeList<::Coplt::TextRange>* ranges, ::Coplt::char16 const* chars, ::Coplt::i32 len) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_ILib
{
    void COPLT_CDECL SetLogger(::Coplt::ILib* self, void* p0, ::Coplt::Func<void, void*, ::Coplt::LogLevel, ::Coplt::StrKind, ::Coplt::i32, void*>* p1, ::Coplt::Func<::Coplt::u8, void*, ::Coplt::LogLevel>* p2, ::Coplt::Func<void, void*>* p3) noexcept;
    void COPLT_CDECL ClearLogger(::Coplt::ILib* self) noexcept;
    ::Coplt::Str8* COPLT_CDECL GetCurrentErrorMessage(::Coplt::ILib* self, ::Coplt::Str8* r) noexcept;
    ::Coplt::i32 COPLT_CDECL CreateAtlasAllocator(::Coplt::ILib* self, ::Coplt::AtlasAllocatorType p0, ::Coplt::i32 p1, ::Coplt::i32 p2, IAtlasAllocator** p3) noexcept;
    ::Coplt::i32 COPLT_CDECL CreateFontManager(::Coplt::ILib* self, IFontManager** p0) noexcept;
    ::Coplt::i32 COPLT_CDECL GetSystemFontCollection(::Coplt::ILib* self, IFontCollection** p0) noexcept;
    ::Coplt::i32 COPLT_CDECL GetSystemFontFallback(::Coplt::ILib* self, IFontFallback** p0) noexcept;
    ::Coplt::i32 COPLT_CDECL CreateFontFallbackBuilder(::Coplt::ILib* self, IFontFallbackBuilder** p0, ::Coplt::FontFallbackBuilderCreateInfo const* p1) noexcept;
    ::Coplt::i32 COPLT_CDECL CreateLayout(::Coplt::ILib* self, ILayout** p0) noexcept;
    ::Coplt::i32 COPLT_CDECL SplitTexts(::Coplt::ILib* self, ::Coplt::NativeList<::Coplt::TextRange>* p0, ::Coplt::char16 const* p1, ::Coplt::i32 p2) noexcept;
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
            .f_ClearLogger = VirtualImpl_Coplt_ILib::ClearLogger,
            .f_GetCurrentErrorMessage = VirtualImpl_Coplt_ILib::GetCurrentErrorMessage,
            .f_CreateAtlasAllocator = VirtualImpl_Coplt_ILib::CreateAtlasAllocator,
            .f_CreateFontManager = VirtualImpl_Coplt_ILib::CreateFontManager,
            .f_GetSystemFontCollection = VirtualImpl_Coplt_ILib::GetSystemFontCollection,
            .f_GetSystemFontFallback = VirtualImpl_Coplt_ILib::GetSystemFontFallback,
            .f_CreateFontFallbackBuilder = VirtualImpl_Coplt_ILib::CreateFontFallbackBuilder,
            .f_CreateLayout = VirtualImpl_Coplt_ILib::CreateLayout,
            .f_SplitTexts = VirtualImpl_Coplt_ILib::SplitTexts,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual void Impl_SetLogger(void* obj, ::Coplt::Func<void, void*, ::Coplt::LogLevel, ::Coplt::StrKind, ::Coplt::i32, void*>* logger, ::Coplt::Func<::Coplt::u8, void*, ::Coplt::LogLevel>* is_enabled, ::Coplt::Func<void, void*>* drop) = 0;
        virtual void Impl_ClearLogger() = 0;
        virtual ::Coplt::Str8 Impl_GetCurrentErrorMessage() = 0;
        virtual ::Coplt::HResult Impl_CreateAtlasAllocator(::Coplt::AtlasAllocatorType Type, ::Coplt::i32 Width, ::Coplt::i32 Height, IAtlasAllocator** aa) = 0;
        virtual ::Coplt::HResult Impl_CreateFontManager(IFontManager** fm) = 0;
        virtual ::Coplt::HResult Impl_GetSystemFontCollection(IFontCollection** fc) = 0;
        virtual ::Coplt::HResult Impl_GetSystemFontFallback(IFontFallback** ff) = 0;
        virtual ::Coplt::HResult Impl_CreateFontFallbackBuilder(IFontFallbackBuilder** ffb, ::Coplt::FontFallbackBuilderCreateInfo const* info) = 0;
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

        static void COPLT_CDECL f_SetLogger(::Coplt::ILib* self, void* p0, ::Coplt::Func<void, void*, ::Coplt::LogLevel, ::Coplt::StrKind, ::Coplt::i32, void*>* p1, ::Coplt::Func<::Coplt::u8, void*, ::Coplt::LogLevel>* p2, ::Coplt::Func<void, void*>* p3) noexcept
        {
            AsImpl(self)->Impl_SetLogger(p0, p1, p2, p3);
        }

        static void COPLT_CDECL f_ClearLogger(::Coplt::ILib* self) noexcept
        {
            AsImpl(self)->Impl_ClearLogger();
        }

        static ::Coplt::Str8* COPLT_CDECL f_GetCurrentErrorMessage(::Coplt::ILib* self, ::Coplt::Str8* r) noexcept
        {
            *r = AsImpl(self)->Impl_GetCurrentErrorMessage();
            return r;
        }

        static ::Coplt::i32 COPLT_CDECL f_CreateAtlasAllocator(::Coplt::ILib* self, ::Coplt::AtlasAllocatorType p0, ::Coplt::i32 p1, ::Coplt::i32 p2, IAtlasAllocator** p3) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_CreateAtlasAllocator(p0, p1, p2, p3));
        }

        static ::Coplt::i32 COPLT_CDECL f_CreateFontManager(::Coplt::ILib* self, IFontManager** p0) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_CreateFontManager(p0));
        }

        static ::Coplt::i32 COPLT_CDECL f_GetSystemFontCollection(::Coplt::ILib* self, IFontCollection** p0) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_GetSystemFontCollection(p0));
        }

        static ::Coplt::i32 COPLT_CDECL f_GetSystemFontFallback(::Coplt::ILib* self, IFontFallback** p0) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_GetSystemFontFallback(p0));
        }

        static ::Coplt::i32 COPLT_CDECL f_CreateFontFallbackBuilder(::Coplt::ILib* self, IFontFallbackBuilder** p0, ::Coplt::FontFallbackBuilderCreateInfo const* p1) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_CreateFontFallbackBuilder(p0, p1));
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
        .f_ClearLogger = VirtualImpl<Impl>::f_ClearLogger,
        .f_GetCurrentErrorMessage = VirtualImpl<Impl>::f_GetCurrentErrorMessage,
        .f_CreateAtlasAllocator = VirtualImpl<Impl>::f_CreateAtlasAllocator,
        .f_CreateFontManager = VirtualImpl<Impl>::f_CreateFontManager,
        .f_GetSystemFontCollection = VirtualImpl<Impl>::f_GetSystemFontCollection,
        .f_GetSystemFontFallback = VirtualImpl<Impl>::f_GetSystemFontFallback,
        .f_CreateFontFallbackBuilder = VirtualImpl<Impl>::f_CreateFontFallbackBuilder,
        .f_CreateLayout = VirtualImpl<Impl>::f_CreateLayout,
        .f_SplitTexts = VirtualImpl<Impl>::f_SplitTexts,
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_ILib
{

    inline void COPLT_CDECL SetLogger(::Coplt::ILib* self, void* p0, ::Coplt::Func<void, void*, ::Coplt::LogLevel, ::Coplt::StrKind, ::Coplt::i32, void*>* p1, ::Coplt::Func<::Coplt::u8, void*, ::Coplt::LogLevel>* p2, ::Coplt::Func<void, void*>* p3) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILib, SetLogger, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::ILib>(self)->Impl_SetLogger(p0, p1, p2, p3);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILib, SetLogger, void)
        #endif
    }

    inline void COPLT_CDECL ClearLogger(::Coplt::ILib* self) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILib, ClearLogger, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::ILib>(self)->Impl_ClearLogger();
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILib, ClearLogger, void)
        #endif
    }

    inline ::Coplt::Str8* COPLT_CDECL GetCurrentErrorMessage(::Coplt::ILib* self, ::Coplt::Str8* r) noexcept
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

    inline ::Coplt::i32 COPLT_CDECL CreateAtlasAllocator(::Coplt::ILib* self, ::Coplt::AtlasAllocatorType p0, ::Coplt::i32 p1, ::Coplt::i32 p2, IAtlasAllocator** p3) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILib, CreateAtlasAllocator, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::ILib>(self)->Impl_CreateAtlasAllocator(p0, p1, p2, p3));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILib, CreateAtlasAllocator, ::Coplt::i32)
        #endif
        return r;
    }

    inline ::Coplt::i32 COPLT_CDECL CreateFontManager(::Coplt::ILib* self, IFontManager** p0) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILib, CreateFontManager, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::ILib>(self)->Impl_CreateFontManager(p0));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILib, CreateFontManager, ::Coplt::i32)
        #endif
        return r;
    }

    inline ::Coplt::i32 COPLT_CDECL GetSystemFontCollection(::Coplt::ILib* self, IFontCollection** p0) noexcept
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

    inline ::Coplt::i32 COPLT_CDECL GetSystemFontFallback(::Coplt::ILib* self, IFontFallback** p0) noexcept
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

    inline ::Coplt::i32 COPLT_CDECL CreateFontFallbackBuilder(::Coplt::ILib* self, IFontFallbackBuilder** p0, ::Coplt::FontFallbackBuilderCreateInfo const* p1) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILib, CreateFontFallbackBuilder, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::ILib>(self)->Impl_CreateFontFallbackBuilder(p0, p1));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILib, CreateFontFallbackBuilder, ::Coplt::i32)
        #endif
        return r;
    }

    inline ::Coplt::i32 COPLT_CDECL CreateLayout(::Coplt::ILib* self, ILayout** p0) noexcept
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

    inline ::Coplt::i32 COPLT_CDECL SplitTexts(::Coplt::ILib* self, ::Coplt::NativeList<::Coplt::TextRange>* p0, ::Coplt::char16 const* p1, ::Coplt::i32 p2) noexcept
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
    static COPLT_FORCE_INLINE void SetLogger(::Coplt::ILib* self, void* p0, ::Coplt::Func<void, void*, ::Coplt::LogLevel, ::Coplt::StrKind, ::Coplt::i32, void*>* p1, ::Coplt::Func<::Coplt::u8, void*, ::Coplt::LogLevel>* p2, ::Coplt::Func<void, void*>* p3) noexcept
    {
        COPLT_COM_PVTB(ILib, self)->f_SetLogger(self, p0, p1, p2, p3);
    }
    static COPLT_FORCE_INLINE void ClearLogger(::Coplt::ILib* self) noexcept
    {
        COPLT_COM_PVTB(ILib, self)->f_ClearLogger(self);
    }
    static COPLT_FORCE_INLINE ::Coplt::Str8 GetCurrentErrorMessage(::Coplt::ILib* self) noexcept
    {
        ::Coplt::Str8 r{};
        return *COPLT_COM_PVTB(ILib, self)->f_GetCurrentErrorMessage(self, &r);
    }
    static COPLT_FORCE_INLINE ::Coplt::HResult CreateAtlasAllocator(::Coplt::ILib* self, ::Coplt::AtlasAllocatorType p0, ::Coplt::i32 p1, ::Coplt::i32 p2, IAtlasAllocator** p3) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(ILib, self)->f_CreateAtlasAllocator(self, p0, p1, p2, p3));
    }
    static COPLT_FORCE_INLINE ::Coplt::HResult CreateFontManager(::Coplt::ILib* self, IFontManager** p0) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(ILib, self)->f_CreateFontManager(self, p0));
    }
    static COPLT_FORCE_INLINE ::Coplt::HResult GetSystemFontCollection(::Coplt::ILib* self, IFontCollection** p0) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(ILib, self)->f_GetSystemFontCollection(self, p0));
    }
    static COPLT_FORCE_INLINE ::Coplt::HResult GetSystemFontFallback(::Coplt::ILib* self, IFontFallback** p0) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(ILib, self)->f_GetSystemFontFallback(self, p0));
    }
    static COPLT_FORCE_INLINE ::Coplt::HResult CreateFontFallbackBuilder(::Coplt::ILib* self, IFontFallbackBuilder** p0, ::Coplt::FontFallbackBuilderCreateInfo const* p1) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(ILib, self)->f_CreateFontFallbackBuilder(self, p0, p1));
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
struct ::Coplt::Internal::VirtualTable<::Coplt::IPath>
{
    VirtualTable<::Coplt::IUnknown> b;
    void (*const COPLT_CDECL f_CalcAABB)(::Coplt::IPath*, ::Coplt::AABB2DF* out_aabb) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IPath
{
    void COPLT_CDECL CalcAABB(::Coplt::IPath* self, ::Coplt::AABB2DF* p0) noexcept;
}

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::IPath>
{
    using VirtualTable = VirtualTable<::Coplt::IPath>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("dac7a459-b942-4a96-b7d6-ee5c74eca806");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::IPath>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::IPath*>(self)));
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
            .f_CalcAABB = VirtualImpl_Coplt_IPath::CalcAABB,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual void Impl_CalcAABB(::Coplt::AABB2DF* out_aabb) = 0;
    };

    template <std::derived_from<::Coplt::IPath> Base = ::Coplt::IPath>
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

        static void COPLT_CDECL f_CalcAABB(::Coplt::IPath* self, ::Coplt::AABB2DF* p0) noexcept
        {
            AsImpl(self)->Impl_CalcAABB(p0);
        }
    };

    template<class Impl>
    constexpr static VirtualTable s_vtb
    {
        .b = ComProxy<::Coplt::IUnknown>::s_vtb<Impl>,
        .f_CalcAABB = VirtualImpl<Impl>::f_CalcAABB,
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_IPath
{

    inline void COPLT_CDECL CalcAABB(::Coplt::IPath* self, ::Coplt::AABB2DF* p0) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IPath, CalcAABB, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IPath>(self)->Impl_CalcAABB(p0);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IPath, CalcAABB, void)
        #endif
    }
}
#define COPLT_COM_INTERFACE_BODY_Coplt_IPath\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::IPath;\
\
    explicit IPath(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::IPath>
{
    static COPLT_FORCE_INLINE void CalcAABB(::Coplt::IPath* self, ::Coplt::AABB2DF* p0) noexcept
    {
        COPLT_COM_PVTB(IPath, self)->f_CalcAABB(self, p0);
    }
};

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::IPathBuilder>
{
    VirtualTable<::Coplt::IUnknown> b;
    ::Coplt::i32 (*const COPLT_CDECL f_Build)(::Coplt::IPathBuilder*, IPath** path) noexcept;
    void (*const COPLT_CDECL f_Reserve)(::Coplt::IPathBuilder*, ::Coplt::i32 Endpoints, ::Coplt::i32 CtrlPoints) noexcept;
    void (*const COPLT_CDECL f_Batch)(::Coplt::IPathBuilder*, ::Coplt::PathBuilderCmd const* cmds, ::Coplt::i32 num_cmds) noexcept;
    void (*const COPLT_CDECL f_Close)(::Coplt::IPathBuilder*) noexcept;
    void (*const COPLT_CDECL f_MoveTo)(::Coplt::IPathBuilder*, ::Coplt::f32 x, ::Coplt::f32 y) noexcept;
    void (*const COPLT_CDECL f_LineTo)(::Coplt::IPathBuilder*, ::Coplt::f32 x, ::Coplt::f32 y) noexcept;
    void (*const COPLT_CDECL f_QuadraticBezierTo)(::Coplt::IPathBuilder*, ::Coplt::f32 ctrl_x, ::Coplt::f32 ctrl_y, ::Coplt::f32 to_x, ::Coplt::f32 to_y) noexcept;
    void (*const COPLT_CDECL f_CubicBezierTo)(::Coplt::IPathBuilder*, ::Coplt::f32 ctrl0_x, ::Coplt::f32 ctrl0_y, ::Coplt::f32 ctrl1_x, ::Coplt::f32 ctrl1_y, ::Coplt::f32 to_x, ::Coplt::f32 to_y) noexcept;
    void (*const COPLT_CDECL f_Arc)(::Coplt::IPathBuilder*, ::Coplt::f32 center_x, ::Coplt::f32 center_y, ::Coplt::f32 radii_x, ::Coplt::f32 radii_y, ::Coplt::f32 sweep_angle, ::Coplt::f32 x_rotation) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_IPathBuilder
{
    ::Coplt::i32 COPLT_CDECL Build(::Coplt::IPathBuilder* self, IPath** p0) noexcept;
    void COPLT_CDECL Reserve(::Coplt::IPathBuilder* self, ::Coplt::i32 p0, ::Coplt::i32 p1) noexcept;
    void COPLT_CDECL Batch(::Coplt::IPathBuilder* self, ::Coplt::PathBuilderCmd const* p0, ::Coplt::i32 p1) noexcept;
    void COPLT_CDECL Close(::Coplt::IPathBuilder* self) noexcept;
    void COPLT_CDECL MoveTo(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1) noexcept;
    void COPLT_CDECL LineTo(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1) noexcept;
    void COPLT_CDECL QuadraticBezierTo(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1, ::Coplt::f32 p2, ::Coplt::f32 p3) noexcept;
    void COPLT_CDECL CubicBezierTo(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1, ::Coplt::f32 p2, ::Coplt::f32 p3, ::Coplt::f32 p4, ::Coplt::f32 p5) noexcept;
    void COPLT_CDECL Arc(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1, ::Coplt::f32 p2, ::Coplt::f32 p3, ::Coplt::f32 p4, ::Coplt::f32 p5) noexcept;
}

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::IPathBuilder>
{
    using VirtualTable = VirtualTable<::Coplt::IPathBuilder>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("ee1c5b1d-b22d-446a-9eef-128cec82e6c0");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::IPathBuilder>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::IPathBuilder*>(self)));
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
            .f_Build = VirtualImpl_Coplt_IPathBuilder::Build,
            .f_Reserve = VirtualImpl_Coplt_IPathBuilder::Reserve,
            .f_Batch = VirtualImpl_Coplt_IPathBuilder::Batch,
            .f_Close = VirtualImpl_Coplt_IPathBuilder::Close,
            .f_MoveTo = VirtualImpl_Coplt_IPathBuilder::MoveTo,
            .f_LineTo = VirtualImpl_Coplt_IPathBuilder::LineTo,
            .f_QuadraticBezierTo = VirtualImpl_Coplt_IPathBuilder::QuadraticBezierTo,
            .f_CubicBezierTo = VirtualImpl_Coplt_IPathBuilder::CubicBezierTo,
            .f_Arc = VirtualImpl_Coplt_IPathBuilder::Arc,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual ::Coplt::HResult Impl_Build(IPath** path) = 0;
        virtual void Impl_Reserve(::Coplt::i32 Endpoints, ::Coplt::i32 CtrlPoints) = 0;
        virtual void Impl_Batch(::Coplt::PathBuilderCmd const* cmds, ::Coplt::i32 num_cmds) = 0;
        virtual void Impl_Close() = 0;
        virtual void Impl_MoveTo(::Coplt::f32 x, ::Coplt::f32 y) = 0;
        virtual void Impl_LineTo(::Coplt::f32 x, ::Coplt::f32 y) = 0;
        virtual void Impl_QuadraticBezierTo(::Coplt::f32 ctrl_x, ::Coplt::f32 ctrl_y, ::Coplt::f32 to_x, ::Coplt::f32 to_y) = 0;
        virtual void Impl_CubicBezierTo(::Coplt::f32 ctrl0_x, ::Coplt::f32 ctrl0_y, ::Coplt::f32 ctrl1_x, ::Coplt::f32 ctrl1_y, ::Coplt::f32 to_x, ::Coplt::f32 to_y) = 0;
        virtual void Impl_Arc(::Coplt::f32 center_x, ::Coplt::f32 center_y, ::Coplt::f32 radii_x, ::Coplt::f32 radii_y, ::Coplt::f32 sweep_angle, ::Coplt::f32 x_rotation) = 0;
    };

    template <std::derived_from<::Coplt::IPathBuilder> Base = ::Coplt::IPathBuilder>
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

        static ::Coplt::i32 COPLT_CDECL f_Build(::Coplt::IPathBuilder* self, IPath** p0) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_Build(p0));
        }

        static void COPLT_CDECL f_Reserve(::Coplt::IPathBuilder* self, ::Coplt::i32 p0, ::Coplt::i32 p1) noexcept
        {
            AsImpl(self)->Impl_Reserve(p0, p1);
        }

        static void COPLT_CDECL f_Batch(::Coplt::IPathBuilder* self, ::Coplt::PathBuilderCmd const* p0, ::Coplt::i32 p1) noexcept
        {
            AsImpl(self)->Impl_Batch(p0, p1);
        }

        static void COPLT_CDECL f_Close(::Coplt::IPathBuilder* self) noexcept
        {
            AsImpl(self)->Impl_Close();
        }

        static void COPLT_CDECL f_MoveTo(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1) noexcept
        {
            AsImpl(self)->Impl_MoveTo(p0, p1);
        }

        static void COPLT_CDECL f_LineTo(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1) noexcept
        {
            AsImpl(self)->Impl_LineTo(p0, p1);
        }

        static void COPLT_CDECL f_QuadraticBezierTo(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1, ::Coplt::f32 p2, ::Coplt::f32 p3) noexcept
        {
            AsImpl(self)->Impl_QuadraticBezierTo(p0, p1, p2, p3);
        }

        static void COPLT_CDECL f_CubicBezierTo(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1, ::Coplt::f32 p2, ::Coplt::f32 p3, ::Coplt::f32 p4, ::Coplt::f32 p5) noexcept
        {
            AsImpl(self)->Impl_CubicBezierTo(p0, p1, p2, p3, p4, p5);
        }

        static void COPLT_CDECL f_Arc(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1, ::Coplt::f32 p2, ::Coplt::f32 p3, ::Coplt::f32 p4, ::Coplt::f32 p5) noexcept
        {
            AsImpl(self)->Impl_Arc(p0, p1, p2, p3, p4, p5);
        }
    };

    template<class Impl>
    constexpr static VirtualTable s_vtb
    {
        .b = ComProxy<::Coplt::IUnknown>::s_vtb<Impl>,
        .f_Build = VirtualImpl<Impl>::f_Build,
        .f_Reserve = VirtualImpl<Impl>::f_Reserve,
        .f_Batch = VirtualImpl<Impl>::f_Batch,
        .f_Close = VirtualImpl<Impl>::f_Close,
        .f_MoveTo = VirtualImpl<Impl>::f_MoveTo,
        .f_LineTo = VirtualImpl<Impl>::f_LineTo,
        .f_QuadraticBezierTo = VirtualImpl<Impl>::f_QuadraticBezierTo,
        .f_CubicBezierTo = VirtualImpl<Impl>::f_CubicBezierTo,
        .f_Arc = VirtualImpl<Impl>::f_Arc,
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_IPathBuilder
{

    inline ::Coplt::i32 COPLT_CDECL Build(::Coplt::IPathBuilder* self, IPath** p0) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IPathBuilder, Build, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::IPathBuilder>(self)->Impl_Build(p0));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IPathBuilder, Build, ::Coplt::i32)
        #endif
        return r;
    }

    inline void COPLT_CDECL Reserve(::Coplt::IPathBuilder* self, ::Coplt::i32 p0, ::Coplt::i32 p1) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IPathBuilder, Reserve, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IPathBuilder>(self)->Impl_Reserve(p0, p1);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IPathBuilder, Reserve, void)
        #endif
    }

    inline void COPLT_CDECL Batch(::Coplt::IPathBuilder* self, ::Coplt::PathBuilderCmd const* p0, ::Coplt::i32 p1) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IPathBuilder, Batch, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IPathBuilder>(self)->Impl_Batch(p0, p1);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IPathBuilder, Batch, void)
        #endif
    }

    inline void COPLT_CDECL Close(::Coplt::IPathBuilder* self) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IPathBuilder, Close, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IPathBuilder>(self)->Impl_Close();
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IPathBuilder, Close, void)
        #endif
    }

    inline void COPLT_CDECL MoveTo(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IPathBuilder, MoveTo, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IPathBuilder>(self)->Impl_MoveTo(p0, p1);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IPathBuilder, MoveTo, void)
        #endif
    }

    inline void COPLT_CDECL LineTo(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IPathBuilder, LineTo, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IPathBuilder>(self)->Impl_LineTo(p0, p1);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IPathBuilder, LineTo, void)
        #endif
    }

    inline void COPLT_CDECL QuadraticBezierTo(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1, ::Coplt::f32 p2, ::Coplt::f32 p3) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IPathBuilder, QuadraticBezierTo, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IPathBuilder>(self)->Impl_QuadraticBezierTo(p0, p1, p2, p3);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IPathBuilder, QuadraticBezierTo, void)
        #endif
    }

    inline void COPLT_CDECL CubicBezierTo(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1, ::Coplt::f32 p2, ::Coplt::f32 p3, ::Coplt::f32 p4, ::Coplt::f32 p5) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IPathBuilder, CubicBezierTo, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IPathBuilder>(self)->Impl_CubicBezierTo(p0, p1, p2, p3, p4, p5);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IPathBuilder, CubicBezierTo, void)
        #endif
    }

    inline void COPLT_CDECL Arc(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1, ::Coplt::f32 p2, ::Coplt::f32 p3, ::Coplt::f32 p4, ::Coplt::f32 p5) noexcept
    {
        struct { } r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::IPathBuilder, Arc, void)
        #endif
        ::Coplt::Internal::AsImpl<::Coplt::IPathBuilder>(self)->Impl_Arc(p0, p1, p2, p3, p4, p5);
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::IPathBuilder, Arc, void)
        #endif
    }
}
#define COPLT_COM_INTERFACE_BODY_Coplt_IPathBuilder\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::IPathBuilder;\
\
    explicit IPathBuilder(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::IPathBuilder>
{
    static COPLT_FORCE_INLINE ::Coplt::HResult Build(::Coplt::IPathBuilder* self, IPath** p0) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(IPathBuilder, self)->f_Build(self, p0));
    }
    static COPLT_FORCE_INLINE void Reserve(::Coplt::IPathBuilder* self, ::Coplt::i32 p0, ::Coplt::i32 p1) noexcept
    {
        COPLT_COM_PVTB(IPathBuilder, self)->f_Reserve(self, p0, p1);
    }
    static COPLT_FORCE_INLINE void Batch(::Coplt::IPathBuilder* self, ::Coplt::PathBuilderCmd const* p0, ::Coplt::i32 p1) noexcept
    {
        COPLT_COM_PVTB(IPathBuilder, self)->f_Batch(self, p0, p1);
    }
    static COPLT_FORCE_INLINE void Close(::Coplt::IPathBuilder* self) noexcept
    {
        COPLT_COM_PVTB(IPathBuilder, self)->f_Close(self);
    }
    static COPLT_FORCE_INLINE void MoveTo(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1) noexcept
    {
        COPLT_COM_PVTB(IPathBuilder, self)->f_MoveTo(self, p0, p1);
    }
    static COPLT_FORCE_INLINE void LineTo(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1) noexcept
    {
        COPLT_COM_PVTB(IPathBuilder, self)->f_LineTo(self, p0, p1);
    }
    static COPLT_FORCE_INLINE void QuadraticBezierTo(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1, ::Coplt::f32 p2, ::Coplt::f32 p3) noexcept
    {
        COPLT_COM_PVTB(IPathBuilder, self)->f_QuadraticBezierTo(self, p0, p1, p2, p3);
    }
    static COPLT_FORCE_INLINE void CubicBezierTo(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1, ::Coplt::f32 p2, ::Coplt::f32 p3, ::Coplt::f32 p4, ::Coplt::f32 p5) noexcept
    {
        COPLT_COM_PVTB(IPathBuilder, self)->f_CubicBezierTo(self, p0, p1, p2, p3, p4, p5);
    }
    static COPLT_FORCE_INLINE void Arc(::Coplt::IPathBuilder* self, ::Coplt::f32 p0, ::Coplt::f32 p1, ::Coplt::f32 p2, ::Coplt::f32 p3, ::Coplt::f32 p4, ::Coplt::f32 p5) noexcept
    {
        COPLT_COM_PVTB(IPathBuilder, self)->f_Arc(self, p0, p1, p2, p3, p4, p5);
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
    void COPLT_CDECL Some(::Coplt::IStub* self, ::Coplt::NodeType p0, ::Coplt::RootData* p1, ::Coplt::NString* p2) noexcept;
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

    inline void COPLT_CDECL Some(::Coplt::IStub* self, ::Coplt::NodeType p0, ::Coplt::RootData* p1, ::Coplt::NString* p2) noexcept
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
struct ::Coplt::Internal::VirtualTable<::Coplt::ITessellator>
{
    VirtualTable<::Coplt::IUnknown> b;
    ::Coplt::i32 (*const COPLT_CDECL f_Fill)(::Coplt::ITessellator*, IPath* path, ::Coplt::TessFillOptions* options) noexcept;
    ::Coplt::i32 (*const COPLT_CDECL f_Stroke)(::Coplt::ITessellator*, IPath* path, ::Coplt::TessStrokeOptions* options) noexcept;
};
namespace Coplt::Internal::VirtualImpl_Coplt_ITessellator
{
    ::Coplt::i32 COPLT_CDECL Fill(::Coplt::ITessellator* self, IPath* p0, ::Coplt::TessFillOptions* p1) noexcept;
    ::Coplt::i32 COPLT_CDECL Stroke(::Coplt::ITessellator* self, IPath* p0, ::Coplt::TessStrokeOptions* p1) noexcept;
}

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::ITessellator>
{
    using VirtualTable = VirtualTable<::Coplt::ITessellator>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("acf5d52e-a656-4c00-a528-09aa4d86b2b2");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::ITessellator>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::ITessellator*>(self)));
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
            .f_Fill = VirtualImpl_Coplt_ITessellator::Fill,
            .f_Stroke = VirtualImpl_Coplt_ITessellator::Stroke,
        };
        return vtb;
    };

    struct Impl : ComProxy<::Coplt::IUnknown>::Impl
    {

        virtual ::Coplt::HResult Impl_Fill(IPath* path, ::Coplt::TessFillOptions* options) = 0;
        virtual ::Coplt::HResult Impl_Stroke(IPath* path, ::Coplt::TessStrokeOptions* options) = 0;
    };

    template <std::derived_from<::Coplt::ITessellator> Base = ::Coplt::ITessellator>
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

        static ::Coplt::i32 COPLT_CDECL f_Fill(::Coplt::ITessellator* self, IPath* p0, ::Coplt::TessFillOptions* p1) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_Fill(p0, p1));
        }

        static ::Coplt::i32 COPLT_CDECL f_Stroke(::Coplt::ITessellator* self, IPath* p0, ::Coplt::TessStrokeOptions* p1) noexcept
        {
            return ::Coplt::Internal::BitCast<::Coplt::i32>(AsImpl(self)->Impl_Stroke(p0, p1));
        }
    };

    template<class Impl>
    constexpr static VirtualTable s_vtb
    {
        .b = ComProxy<::Coplt::IUnknown>::s_vtb<Impl>,
        .f_Fill = VirtualImpl<Impl>::f_Fill,
        .f_Stroke = VirtualImpl<Impl>::f_Stroke,
    };
};
namespace Coplt::Internal::VirtualImpl_Coplt_ITessellator
{

    inline ::Coplt::i32 COPLT_CDECL Fill(::Coplt::ITessellator* self, IPath* p0, ::Coplt::TessFillOptions* p1) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ITessellator, Fill, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::ITessellator>(self)->Impl_Fill(p0, p1));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ITessellator, Fill, ::Coplt::i32)
        #endif
        return r;
    }

    inline ::Coplt::i32 COPLT_CDECL Stroke(::Coplt::ITessellator* self, IPath* p0, ::Coplt::TessStrokeOptions* p1) noexcept
    {
        ::Coplt::i32 r;
        #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
        COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ITessellator, Stroke, ::Coplt::i32)
        #endif
        r = ::Coplt::Internal::BitCast<::Coplt::i32>(::Coplt::Internal::AsImpl<::Coplt::ITessellator>(self)->Impl_Stroke(p0, p1));
        #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
        COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ITessellator, Stroke, ::Coplt::i32)
        #endif
        return r;
    }
}
#define COPLT_COM_INTERFACE_BODY_Coplt_ITessellator\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::ITessellator;\
\
    explicit ITessellator(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::CallComMethod<::Coplt::ITessellator>
{
    static COPLT_FORCE_INLINE ::Coplt::HResult Fill(::Coplt::ITessellator* self, IPath* p0, ::Coplt::TessFillOptions* p1) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(ITessellator, self)->f_Fill(self, p0, p1));
    }
    static COPLT_FORCE_INLINE ::Coplt::HResult Stroke(::Coplt::ITessellator* self, IPath* p0, ::Coplt::TessStrokeOptions* p1) noexcept
    {
        return ::Coplt::Internal::BitCast<::Coplt::HResult>(COPLT_COM_PVTB(ITessellator, self)->f_Stroke(self, p0, p1));
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
