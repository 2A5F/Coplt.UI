#include "fvar.h"

namespace Coplt::OT
{
    void OtRef_fvar::BuildTuple(const FontStyleInfo& style, const std::span<Fixed> tuple) const
    {
        COPLT_DEBUG_ASSERT(tuple.size() == m_header->AxisCount);
        const auto axes = Axes();
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
            tuple[i] = value;
        }
    }
}
