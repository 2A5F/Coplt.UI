#include "TextLayout.h"

#include <span>
#include <fmt/xchar.h>

#include "../lib.h"
#include "../Algorithm.h"
#include "../Text.h"
#include "../Layout.h"
#include "Layout.h"
#include "Error.h"
#include "BaseFontFallback.h"
#include "Utils.h"

using namespace Coplt;
using namespace Coplt::LayoutCalc;
using namespace Coplt::LayoutCalc::Texts;

HResultE Texts::coplt_ui_layout_text_compute(
    ITextLayout* layout, NLayoutContext* ctx, const NodeId& node,
    const LayoutInputs* inputs, LayoutOutput* outputs
)
{
    return feb([&]
    {
        const auto l = static_cast<TextLayout*>(layout);
        l->Compute(*outputs, *inputs, CtxNodeRef(ctx, node));
        return HResultE::Ok;
    });
}

std::optional<LayoutOutput> TextLayoutCache::GetOutput(const LayoutInputs& inputs)
{
    switch (inputs.RunMode)
    {
    case LayoutRunMode::PerformLayout:
        {
            if (!HasFlags(Flags, LayoutCacheFlags::Final)) return std::nullopt;
            const auto& entry = Final;
            const auto cached_size = GetSize(entry.Output);
            const auto input_known_size = GetKnownSize(inputs);
            const auto cache_known_size = GetKnownSize(entry);
            const auto input_available_space = GetAvailableSpace(inputs);
            const auto cache_available_space = GetAvailableSpace(entry);

            if (
                (input_known_size.Width == cache_known_size.Width || input_known_size.Width == cached_size.Width)
                && (input_known_size.Height == cache_known_size.Height || input_known_size.Height == cached_size.Height)
                && (input_known_size.Width.has_value() || IsRoughlyEqual(
                    cache_available_space.Width, input_available_space.Width))
                && (input_known_size.Height.has_value() || IsRoughlyEqual(
                    cache_available_space.Height, input_available_space.Height))
            )
                return entry.Output;
        }
    case LayoutRunMode::ComputeSize:
        {
            if ((Flags & ~LayoutCacheFlags::Final) == 0) return std::nullopt;
            for (u16 i = 0; i < 9; ++i)
            {
                if (!HasFlags(Flags, static_cast<LayoutCacheFlags>(1 << (i + 1)))) continue;
                const auto& entry = Measure[i];
                const auto size = entry.Size();
                const auto input_known_size = GetKnownSize(inputs);
                const auto cache_known_size = GetKnownSize(entry);
                const auto input_available_space = GetAvailableSpace(inputs);
                const auto cache_available_space = GetAvailableSpace(entry);
                if (
                    (input_known_size.Width == cache_known_size.Width || input_known_size.Width == size.Width)
                    && (input_known_size.Height == cache_known_size.Height || input_known_size.Height == size.Height)
                    && (input_known_size.Width.has_value() || IsRoughlyEqual(
                        cache_available_space.Width, input_available_space.Width))
                    && (input_known_size.Height.has_value() || IsRoughlyEqual(
                        cache_available_space.Height, input_available_space.Height))
                )
                    return LayoutOutputFromOuterSize(size);
            }
            break;
        }
    case LayoutRunMode::PerformHiddenLayout:
        break;
    }
    return std::nullopt;
}

void TextLayoutCache::StoreFinal(const LayoutInputs& inputs, LayoutOutput output)
{
    Flags |= LayoutCacheFlags::Final;
    Final = {
        {
            .KnownWidth = inputs.KnownWidth,
            .KnownHeight = inputs.KnownHeight,
            .AvailableSpaceWidthValue = inputs.AvailableSpaceWidthValue,
            .AvailableSpaceHeightValue = inputs.AvailableSpaceHeightValue,
            .HasKnownWidth = inputs.HasKnownWidth,
            .HasKnownHeight = inputs.HasKnownHeight,
            .AvailableSpaceWidth = inputs.AvailableSpaceWidth,
            .AvailableSpaceHeight = inputs.AvailableSpaceHeight,
        },
        .Output = output,
    };
}

void TextLayoutCache::StoreMeasure(const LayoutInputs& inputs, const f32 width, const f32 height)
{
    const auto i = ComputeCacheSlot(
        inputs.HasKnownWidth, inputs.HasKnownHeight,
        inputs.AvailableSpaceWidth, inputs.AvailableSpaceHeight
    );
    Flags |= static_cast<LayoutCacheFlags>(1 << (i + 1));
    Measure[i] = TextLayoutCache_Measure
    {
        {
            .KnownWidth = inputs.KnownWidth,
            .KnownHeight = inputs.KnownHeight,
            .AvailableSpaceWidthValue = inputs.AvailableSpaceWidthValue,
            .AvailableSpaceHeightValue = inputs.AvailableSpaceHeightValue,
            .HasKnownWidth = inputs.HasKnownWidth,
            .HasKnownHeight = inputs.HasKnownHeight,
            .AvailableSpaceWidth = inputs.AvailableSpaceWidth,
            .AvailableSpaceHeight = inputs.AvailableSpaceHeight,
        },
        .Width = width,
        .Height = height,
    };
}

void TextLayoutCache::Clear()
{
    Flags = LayoutCacheFlags::Empty;
}

void TextLayout::Compute(LayoutOutput& out, const LayoutInputs& inputs, CtxNodeRef node)
{
    m_node = node;
    out = Compute(inputs);
    m_node = {};
}

LayoutOutput TextLayout::Compute(const LayoutInputs& inputs)
{
    const auto& style = m_node.StyleData();

    #pragma region known_dimensions

    const auto available_space = GetAvailableSpace(inputs);
    const auto known_size = GetKnownSize(inputs);

    const auto parent_size = GetParentSize(inputs);

    const auto padding = GetPadding(style).ResolveOrZero(parent_size);
    const auto border = GetBorder(style).ResolveOrZero(parent_size);
    const auto padding_border = (padding + border).SumAxes();
    const auto box_sizing_adjustment = style.BoxSizing == BoxSizing::ContentBox ? padding_border : Size<f32>{};

    const auto min_size = GetMinSize(style)
        .TryResolve(parent_size)
        .TryApplyAspectRatio(GetAspectRatio(style))
        .TryAdd(box_sizing_adjustment);
    const auto max_size = GetMaxSize(style)
        .TryResolve(parent_size)
        .TryApplyAspectRatio(GetAspectRatio(style))
        .TryAdd(box_sizing_adjustment);
    const auto clamped_size = inputs.SizingMode == LayoutSizingMode::InherentSize ?
        (GetSize(style)
            .TryResolve(parent_size)
            .TryApplyAspectRatio(GetAspectRatio(style))
            .TryAdd(box_sizing_adjustment)
            .TryClamp(min_size, max_size)) :
        Size<std::optional<f32>>{};

    const Size min_max_definite_size{
        .Width = (min_size.Width.has_value() && max_size.Width.has_value()
            && min_size.Width.value() < max_size.Width.value())
        ? min_size.Width : std::nullopt,
        .Height = (min_size.Height.has_value() && max_size.Height.has_value()
            && min_size.Height.value() < max_size.Height.value())
        ? min_size.Height : std::nullopt,
    };
    const auto known_dimensions = known_size.Or(min_max_definite_size.Or(clamped_size).TryMax(padding_border));

    if (inputs.RunMode == LayoutRunMode::ComputeSize)
    {
        if (known_dimensions.Width.has_value() && known_dimensions.Height.has_value())
        {
            return LayoutOutputFromOuterSize(Size{known_dimensions.Width.value(), known_dimensions.Height.value()});
        }
    }

    #pragma endregion

    auto last_available_space = available_space;
    Size<f32> size{};
    for (auto& data : m_paragraph_datas)
    {
        auto output = data.Compute(*this, inputs.RunMode, inputs.Axis, last_available_space, known_dimensions);
        last_available_space = last_available_space.TrySub(GetSize(output));
        if (style.WritingDirection == WritingDirection::Horizontal)
        {
            size.Width += output.Width;
            size.Height = std::max(size.Height, output.Height);
        }
        else
        {
            size.Width = std::max(size.Width, output.Width);
            size.Height += output.Height;
        }
    }
    if (inputs.RunMode == LayoutRunMode::ComputeSize)
    {
        return LayoutOutputFromOuterSize(size);
    }
    // todo
    return {
        .Width = size.Width,
        .Height = size.Height,
    };
}

LayoutOutput ParagraphData::Compute(
    TextLayout& layout, LayoutRunMode RunMode, LayoutRequestedAxis Axis,
    const Size<AvailableSpace>& AvailableSpace, const Size<std::optional<f32>>& KnownSize
)
{
    const auto& root_style = layout.m_node.StyleData();
    const auto& paragraph = layout.m_paragraphs[m_index];
    const auto axis = ToAxis(root_style.WritingDirection);

    const auto defined_line_height = Resolve(GetLineHeight(root_style), root_style.FontSize);

    const auto space = AvailableSpace.Or(KnownSize);
    const auto space_main = space.MainAxis(axis).value_or(0);

    const auto allow_wrap =
        root_style.TextWrap == TextWrap::Wrap
        && (
            root_style.WritingDirection == WritingDirection::Horizontal
            ? KnownSize.Width.has_value() || AvailableSpace.Width.first != AvailableSpaceType::MinContent
            : KnownSize.Height.has_value() || AvailableSpace.Height.first != AvailableSpaceType::MinContent
        );

    Size<f32> result_size{};
    auto& max_main = result_size.MainAxis(axis);
    auto& sum_cross = result_size.CrossAxis(axis);

    f32 cur_main = 0;
    if (paragraph.Type == TextParagraphType::Inline)
    {
        u32 nth_line = 0;
        f32 max_ascent{};
        f32 max_descent{};
        f32 max_line_cap{};
        f32 cur_line_height = defined_line_height;

        for (auto& run : m_runs)
        {
            if (run.IsInlineBlock(*this))
            {
                // todo
            }
            else
            {
                const auto& single_line_size = run.GetSingleLineSize(*this);

                max_ascent = std::max(max_ascent, single_line_size.Ascent);
                max_descent = std::max(max_descent, single_line_size.Descent);
                max_line_cap = std::max(max_line_cap, single_line_size.LineGap);

                const auto font_line_height = max_ascent + max_line_cap + max_descent;

                const auto new_size = cur_main + single_line_size.LineSize;
                if (allow_wrap && new_size > space_main)
                {
                    u32 cur_nth_line = nth_line;
                    for (const auto line : run.BreakLines(*this, cur_main, space_main))
                    {
                        if (line.Line == 0)
                        {
                            const auto final_size = cur_main + line.Size;
                            max_main = std::max(max_main, final_size);
                            cur_main = 0;
                        }
                        else
                        {
                            const auto new_nth_line = nth_line + line.Line;
                            COPLT_DEBUG_ASSERT(new_nth_line - cur_nth_line == 1, "Should not skip lines");
                            cur_nth_line = new_nth_line;

                            if (new_nth_line == 1) sum_cross += max_ascent;
                            sum_cross += std::max(cur_line_height, font_line_height);
                            max_main = std::max(max_main, line.Size);
                            cur_main = line.Size;
                            cur_line_height = defined_line_height;
                        }
                    }
                    COPLT_DEBUG_ASSERT(cur_nth_line - nth_line >= 1, "There should be at least one line");
                    nth_line = cur_nth_line;
                    continue;
                }
                else
                {
                    cur_main = new_size;
                }
            }
        }

        if (nth_line == 0) sum_cross += max_ascent;
        sum_cross += max_descent;
    }
    else
    {
        //not support float layout yet
        // todo block
    }

    return LayoutOutputFromOuterSize(result_size);
}

bool Run::IsInlineBlock(const ParagraphData& data) const
{
    const auto& font = data.m_font_ranges[FontRangeIndex];
    return !font.Font;
}

const RunLineSize& Run::GetSingleLineSize(const ParagraphData& data)
{
    if (HasSingleLineSize) return SingleLineSize;
    const auto& font = data.m_font_ranges[FontRangeIndex];
    const auto& style_range = data.m_same_style_ranges[StyleRangeIndex];
    const auto& style = data.GetScope(style_range).StyleData();
    if (!font.Font) return SingleLineSize;

    HasSingleLineSize = true;
    if (style.WritingDirection == WritingDirection::Horizontal)
    {
        DWRITE_FONT_METRICS1 metrics{};
        font.Font->GetMetrics(&metrics);
        const auto scale = style.FontSize / metrics.designUnitsPerEm;
        SingleLineSize.Ascent = metrics.ascent * scale;
        SingleLineSize.Descent = metrics.descent * scale;
        SingleLineSize.LineGap = metrics.lineGap * scale;
    }
    else
    {
        // todo: not sure, need test
        DWRITE_FONT_METRICS1 metrics{};
        font.Font->GetMetrics(&metrics);
        const auto scale = style.FontSize / metrics.designUnitsPerEm;
        SingleLineSize.LineGap = metrics.lineGap * scale;
        const auto glyph_box_size = metrics.glyphBoxRight - metrics.glyphBoxLeft;
        SingleLineSize.Ascent = SingleLineSize.Descent = glyph_box_size * scale * 0.5f;
    }

    f32 sum_size = 0;
    const std::span glyph_advances(data.m_glyph_advances.data() + GlyphStartIndex, ActualGlyphCount);
    for (u32 i = 0; i < ActualGlyphCount; ++i)
    {
        sum_size += glyph_advances[i];
    }
    SingleLineSize.LineSize = sum_size;

    return SingleLineSize;
}

std::generator<RunBreakLine> Run::BreakLines(
    const ParagraphData& data, const f32 init_size, const f32 space
) const
{
    f32 cur_space = space - init_size;
    f32 sum_size = 0;
    const std::span break_points(data.m_line_breakpoints);
    const std::span glyph_advances(data.m_glyph_advances.data() + GlyphStartIndex, ActualGlyphCount);
    u32 pre_i = 0;
    u32 line = 0;
    for (u32 i = 0, c = 0; i < ActualGlyphCount; ++i)
    {
        // todo check break points
        const f32 glyph_advance = glyph_advances[i];
        const f32 new_size = sum_size + glyph_advance;
        if (new_size < cur_space)
        {
            sum_size = new_size;
            continue;
        }
        const u32 len = i - pre_i;
        if (len == 0)
        {
            cur_space = space;
            line++;
            continue;
        }
        const u32 start = std::exchange(pre_i, i);
        const f32 size = std::exchange(sum_size, 0);
        cur_space = space;
        co_yield RunBreakLine{
            .Start = start,
            .Length = len,
            .Size = size,
            .Line = line,
        };
        line++;
    }
    const u32 len = ActualGlyphCount - pre_i;
    if (len == 0) co_return;
    co_yield RunBreakLine{
        .Start = pre_i,
        .Length = len,
        .Size = sum_size,
        .Line = line,
    };
    co_return;
}
