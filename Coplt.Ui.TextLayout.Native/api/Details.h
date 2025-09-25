#pragma once
#ifndef COPLT_UL_TEXT_LAYOUT_DETAILS_H
#define COPLT_UL_TEXT_LAYOUT_DETAILS_H

#include "CoCom.h"
#include "./Types.h"

namespace Coplt {

    using IUnknown = ::Coplt::IUnknown;
    using IWeak = ::Coplt::IWeak;

    struct IFace;
    struct ILibTextLayout;

} // namespace Coplt

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::IFace>
{
    VirtualTable<::Coplt::IUnknown> b;
};

template <>
struct ::Coplt::Internal::ComProxy<::Coplt::IFace>
{
    using VirtualTable = VirtualTable<::Coplt::IFace>;

    static COPLT_FORCE_INLINE constexpr inline const ::Coplt::Guid& get_Guid()
    {
        static ::Coplt::Guid s_guid("805e2d1f-6be2-4ebd-ac64-60c6f5f73d63");
        return s_guid;
    }

    template <class Self>
    COPLT_FORCE_INLINE
    static HResult QueryInterface(const Self* self, const ::Coplt::Guid& guid, COPLT_OUT void*& object)
    {
        if (guid == guid_of<::Coplt::IFace>())
        {
            object = const_cast<void*>(static_cast<const void*>(static_cast<const ::Coplt::IFace*>(self)));
            return ::Coplt::HResultE::Ok;
        }
        return ComProxy<::Coplt::IUnknown>::QueryInterface(self, guid, object);
    }

    template <std::derived_from<::Coplt::IFace> Base = ::Coplt::IFace>
    struct Proxy : ComProxy<::Coplt::IUnknown>::Proxy<Base>
    {
        using Super = ComProxy<::Coplt::IUnknown>::Proxy<Base>;
        using Self = Proxy;

    protected:
        virtual ~Proxy() = default;

        COPLT_FORCE_INLINE
        static const VirtualTable& GetVtb()
        {
            static VirtualTable vtb
            {
                .b = Super::GetVtb(),
            };
            return vtb;
        };

        explicit Proxy(const ::Coplt::Internal::VirtualTable<Base>* vtb) : Base(vtb) {}

        explicit Proxy() : Super(&GetVtb()) {}
    };
};

#define COPLT_COM_INTERFACE_BODY_Coplt_IFace\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::IFace;\
\
    explicit IFace(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

template <>
struct ::Coplt::Internal::VirtualTable<::Coplt::ILibTextLayout>
{
    VirtualTable<::Coplt::IUnknown> b;
    IFace* (*const f_CreateFace)(::Coplt::ILibTextLayout*);
};

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

    template <std::derived_from<::Coplt::ILibTextLayout> Base = ::Coplt::ILibTextLayout>
    struct Proxy : ComProxy<::Coplt::IUnknown>::Proxy<Base>
    {
        using Super = ComProxy<::Coplt::IUnknown>::Proxy<Base>;
        using Self = Proxy;

    protected:
        virtual ~Proxy() = default;

        COPLT_FORCE_INLINE
        static const VirtualTable& GetVtb()
        {
            static VirtualTable vtb
            {
                .b = Super::GetVtb(),
                .f_CreateFace = [](::Coplt::ILibTextLayout* self)
                {
                    #ifdef COPLT_COM_BEFORE_VIRTUAL_CALL
                    COPLT_COM_BEFORE_VIRTUAL_CALL(::Coplt::ILibTextLayout, CreateFace)
                    #endif
                    return static_cast<Self*>(self)->Impl_CreateFace();
                    #ifdef COPLT_COM_AFTER_VIRTUAL_CALL
                    COPLT_COM_AFTER_VIRTUAL_CALL(::Coplt::ILibTextLayout, CreateFace)
                    #endif
                },
            };
            return vtb;
        };

        explicit Proxy(const ::Coplt::Internal::VirtualTable<Base>* vtb) : Base(vtb) {}

        explicit Proxy() : Super(&GetVtb()) {}

        virtual IFace* Impl_CreateFace() = 0;
    };
};

#define COPLT_COM_INTERFACE_BODY_Coplt_ILibTextLayout\
    using Super = ::Coplt::IUnknown;\
    using Self = ::Coplt::ILibTextLayout;\
\
    explicit ILibTextLayout(const ::Coplt::Internal::VirtualTable<Self>* vtbl) : Super(&vtbl->b) {}

#endif //COPLT_UL_TEXT_LAYOUT_DETAILS_H
