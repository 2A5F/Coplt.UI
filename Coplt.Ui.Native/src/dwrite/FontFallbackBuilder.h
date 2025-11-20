#pragma once

#include <dwrite_3.h>

#include "../Com.h"

namespace Coplt
{
    struct CustomFontFallback;

    struct FontFallbackBuilder final : ComImpl<FontFallbackBuilder, IFontFallbackBuilder>
    {
        Rc<IDWriteFactory7> m_factory{};
        Rc<IDWriteFontCollection3> m_system_font_collection{};
        Rc<IDWriteFontFallbackBuilder> m_builder{};
        bool m_use_system_fallback{false};

        explicit FontFallbackBuilder(
            const TextBackend* backend, const FontFallbackBuilderCreateInfo& info
        );

        Rc<CustomFontFallback> Build() const;

        bool Add(char16 const* name, i32 length) const;
        bool Add(char16 const* locale, char16 const* name, i32 length) const;

        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        HResult Impl_Build(IFontFallback** ff);

        COPLT_FORCE_INLINE
        HResult Impl_Add(char16 const* name, i32 length, bool* exists);

        COPLT_FORCE_INLINE
        HResult Impl_AddLocaled(char16 const* locale, char16 const* name, i32 name_length, bool* exists);

        COPLT_IMPL_END
    };
} // namespace Coplt
