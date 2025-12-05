#pragma once

#include "Com.h"

namespace Coplt {
    struct FrameSource final : ComImpl<FrameSource, IFrameSource>
    {
        FrameTime m_frame_time{};

        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        void Impl_Get(FrameTime* ft);

        COPLT_FORCE_INLINE
        void Impl_Set(FrameTime const* ft);

        COPLT_IMPL_END
    };
} // namespace Coplt
