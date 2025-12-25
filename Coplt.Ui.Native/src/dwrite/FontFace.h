#pragma once

#include <dwrite_3.h>
#include "../Com.h"

namespace Coplt
{
    extern "C" void coplt_ui_dwrite_get_font_face_info(IDWriteFontFace5* face, NFontInfo* info);
}

namespace Coplt {
    struct DWriteFontFace final : ComImpl<DWriteFontFace, IFontFace>
    {
        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        void Impl_SetManagedHandle(void* Handle, Func<void, void*>* OnDrop);

        COPLT_FORCE_INLINE
        void* Impl_GetManagedHandle();

        COPLT_FORCE_INLINE
        u64 Impl_get_Id() const;

        COPLT_FORCE_INLINE
        u32 Impl_get_RefCount() const;

        COPLT_FORCE_INLINE
        FrameTime const* Impl_get_FrameTime() const;

        COPLT_FORCE_INLINE
        IFrameSource* Impl_GetFrameSource() const;

        COPLT_FORCE_INLINE
        IFontManager* Impl_GetFontManager() const;

        COPLT_FORCE_INLINE
        NFontInfo const* Impl_get_Info() const;

        COPLT_FORCE_INLINE
        void Impl_GetData(u8** p_data, usize* size, u32* index) const;

        COPLT_FORCE_INLINE
        bool Impl_Equals(IFontFace* other) const;

        COPLT_FORCE_INLINE
        i32 Impl_HashCode() const;

        COPLT_FORCE_INLINE
        HResult Impl_GetFamilyNames(void* ctx, Func<void, void*, char16*, i32, char16*, i32>* add) const;

        COPLT_FORCE_INLINE
        HResult Impl_GetFaceNames(void* ctx, Func<void, void*, char16*, i32, char16*, i32>* add) const;

        COPLT_IMPL_END
    };
} // namespace Coplt
