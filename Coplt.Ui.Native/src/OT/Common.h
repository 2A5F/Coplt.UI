#pragma once

#include <cmath>
#include <span>

#include "../Com.h"
#include "../Algorithm.h"

namespace Coplt::OT
{
    struct FontBaseInfo
    {
        u16 DesignUnitsPerEm;
    };

    struct FontStyleInfo
    {
        f32 FontSize;
        bool IsVertical;
    };

    struct FontCalcCtx
    {
        const FontBaseInfo* m_info;
        const FontStyleInfo* m_style;
        f32 m_scale;
        u16 m_ppem;

        FontCalcCtx() = default;

        explicit FontCalcCtx(const FontBaseInfo* info, const FontStyleInfo* style)
            : m_info(info), m_style(style),
              m_scale(style->FontSize / info->DesignUnitsPerEm),
              m_ppem(std::round(m_style->FontSize))
        {
        }
    };

    struct F2DOT14
    {
        u16 value;

        F2DOT14() = default;

        F2DOT14(f32 value)
            : value(std::round(value * 16384.f))
        {
        }

        operator f32() const
        {
            return value * (1 / 16384.f);
        }
    };

    struct Fixed
    {
        u32 value;

        Fixed() = default;

        Fixed(f32 value)
            : value(std::round(value * 65536.f))
        {
        }

        operator f32() const
        {
            return value * (1 / 65536.f);
        }
    };

    struct VersionBase
    {
        /// Major version number
        u16 MajorVersion;
        /// Minor version number
        u16 MinorVersion;
    };

    struct FormatBase8
    {
        /// Format identifier
        u8 Format;
    };

    struct FormatBase
    {
        /// Format identifier
        u16 Format;
    };

    struct CoverageFormat1 : FormatBase
    {
        /// Number of glyphs in the glyph array.
        u16 GlyphCount;
        /// Array of glyph IDs — in numerical order.
        u16 GlyphArray[];

        std::span<const u16> GlyphSpan() const
        {
            return std::span(GlyphArray, GlyphCount);
        }

        i32 GetCoverage(const u32 GlyphId) const
        {
            return Algorithm::BinarySearch(GlyphArray, GlyphCount, GlyphId);
        }
    };

    struct CoverageRange
    {
        /// First glyph ID in the range.
        u16 StartGlyphID;
        /// Last glyph ID in the range.
        u16 EndGlyphID;
        /// Coverage Index of first glyph ID in range.
        u16 StartCoverageIndex;
    };

    struct CoverageFormat2 : FormatBase
    {
        /// Number of RangeRecords.
        u16 RangeCount;
        /// Array of glyph ranges — ordered by startGlyphID.
        CoverageRange RangeRecords[];

        std::span<const CoverageRange> RangeSpan() const
        {
            return std::span(RangeRecords, RangeCount);
        }

        i32 GetCoverage(const u32 GlyphId) const
        {
            const auto range_index = Algorithm::BinarySearch(
                RangeRecords, RangeCount, GlyphId, [](const CoverageRange& range, const u32& GlyphId)
                {
                    if (GlyphId < range.StartGlyphID) return -1;
                    if (GlyphId > range.EndGlyphID) return 1;
                    return 0;
                }
            );
            if (range_index == -1) [[unlikely]] return -1;
            const auto& range = RangeRecords[range_index];
            return range.StartCoverageIndex + (GlyphId - range.StartGlyphID);
        }
    };

    struct ClassDefFormat1 : FormatBase
    {
        /// First glyph ID assigned to a class.
        u16 StartGlyphID;
        /// Number of elements in the classValues array.
        u16 GlyphCount;
        /// Array of class values — one per glyph ID.
        u16 ClassValues[];

        std::span<const u16> ClassValueSpan() const
        {
            return std::span(ClassValues, GlyphCount);
        }
    };

    struct ClassRange
    {
        /// First glyph ID in the range.
        u16 StartGlyphID;
        /// Last glyph ID in the range.
        u16 EndGlyphID;
        /// Applied to all glyphs in the range.
        u16 Class;
    };

    struct ClassDefFormat2 : FormatBase
    {
        /// Number of ClassRange records.
        u16 ClassRangeCount;
        /// Array of ClassRangeRecords — ordered by startGlyphID.
        ClassRange ClassRangeRecords[];

        std::span<const ClassRange> RangeSpan() const
        {
            return std::span(ClassRangeRecords, ClassRangeCount);
        }
    };

    enum class DeltaFormat : u16
    {
        /// Signed 2-bit value, 8 values per uint16.
        Local_2_bit_deltas = 0x0001,
        /// Signed 4-bit value, 4 values per uint16.
        Local_4_bit_deltas = 0x0002,
        /// Signed 8-bit value, 2 values per uint16.
        Local_8_bit_deltas = 0x0003,
        /// VariationIndex table, contains a delta-set index pair.
        VariationIndex = 0x8000,
    };

    struct Device_Or_VariationIndex
    {
        u16 _pad[2];
        DeltaFormat DeltaFormat;
    };

    struct Device
    {
        /// Smallest size to correct, in ppem.
        u16 StartSize;
        /// Largest size to correct, in ppem.
        u16 EndSize;
        /// Format of deltaValue array data: 0x0001, 0x0002, or 0x0003.
        DeltaFormat DeltaFormat;
        /// Array of compressed data.
        u16 DeltaValue[];

        f32 GetDelta(const FontCalcCtx& ctx) const
        {
            const u16 ppem = ctx.m_ppem;
            if (ppem == 0) [[unlikely]] return 0;

            if (ppem < StartSize || ppem > EndSize) [[unlikely]]
                return 0;

            const u32 f = static_cast<u32>(DeltaFormat);
            if (f < 1 || f > 3) [[unlikely]] return 0;

            const u32 s = ppem - StartSize;

            const u32 byte = DeltaValue[s >> (4 - f)];
            const u32 bits = byte >> (16 - (((s & (1 << (4 - f)) - 1) + 1) << f));
            const u32 mask = 0xFFFFu >> (16 - (1 << f));

            i32 delta = bits & mask;

            if (static_cast<unsigned int>(delta) >= (mask + 1) >> 1)
                delta -= mask + 1;

            return delta;
        }
    };

    struct VariationIndex
    {
        /// A delta-set outer index — used to select an item variation data subtable within the item variation store.
        u16 DeltaSetOuterIndex;
        /// A delta-set inner index — used to select a delta-set row within an item variation data subtable.
        u16 DeltaSetInnerIndex;
        /// Format, = 0x8000.
        DeltaFormat DeltaFormat;
    };

    struct OtRef_Device_Or_VariationIndex
    {
        union
        {
            const Device_Or_VariationIndex* m_unknown;
            const Device* m_device;
            const VariationIndex* m_variation;
        };

        OtRef_Device_Or_VariationIndex() = default;

        explicit OtRef_Device_Or_VariationIndex(const Device_Or_VariationIndex* ptr)
            : m_unknown(ptr)
        {
        }
    };

    struct OtRef_Coverage
    {
        union
        {
            const FormatBase* m_unknown;
            const CoverageFormat1* m_format1;
            const CoverageFormat2* m_format2;
        };

        OtRef_Coverage() = default;

        explicit OtRef_Coverage(const FormatBase* ptr)
            : m_unknown(ptr)
        {
        }

        i32 GetCoverage(const u32 GlyphId) const
        {
            switch (m_unknown->Format)
            {
            case 1: return m_format1->GetCoverage(GlyphId);
            case 2: return m_format2->GetCoverage(GlyphId);
            default: return -1;
            }
        }
    };
}
