#include "Var.h"
#include "fvar.h"

namespace Coplt::OT
{
    f32 RegionAxisCoordinates::Calc(const F2DOT14 value) const
    {
        const i16 peek = PeakCoord.value;
        if (peek == 0 || value.value == peek) return 1.0f;
        if (value.value == 0) return 0.0f;

        const i16 start = StartCoord.value;
        const i16 end = EndCoord.value;

        if (start > peek || peek > end) [[unlikely]] return 1.0f;
        if (start < 0 && end > 0) [[unlikely]] return 1.0f;

        if (value.value <= start || end <= value.value) return 0.0f;

        if (value.value < peek)
            return static_cast<f32>(value.value - start) / (peek - start);
        else
            return static_cast<f32>(end - value.value) / (end - peek);
    }

    f32 VariationRegionList::Calc(u32 index, const std::span<F2DOT14> tuple) const
    {
        COPLT_DEBUG_ASSERT(tuple.size() == AxisCount);

        if (index >= RegionCount) [[unlikely]] return 0;
        const auto axes = AxesSpanAtIndex(index);

        f32 v = 1.0f;
        const u16 count = AxisCount;
        for (u16 i = 0; i < count; ++i)
        {
            const auto value = tuple[i];
            const auto& axis = axes[i];
            const f32 factor = axis.Calc(value);
            if (factor == 0.0f) return 0.0f;
            v *= factor;
        }

        return v;
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

        const auto tuple = ctx.GetTuple(fvar);
        fvar.BuildTuple(*ctx.m_style, tuple);

        f32 delta = 0.0f;
        u32 i = 0;

        auto lcursor = reinterpret_cast<const u32*>(row);
        for (; i < lcount; i++)
        {
            const u32 index = RegionIndexes[i];
            const f32 scalar = range->Calc(index, tuple);
            if (scalar) delta += scalar * *lcursor;
            lcursor++;
        }
        auto scursor = reinterpret_cast<const u16*>(lcursor);
        for (; i < scount; i++)
        {
            const u32 index = RegionIndexes[i];
            const f32 scalar = range->Calc(index, tuple);
            if (scalar) delta += scalar * *scursor;
            scursor++;
        }
        auto bcursor = reinterpret_cast<const u8*>(scursor);
        for (; i < count; i++)
        {
            const u32 index = RegionIndexes[i];
            const f32 scalar = range->Calc(index, tuple);
            if (scalar) delta += scalar * *bcursor;
            bcursor++;
        }

        return delta;
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
