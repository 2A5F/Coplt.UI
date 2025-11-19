#pragma once

#include "Common.h"

namespace Coplt::OT
{
    struct AxisValueMap
    {
        /// A normalized coordinate value obtained using default normalization.
        F2DOT14 FromCoordinate;
        /// The modified, normalized coordinate value.
        F2DOT14 ToCoordinate;
    };

    struct SegmentMaps
    {
        /// The number of correspondence pairs for this axis.
        u16 PositionMapCount;
        /// The array of axis value map records for this axis.
        AxisValueMap AxisValueMaps[];

        std::span<const AxisValueMap> Span() const
        {
            return std::span(AxisValueMaps, PositionMapCount);
        }

        const SegmentMaps* NextMap() const
        {
            return reinterpret_cast<const SegmentMaps*>(AxisValueMaps + PositionMapCount);
        }

        f32 Map(f32 value) const;
    };

    struct AxisVariation : VersionBase
    {
        /// Permanently reserved; set to 0.
        u16 Reserved;
        /// The number of variation axes for this font. This must be the same number as axisCount in the 'fvar' table.
        u16 AxisCount;
        /// The segment maps array — one segment map for each axis, in the order of axes specified in the 'fvar' table.
        SegmentMaps AxisSegmentMaps[];

        void Normalize(std::span<const Fixed> input, std::span<F2DOT14> output) const;
    };
}
