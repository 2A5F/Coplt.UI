#include "TextLayout.h"

#include <span>
#include <array>
#include <deque>
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
    void* sub_doc, ITextLayout* layout, NLayoutContext* ctx, const NodeId& node,
    const LayoutInputs* inputs, LayoutOutput* outputs
)
{
    return feb(
        [&]
        {
            const auto l = static_cast<TextLayout*>(layout);
            l->Compute(sub_doc, *outputs, *inputs, CtxNodeRef(ctx, node));
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

void TextLayout::Compute(void* sub_doc, LayoutOutput& out, const LayoutInputs& inputs, CtxNodeRef node)
{
    m_node = node;
    out = Compute(sub_doc, inputs);
    m_node = {};
}

LayoutOutput TextLayout::Compute(void* sub_doc, const LayoutInputs& inputs)
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

    Size max_only{
        .Width = !clamped_size.Width.has_value() && !min_size.Width.has_value() && max_size.Width.has_value(),
        .Height = !clamped_size.Height.has_value() && !min_size.Height.has_value() && max_size.Height.has_value(),
    };

    #pragma endregion

    u32 order = 0;
    auto last_available_space = available_space.Normalize(clamped_size, min_size, max_size);
    Size<f32> size{};
    for (auto& data : m_paragraph_datas)
    {
        auto output = data.ComputeContent(sub_doc, *this, order, inputs, max_only, last_available_space, known_dimensions);
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

LayoutOutput ParagraphData::ComputeContent(
    void* sub_doc, TextLayout& layout, u32& order, const LayoutInputs& inputs, Size<bool> MaxOnly,
    const Size<AvailableSpace>& AvailableSpace, const Size<std::optional<f32>>& KnownSize
)
{
    const auto& root_style = layout.m_node.StyleData();
    const auto& paragraph = layout.m_paragraphs[m_index];
    const auto axis = ToAxis(root_style.WritingDirection);

    const bool is_max_only = MaxOnly.MainAxis(axis);
    const auto space = AvailableSpace.Or(KnownSize);
    const auto space_main = space.MainAxis(axis).value_or(std::numeric_limits<f32>::infinity());

    Size<f32> result_size{};
    auto& max_main = result_size.MainAxis(axis);
    auto& sum_cross = result_size.CrossAxis(axis);

    if (paragraph.Type == TextParagraphType::Inline)
    {
        std::vector<ParagraphSpanInlineBlockTmpData> inline_block_tmp_datas{};
        std::vector<ParagraphSpan> spans{};
        std::vector<ParagraphLine> lines{};
        if (inputs.RunMode == LayoutRunMode::ComputeSize)
        {
            spans = std::move(m_final_spans);
            lines = std::move(m_final_lines);
            spans.clear();
            lines.clear();
        }

        ParagraphLine cur_line{};

        u32 cur_nth_line = 0;
        RunBreakLineCtx ctx{
            .NthLine = 0,
            .AvailableSpace = space_main
        };

        f32 defined_line_height = Resolve(GetLineHeight(root_style), root_style.FontSize);
        for (auto& run : m_runs)
        {
            const auto& same_style = m_same_style_ranges[run.StyleRangeIndex];
            const auto scope = GetScope(same_style);
            const auto& style = scope.StyleData();
            defined_line_height = Resolve(GetLineHeight(style), style.FontSize);
            const auto allow_wrap = style.TextWrap != TextWrap::NoWrap;

            if (run.IsInlineBlock(*this))
            {
                const auto& font = m_font_ranges[run.FontRangeIndex];
                const auto items = GetItems(font.ItemStart, font.ItemLength);
                inline_block_tmp_datas.reserve(inline_block_tmp_datas.size() + items.size());
                for (const auto& item : items)
                {
                    COPLT_DEBUG_ASSERT(item.Type == TextItemType::InlineBlock);

                    const u32 inline_block_index = inline_block_tmp_datas.size();
                    if (inputs.RunMode == LayoutRunMode::PerformLayout)
                    {
                        inline_block_tmp_datas.emplace_back(GetScope(item.NodeOrParent), KnownSize);
                    }
                    else
                    {
                        inline_block_tmp_datas.emplace_back();
                    }
                    auto& ibd = inline_block_tmp_datas.back();
                    auto& sub_outputs = ibd.m_layout_output;

                    Size<::AvailableSpace> available_space = AvailableSpace;
                    ::AvailableSpace& cross_available_space = available_space.CrossAxis(axis);
                    if (cross_available_space.first == AvailableSpaceType::Definite)
                    {
                        cross_available_space.second = std::max(0.0f, cross_available_space.second - sum_cross);
                    }
                    LayoutInputs sub_inputs(
                        inputs.RunMode, inputs.SizingMode, LayoutRequestedAxis::Both,
                        Size<std::optional<f32>>(), GetParentSize(inputs), available_space
                    );

                    ComputeChildLayout(sub_doc, item.NodeOrParent, &sub_inputs, &sub_outputs);

                    const f32 main_size = GetSize(sub_outputs).MainAxis(axis);
                    const f32 cross_size = GetSize(sub_outputs).CrossAxis(axis);

                    auto cur_line_offset = ctx.CurrentLineOffset;
                    const auto new_line_offset = ctx.CurrentLineOffset + main_size;
                    if (allow_wrap && new_line_offset > ctx.AvailableSpace)
                    {
                        ctx.NthLine++;
                        cur_line_offset = 0;
                        ctx.CurrentLineOffset = main_size;

                        cur_line.NthLine = cur_nth_line;
                        cur_line.MainSize = spans.empty() ? 0 : spans.back().Offset + spans.back().Size;
                        cur_line.SpanLength = spans.size() - cur_line.SpanStart;
                        cur_line.CrossSize = cur_line.CalcSize(defined_line_height);
                        max_main = std::max(max_main, cur_line.MainSize);
                        sum_cross += cur_line.CrossSize;
                        lines.push_back(cur_line);
                        cur_line = ParagraphLine{
                            .CrossOffset = sum_cross,
                            .SpanStart = static_cast<u32>(spans.size()),
                        };
                        cur_nth_line = ctx.NthLine;
                    }
                    else
                    {
                        ctx.CurrentLineOffset = new_line_offset;
                    }

                    if (style.LineAlign == LineAlign::Baseline)
                    {
                        // todo baseline
                        cur_line.MinSize = std::max(cur_line.MinSize, cross_size);
                    }
                    else
                    {
                        cur_line.MinSize = std::max(cur_line.MinSize, cross_size);
                    }

                    spans.push_back(
                        ParagraphSpan{
                            .NthLine = cur_nth_line,
                            .Node = item.NodeOrParent,
                            .InlineBlockIndex = inline_block_index,
                            .CrossSize = cross_size,
                            .Offset = cur_line_offset,
                            .Size = main_size,
                            .Type = ParagraphSpanType::Block,
                        }
                    );
                }
            }
            else
            {
                const auto& single_line_size = run.GetLineInfo(*this);
                cur_line.Ascent = std::max(cur_line.Ascent, single_line_size.Ascent);
                cur_line.Descent = std::max(cur_line.Descent, single_line_size.Descent);
                cur_line.LineGap = std::max(cur_line.LineGap, single_line_size.LineGap);

                auto spans_iter = run.BreakLines(*this, style, ctx, single_line_size);
                for (const auto& span : spans_iter)
                {
                    #ifdef _DEBUG
                    // if (Logger().IsEnabled(LogLevel::Trace))
                    // {
                    //     std::wstring text(m_chars.data() + run.Start + span.CharStart, span.CharLength);
                    //     Logger().Log(
                    //         LogLevel::Trace,
                    //         fmt::format(
                    //             L"line {:<6} {:>15.7f} .. {:<15.7f} {:>15.7f} ; {} {:>6}..{:<6} \"{}\"",
                    //             span.NthLine, span.Offset, span.Offset + span.Size, span.Size,
                    //             ToStr16Pad(span.Type), run.Start + span.CharStart, run.Start + span.CharStart + span.CharLength, text
                    //         )
                    //     );
                    // }
                    #endif

                    if (span.NthLine != cur_nth_line)
                    {
                        cur_line.NthLine = cur_nth_line;
                        cur_line.MainSize = spans.empty() ? 0 : spans.back().Offset + spans.back().Size;
                        cur_line.SpanLength = spans.size() - cur_line.SpanStart;
                        cur_line.CrossSize = cur_line.CalcSize(defined_line_height);
                        max_main = std::max(max_main, cur_line.MainSize);
                        sum_cross += cur_line.CrossSize;
                        lines.push_back(cur_line);
                        cur_line = ParagraphLine{
                            .CrossOffset = sum_cross,
                            .SpanStart = static_cast<u32>(spans.size()),
                        };
                        cur_nth_line = span.NthLine;
                    }
                    spans.push_back(span);
                }
            }
        }

        if (spans.size() != cur_line.SpanStart)
        {
            cur_line.NthLine = cur_nth_line;
            cur_line.MainSize = spans.empty() ? 0 : spans.back().Offset + spans.back().Size;
            cur_line.SpanLength = spans.size() - cur_line.SpanStart;
            cur_line.CrossSize = cur_line.CalcSize(defined_line_height);
            max_main = std::max(max_main, cur_line.MainSize);
            sum_cross += cur_line.CrossSize;
            lines.push_back(cur_line);
        }

        if (inputs.RunMode == LayoutRunMode::PerformLayout)
        {
            if (!std::isinf(space_main) && !is_max_only) max_main = space_main;
            switch (root_style.TextAlign)
            {
            case TextAlign::End:
                for (auto& line : lines)
                {
                    line.MainOffset = max_main - line.MainSize;
                }
                break;

            case TextAlign::Center:
                for (auto& line : lines)
                {
                    line.MainOffset = (max_main - line.MainSize) / 2;
                }
                break;
            case TextAlign::Start:
            default:
                break;
            }
            for (auto& line : lines)
            {
                for (auto& span : std::span(spans.data() + line.SpanStart, line.SpanLength))
                {
                    if (span.Type == ParagraphSpanType::Block)
                    {
                        const CtxNodeRef node = GetScope(span.Node);
                        const auto& data = inline_block_tmp_datas[span.InlineBlockIndex];
                        // todo
                        node.CommonData().UnRoundedLayout = LayoutData{
                            .Order = order++,
                            .LocationX = 0,
                            .LocationY = 0,
                            .Width = data.m_layout_output.Width,
                            .Height = data.m_layout_output.Height,
                            .ContentWidth = data.m_layout_output.ContentWidth,
                            .ContentHeight = data.m_layout_output.ContentHeight,
                            .ScrollXSize = 0, // todo scroll size
                            .ScrollYSize = 0,
                            .BorderTopSize = 0,
                            .BorderRightSize = 0,
                            .BorderBottomSize = 0,
                            .BorderLeftSize = 0,
                            .PaddingTopSize = 0,
                            .PaddingRightSize = 0,
                            .PaddingBottomSize = 0,
                            .PaddingLeftSize = 0,
                            .MarginTopSize = 0,
                            .MarginRightSize = 0,
                            .MarginBottomSize = 0,
                            .MarginLeftSize = 0,
                        };
                    }
                    else
                    {
                        // todo save text span layout
                    }
                }
            }
            m_final_spans = std::move(spans);
            m_final_lines = std::move(lines);
        }
    }
    else
    {
        //not support float layout yet
        // todo block
    }

    return LayoutOutputFromOuterSize(result_size);
}

std::span<const char16> Run::Chars(const ParagraphData& data) const
{
    return std::span(data.m_chars.data() + Start, Length);
}

std::span<const CharMeta> Run::CharMetas(const ParagraphData& data) const
{
    return std::span(data.m_char_metas.data() + Start, Length);
}

std::span<const DWRITE_LINE_BREAKPOINT> Run::LineBreakpoints(const ParagraphData& data) const
{
    return std::span(data.m_line_breakpoints.data() + Start, Length);
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
    LineInfo.MinSize = LineInfo.Ascent + LineInfo.Descent;

    return LineInfo;
}

namespace Coplt::LayoutCalc::Texts::Compute
{
    struct Cursor
    {
        i32 Char;
        f32 Offset;

        COPLT_FORCE_INLINE
        Cursor() = default;

        COPLT_FORCE_INLINE
        explicit Cursor(const i32 c, const f32 o)
            : Char(c), Offset(o)
        {
        }
    };

    COPLT_FORCE_INLINE
    u16 NextGlyph(const u32 next_char, const std::span<const u16> cluster_map)
    {
        if (next_char == 0) return 0;
        if (next_char >= cluster_map.size()) return cluster_map[next_char - 1] + 1;
        return cluster_map[next_char];
    }

    COPLT_FORCE_INLINE
    u16 GlyphLength(const u32 next_char, const std::span<const u16> cluster_map, const u16 first_glyh)
    {
        if (next_char == 0) return 0;
        return NextGlyph(next_char, cluster_map) - first_glyh;
    }

    COPLT_FORCE_INLINE
    i32 StepCluster(const Run& self, const std::span<const u16> cluster_map, const u16 first_cluster, i32& cur_char)
    {
        for (;;)
        {
            const i32 next_char = cur_char + 1;
            if (next_char < self.Length)
            {
                const u16 next_cluster = cluster_map[next_char];
                if (next_cluster == first_cluster)
                {
                    cur_char = next_char;
                    continue;
                }
            }
            return next_char;
        }
    }

    COPLT_FORCE_INLINE
    f32 SumSize(
        const Run& self,
        const std::span<const DWRITE_SHAPING_GLYPH_PROPERTIES> glyph_props,
        const std::span<const f32> glyph_advances,
        const std::span<const DWRITE_GLYPH_OFFSET> glyph_offsets,
        const u16 first_cluster, const u16 last_cluster
    )
    {
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
            for (; i < self.ActualGlyphCount; ++i)
            {
                const auto& glyph_prop = glyph_props[i];
                if (glyph_prop.isClusterStart) break;
                const auto glyph_advance = glyph_advances[i];
                const auto& glyph_offset = glyph_offsets[i];
                sum_size += glyph_advance + glyph_offset.advanceOffset;
            }
        }
        return sum_size;
    }

    COPLT_FORCE_INLINE
    ParagraphSpanType CheckParagraphSpanType(const char16 the_char, const RawCharType char_raw, const bool allow_newline)
    {
        switch (char_raw)
        {
        case RawCharType::LF:
        case RawCharType::CR:
            return allow_newline ? ParagraphSpanType::NewLine : ParagraphSpanType::Space;
        case RawCharType::HT:
        case RawCharType::VT:
        case RawCharType::AsIs:
        default:
            return the_char == 0x0020 ? ParagraphSpanType::Space : ParagraphSpanType::Common;
        }
    }
}

#ifdef _DEBUG
std::vector<ParagraphSpan> Run::BreakLines(const ParagraphData& data, const StyleData& style, RunBreakLineCtx& ctx, const ParagraphLineInfo& line_info) const
#else
std::generator<ParagraphSpan> Run::BreakLines(const ParagraphData& data, const StyleData& style, RunBreakLineCtx& ctx, const ParagraphLineInfo& line_info) const
#endif
{
    using namespace Coplt::LayoutCalc::Texts::Compute;

    #pragma push_macro("YIELD")
    #pragma push_macro("RETURN")
    #undef YIELD
    #undef RETURN
    #ifdef _DEBUG
    std::vector<ParagraphSpan> result;
    #define YIELD result.push_back
    #define RETURN return result
    #else
    #define YIELD co_yield
    #define RETURN co_return
    #endif

    if (Length == 0 || ActualGlyphCount == 0) [[unlikely]] RETURN;

    const auto allow_newline = HasFlags(style.WrapFlags, WrapFlags::AllowNewLine);
    const auto wrap_in_space = HasFlags(style.WrapFlags, WrapFlags::WrapInSpace);
    const auto allow_wrap = style.TextWrap != TextWrap::NoWrap;

    const std::span chars = Chars(data);
    const std::span char_metas = CharMetas(data);
    const std::span line_breakpoints = LineBreakpoints(data);
    const std::span cluster_map = ClusterMap(data);
    const std::span text_props = TextProps(data);
    const std::span glyph_props = GlyphProps(data);
    const std::span glyph_advances = GlyphAdvances(data);
    const std::span glyph_offsets = GlyphOffsets(data);

    // todo support WordBreak

    u32 nth_line = ctx.NthLine;
    f32 cur_offset = ctx.CurrentLineOffset;

    if (allow_wrap)
    {
        f32 last_sub_span_offset = 0;
        Cursor span_start(0, cur_offset);
        std::optional<Cursor> break_after = cur_offset == 0 ? std::nullopt : std::optional(Cursor(-1, cur_offset));
        auto last_type = static_cast<ParagraphSpanType>(-1);
        struct SubSpan
        {
            i32 EndChar; // not include
            f32 Size;
            ParagraphSpanType Type;
        };
        std::deque<SubSpan> sub_spans{};

        i32 c = span_start.Char;
        for (;;)
        {
            const char16 the_char = chars[c];
            const RawCharType char_raw = char_metas[c].RawType;

            const i32 start_char = c;
            const u16 first_cluster = cluster_map[c];
            const i32 next_char = StepCluster(*this, cluster_map, first_cluster, c);
            const u16 last_cluster = cluster_map[c];

            const f32 sum_size = SumSize(*this, glyph_props, glyph_advances, glyph_offsets, first_cluster, last_cluster);
            const f32 new_line_offset = sum_size + cur_offset;

            const ParagraphSpanType type = CheckParagraphSpanType(the_char, char_raw, allow_newline);
            if (last_type == static_cast<ParagraphSpanType>(-1)) last_type = type;
            else if (last_type != type)
            {
                const f32 size = sub_spans.empty() ? cur_offset : cur_offset - last_sub_span_offset;
                sub_spans.push_back(
                    SubSpan{
                        .EndChar = start_char,
                        .Size = size,
                        .Type = last_type,
                    }
                );
                last_type = type;
                last_sub_span_offset = cur_offset;
            }

            const auto& break_info = line_breakpoints[c];
            const bool is_break_point_after =
                (wrap_in_space && the_char == 0x0020)
                || break_info.breakConditionAfter == DWRITE_BREAK_CONDITION_MUST_BREAK
                || break_info.breakConditionAfter == DWRITE_BREAK_CONDITION_CAN_BREAK;

            if (allow_newline && char_raw == RawCharType::LF)
            {
                for (const auto& sub_span : sub_spans)
                {
                    const u16 first_glyph = cluster_map[span_start.Char];
                    const u32 char_len = sub_span.EndChar - span_start.Char;
                    const u32 glyph_length = GlyphLength(sub_span.EndChar, cluster_map, first_glyph);
                    const f32 size = sub_span.Size;
                    const f32 next_offset = span_start.Offset + sub_span.Size;
                    ctx.NthLine = nth_line;
                    ctx.CurrentLineOffset = next_offset;
                    YIELD(
                        ParagraphSpan{
                            .NthLine = nth_line,
                            .CharStart = static_cast<u32>(span_start.Char),
                            .CharLength = char_len,
                            .GlyphStart = first_glyph,
                            .GlyphLength = glyph_length,
                            .Ascent = line_info.Ascent,
                            .Descent = line_info.Descent,
                            .Offset = span_start.Offset,
                            .Size = size,
                            .Type = sub_span.Type,
                            .NeedReShape = false,
                        }
                    );
                    span_start = Cursor(sub_span.EndChar, next_offset);
                }
                sub_spans.clear();
                {
                    cur_offset = new_line_offset;
                    const u16 first_glyph = cluster_map[span_start.Char];
                    const u32 char_len = next_char - span_start.Char;
                    const u32 glyph_length = GlyphLength(next_char, cluster_map, first_glyph);
                    const f32 size = cur_offset - span_start.Offset;
                    ctx.NthLine = nth_line;
                    ctx.CurrentLineOffset = cur_offset;
                    YIELD(
                        ParagraphSpan{
                            .NthLine = nth_line,
                            .CharStart = static_cast<u32>(span_start.Char),
                            .CharLength = char_len,
                            .GlyphStart = first_glyph,
                            .GlyphLength = glyph_length,
                            .Ascent = line_info.Ascent,
                            .Descent = line_info.Descent,
                            .Offset = span_start.Offset,
                            .Size = size,
                            .Type = ParagraphSpanType::NewLine,
                            .NeedReShape = false,
                        }
                    );
                }

                nth_line++;
                last_sub_span_offset = cur_offset = 0;
                span_start = Cursor(next_char, 0);
                break_after = std::nullopt;
                last_type = static_cast<ParagraphSpanType>(-1);
            }
            else if (new_line_offset > ctx.AvailableSpace && break_after.has_value() && static_cast<i32>(c) > break_after.value().Char)
            {
                u32 i = 0;
                for (; i < sub_spans.size(); ++i)
                {
                    const auto& sub_span = sub_spans[i];
                    if (sub_span.EndChar > break_after.value().Char) break;
                    const u16 first_glyph = cluster_map[span_start.Char];
                    const u32 char_len = sub_span.EndChar - span_start.Char;
                    const u32 glyph_length = GlyphLength(sub_span.EndChar, cluster_map, first_glyph);
                    const f32 size = sub_span.Size;
                    const f32 next_offset = span_start.Offset + sub_span.Size;
                    ctx.NthLine = nth_line;
                    ctx.CurrentLineOffset = next_offset;
                    YIELD(
                        ParagraphSpan{
                            .NthLine = nth_line,
                            .CharStart = static_cast<u32>(span_start.Char),
                            .CharLength = char_len,
                            .GlyphStart = first_glyph,
                            .GlyphLength = glyph_length,
                            .Ascent = line_info.Ascent,
                            .Descent = line_info.Descent,
                            .Offset = span_start.Offset,
                            .Size = size,
                            .Type = sub_span.Type,
                            .NeedReShape = false,
                        }
                    );
                    span_start = Cursor(sub_span.EndChar, next_offset);
                }
                sub_spans.erase(sub_spans.begin(), sub_spans.begin() + i);

                if (break_after.value().Char == -1)
                {
                    nth_line++;
                    const f32 rem_offset = cur_offset - break_after.value().Offset;
                    last_sub_span_offset = cur_offset = rem_offset + sum_size;
                    span_start.Offset = 0;
                }
                else
                {
                    const auto break_span_type = sub_spans.empty() ? type : sub_spans.back().Type;
                    const auto& text_prop = text_props[break_after.value().Char];
                    const u16 first_glyph = cluster_map[span_start.Char];
                    const u32 break_next_char = break_after.value().Char + 1;
                    const u32 char_len = break_next_char - span_start.Char;
                    const u32 glyph_length = GlyphLength(break_next_char, cluster_map, first_glyph);
                    const f32 size = break_after.value().Offset - span_start.Offset;
                    ctx.NthLine = nth_line;
                    ctx.CurrentLineOffset = break_after.value().Offset;
                    YIELD(
                        ParagraphSpan{
                            .NthLine = nth_line,
                            .CharStart = static_cast<u32>(span_start.Char),
                            .CharLength = char_len,
                            .GlyphStart = first_glyph,
                            .GlyphLength = glyph_length,
                            .Ascent = line_info.Ascent,
                            .Descent = line_info.Descent,
                            .Offset = span_start.Offset,
                            .Size = size,
                            .Type = break_span_type,
                            .NeedReShape = !text_prop.canBreakShapingAfter,
                        }
                    );

                    nth_line++;
                    if (!sub_spans.empty())
                    {
                        auto& sub_span = sub_spans.back();
                        if (sub_span.EndChar == break_next_char)
                        {
                            sub_spans.pop_front();
                        }
                        else
                        {
                            const f32 next_offset = span_start.Offset + sub_span.Size;
                            sub_span.Size = next_offset - break_after.value().Offset;
                        }
                    }
                    const f32 rem_offset = cur_offset - break_after.value().Offset;
                    last_sub_span_offset = cur_offset = rem_offset + sum_size;
                    span_start = Cursor(break_next_char, 0);
                }

                if (is_break_point_after)
                {
                    break_after = Cursor(c, cur_offset);
                }
                else
                {
                    break_after = std::nullopt;
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

            if (next_char >= Length)
            {
                for (const auto& sub_span : sub_spans)
                {
                    const u16 first_glyph = cluster_map[span_start.Char];
                    const u32 char_len = sub_span.EndChar - span_start.Char;
                    const u32 glyph_length = GlyphLength(sub_span.EndChar, cluster_map, first_glyph);
                    const f32 size = sub_span.Size;
                    const f32 next_offset = span_start.Offset + sub_span.Size;
                    ctx.NthLine = nth_line;
                    ctx.CurrentLineOffset = next_offset;
                    YIELD(
                        ParagraphSpan{
                            .NthLine = nth_line,
                            .CharStart = static_cast<u32>(span_start.Char),
                            .CharLength = char_len,
                            .GlyphStart = first_glyph,
                            .GlyphLength = glyph_length,
                            .Ascent = line_info.Ascent,
                            .Descent = line_info.Descent,
                            .Offset = span_start.Offset,
                            .Size = size,
                            .Type = sub_span.Type,
                            .NeedReShape = false,
                        }
                    );
                    span_start = Cursor(sub_span.EndChar, next_offset);
                }
                {
                    const u16 first_glyph = cluster_map[span_start.Char];
                    const u32 char_len = next_char - span_start.Char;
                    const u32 glyph_length = GlyphLength(next_char, cluster_map, first_glyph);
                    const f32 size = cur_offset - span_start.Offset;
                    ctx.NthLine = nth_line;
                    ctx.CurrentLineOffset = cur_offset;
                    YIELD(
                        ParagraphSpan{
                            .NthLine = nth_line,
                            .CharStart = static_cast<u32>(span_start.Char),
                            .CharLength = char_len,
                            .GlyphStart = first_glyph,
                            .GlyphLength = glyph_length,
                            .Ascent = line_info.Ascent,
                            .Descent = line_info.Descent,
                            .Offset = span_start.Offset,
                            .Size = size,
                            .Type = type,
                            .NeedReShape = false,
                        }
                    );
                }
                RETURN;
            }

            c = next_char;
        }
    }
    else
    {
        Cursor span_start(0, cur_offset);
        auto last_type = static_cast<ParagraphSpanType>(-1);
        i32 c = span_start.Char;
        for (;;)
        {
            const char16 the_char = chars[c];
            const RawCharType char_raw = char_metas[c].RawType;

            const i32 start_char = c;
            const u16 first_cluster = cluster_map[c];
            const i32 next_char = StepCluster(*this, cluster_map, first_cluster, c);
            const u16 last_cluster = cluster_map[c];

            const f32 sum_size = SumSize(*this, glyph_props, glyph_advances, glyph_offsets, first_cluster, last_cluster);
            const f32 new_line_offset = sum_size + cur_offset;

            const ParagraphSpanType type = CheckParagraphSpanType(the_char, char_raw, allow_newline);
            if (last_type == static_cast<ParagraphSpanType>(-1)) last_type = type;
            else if (last_type != type)
            {
                const u32 char_len = start_char - span_start.Char;
                COPLT_DEBUG_ASSERT(char_len > 0);
                const u16 first_glyph = cluster_map[span_start.Char];
                const u32 glyph_length = GlyphLength(start_char, cluster_map, first_glyph);
                const f32 size = cur_offset - span_start.Offset;
                ctx.CurrentLineOffset = cur_offset;
                YIELD(
                    ParagraphSpan{
                        .NthLine = nth_line,
                        .CharStart = static_cast<u32>(span_start.Char),
                        .CharLength = char_len,
                        .GlyphStart = first_glyph,
                        .GlyphLength = glyph_length,
                        .Ascent = line_info.Ascent,
                        .Descent = line_info.Descent,
                        .Offset = span_start.Offset,
                        .Size = size,
                        .Type = last_type,
                        .NeedReShape = false,
                    }
                );

                span_start = Cursor(start_char, cur_offset);
                last_type = type;
            }

            cur_offset = new_line_offset;
            if (next_char >= Length)
            {
                const u32 char_len = next_char - span_start.Char;
                COPLT_DEBUG_ASSERT(char_len > 0);
                const u16 first_glyph = cluster_map[span_start.Char];
                const u32 glyph_length = GlyphLength(next_char, cluster_map, first_glyph);
                const f32 size = cur_offset - span_start.Offset;
                ctx.CurrentLineOffset = cur_offset;
                YIELD(
                    ParagraphSpan{
                        .NthLine = nth_line,
                        .CharStart = static_cast<u32>(span_start.Char),
                        .CharLength = char_len,
                        .GlyphStart = first_glyph,
                        .GlyphLength = glyph_length,
                        .Ascent = line_info.Ascent,
                        .Descent = line_info.Descent,
                        .Offset = span_start.Offset,
                        .Size = size,
                        .Type = last_type,
                        .NeedReShape = false,
                    }
                );
                RETURN;
            }
            c = next_char;
        }
    }

    #pragma pop_macro("YIELD")
    #pragma pop_macro("RETURN")
}
