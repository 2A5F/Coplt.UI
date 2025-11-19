#include "Utils.h"

#include "../OT/OT.h"

using namespace Coplt;

bool Coplt::GetGlyphContourPoint(const OT::FontCalcCtx& ctx, u32 glyph, u32 point_index, u32* out_x, u32* out_y)
{
    const auto font = static_cast<IDWriteFontFace5*>(ctx.m_info->RawFont);
    return false;
}
