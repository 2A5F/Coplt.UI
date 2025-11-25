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
#include "FontFace.h"

using namespace Coplt;
using namespace Coplt::LayoutCalc;
using namespace Coplt::LayoutCalc::Texts;

HResultE Texts::coplt_ui_layout_text_compute(
    ITextLayout* layout, NLayoutContext* ctx, const NodeId& node,
    const LayoutInputs* inputs, LayoutOutput* outputs
)
{
    return feb(
        [&]
        {
            const auto l = static_cast<TextLayout*>(layout);
            l->Compute(*outputs, *inputs, CtxNodeRef(ctx, node));
            return HResultE::Ok;
        }
    );
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
                    cache_available_space.Width, input_available_space.Width
                ))
                && (input_known_size.Height.has_value() || IsRoughlyEqual(
                    cache_available_space.Height, input_available_space.Height
                ))
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
                        cache_available_space.Width, input_available_space.Width
                    ))
                    && (input_known_size.Height.has_value() || IsRoughlyEqual(
                        cache_available_space.Height, input_available_space.Height
                    ))
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

    const auto space = AvailableSpace.Or(KnownSize);
    const auto space_main = space.MainAxis(axis).value_or(std::numeric_limits<f32>::infinity());

    Size<f32> result_size{};
    auto& max_main = result_size.MainAxis(axis);
    auto& sum_cross = result_size.CrossAxis(axis);

    f32 cur_main = 0;
    if (paragraph.Type == TextParagraphType::Inline)
    {
        u32 nth_line = 0;
        f32 max_ascent{};
        f32 max_descent{};
        f32 max_line_gap{};

        RunBreakLineCtx ctx{
            .AvailableSpace = space_main
        };

        for (auto& run : m_runs)
        {
            const auto& same_style = m_same_style_ranges[run.StyleRangeIndex];
            const auto scope = GetScope(same_style);
            const auto& style = scope.StyleData();
            const auto defined_line_height = Resolve(GetLineHeight(style), style.FontSize);

            if (run.IsInlineBlock(*this))
            {
                // todo
            }
            else
            {
                const auto& single_line_size = run.GetLineInfo(*this);
                max_ascent = std::max(max_ascent, single_line_size.Ascent);
                max_descent = std::max(max_descent, single_line_size.Descent);
                max_line_gap = std::max(max_line_gap, single_line_size.LineGap);

                std::vector<ParagraphSpan> spans{};
                run.SplitSpans(spans, *this, style);
                for (const auto& span : spans)
                {
                    std::wstring text(m_chars.data() + run.Start + span.CharStart, span.CharLength);
                    Logger().Log(
                        LogLevel::Trace,
                        fmt::format(
                            L"{} \t {}..{}; \t {:.7f}; \t \"{}\"",
                            ToStr16_Pad(span.Type), span.CharStart, span.CharStart + span.CharLength, span.Size, text
                        )
                    );
                }

                // auto break_lines = run.BreakLines(*this, style, ctx);
                // for (const auto span : break_lines)
                // {
                //     const char16* text_ptr;
                //     u32 text_len;
                //     m_src->GetTextAtPosition(span.CharStart, &text_ptr, &text_len);
                //     std::wstring text(text_ptr, std::min(text_len, span.CharLength));
                //     Logger().Log(LogLevel::Trace, fmt::format(L"{} {} \t\t {}", span.NthLine, span.Size, text));
                // }
            }
        }
    }
    else
    {
        //not support float layout yet
        // todo block
    }

    return LayoutOutputFromOuterSize(result_size);
}

std::span<const u16> Run::ClusterMap(const ParagraphData& data) const
{
    return std::span(data.m_cluster_map.data() + ClusterStartIndex, Length);
}

std::span<const DWRITE_SHAPING_TEXT_PROPERTIES> Run::TextProps(const ParagraphData& data) const
{
    return std::span(data.m_text_props.data() + ClusterStartIndex, Length);
}

std::span<const u16> Run::GlyphIndices(const ParagraphData& data) const
{
    return std::span(data.m_glyph_indices.data() + GlyphStartIndex, ActualGlyphCount);
}

std::span<const DWRITE_SHAPING_GLYPH_PROPERTIES> Run::GlyphProps(const ParagraphData& data) const
{
    return std::span(data.m_glyph_props.data() + GlyphStartIndex, ActualGlyphCount);
}

std::span<const f32> Run::GlyphAdvances(const ParagraphData& data) const
{
    return std::span(data.m_glyph_advances.data() + GlyphStartIndex, ActualGlyphCount);
}

std::span<const DWRITE_GLYPH_OFFSET> Run::GlyphOffsets(const ParagraphData& data) const
{
    return std::span(data.m_glyph_offsets.data() + GlyphStartIndex, ActualGlyphCount);
}

bool Run::IsInlineBlock(const ParagraphData& data) const
{
    const auto& font = data.m_font_ranges[FontRangeIndex];
    return !font.Font;
}

const ParagraphLineInfo& Run::GetLineInfo(const ParagraphData& data)
{
    if (HasLineInfo) return LineInfo;
    const auto& font = data.m_font_ranges[FontRangeIndex];
    const auto& style_range = data.m_same_style_ranges[StyleRangeIndex];
    const auto& style = data.GetScope(style_range).StyleData();
    if (!font.Font) return LineInfo;

    HasLineInfo = true;
    if (style.WritingDirection == WritingDirection::Horizontal)
    {
        DWRITE_FONT_METRICS1 metrics{};
        font.Font->m_face->GetMetrics(&metrics);
        const auto scale = style.FontSize / metrics.designUnitsPerEm;
        LineInfo.Ascent = metrics.ascent * scale;
        LineInfo.Descent = metrics.descent * scale;
        LineInfo.LineGap = metrics.lineGap * scale;
    }
    else
    {
        // todo: not sure, need test
        DWRITE_FONT_METRICS1 metrics{};
        font.Font->m_face->GetMetrics(&metrics);
        const auto scale = style.FontSize / metrics.designUnitsPerEm;
        LineInfo.LineGap = metrics.lineGap * scale;
        const auto glyph_box_size = metrics.glyphBoxRight - metrics.glyphBoxLeft;
        LineInfo.Ascent = LineInfo.Descent = glyph_box_size * scale * 0.5f;
    }

    return LineInfo;
}

namespace Coplt::LayoutCalc::Texts::Compute
{
    struct Cursor
    {
        u32 Char;
        f32 Offset;

        Cursor() = default;

        explicit Cursor(const u32 c, const f32 o)
            : Char(c), Offset(o)
        {
        }

        u16 NextGlyph(const std::span<const u16> cluster_map) const
        {
            const auto next_char = Char + 1;
            if (next_char >= cluster_map.size()) return cluster_map[Char] + 1;
            return cluster_map[next_char];
        }
    };
}

void Run::SplitSpans(std::vector<ParagraphSpan>& spans, const ParagraphData& data, const StyleData& style)
{
    using namespace Coplt::LayoutCalc::Texts::Compute;

    if (Length == 0 || ActualGlyphCount == 0) [[unlikely]] return;

    const auto collapse_space = ShouldCollapseSpace(style.WhiteSpace);
    const auto keep_newline = ShouldKeepNewLine(style.WhiteSpace);

    const std::span cluster_map = ClusterMap(data);
    const std::span text_props = TextProps(data);
    const std::span glyph_props = GlyphProps(data);
    const std::span glyph_advances = GlyphAdvances(data);
    const std::span glyph_offsets = GlyphOffsets(data);

    u32 span_start = 0;
    u32 cur_char = 0;
    f32 cur_size = 0;
    u32 glyph_start = 0;
    u32 glyph_len = 0;
    auto cur_type = ParagraphSpanType::Common;

    u16 first_cluster = cluster_map[span_start];
    for (;;)
    {
        const auto text = data.m_chars.data() + Start + cur_char;
        u32 ci = 0;
        UChar32 uc;
        U16_NEXT(text, ci, Length, uc);

        ParagraphSpanType type;
        switch (uc)
        {
        case 0x000A: // LF \n
        case 0x000D: // CR \r
            type = keep_newline ? ParagraphSpanType::NewLine : ParagraphSpanType::Space;
            break;
        case 0x0009: // HT \t
        case 0x000B: // VT \v
        case 0x0020: // Space
            type = ParagraphSpanType::Space;
            break;
        default:
            type = ParagraphSpanType::Common;
            break;
        }

        if (type != cur_type)
        {
            if (cur_char != span_start)
            {
                spans.push_back(
                    ParagraphSpan{
                        .CharStart = span_start,
                        .CharLength = cur_char - span_start,
                        .GlyphStart = glyph_start,
                        .GlyphLength = glyph_len,
                        .Size = cur_size,
                        .Type = cur_type,
                    }
                );
                span_start = cur_char;
                cur_size = 0;
                glyph_start += glyph_len;
                glyph_len = 0;
            }
            cur_type = type;
        }

        const u32 next_char = cur_char + 1;
        if (next_char < Length)
        {
            const u16 next_cluster = cluster_map[next_char];
            if (next_cluster == first_cluster)
            {
                cur_char = next_char;
                continue;
            }
        }
        const u16 last_cluster = cluster_map[cur_char];

        f32 sum_size = 0;
        {
            u16 i = first_cluster;
            for (; i <= last_cluster; ++i)
            {
                const auto glyph_advance = glyph_advances[i];
                const auto& glyph_offset = glyph_offsets[i];
                sum_size += glyph_advance + glyph_offset.advanceOffset;
                glyph_len++;
            }
            i = last_cluster + 1;
            for (; i < ActualGlyphCount; ++i)
            {
                const auto& glyph_prop = glyph_props[i];
                if (glyph_prop.isClusterStart) break;
                const auto glyph_advance = glyph_advances[i];
                const auto& glyph_offset = glyph_offsets[i];
                sum_size += glyph_advance + glyph_offset.advanceOffset;
                glyph_len++;
            }
        }
        cur_size += sum_size;

        if (next_char >= Length)
        {
            spans.push_back(
                ParagraphSpan{
                    .CharStart = span_start,
                    .CharLength = next_char - span_start,
                    .GlyphStart = glyph_start,
                    .GlyphLength = glyph_len,
                    .Size = cur_size,
                    .Type = cur_type,
                }
            );
            return;
        }

        cur_char = next_char;
        first_cluster = cluster_map[cur_char];
    }
}

#ifdef _DEBUG
std::vector<ParagraphLineSpan> Run::BreakLines(const ParagraphData& data, const StyleData& style, RunBreakLineCtx& ctx) const
#else
std::generator<ParagraphLineSpan> Run::BreakLines(const ParagraphData& data, const StyleData& style, RunBreakLineCtx& ctx) const
#endif
{
    using namespace Coplt::LayoutCalc::Texts::Compute;

    #pragma push_macro("YIELD")
    #pragma push_macro("RETURN")
    #undef YIELD
    #undef RETURN
    #ifdef _DEBUG
    std::vector<ParagraphLineSpan> result;
    #define YIELD result.push_back
    #define RETURN return result
    #else
    #define YIELD co_yield
    #define RETURN co_return
    #endif

    if (Length == 0 || ActualGlyphCount == 0) [[unlikely]] RETURN;

    const auto collapse_space = ShouldCollapseSpace(style.WhiteSpace);
    const auto wrap_line_only = style.WhiteSpace == WhiteSpace::Pre;
    const auto allow_wrap = style.TextWrap != TextWrap::NoWrap;

    const std::span cluster_map = ClusterMap(data);
    const std::span text_props = TextProps(data);
    const std::span glyph_props = GlyphProps(data);
    const std::span glyph_advances = GlyphAdvances(data);
    const std::span glyph_offsets = GlyphOffsets(data);

    // todo support WordBreak

    u32 nth_line = ctx.NthLine;
    f32 cur_offset = ctx.CurrentLineOffset;

    Cursor span_start(0, cur_offset);
    Cursor break_after(-1, 0);

    u32 c = 0;
    u16 first_cluster = cluster_map[span_start.Char];
    for (;;)
    {
        const u32 next_char = c + 1;
        if (next_char < Length)
        {
            const u16 next_cluster = cluster_map[next_char];
            if (next_cluster == first_cluster)
            {
                c = next_char;
                continue;
            }
        }
        const u16 last_cluster = cluster_map[c];

        f32 sum_size = 0;
        {
            u16 i = first_cluster;
            for (; i <= last_cluster; ++i)
            {
                const auto glyph_advance = glyph_advances[i];
                const auto& glyph_offset = glyph_offsets[i];
                sum_size += glyph_advance + glyph_offset.advanceOffset;
            }
            i = last_cluster + 1;
            for (; i < ActualGlyphCount; ++i)
            {
                const auto& glyph_prop = glyph_props[i];
                if (glyph_prop.isClusterStart) break;
                const auto glyph_advance = glyph_advances[i];
                const auto& glyph_offset = glyph_offsets[i];
                sum_size += glyph_advance + glyph_offset.advanceOffset;
            }
        }

        const f32 new_line_offset = sum_size + cur_offset;

        if (!allow_wrap) goto NEXT;

        const auto& break_info = data.m_line_breakpoints[c];
        const bool is_break_point_after = break_info.breakConditionAfter == DWRITE_BREAK_CONDITION_MUST_BREAK
            || (!wrap_line_only && break_info.breakConditionAfter == DWRITE_BREAK_CONDITION_CAN_BREAK);

        const bool should_break =
            new_line_offset > ctx.AvailableSpace
            && break_after.Char != -1 && c > break_after.Char;
        if (should_break)
        {
            const auto& text_prop = text_props[break_after.Char];
            const u16 first_glyph = cluster_map[span_start.Char];
            const u32 char_len = break_after.Char + 1 - span_start.Char;
            const u32 glyph_length = break_after.NextGlyph(cluster_map) - first_glyph;
            const f32 size = break_after.Offset - span_start.Offset;
            ctx.NthLine = nth_line;
            ctx.CurrentLineOffset = break_after.Offset;
            YIELD(
                ParagraphLineSpan{
                    .NthLine = nth_line,
                    .CharStart = span_start.Char,
                    .CharLength = char_len,
                    .GlyphStart = first_glyph,
                    .GlyphLength = glyph_length,
                    .Offset = span_start.Offset,
                    .Size = size,
                    .NeedReShape = !text_prop.canBreakShapingAfter,
                }
            );

            nth_line++;
            const f32 rem_offset = cur_offset - break_after.Offset;
            cur_offset = rem_offset + sum_size;
            COPLT_DEBUG_ASSERT(break_after.Char + 1 <= c);
            span_start = Cursor(break_after.Char + 1, 0);

            if (is_break_point_after)
            {
                break_after = Cursor(c, cur_offset);
            }
            else
            {
                break_after = Cursor(-1, 0);
            }
        }
        else
        {
            cur_offset = new_line_offset;

            if (is_break_point_after)
            {
                break_after = Cursor(c, cur_offset);
            }
        }

    NEXT:
        if (next_char >= Length)
        {
            const u16 first_glyph = cluster_map[span_start.Char];
            ctx.NthLine = nth_line;
            ctx.CurrentLineOffset = cur_offset;
            YIELD(
                ParagraphLineSpan{
                    .NthLine = nth_line,
                    .CharStart = span_start.Char,
                    .CharLength = next_char - span_start.Char,
                    .GlyphStart = first_glyph,
                    .GlyphLength = ActualGlyphCount - first_glyph,
                    .Offset = span_start.Offset,
                    .Size = cur_offset - span_start.Offset,
                    .NeedReShape = false,
                }
            );
            RETURN;
        }

        c = next_char;
        first_cluster = cluster_map[c];
    }

    #pragma pop_macro("YIELD")
    #pragma pop_macro("RETURN")
}
