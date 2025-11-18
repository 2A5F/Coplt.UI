#pragma once

#include "fvar.h"
#include "GDEF.h"

namespace Coplt::OT
{
    u16 GetLigCarets(
        OtRef_fvar fvar, OtRef_GDEF gdef, const FontCalcCtx& ctx, u32 glyph_id, std::vector<f32>& inout_carets
    );
}
