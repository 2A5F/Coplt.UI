#pragma once

#include <vector>

#include "../Com.h"
#include "Common.h"
#include "Var.h"

namespace Coplt::OT
{
    enum class GlyphClassDef : u16
    {
        None = 0,
        /// Base glyph (single character, spacing glyph)
        BaseGlyph = 1,
        /// Ligature glyph (multiple character, spacing glyph)
        LigatureGlyph = 2,
        /// Mark glyph (non-spacing combining glyph)
        MarkGlyph = 3,
        /// Component glyph (part of single character, spacing glyph)
        ComponentGlyph = 4,
    };

    struct CaretValueFormat1 : FormatBase
    {
        /// X or Y value, in design units.
        i16 Coordinate;

        f32 GetValue(const FontCalcCtx& ctx) const
        {
            return Coordinate * ctx.m_scale;
        }
    };

    struct CaretValueFormat2 : FormatBase
    {
        /// Contour point index on glyph.
        u16 caretValuePointIndex;

        f32 GetValue(const FontCalcCtx& ctx, const u32 glyph_id) const
        {
            // todo
            return 0;
        }
    };

    struct CaretValueFormat3 : CaretValueFormat1
    {
        /// X or Y value, in design units.
        i16 Coordinate;
        /// Offset to Device table (non-variable font) / VariationIndex table (variable font) for X or Y value-from beginning of CaretValue table.
        u16 DeviceOffset;

        OtRef_Device_Or_VariationIndex GetDevice() const
        {
            return OtRef_Device_Or_VariationIndex(reinterpret_cast<const Device_Or_VariationIndex*>(reinterpret_cast<const u8*>(this) + DeviceOffset));
        }

        f32 GetValue(const FontCalcCtx& ctx, const OtRef_fvar fvar, const ItemVariationStore* item_var_store) const
        {
            const auto device = GetDevice();
            return Coordinate * ctx.m_scale + GetDelta(ctx, fvar, device, item_var_store);
        }
    };

    struct OtRef_CaretValue
    {
        union
        {
            const FormatBase* m_unknown;
            const CaretValueFormat1* m_format_1;
            const CaretValueFormat2* m_format_2;
            const CaretValueFormat3* m_format_3;
        };

        OtRef_CaretValue() = default;

        explicit OtRef_CaretValue(const FormatBase* ptr)
            : m_unknown(ptr)
        {
        }

        f32 GetValue(const OtRef_fvar fvar, const FontCalcCtx& ctx, const u32 glyph_id, const ItemVariationStore* item_var_store) const
        {
            switch (m_unknown->Format)
            {
            case 1: return m_format_1->GetValue(ctx);
            case 2: return m_format_2->GetValue(ctx, glyph_id);
            case 3: return m_format_3->GetValue(ctx, fvar, item_var_store);
            default:
                return 0;
            }
        }
    };

    struct LigGlyph
    {
        /// Number of caret value tables for this ligature (components - 1).
        u16 CaretCount;
        /// Array of offsets to caret value tables, from beginning of LigGlyph table, in increasing coordinate order.
        u16 CaretValueOffsets[];

        std::span<const u16> CaretValueOffsetsSpan() const
        {
            return std::span(CaretValueOffsets, CaretCount);
        }

        OtRef_CaretValue CaretValueAtOffset(const u16 offset) const
        {
            return OtRef_CaretValue(reinterpret_cast<const FormatBase*>(reinterpret_cast<const u8*>(this) + offset));
        }
    };

    struct LigCaretList
    {
        /// Offset to Coverage table, from beginning of LigCaretList table.
        u16 CoverageOffset;
        /// Number of ligature glyphs.
        u16 LigGlyphCount;
        /// Array of offsets to LigGlyph tables, from beginning of LigCaretList table, in Coverage index order.
        u16 LigGlyphOffsets[];

        OtRef_Coverage Coverage() const
        {
            return OtRef_Coverage(reinterpret_cast<const FormatBase*>(reinterpret_cast<const u8*>(this) + CoverageOffset));
        }

        std::span<const u16> LigGlyphOffsetsSpan() const
        {
            return std::span(LigGlyphOffsets, LigGlyphCount);
        }

        const LigGlyph* LigGlyphAtOffset(const u16 offset) const
        {
            return reinterpret_cast<const LigGlyph*>(reinterpret_cast<const u8*>(this) + offset);
        }

        const LigGlyph* LigGlyphAtIndex(const u16 index) const
        {
            if (index > LigGlyphCount) [[unlikely]] return nullptr;
            return LigGlyphAtOffset(LigGlyphOffsets[index]);
        }
    };

    struct AttachPoint
    {
        /// Number of attachment points on this glyph.
        u16 PointCount;
        /// Array of contour point indices, in increasing numerical order.
        u16 PointIndices[];
    };

    struct AttachList
    {
        /// Offset to Coverage table, from beginning of AttachList table.
        u16 CoverageOffset;
        /// Number of glyphs with attachment points.
        u16 GlyphCount;
        /// Array of offsets to AttachPoint tables, from beginning of AttachList table, in Coverage Index order.
        u16 AttachPointOffsets[];
    };

    struct MarkGlyphSets : FormatBase
    {
        /// Number of mark glyph sets defined.
        u16 MarkGlyphSetCount;
        /// Array of offsets to mark glyph set coverage tables, from the start of the MarkGlyphSets table.
        u32 CoverageOffsets[];
    };

    struct GDEF_Header_1_0 : VersionBase
    {
        /// Offset to class definition table for glyph type, from beginning of GDEF header (may be NULL).
        u16 GlyphClassDefOffset;
        /// Offset to attachment point list table, from beginning of GDEF header (may be NULL).
        u16 AttachListOffset;
        /// Offset to ligature caret list table, from beginning of GDEF header (may be NULL).
        u16 LigCaretListOffset;
        /// Offset to class definition table for mark attachment type, from beginning of GDEF header (may be NULL).
        u16 MarkAttachClassDefOffset;

        const LigCaretList* LigCaretListTable() const
        {
            if (LigCaretListOffset == 0) return nullptr;
            return reinterpret_cast<const LigCaretList*>(reinterpret_cast<const u8*>(this) + LigCaretListOffset);
        }
    };

    struct GDEF_Header_1_2 : GDEF_Header_1_0
    {
        /// Offset to the table of mark glyph set definitions, from beginning of GDEF header (may be NULL).
        u16 MarkGlyphSetsDefOffset;
    };

    struct GDEF_Header_1_3 : GDEF_Header_1_2
    {
        /// Offset to the item variation store table, from beginning of GDEF header (may be NULL).
        u32 ItemVarStoreOffset;
    };

    struct OtRef_GDEF
    {
        union
        {
            const GDEF_Header_1_0* m_header;
            const GDEF_Header_1_2* m_header_1_2;
            const GDEF_Header_1_3* m_header_1_3;
        };

        OtRef_GDEF() = default;

        explicit OtRef_GDEF(const GDEF_Header_1_0* header)
            : m_header(header)
        {
        }

        const ItemVariationStore* GetItemVariationStore() const
        {
            if (m_header == nullptr) [[unlikely]] return nullptr;
            if (m_header->MajorVersion != 1 || m_header->MinorVersion < 3) [[unlikely]] return nullptr;
            if (m_header_1_3->ItemVarStoreOffset == 0) return nullptr;
            return reinterpret_cast<const ItemVariationStore*>(reinterpret_cast<const u8*>(m_header) + m_header_1_3->ItemVarStoreOffset);
        }

        const LigCaretList* LigCaretListTable() const
        {
            if (m_header == nullptr) [[unlikely]] return nullptr;
            return m_header->LigCaretListTable();
        }
    };
}
