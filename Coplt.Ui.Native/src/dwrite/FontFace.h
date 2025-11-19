#pragma once

#include <dwrite_3.h>
#include "../Com.h"
#include "../Harfpp.h"

namespace Coplt
{
    struct DWriteFontFace final : ComImpl<DWriteFontFace, IFontFace>
    {
        u64 m_id{};
        Rc<IDWriteFontFace5> m_face{};
        Harf::HFace m_hb_face{};
        NFontInfo m_info{};

        explicit DWriteFontFace(Rc<IDWriteFontFace5>&& face, u64 id);
        explicit DWriteFontFace(const Rc<IDWriteFontFace5>& face, u64 id);

        void Init();
        void InitInfo();

        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        u64 Impl_get_Id() const;

        COPLT_FORCE_INLINE
        NFontInfo const* Impl_get_Info() const;

        COPLT_FORCE_INLINE
        bool Impl_Equals(IFontFace* other) const;

        COPLT_FORCE_INLINE
        i32 Impl_HashCode() const;

        COPLT_FORCE_INLINE
        HResult Impl_GetFamilyNames(void* ctx, Func<void, void*, char16*, i32, char16*, i32>* add) const;

        COPLT_FORCE_INLINE
        HResult Impl_GetFaceNames(void* ctx, Func<void, void*, char16*, i32, char16*, i32>* add) const;

        COPLT_IMPL_END

        bool Equals(IFontFace* other) const;
        i32 GetHashCode() const;

        void GetFamilyNames(void* ctx, Func<void, void*, char16*, i32, char16*, i32>* add) const;
        void GetFaceNames(void* ctx, Func<void, void*, char16*, i32, char16*, i32>* add) const;

        static void GetNames(IDWriteLocalizedStrings* names, void* ctx, Func<void, void*, char16*, i32, char16*, i32>* add);
    };
}
