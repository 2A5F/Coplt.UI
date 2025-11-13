#pragma once

#include "../FontManager.h"

namespace Coplt
{
    struct DWriteFontManager final : FontManagerBase<DWriteFontManager>
    {
        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        u64 Impl_SetAssocUpdate(void* Data, Func<void, void*, IFontFace*, u64>* OnAdd, Func<void, void*, IFontFace*, u64>* OnExpired) const;

        COPLT_FORCE_INLINE
        void* Impl_RemoveAssocUpdate(u64 AssocUpdateId) const;

        COPLT_FORCE_INLINE
        void Impl_SetExpiredFrame(u64 FrameCount) const;

        COPLT_FORCE_INLINE
        u64 Impl_GetCurrentFrame() const;

        COPLT_FORCE_INLINE
        void Impl_Update() const;

        COPLT_FORCE_INLINE
        u64 Impl_FontFaceToId(IFontFace* Face) const;

        COPLT_FORCE_INLINE
        IFontFace* Impl_IdToFontFace(u64 Id) const;

        COPLT_IMPL_END

    };
}
