#include "FrameSource.h"

void FrameSource::Impl_Get(FrameTime* ft)
{
    *ft = m_frame_time;
}

void FrameSource::Impl_Set(FrameTime const* ft)
{
    m_frame_time = *ft;
}
