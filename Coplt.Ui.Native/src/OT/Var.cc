#include "Var.h"
#include "fvar.h"

namespace Coplt::OT
{
    f32 VariationRegionList::Calc(const FontCalcCtx& ctx) const
    {
        // todo
        return 0;
    }

    f32 ItemVariationData::GetDelta(const FontCalcCtx& ctx, const OtRef_fvar fvar, const VariationIndex* vi, const VariationRegionList* range) const
    {
        if (vi->DeltaSetInnerIndex >= ItemCount) [[unlikely]] return 0;
        const bool is_long = WordDeltaCount.LongWords;
        const u32 count = RegionIndexCount;
        const u16 word_count = WordDeltaCount.Count;
        const u32 scount = is_long ? count : word_count;
        const u32 lcount = is_long ? word_count : 0;

        const u32 row_size = GetRowSize();
        const u8* delta_sets = GetDeltaSetPtr();
        const u8* row = delta_sets + vi->DeltaSetInnerIndex * row_size;

        Fixed _tuple[fvar.AxisCount()];
        const auto tuple = std::span(_tuple, fvar.AxisCount());
        fvar.BuildTuple(*ctx.m_style, tuple);

        f32 delta = 0.0f;
        u32 i = 0;

        // todo
        return 0;
    }

    f32 ItemVariationStore::GetDelta(const FontCalcCtx& ctx, const OtRef_fvar fvar, const VariationIndex* vi) const
    {
        const auto range = VariationRegionListOffset();
        const auto data = DataAtIndex(vi->DeltaSetOuterIndex);
        if (data->ItemCount == 0) [[likely]] return 0;
        return data->GetDelta(ctx, fvar, vi, range);
    }

    f32 GetDelta(const FontCalcCtx& ctx, const OtRef_fvar fvar, const VariationIndex* vi, const ItemVariationStore* item_var_store)
    {
        if (item_var_store == nullptr) [[unlikely]] return 0;
        return item_var_store->GetDelta(ctx, fvar, vi);
    }

    f32 GetDelta(const FontCalcCtx& ctx, const OtRef_fvar fvar, const OtRef_Device_Or_VariationIndex device, const ItemVariationStore* item_var_store)
    {
        switch (device.m_unknown->DeltaFormat)
        {
        case DeltaFormat::Local_2_bit_deltas:
        case DeltaFormat::Local_4_bit_deltas:
        case DeltaFormat::Local_8_bit_deltas:
            return device.m_device->GetDelta(ctx);
        case DeltaFormat::VariationIndex:
            return GetDelta(ctx, fvar, device.m_variation, item_var_store);
        default:
            return 0;
        }
    }
}
