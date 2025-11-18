#include "Compute.h"

namespace Coplt::OT
{
    u16 GetLigCarets(
        const OtRef_fvar fvar, const OtRef_GDEF gdef, const FontCalcCtx& ctx, const u32 glyph_id, std::vector<f32>& inout_carets
    )
    {
        const auto lig_caret_list = gdef.LigCaretListTable();
        if (!lig_caret_list) throw NullPointerError();
        const auto item_var_store = gdef.GetItemVariationStore();
        const auto coverage = lig_caret_list->Coverage();
        const auto index = coverage.GetCoverage(glyph_id);
        if (index == -1) [[unlikely]] return 0;
        const auto& lig_glyph = lig_caret_list->LigGlyphAtIndex(index);
        if (!lig_glyph) [[unlikely]] return 0;
        if (lig_glyph->CaretCount == 0) [[unlikely]] return 0;
        const auto init_offset = inout_carets.size();
        inout_carets.resize(inout_carets.size() + lig_glyph->CaretCount);
        const std::span output(inout_carets.data() + init_offset, lig_glyph->CaretCount);
        const auto value_offsets = lig_glyph->CaretValueOffsetsSpan();
        for (usize i = 0; i < value_offsets.size(); ++i)
        {
            const OtRef_CaretValue value = lig_glyph->CaretValueAtOffset(value_offsets[i]);
            output[i] = value.GetValue(fvar, ctx, glyph_id, item_var_store);
        }
        return lig_glyph->CaretCount;
    }
}
