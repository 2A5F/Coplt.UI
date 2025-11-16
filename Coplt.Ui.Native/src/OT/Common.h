#pragma once

#include <span>

#include "../Com.h"

namespace Coplt::OT
{
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
}
