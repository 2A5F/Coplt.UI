#pragma once

#include <dwrite_3.h>

#include "../Com.h"
#include "FontFamily.h"

namespace Coplt
{
    struct TextBackend;

    struct SystemFontCollection final : ComImpl<SystemFontCollection, IFontCollection>
    {
        Rc<IDWriteFactory7> m_dw_factory;
        Rc<IDWriteFontCollection3> m_collection;
        std::vector<Rc<FontFamily>> m_families;
        std::vector<IFontFamily*> m_p_families;

        explicit SystemFontCollection(
            Rc<IDWriteFactory7> dw_factory,
            Rc<IDWriteFontCollection3>& collection,
            std::vector<Rc<FontFamily>>& families,
            std::vector<IFontFamily*>& p_families
        );

        static Rc<SystemFontCollection> Create(const TextBackend* backend);

        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        IFontFamily* const* Impl_GetFamilies(COPLT_OUT u32* count) const;

        COPLT_FORCE_INLINE
        void Impl_ClearNativeFamiliesCache();

        COPLT_FORCE_INLINE
        u32 Impl_FindDefaultFamily();

        COPLT_IMPL_END
    };
} // namespace Coplt
