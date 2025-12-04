#pragma once

#include <dwrite_3.h>
#include "../Com.h"

namespace Coplt
{
    struct DWriteFontFace final : ComImpl<DWriteFontFace, IFontFace>
    {
        Rc<IFrameSource> m_frame_source{};
        Weak<IFontManager> m_manager{};
        FrameTime m_frame_time{};

        Rc<IDWriteFontFace5> m_face{};
        NFontInfo m_info{};

        explicit DWriteFontFace(Rc<IDWriteFontFace5>&& face, IFontManager* manager, bool do_register);
        explicit DWriteFontFace(const Rc<IDWriteFontFace5>& face, IFontManager* manager, bool do_register);

        static Rc<DWriteFontFace> Get(IFontManager* manager, const Rc<IDWriteFontFace5>& face);

        void Init(IFontManager* manager, bool do_register);
        void InitInfo();

        void OnStrongCountSub(u32 old_count);

        COPLT_IMPL_START

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
