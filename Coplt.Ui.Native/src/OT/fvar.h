#pragma once

#include <span>

#include "Common.h"
#include "avar.h"
#include "../StrideSpan.h"

namespace Coplt::OT
{
    struct fvar_Header : VersionBase
    {
        /// Offset in bytes from the beginning of the table to the start of the VariationAxisRecord array.
        u16 AxesArrayOffset;
        /// This field is permanently reserved. Set to 2.
        u16 _reserved;
        /// The number of variation axes in the font (the number of records in the axes array).
        u16 AxisCount;
        /// The size in bytes of each VariationAxisRecord
        u16 AxisSize;
        /// The number of named instances defined in the font (the number of records in the instances array).
        u16 InstanceCount;
        /// The size in bytes of each InstanceRecord — set to either axisCount * sizeof(Fixed) + 4, or axisCount * sizeof(Fixed) + 6.
        u16 InstanceSize;
    };

    constexpr u32 BuildTag(const char (&name)[5])
    {
        if consteval
        {
            return (static_cast<u32>(name[3]) << 24) | (static_cast<u32>(name[2]) << 16) | (static_cast<u32>(name[1]) << 8) | static_cast<u32>(name[0]);
        }
        else
        {
            return *reinterpret_cast<const u32*>(static_cast<const char*>(name));
        }
    }

    enum Tag : u32
    {
        Italic = BuildTag("ital"),
        Optical = BuildTag("opsz"),
        Slant = BuildTag("slnt"),
        Width = BuildTag("wdth"),
        Weight = BuildTag("wght"),
    };

    constexpr Tag TagOf(const char (&name)[5])
    {
        return static_cast<Tag>(BuildTag(name));
    }

    COPLT_ENUM_FLAGS(AxisQualifiers, u16)
    {
        None = 0,
        /// The axis should not be exposed directly in user interfaces.
        HiddenAxis = 1,
    };

    struct VariationAxis
    {
        /// Tag identifying the design variation for the axis.
        Tag Tag;
        /// The minimum coordinate value for the axis.
        Fixed MinValue;
        /// The default coordinate value for the axis.
        Fixed DefaultValue;
        /// The maximum coordinate value for the axis.
        Fixed MaxValue;
        /// Axis qualifiers
        AxisQualifiers Flags;
        /// The name ID for entries in the 'name' table that provide a display name for this axis.
        u16 AxisNameId;
    };

    struct Instance
    {
        /// The name ID for entries in the 'name' table that provide subfamily names for this instance.
        u16 SubfamilyNameID;
        /// Reserved for future use
        u16 Flags;

        /// Coordinate array specifying a position within the font’s variation space.
        Fixed Coordinates[];

        std::span<const Fixed> GetCoordinates(const u16 AxisCount) const
        {
            return std::span(Coordinates, AxisCount);
        }

        /// Optional. The name ID for entries in the 'name' table that provide PostScript names for this instance.
        u16 PostScriptNameID(const u16 AxisCount) const
        {
            return *reinterpret_cast<const u16*>(Coordinates + AxisCount);
        }
    };

    struct OtRef_fvar
    {
        const fvar_Header* m_header;
        const AxisVariation* m_avar;

        OtRef_fvar() = default;

        explicit OtRef_fvar(const fvar_Header* ptr, const AxisVariation* avar)
            : m_header(ptr), m_avar(avar)
        {
        }

        u16 AxisCount() const
        {
            return m_header->AxisCount;
        }

        StrideSpan<const VariationAxis> Axes() const
        {
            const auto axes_start = reinterpret_cast<const u8*>(m_header) + m_header->AxesArrayOffset;
            return StrideSpan(reinterpret_cast<const VariationAxis*>(axes_start), m_header->AxisSize, m_header->AxisCount);
        }

        StrideSpan<const Instance> Instances() const
        {
            const auto axes_start = reinterpret_cast<const u8*>(m_header) + m_header->AxesArrayOffset;
            const auto instances_start = axes_start + m_header->AxisSize * m_header->AxisCount;
            return StrideSpan(reinterpret_cast<const Instance*>(instances_start), m_header->InstanceSize, m_header->InstanceCount);
        }

        void BuildTuple(const FontStyleInfo& style, std::span<F2DOT14> tuple) const;
    };
}
