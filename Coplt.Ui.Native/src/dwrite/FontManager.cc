#include "FontManager.h"

using namespace Coplt;

u64 DWriteFontManager::Impl_SetAssocUpdate(
    void* Data,
    Func<void, void*, IFontFace*, u64>* OnAdd,
    Func<void, void*, IFontFace*, u64>* OnExpired
) const
{
    return 0; // todo
}

void* DWriteFontManager::Impl_RemoveAssocUpdate(u64 AssocUpdateId) const
{
    return 0; // todo
}

void DWriteFontManager::Impl_SetExpiredFrame(u64 FrameCount) const
{
    return; // todo
}

u64 DWriteFontManager::Impl_GetCurrentFrame() const
{
    return 0; // todo
}

void DWriteFontManager::Impl_Update() const
{
    return; // todo
}

u64 DWriteFontManager::Impl_FontFaceToId(IFontFace* Face) const
{
    return 0; // todo
}

IFontFace* DWriteFontManager::Impl_IdToFontFace(u64 Id) const
{
    return 0; // todo
}
