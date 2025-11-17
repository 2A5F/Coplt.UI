#pragma once

#include <cmath>
#include <span>

#include "../Com.h"
#include "../Algorithm.h"

namespace Coplt::OT
{
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
        None = 0,
        /// Signed 2-bit value, 8 values per uint16.
        Local_2_bit_deltas = 0x0001,
        /// Signed 4-bit value, 4 values per uint16.
        Local_4_bit_deltas = 0x0002,
        /// Signed 8-bit value, 2 values per uint16.
        Local_8_bit_deltas = 0x0003,
        /// VariationIndex table, contains a delta-set index pair.
        VariationIndex = 0x8000,
        /// For future use — set to 0.
        Reserved = 0x7FFC,
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
