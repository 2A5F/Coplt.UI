#pragma once

#include "Com.h"

namespace Coplt {
    struct FrameSource final : ComImpl<FrameSource, IFrameSource>
    {
        FrameTime m_frame_time{};

        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        FrameTime* Impl_get_Data();

        COPLT_IMPL_END
    };
} // namespace Coplt
