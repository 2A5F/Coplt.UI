#pragma once


namespace Coplt {
    struct Layout final : ComImpl<Layout, ILayout>
    {
        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        HResult Impl_Calc(NLayoutContext* ctx);

        COPLT_IMPL_END
    };
} // namespace Coplt
