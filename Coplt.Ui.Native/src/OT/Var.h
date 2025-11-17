#pragma once

#include "../Com.h"
#include "Common.h"

namespace Coplt::OT
{
    COPLT_ENUM_FLAGS(TupleIndexFlags, u16)
    {
        None = 0,
        EmbeddedPeakTuple = 8,
        IntermediateRegion = 4,
        PrivatePointNumbers = 2,
    };

    struct PackedTupleIndex
    {
        u16 Count : 12;
        TupleIndexFlags Flags : 4;
    };

    struct TupleVariationHeader
    {
        /// The size in bytes of the serialized data for this tuple variation table.
        u16 VariationDataSize;
        /// A packed field. The high 4 bits are flags. The low 12 bits are an index into a shared tuple records array.
        PackedTupleIndex TupleIndex;
        F2DOT14 TupleValues[];

        std::span<const F2DOT14> PeakTuple(const i32 AxisCount) const
        {
            const auto& TupleIndex = this->TupleIndex;
            const auto flags = TupleIndex.Flags;
            if (!HasFlags(flags, TupleIndexFlags::EmbeddedPeakTuple)) return {};
            return std::span(TupleValues, AxisCount);
        }

        std::span<const F2DOT14> IntermediateStartTuple(const i32 AxisCount) const
        {
            const auto& TupleIndex = this->TupleIndex;
            const auto flags = TupleIndex.Flags;
            if (!HasFlags(flags, TupleIndexFlags::IntermediateRegion)) return {};
            usize offset = 0;
            if (HasFlags(flags, TupleIndexFlags::EmbeddedPeakTuple))
                offset += AxisCount;
            return std::span(TupleValues + offset, AxisCount);
        }

        std::span<const F2DOT14> IntermediateEndTuple(const i32 AxisCount) const
        {
            const auto& TupleIndex = this->TupleIndex;
            const auto flags = TupleIndex.Flags;
            if (!HasFlags(flags, TupleIndexFlags::IntermediateRegion)) return {};
            usize offset = AxisCount;
            if (HasFlags(flags, TupleIndexFlags::EmbeddedPeakTuple))
                offset += AxisCount;
            return std::span(TupleValues + offset, AxisCount);
        }

        const TupleVariationHeader* NextHeader(const i32 AxisCount) const
        {
            usize bump = 2 * sizeof(u16);
            const auto& TupleIndex = this->TupleIndex;
            const auto flags = TupleIndex.Flags;
            if (HasFlags(flags, TupleIndexFlags::EmbeddedPeakTuple))
                bump += AxisCount * sizeof(F2DOT14);
            if (HasFlags(flags, TupleIndexFlags::IntermediateRegion))
                bump += 2 * AxisCount * sizeof(F2DOT14);
            return reinterpret_cast<const TupleVariationHeader*>(reinterpret_cast<const u8*>(this) + bump);
        }
    };

    COPLT_ENUM_FLAGS(TupleVariationFlags, u16)
    {
        None = 0,
        SharedPointNumbers = 8,
    };

    struct PackedTupleVariationCount
    {
        u16 Count : 12;
        TupleVariationFlags Flags : 4;
    };

    struct GlyphVariationData
    {
        /// A packed field. The high 4 bits are flags, and the low 12 bits are the number of tuple variation tables for this glyph. The count can be any number between 1 and 4095.
        PackedTupleVariationCount TupleVariationCount;
        /// Offset from the start of the GlyphVariationData table to the serialized data.
        u16 DataOffset;
        /// Array of tuple variation headers.
        TupleVariationHeader TupleVariationHeaders[];
    };

    struct CVarHeader : VersionBase, GlyphVariationData
    {
    };

    struct PackedEntryFormat
    {
        u8 InnerIndexBitCount : 4;
        u8 MapEntrySize : 2;
    };

    struct DeltaSetIndexMapFormatBase : FormatBase8
    {
        /// A packed field that describes the compressed representation of delta-set indices.
        PackedEntryFormat EntryFormat;
    };

    struct DeltaSetIndexMapFormat0 : DeltaSetIndexMapFormatBase
    {
        /// The number of mapping entries.
        u16 MapCount;
        /// The delta-set index mapping data.
        u8 MapData[];
    };

    struct DeltaSetIndexMapFormat1 : DeltaSetIndexMapFormatBase
    {
        /// The number of mapping entries.
        u32 MapCount;
        /// The delta-set index mapping data.
        u8 MapData[];
    };

    struct RegionAxisCoordinates
    {
        /// The region start coordinate value for the current axis.
        F2DOT14 StartCoord;
        /// The region peak coordinate value for the current axis.
        F2DOT14 PeakCoord;
        /// The region end coordinate value for the current axis.
        F2DOT14 EndCoord;
    };

    struct VariationRegion
    {
        /// Array of region axis coordinates records, in the order of axes given in the 'fvar' table.
        RegionAxisCoordinates RegionAxes[];
    };

    struct VariationRegionList
    {
        /// The number of variation axes for this font. This must be the same number as axisCount in the 'fvar' table.
        u16 AxisCount;
        /// The number of variation region tables in the variation region list. Must be less than 32,768.
        u16 RegionCount;
        /// Array of variation regions.
        VariationRegion VariationRegions[];
    };

    struct PackedWordDeltaCount
    {
        u16 Count : 15;
        u16 LongWords : 1;
    };

    struct ItemVariationData
    {
        /// The number of delta sets for distinct items.
        u16 ItemCount;
        /// A packed field: the high bit is a flag
        PackedWordDeltaCount WordDeltaCount;
        /// The number of variation regions referenced.
        u16 RegionIndexCount;
        /// Array of indices into the variation region list for the regions referenced by this item variation data table.
        u16 RegionIndexes[];

        f32 GetDelta(
            const FontCalcCtx& ctx, const VariationIndex* vi, const VariationRegionList* range
        ) const
        {
            if (vi->DeltaSetInnerIndex >= ItemCount) [[unlikely]] return 0;
            const bool is_long = WordDeltaCount.LongWords;
            const u32 count = RegionIndexCount;
            const u16 word_count = WordDeltaCount.Count;
            const u32 scount = is_long ? count : word_count;
            const u32 lcount = is_long ? word_count : 0;

            // todo
            return 0;
        }
    };

    struct ItemVariationStore : FormatBase
    {
        u16 _VariationRegionListOffset[2];
        /// The number of item variation data subtables.
        u16 ItemVariationDataCount;
        u8 _ItemVariationDataOffsets[];

        /// Offset in bytes from the start of the item variation store to the variation region list.
        const VariationRegionList* VariationRegionListOffset() const
        {
            return reinterpret_cast<const VariationRegionList*>(
                reinterpret_cast<const u8*>(this) + *reinterpret_cast<const u32*>(_VariationRegionListOffset)
            );
        }

        /// Offsets in bytes from the start of the item variation store to each item variation data subtable.
        const u32* p_ItemVariationDataOffsets() const
        {
            return reinterpret_cast<const u32*>(_ItemVariationDataOffsets);
        }

        /// Offsets in bytes from the start of the item variation store to each item variation data subtable.
        std::span<const u32> ItemVariationDataOffsets() const
        {
            return std::span(p_ItemVariationDataOffsets(), ItemVariationDataCount);
        }

        const ItemVariationData* DataAtIndex(const u16 index) const
        {
            const auto offset = ItemVariationDataOffsets()[index];
            return reinterpret_cast<const ItemVariationData*>(reinterpret_cast<const u8*>(this) + offset);
        }

        f32 GetDelta(const FontCalcCtx& ctx, const VariationIndex* vi) const
        {
            const auto range = VariationRegionListOffset();
            const auto data = DataAtIndex(vi->DeltaSetOuterIndex);
            if (data->ItemCount == 0) [[likely]] return 0;
            return data->GetDelta(ctx, vi, range);
        }
    };

    inline f32 GetDelta(const FontCalcCtx& ctx, const VariationIndex* vi, const ItemVariationStore* item_var_store)
    {
        if (item_var_store == nullptr) [[unlikely]] return 0;
        return item_var_store->GetDelta(ctx, vi);
    }

    inline f32 GetDelta(const FontCalcCtx& ctx, const OtRef_Device_Or_VariationIndex device, const ItemVariationStore* item_var_store)
    {
        switch (device.m_unknown->DeltaFormat)
        {
        case DeltaFormat::Local_2_bit_deltas:
        case DeltaFormat::Local_4_bit_deltas:
        case DeltaFormat::Local_8_bit_deltas:
            return device.m_device->GetDelta(ctx);
        case DeltaFormat::VariationIndex:
            return GetDelta(ctx, device.m_variation, item_var_store);
        default:
            return 0;
        }
    }
}
