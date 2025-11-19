#include "fvar.h"

namespace Coplt::OT
{
    std::span<F2DOT14> FontCalcCtx::GetTuple(const OtRef_fvar fvar) const
    {
        if (m_tuple_size != 0)
        {
            COPLT_DEBUG_ASSERT(m_tuple_size == fvar.AxisCount());
            return std::span(m_tuple, m_tuple_size);
        }
        if (fvar.AxisCount() > 16) throw Exception("Too many axes");
        fvar.BuildTuple(*m_style, std::span(m_tuple, fvar.AxisCount()));
        m_tuple_size = fvar.AxisCount();
        return std::span(m_tuple, m_tuple_size);
    }

    void OtRef_fvar::BuildTuple(const FontStyleInfo& style, const std::span<F2DOT14> tuple) const
    {
        COPLT_DEBUG_ASSERT(tuple.size() == m_header->AxisCount);
        const auto axes = Axes();
        Fixed tmp_tuple[m_header->AxisCount];
        for (auto i = 0; i < m_header->AxisCount; ++i)
        {
            const VariationAxis& axis = axes[i];
            Fixed value;
            switch (axis.Tag)
            {
            case Italic:
                value = style.IsItalic ? 1.0f : 0.0f;
                break;
            case Optical:
                value = axis.DefaultValue;
                break;
            case Slant:
                value = style.IsItalic ? style.FontOblique : 0.0f;
                break;
            case Width:
                value = style.FontWidth.Width;
                break;
            case Weight:
                value = static_cast<f32>(style.FontWeight);
                break;
            default:
                value = axis.DefaultValue;
                break;
            }
            value = Fixed(std::clamp(value.value, axis.MinValue.value, axis.MaxValue.value));
            tmp_tuple[i] = value;
        }
        m_avar->Normalize(std::span(tmp_tuple, m_header->AxisCount), tuple);
    }

    void AxisVariation::Normalize(const std::span<const Fixed> input, const std::span<F2DOT14> output) const
    {
        const SegmentMaps* map = AxisSegmentMaps;
        for (u32 i = 0; i < AxisCount; ++i)
        {
            output[i] = map->Map(input[i]);
            map = map->NextMap();
        }
    }

    f32 SegmentMaps::Map(const f32 value) const
    {
        const auto map = Span();
        const auto len = PositionMapCount;

        if (len < 2)
        {
            if (!len) return value;
            else return value - map[0].FromCoordinate + map[0].ToCoordinate;
        }

        u16 start = 0;
        u16 end = len;
        if (map[start].FromCoordinate == -1.0f && map[start].ToCoordinate == -1.0f && map[start + 1].FromCoordinate == -1.0f)
            start++;
        if (map[end - 1].FromCoordinate == +1.0f && map[end - 1].ToCoordinate == +1.0f && map[end - 2].FromCoordinate == +1.0f)
            end--;

        u16 i;
        for (i = start; i < end; ++i)
            if (value == map[i].FromCoordinate) break;
        if (i < end)
        {
            u16 j = i;
            for (; j + 1 < end; j++)
                if (value != map[j + 1].FromCoordinate)
                    break;

            if (i == j)
                return map[i].ToCoordinate;
            if (i + 2 == j)
                return map[i + 1].ToCoordinate;

            if (value < 0) return map[j].ToCoordinate;
            if (value > 0) return map[i].ToCoordinate;

            return std::abs(map[i].ToCoordinate) < std::abs(map[j].ToCoordinate) ? map[i].ToCoordinate : map[j].ToCoordinate;
        }

        for (i = start; i < end; i++)
            if (value < map[i].FromCoordinate) break;

        if (i == 0)
        {
            return value - map[0].FromCoordinate + map[0].ToCoordinate;
        }
        if (i == end)
        {
            return value - map[end - 1].FromCoordinate + map[end - 1].ToCoordinate;
        }

        const auto& before = map[i - 1];
        const auto& after = map[i];
        const f32 denom = after.FromCoordinate - before.FromCoordinate;
        return before.ToCoordinate + (after.ToCoordinate - before.ToCoordinate) * (value - before.FromCoordinate) / denom;
    }
}
