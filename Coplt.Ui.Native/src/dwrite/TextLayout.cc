#include "TextLayout.h"

#include <span>
#include <print>

#include <icu.h>

#include "../Algorithm.h"
#include "../Text.h"
#include "../Layout.h"
#include "Layout.h"
#include "Error.h"
#include "BaseFontFallback.h"
#include "Utils.h"
#include "FontFace.h"

using namespace Coplt::LayoutCalc::Texts;

ParagraphData::ParagraphData(TextLayout* text_layout)
    : m_text_layout(text_layout)
{
}

const LoggerData& ParagraphData::Logger() const
{
    return m_layout->m_lib->m_logger;
}

void ParagraphData::ReBuild()
{
    if (!m_src) m_src = Rc(new TextAnalysisSource(this));
    if (!m_sink) m_sink = Rc(new TextAnalysisSink(this));
    m_chars.clear();
    m_char_metas.clear();
    m_script_ranges.clear();
    m_bidi_ranges.clear();
    m_line_breakpoints.clear();
    m_font_ranges.clear();
    m_font_ranges_tmp.clear();
    m_same_style_ranges.clear();
    m_runs.clear();

    m_cluster_map.clear();
    m_text_props.clear();
    m_glyph_indices.clear();
    m_glyph_props.clear();
    m_glyph_advances.clear();
    m_glyph_offsets.clear();

    m_cache.Clear();
    m_final_spans.clear();
    m_final_lines.clear();
}

std::vector<Paragraph>& ParagraphData::GetTextLayoutParagraphs() const
{
    return m_text_layout->m_paragraphs;
}

Paragraph& ParagraphData::GetParagraph() const
{
    return GetTextLayoutParagraphs()[m_index];
}

std::span<TextItem> ParagraphData::GetItems() const
{
    const auto& paragraph = GetParagraph();
    return GetItems(paragraph.ItemStart, paragraph.ItemLength);
}

std::span<TextItem> ParagraphData::GetItems(const u32 Start, const u32 Length) const
{
    return std::span(m_text_layout->m_items.data() + Start, Length);
}

CtxNodeRef ParagraphData::GetScope(const TextItem& item) const
{
    const auto root_scope = m_text_layout->m_node;
    return
        item.Scope == -1
        ? root_scope
        : CtxNodeRef(root_scope.ctx, m_text_layout->m_scopes[item.Scope]);
}

CtxNodeRef ParagraphData::GetScope(const TextScopeRange& range) const
{
    const auto root_scope = m_text_layout->m_node;
    return
        range.Scope == -1
        ? root_scope
        : CtxNodeRef(root_scope.ctx, m_text_layout->m_scopes[range.Scope]);
}

CtxNodeRef ParagraphData::GetScope(const SameStyleRange& range) const
{
    const auto root_scope = m_text_layout->m_node;
    return
        range.FirstScope == -1
        ? root_scope
        : CtxNodeRef(root_scope.ctx, m_text_layout->m_scopes[range.FirstScope]);
}

void TextLayout::ReBuild(Layout* layout, CtxNodeRef node)
{
    m_node = node;
    m_layout = layout;
    m_paragraph_datas.resize(m_paragraphs.size(), ParagraphData(this));
    for (int i = 0; i < m_paragraphs.size(); ++i)
    {
        auto& paragraph = m_paragraphs[i];
        if (paragraph.Type == TextParagraphType::Block) continue;
        auto& data = m_paragraph_datas[i];
        data.m_index = i;
        data.m_layout = layout;
        data.ReBuild();
        data.CollectChars();
        if (const auto hr = layout->m_text_analyzer->AnalyzeScript(
            data.m_src.get(), 0, paragraph.LogicTextLength, data.m_sink.get()
        ); FAILED(hr))
            throw ComException(hr, "Failed to analyze script");
        if (const auto hr = layout->m_text_analyzer->AnalyzeBidi(
            data.m_src.get(), 0, paragraph.LogicTextLength, data.m_sink.get()
        ); FAILED(hr))
            throw ComException(hr, "Failed to analyze bidi");
        if (const auto hr = layout->m_text_analyzer->AnalyzeLineBreakpoints(
            data.m_src.get(), 0, paragraph.LogicTextLength, data.m_sink.get()
        ); FAILED(hr))
            throw ComException(hr, "Failed to analyze LineBreakpoints");
        data.AnalyzeFonts();
        data.AnalyzeStyles();
        data.CollectRuns();
        data.AnalyzeGlyphsFirst();
        // data.AnalyzeGlyphsCarets();
    }
    m_node = {};
}

const Rc<DWriteFontFace>& TextLayout::GetFallbackUndefFont()
{
    if (m_fallback_undef_font) return m_fallback_undef_font;

    if (!m_one_space_analysis_source) m_one_space_analysis_source = Rc(new OneSpaceTextAnalysisSource());

    const auto fm = m_node.ctx->font_manager;
    const auto& system_font_fallback = m_layout->m_system_font_fallback;

    u32 mapped_length{};
    Rc<IDWriteFontFace5> mapped_font{};
    f32 scale{};

    if (const auto hr = system_font_fallback->MapCharacters(
        m_one_space_analysis_source.get(), 0, 1,
        nullptr, nullptr,
        nullptr, 0,
        &mapped_length, &scale,
        mapped_font.put()
    ); FAILED(hr) || !mapped_font)
        throw ComException(hr, "Failed to map characters");

    m_fallback_undef_font = DWriteFontFace::Get(fm, mapped_font);
    return m_fallback_undef_font;
}

void ParagraphData::CollectChars()
{
    const auto items = GetItems();
    const auto& paragraph = GetParagraph();
    m_chars.reserve(paragraph.LogicTextLength);
    m_char_metas.resize(paragraph.LogicTextLength);
    for (const auto& item : items)
    {
        const auto text = GetText(m_text_layout->m_node.ctx, &item);
        const u32 len = item.LogicTextLength;
        const u32 start = m_chars.size();
        m_chars.insert(m_chars.end(), text, text + len);
        const auto str = m_chars.data();
        for (u32 i = start; i < m_chars.size();)
        {
            const u32 li = i;
            UChar32 c;
            U16_NEXT(str, i, len, c);
            switch (c)
            {
            case 0x0009:
                str[li] = 0x0020;
                m_char_metas[li].RawType = RawCharType::HT;
                break;
            case 0x000B:
                str[li] = 0x0020;
                m_char_metas[li].RawType = RawCharType::VT;
                break;
            case 0x000A:
                str[li] = 0x0020;
                m_char_metas[li].RawType = RawCharType::LF;
                break;
            case 0x000D:
                str[li] = 0x0020;
                m_char_metas[li].RawType = RawCharType::CR;
                break;
            default: break;
            }
        }
    }
}

void ParagraphData::AnalyzeFonts()
{
    const auto fm = m_text_layout->m_node.ctx->font_manager;

    const auto& system_font_fallback = m_layout->m_system_font_fallback;

    const auto& items = GetItems();
    u32 item_start = 0;
    u32 item_length = 0;
    u32 logic_text_start = 0;
    u32 logic_text_length = 0;
    IDWriteFontFallback1* cur_fall_back{};
    auto cur_item_type = TextItemType::Block;
    auto cur_font_weight = FontWeight::Normal;
    auto cur_font_width = 1.0f;
    auto cur_font_italic = false;
    auto cur_font_oblique = 1.0f;
    const auto add_range = [&]
    {
        if (cur_item_type == TextItemType::InlineBlock)
        {
            m_font_ranges.push_back(
                FontRange{
                    .Start = logic_text_start,
                    .Length = logic_text_length,
                    .Font = nullptr,
                    .ItemStart = item_start,
                    .ItemLength = item_length,
                    .IsInlineBlock = true,
                }
            );
            return;
        }

        const DWRITE_FONT_AXIS_VALUE axis_values[] = {
            DWRITE_FONT_AXIS_VALUE{
                .axisTag = DWRITE_FONT_AXIS_TAG_WEIGHT,
                .value = static_cast<f32>(cur_font_weight),
            },
            DWRITE_FONT_AXIS_VALUE{
                .axisTag = DWRITE_FONT_AXIS_TAG_WIDTH,
                .value = cur_font_width * 100,
            },
            DWRITE_FONT_AXIS_VALUE{
                .axisTag = DWRITE_FONT_AXIS_TAG_ITALIC,
                .value = cur_font_italic ? 1.0f : 0.0f,
            },
            DWRITE_FONT_AXIS_VALUE{
                .axisTag = DWRITE_FONT_AXIS_TAG_SLANT,
                .value = cur_font_italic ? cur_font_oblique : 0.0f,
            },
        };

        auto text_start = logic_text_start;
        auto text_length = logic_text_length;

        for (;;)
        {
            u32 mapped_length{};
            Rc<IDWriteFontFace5> mapped_font{};
            f32 scale{};

            if (const auto hr = cur_fall_back->MapCharacters(
                m_src.get(), text_start, text_length,
                nullptr, nullptr,
                axis_values, std::size(axis_values),
                &mapped_length, &scale,
                mapped_font.put()
            ); FAILED(hr))
                throw ComException(hr, "Failed to map characters");

            // // not find, retry use system font fallback
            // if (mapped_font == nullptr && cur_fall_back != system_font_fallback)
            // {
            //     if (const auto hr = system_font_fallback->MapCharacters(
            //         m_src.get(), text_start, text_length,
            //         nullptr, nullptr,
            //         axis_values, std::size(axis_values),
            //         &mapped_length, &scale,
            //         mapped_font.put()
            //     ); FAILED(hr))
            //         throw ComException(hr, "Failed to map characters");
            // }

            #ifdef _DEBUG
            // if (mapped_font && Logger().IsEnabled(LogLevel::Trace))
            // {
            //     u32 len{};
            //     const char16* local{};
            //     m_src->GetLocaleName(text_start, &len, &local);
            //
            //     const auto name = GetFamilyNames(mapped_font);
            //     Logger().Log(
            //         LogLevel::Trace,
            //         fmt::format(L"{}; {} :: {}", ((usize)mapped_font.get()), local, name)
            //     );
            // }
            #endif

            m_font_ranges.push_back(
                FontRange{
                    .Start = text_start,
                    .Length = mapped_length,
                    .Font = mapped_font ? DWriteFontFace::Get(fm, mapped_font) : nullptr,
                    .ItemStart = 0,
                    .ItemLength = 0,
                    .IsInlineBlock = false,
                }
            );

            if (mapped_length >= text_length) break;
            text_start += mapped_length;
            text_length -= mapped_length;
        }
    };
    for (u32 i = 0; i < items.size(); ++i)
    {
        const auto& item = items[i];
        const auto scope = GetScope(item);
        const auto& style = scope.StyleData();
        const auto scope_fallback =
            style.FontFallback
            ? static_cast<BaseFontFallback*>(style.FontFallback)->m_fallback.get()
            : system_font_fallback.get();
        if (item.Type == TextItemType::Text)
        {
            const auto font_oblique = std::clamp(style.FontOblique, -90.0f, 90.0f);
            if (
                cur_item_type == item.Type
                && cur_fall_back == scope_fallback
                && cur_font_weight == style.FontWeight
                && cur_font_width == style.FontWidth.Width
                && cur_font_italic == style.FontItalic
                && cur_font_oblique == font_oblique
            )
            {
                if (logic_text_length == 0)
                {
                    item_start = i;
                    logic_text_start = item.LogicTextStart;
                }
                item_length++;
                logic_text_length += item.LogicTextLength;
                continue;
            }
            if (logic_text_length != 0) add_range();
            item_start = i;
            item_length = 1;
            logic_text_start = item.LogicTextStart;
            logic_text_length = item.LogicTextLength;
            cur_item_type = item.Type;
            cur_fall_back = scope_fallback;
            cur_font_weight = style.FontWeight;
            cur_font_width = style.FontWidth.Width;
            cur_font_italic = style.FontItalic;
            cur_font_oblique = font_oblique;
        }
        else
        {
            if (cur_item_type == item.Type)
            {
                if (logic_text_length == 0)
                {
                    item_start = i;
                    logic_text_start = item.LogicTextStart;
                }
                item_length++;
                logic_text_length += item.LogicTextLength;
                continue;
            }
            if (logic_text_length != 0) add_range();
            item_start = i;
            item_length = 1;
            logic_text_start = item.LogicTextStart;
            logic_text_length = item.LogicTextLength;
            cur_item_type = item.Type;
            cur_fall_back = nullptr;
        }
    }
    if (logic_text_length != 0) add_range();

    // Forward merge
    u32 pre_has_font_text = -1;
    for (u32 i = 0; i < m_font_ranges.size(); ++i)
    {
        auto& range = m_font_ranges[i];
        if (range.IsInlineBlock) pre_has_font_text = -1;
        else if (range.Font)
        {
            if (pre_has_font_text == -1) pre_has_font_text = i;
            else
            {
                auto& pre_range = m_font_ranges[pre_has_font_text];
                if (pre_range.Font.get() != range.Font.get()) pre_has_font_text = i;
                else
                {
                    COPLT_DEBUG_ASSERT(pre_range.Start + pre_range.Length == range.Start);
                    pre_range.Length += range.Length;
                    range.Length = 0;
                }
            }
        }
        else if (pre_has_font_text != -1)
        {
            auto& pre_range = m_font_ranges[pre_has_font_text];
            COPLT_DEBUG_ASSERT(pre_range.Start + pre_range.Length == range.Start);
            pre_range.Length += range.Length;
            range.Length = 0;
        }
    }

    // Reverse merge
    pre_has_font_text = -1;
    for (u32 n = m_font_ranges.size(); n > 0; --n)
    {
        const auto i = n - 1;
        auto& range = m_font_ranges[i];
        if (range.IsInlineBlock || range.Length == 0) pre_has_font_text = -1;
        else if (range.Font)
        {
            if (pre_has_font_text == -1) pre_has_font_text = i;
            else
            {
                auto& pre_range = m_font_ranges[pre_has_font_text];
                if (pre_range.Font.get() != range.Font.get()) pre_has_font_text = i;
                else
                {
                    COPLT_DEBUG_ASSERT(range.Start + range.Length == pre_range.Start);
                    pre_range.Start = range.Start;
                    pre_range.Length += range.Length;
                    range.Length = 0;
                }
            }
        }
        else if (pre_has_font_text != -1)
        {
            auto& pre_range = m_font_ranges[pre_has_font_text];
            COPLT_DEBUG_ASSERT(range.Start + range.Length == pre_range.Start);
            pre_range.Start = range.Start;
            pre_range.Length += range.Length;
            range.Length = 0;
        }
    }

    // Finally, make sure all text range have fonts, and remove empty range
    for (u32 i = 0; i < m_font_ranges.size(); ++i)
    {
        auto& range = m_font_ranges[i];
        if (range.IsInlineBlock) m_font_ranges_tmp.push_back(std::move(range));
        else if (range.Length == 0) continue;
        else if (range.Font) m_font_ranges_tmp.push_back(std::move(range));
        else
        {
            range.Font = m_text_layout->GetFallbackUndefFont();
            m_font_ranges_tmp.push_back(std::move(range));
        }
    }
    std::swap(m_font_ranges, m_font_ranges_tmp);
    m_font_ranges_tmp.clear();
}

void ParagraphData::AnalyzeStyles()
{
    const auto& paragraph = GetParagraph();
    const auto& scopes = paragraph.ScopeRanges;
    u32 logic_text_start = 0;
    u32 logic_text_length = 0;
    u32 first_scope = -1;
    f32 font_size = 0;
    TextOrientation text_orientation{};
    const auto add_range = [&]
    {
        m_same_style_ranges.push_back(
            SameStyleRange{
                .Start = logic_text_start,
                .Length = logic_text_length,
                .FirstScope = first_scope,
            }
        );
    };
    for (const auto& scope : scopes)
    {
        const auto node = GetScope(scope);
        const auto& style = node.StyleData();
        if (
            first_scope == -1
            || !IsZeroLength(style.InsertTop, style.InsertTopValue)
            || !IsZeroLength(style.InsertRight, style.InsertRightValue)
            || !IsZeroLength(style.InsertBottom, style.InsertBottomValue)
            || !IsZeroLength(style.InsertLeft, style.InsertLeftValue)
            || !IsZeroLength(style.MarginTop, style.MarginTopValue)
            || !IsZeroLength(style.MarginRight, style.MarginRightValue)
            || !IsZeroLength(style.MarginBottom, style.MarginBottomValue)
            || !IsZeroLength(style.MarginLeft, style.MarginLeftValue)
            || !IsZeroLength(style.PaddingTop, style.PaddingTopValue)
            || !IsZeroLength(style.PaddingRight, style.PaddingRightValue)
            || !IsZeroLength(style.PaddingBottom, style.PaddingBottomValue)
            || !IsZeroLength(style.PaddingLeft, style.PaddingLeftValue)
            || !IsZeroLength(style.BorderTop, style.BorderTopValue)
            || !IsZeroLength(style.BorderRight, style.BorderRightValue)
            || !IsZeroLength(style.BorderBottom, style.BorderBottomValue)
            || !IsZeroLength(style.BorderLeft, style.BorderLeftValue)
            || font_size != style.FontSize
            || text_orientation != style.TextOrientation
        )
        {
            if (logic_text_length != 0) add_range();
            logic_text_start = scope.LogicTextStart;
            logic_text_length = scope.LogicTextLength;
            first_scope = scope.Scope;
            font_size = style.FontSize;
            text_orientation = style.TextOrientation;
        }
        else
        {
            logic_text_length += scope.LogicTextLength;
        }
    }
    if (logic_text_length != 0) add_range();
}

void ParagraphData::CollectRuns()
{
    COPLT_DEBUG_ASSERT(
        !m_script_ranges.empty() && !m_bidi_ranges.empty() && !m_font_ranges.empty() && !m_same_style_ranges.empty()
    );

    u32 logic_text_start = 0;
    u32 script_range_index = 0, bidi_range_index = 0, font_range_index = 0, style_range_index = 0;

    #pragma region assert range end all same
    const auto& script_last = m_script_ranges.back();
    const auto& bidi_last = m_bidi_ranges.back();
    const auto& font_last = m_font_ranges.back();
    const auto& style_last = m_same_style_ranges.back();
    const auto script_end = script_last.Start + script_last.Length;
    const auto bidi_end = bidi_last.Start + bidi_last.Length;
    const auto font_end = font_last.Start + font_last.Length;
    const auto style_end = style_last.Start + style_last.Length;
    COPLT_DEBUG_ASSERT(script_end == bidi_end && script_end == font_end && script_end == style_end);
    #pragma endregion

    while (
        script_range_index < m_script_ranges.size()
        && bidi_range_index < m_bidi_ranges.size()
        && font_range_index < m_font_ranges.size()
        && style_range_index < m_same_style_ranges.size()
    )
    {
        const auto& script_range = m_script_ranges[script_range_index];
        const auto& bidi_range = m_bidi_ranges[bidi_range_index];
        const auto& font_range = m_font_ranges[font_range_index];
        const auto& style_range = m_same_style_ranges[style_range_index];

        const auto script_len = script_range.Length - (logic_text_start - script_range.Start);
        const auto bidi_len = bidi_range.Length - (logic_text_start - bidi_range.Start);
        const auto font_len = font_range.Length - (logic_text_start - font_range.Start);
        const auto style_len = style_range.Length - (logic_text_start - style_range.Start);

        const auto min_len = std::min(std::min(script_len, bidi_len), std::min(font_len, style_len));

        m_runs.push_back(
            Run{
                .Start = logic_text_start,
                .Length = min_len,
                .ScriptRangeIndex = script_range_index,
                .BidiRangeIndex = bidi_range_index,
                .FontRangeIndex = font_range_index,
                .StyleRangeIndex = style_range_index,
            }
        );

        logic_text_start += min_len;
        if (logic_text_start >= script_range.Start + script_range.Length) script_range_index++;
        if (logic_text_start >= bidi_range.Start + bidi_range.Length) bidi_range_index++;
        if (logic_text_start >= font_range.Start + font_range.Length) font_range_index++;
        if (logic_text_start >= style_range.Start + style_range.Length) style_range_index++;
    }
}

void ParagraphData::AnalyzeGlyphsFirst()
{
    std::vector<u16> cluster_map;
    std::vector<DWRITE_SHAPING_TEXT_PROPERTIES> text_props;
    std::vector<u16> glyph_indices;
    std::vector<DWRITE_SHAPING_GLYPH_PROPERTIES> glyph_props;

    auto& analyzer = m_layout->m_text_analyzer;
    const auto& items = m_text_layout->m_items;
    auto item_index = 0;
    for (auto& run : m_runs)
    {
        const auto& script = m_script_ranges[run.ScriptRangeIndex];
        const auto& bidi = m_bidi_ranges[run.BidiRangeIndex];
        const auto& font = m_font_ranges[run.FontRangeIndex];
        const auto& same_style = m_same_style_ranges[run.StyleRangeIndex];

        COPLT_DEBUG_ASSERT(font.IsInlineBlock ? !font.Font : true, "inline block definitely no font");
        if (!font.Font) continue; // skip if no font find

        const auto scope = GetScope(same_style);
        const auto& style = scope.StyleData();

        const auto is_rtl = bidi.ResolvedLevel % 2 == 1;
        const auto locale = style.LocaleMode == LocaleMode::ByScript ? script.Locale : style.Locale.Name;

        // todo features from style
        DWRITE_FONT_FEATURE features[] = {
            DWRITE_FONT_FEATURE{
                .nameTag = DWRITE_FONT_FEATURE_TAG_REQUIRED_LIGATURES,
                .parameter = 1,
            },
            DWRITE_FONT_FEATURE{
                .nameTag = DWRITE_FONT_FEATURE_TAG_CONTEXTUAL_ALTERNATES,
                .parameter = 1,
            },
            DWRITE_FONT_FEATURE{
                .nameTag = DWRITE_FONT_FEATURE_TAG_STANDARD_LIGATURES,
                .parameter = 1,
            },
            DWRITE_FONT_FEATURE{
                .nameTag = DWRITE_FONT_FEATURE_TAG_CONTEXTUAL_LIGATURES,
                .parameter = 1,
            },
            DWRITE_FONT_FEATURE{
                .nameTag = DWRITE_FONT_FEATURE_TAG_LOCALIZED_FORMS,
                .parameter = 1,
            },
            DWRITE_FONT_FEATURE{
                .nameTag = DWRITE_FONT_FEATURE_TAG_GLYPH_COMPOSITION_DECOMPOSITION,
                .parameter = 1,
            },
            DWRITE_FONT_FEATURE{
                .nameTag = DWRITE_FONT_FEATURE_TAG_MARK_POSITIONING,
                .parameter = 1,
            },
            DWRITE_FONT_FEATURE{
                .nameTag = DWRITE_FONT_FEATURE_TAG_MARK_TO_MARK_POSITIONING,
                .parameter = 1,
            },
            DWRITE_FONT_FEATURE{
                .nameTag = DWRITE_FONT_FEATURE_TAG_KERNING,
                .parameter = 1,
            },
        };
        DWRITE_TYPOGRAPHIC_FEATURES typ_features[] = {
            DWRITE_TYPOGRAPHIC_FEATURES{
                .features = features,
                .featureCount = std::size(features)
            },
        };
        const DWRITE_TYPOGRAPHIC_FEATURES* arg_features = typ_features;
        const u32 feature_range_length = run.Length;

        const auto text = m_chars.data() + run.Start;

        HRESULT hr{};
        u32 actual_glyph_count{};
        auto buf_size = 3 * run.Length / 2 + 16;
        for (;;)
        {
            if (cluster_map.size() < run.Length)
            {
                cluster_map.resize(run.Length, {});
                text_props.resize(run.Length, {});
            }
            if (glyph_indices.size() < buf_size)
            {
                glyph_indices.resize(buf_size, {});
                glyph_props.resize(buf_size, {});
            }
            hr = analyzer->GetGlyphs(
                text,
                run.Length,
                font.Font->m_face.get(),
                false, // sideways
                is_rtl,
                &script.Analysis,
                locale,
                nullptr, // todo number subsitiution
                &arg_features,
                &feature_range_length,
                1,
                glyph_indices.size(),
                cluster_map.data(),
                text_props.data(),
                glyph_indices.data(),
                glyph_props.data(),
                &actual_glyph_count
            );
            if (hr != ERROR_INSUFFICIENT_BUFFER) break;
            buf_size *= 2;
        }
        if (FAILED(hr)) throw ComException(hr, "Failed to get glyphs");

        run.ClusterStartIndex = m_cluster_map.size();
        run.GlyphStartIndex = m_glyph_indices.size();
        run.ActualGlyphCount = actual_glyph_count;

        m_cluster_map.insert(m_cluster_map.end(), cluster_map.data(), cluster_map.data() + run.Length);
        m_text_props.insert(m_text_props.end(), text_props.data(), text_props.data() + run.Length);
        m_glyph_indices.insert(m_glyph_indices.end(), glyph_indices.data(), glyph_indices.data() + actual_glyph_count);
        m_glyph_props.insert(m_glyph_props.end(), glyph_props.data(), glyph_props.data() + actual_glyph_count);

        m_glyph_advances.resize(m_glyph_advances.size() + actual_glyph_count, {});
        m_glyph_offsets.resize(m_glyph_offsets.size() + actual_glyph_count, {});

        hr = analyzer->GetGlyphPlacements(
            text,
            cluster_map.data(),
            text_props.data(),
            run.Length,
            glyph_indices.data(),
            glyph_props.data(),
            actual_glyph_count,
            font.Font->m_face.get(),
            style.FontSize,
            false, // sideways
            is_rtl,
            &script.Analysis,
            locale,
            &arg_features,
            &feature_range_length,
            1,
            m_glyph_advances.data() + run.GlyphStartIndex,
            m_glyph_offsets.data() + run.GlyphStartIndex
        );
        if (FAILED(hr)) throw ComException(hr, "Failed to get glyphs");
    }
}

// void ParagraphData::AnalyzeGlyphsCarets()
// {
//     auto& hb_font_cache = m_text_layout->m_hb_font_cache;
//     for (auto& run : m_runs)
//     {
//         const auto& font = m_font_ranges[run.FontRangeIndex];
//         const auto& same_style = m_same_style_ranges[run.StyleRangeIndex];
//
//         if (!font.Font) continue; // skip if no font find
//         if (run.Length <= 1) continue; // one char never ligature
//
//         const auto scope = GetScope(same_style);
//         const auto& style = scope.StyleData();
//
//         Harf::HFont hb_font{};
//         const auto ensure_hb_font = [&]
//         {
//             if (hb_font) return;
//             auto entry = hb_font_cache.GetValueRefOrUninitializedValue(HBFontKey(font.Font, style));
//             if (!entry.Exists())
//             {
//                 const auto& key = entry.GetKey();
//                 const auto& value = entry.SetValue(HBFontValue(Harf::HFont(font.Font->m_hb_face)));
//                 value.Font.SetPixelsPerEm(key.FontSize);
//                 value.Font.SetVariations(
//                     {
//                         {HB_OT_TAG_VAR_AXIS_ITALIC, key.FontItalic ? 1.0f : 0.0f},
//                         {HB_OT_TAG_VAR_AXIS_SLANT, key.FontOblique_x100 / 100.0f},
//                         {HB_OT_TAG_VAR_AXIS_WIDTH, static_cast<f32>(key.FontWidth)},
//                         {HB_OT_TAG_VAR_AXIS_WEIGHT, static_cast<f32>(key.FontWeight)},
//                     }
//                 );
//             }
//             const auto& value = entry.GetValue();
//             hb_font = value.Font;
//         };
//
//         const std::span cluster_map{m_cluster_map.data() + run.ClusterStartIndex, run.Length};
//         const std::span glyph_indices{m_glyph_indices.data() + run.GlyphStartIndex, run.ActualGlyphCount};
//
//         std::vector<hb_position_t> caret_buf{};
//
//         for (u32 ci = 0, i = 1; i < run.Length; ci = i)
//         {
//             const u16 cur = cluster_map[ci];
//             for (; i < run.Length; i++)
//                 if (cur != cluster_map[i]) break;
//             const u32 len = i - ci;
//             COPLT_DEBUG_ASSERT(len != 0);
//             if (len == 1) continue;
//             const u16 glyph = glyph_indices[cur];
//             ensure_hb_font();
//             if (caret_buf.size() < len) caret_buf.resize(len, 0);
//             u32 count = len;
//             const auto num = hb_font.GetLigatureCarets(HB_DIRECTION_LTR, glyph, 0, &count, caret_buf.data());
//             // todo ; No font found for this data block
//             std::print("{}", num);
//         }
//     }
// }

TextAnalysisSource::TextAnalysisSource(ParagraphData* paragraph_data)
    : m_paragraph_data(paragraph_data)
{
}

HRESULT TextAnalysisSource::QueryInterface(const IID& riid, void** ppvObject)
{
    if (!ppvObject)
        return E_INVALIDARG;

    if (riid == __uuidof(IUnknown))
    {
        *ppvObject = static_cast<IUnknown*>(this);
    }
    else if (riid == __uuidof(IDWriteTextAnalysisSource))
    {
        *ppvObject = static_cast<IDWriteTextAnalysisSource*>(this);
    }
    else if (riid == __uuidof(IDWriteTextAnalysisSource1))
    {
        *ppvObject = static_cast<IDWriteTextAnalysisSource1*>(this);
    }
    else
    {
        *ppvObject = nullptr;
        return E_NOINTERFACE;
    }

    AddRef();
    return S_OK;
}

ULONG TextAnalysisSource::AddRef()
{
    return Impl_AddRef();
}

ULONG TextAnalysisSource::Release()
{
    return Impl_Release();
}

HRESULT TextAnalysisSource::GetTextAtPosition(
    UINT32 textPosition, const WCHAR** textString, UINT32* textLength
)
{
    const auto& chars = m_paragraph_data->m_chars;
    if (textPosition >= chars.size())
    {
        *textString = nullptr;
        *textLength = 0;
    }
    else
    {
        *textString = chars.data() + textPosition;
        *textLength = chars.size() - textPosition;
    }
    return S_OK;
}

HRESULT TextAnalysisSource::GetTextBeforePosition(
    UINT32 textPosition, const WCHAR** textString, UINT32* textLength
)
{
    const auto& chars = m_paragraph_data->m_chars;
    if (textPosition >= chars.size())
    {
        *textString = nullptr;
        *textLength = 0;
    }
    else
    {
        *textString = chars.data();
        *textLength = textPosition;
    }
    return S_OK;
}

DWRITE_READING_DIRECTION TextAnalysisSource::GetParagraphReadingDirection()
{
    return DWRITE_READING_DIRECTION_LEFT_TO_RIGHT;
}

HRESULT TextAnalysisSource::GetLocaleName(
    UINT32 textPosition, UINT32* textLength, const WCHAR** localeName
)
{
    const auto& paragraph = m_paragraph_data->GetParagraph();
    const auto scope_range_index = Algorithm::BinarySearch(
        paragraph.ScopeRanges.data(), paragraph.ScopeRanges.size(), textPosition,
        [](const TextScopeRange& item, const u32 pos)
        {
            if (pos < item.LogicTextStart) return 1;
            if (pos >= item.LogicTextStart + item.LogicTextLength) return -1;
            return 0;
        }
    );
    if (scope_range_index > 0)
    {
        const auto& scope_range = paragraph.ScopeRanges[scope_range_index];
        const auto scope = m_paragraph_data->GetScope(scope_range);
        const auto& scope_style = scope.StyleData();
        if (scope_style.Locale.Name)
        {
            const auto offset = textPosition - scope_range.LogicTextStart;
            *textLength = scope_range.LogicTextLength - offset;
            *localeName = scope_style.Locale.Name;
            return S_OK;
        }
        if (scope_style.LocaleMode == LocaleMode::ByScript)
        {
            const auto& script_ranges = m_paragraph_data->m_script_ranges;
            if (script_ranges.empty() || textPosition >= paragraph.LogicTextLength)
            {
                *textLength = 0;
                *localeName = nullptr;
                return S_OK;
            }
            if (paragraph.Type == TextParagraphType::Block)
            {
                *textLength = 1;
                *localeName = nullptr;
                return S_OK;
            }
            const auto index = Algorithm::BinarySearch(
                script_ranges.data(), static_cast<i32>(script_ranges.size()), textPosition,
                [](const ScriptRange& item, const u32 pos)
                {
                    if (pos < item.Start) return 1;
                    if (pos >= item.Start + item.Length) return -1;
                    return 0;
                }
            );
            if (index < 0)
            {
                *textLength = 0;
                *localeName = nullptr;
                return S_OK;
            }
            const auto& range = script_ranges[index];
            const auto offset = textPosition - range.Start;
            *textLength = range.Length - offset;
            *localeName = range.Locale;
            return S_OK;
        }
    }
    *textLength = 0;
    *localeName = L"";
    return S_OK;
}

HRESULT TextAnalysisSource::GetNumberSubstitution(
    UINT32 textPosition, UINT32* textLength, IDWriteNumberSubstitution** numberSubstitution
)
{
    return E_NOTIMPL;
}

HRESULT TextAnalysisSource::GetVerticalGlyphOrientation(
    UINT32 textPosition, UINT32* textLength,
    DWRITE_VERTICAL_GLYPH_ORIENTATION* glyphOrientation, UINT8* bidiLevel
)
{
    return E_NOTIMPL;
}

TextAnalysisSink::TextAnalysisSink(ParagraphData* paragraph_data)
    : m_paragraph_data(paragraph_data)
{
}

HRESULT TextAnalysisSink::QueryInterface(const IID& riid, void** ppvObject)
{
    if (!ppvObject)
        return E_INVALIDARG;

    if (riid == __uuidof(IUnknown))
    {
        *ppvObject = static_cast<IUnknown*>(this);
    }
    else if (riid == __uuidof(IDWriteTextAnalysisSink))
    {
        *ppvObject = static_cast<IDWriteTextAnalysisSink*>(this);
    }
    else if (riid == __uuidof(IDWriteTextAnalysisSink1))
    {
        *ppvObject = static_cast<IDWriteTextAnalysisSink1*>(this);
    }
    else
    {
        *ppvObject = nullptr;
        return E_NOINTERFACE;
    }

    AddRef();
    return S_OK;
}

ULONG TextAnalysisSink::AddRef()
{
    return Impl_AddRef();
}

ULONG TextAnalysisSink::Release()
{
    return Impl_Release();
}

HRESULT TextAnalysisSink::SetScriptAnalysis(
    UINT32 textPosition, UINT32 textLength, const DWRITE_SCRIPT_ANALYSIS* scriptAnalysis
)
{
    DWRITE_SCRIPT_PROPERTIES properties{};
    if (const auto hr = m_paragraph_data->m_layout->m_text_analyzer->GetScriptProperties(*scriptAnalysis, &properties);
        FAILED(hr))
        throw ComException(hr, "Failed to get script properties");

    const auto script_code_ = reinterpret_cast<const char*>(&properties.isoScriptCode);
    const char script_code[] = {script_code_[0], script_code_[1], script_code_[2], script_code_[3], 0};
    auto u_script = static_cast<UScriptCode>(u_getPropertyValueEnum(UCHAR_SCRIPT, script_code));
    if (u_script == -1) u_script = USCRIPT_COMMON;
    const auto locale = UnicodeUtils::LikelyLocale(u_script);

    m_paragraph_data->m_script_ranges.push_back(
        ScriptRange{
            .Start = textPosition,
            .Length = textLength,
            .Analysis = *scriptAnalysis,
            .Script = u_script,
            .Locale = locale,
        }
    );
    return S_OK;
}

HRESULT TextAnalysisSink::SetLineBreakpoints(
    UINT32 textPosition, UINT32 textLength, const DWRITE_LINE_BREAKPOINT* lineBreakpoints
)
{
    auto& line_breakpoints = m_paragraph_data->m_line_breakpoints;
    line_breakpoints.insert(line_breakpoints.end(), lineBreakpoints, lineBreakpoints + textLength);
    return S_OK;
}

HRESULT TextAnalysisSink::SetBidiLevel(
    UINT32 textPosition, UINT32 textLength, UINT8 explicitLevel, UINT8 resolvedLevel
)
{
    m_paragraph_data->m_bidi_ranges.push_back(
        BidiRange{
            .Start = textPosition,
            .Length = textLength,
            .ExplicitLevel = explicitLevel,
            .ResolvedLevel = resolvedLevel,
        }
    );
    return S_OK;
}

HRESULT TextAnalysisSink::SetNumberSubstitution(
    UINT32 textPosition, UINT32 textLength, IDWriteNumberSubstitution* numberSubstitution
)
{
    return E_NOTIMPL;
}

HRESULT TextAnalysisSink::SetGlyphOrientation(
    UINT32 textPosition, UINT32 textLength, DWRITE_GLYPH_ORIENTATION_ANGLE glyphOrientationAngle,
    UINT8 adjustedBidiLevel, BOOL isSideways, BOOL isRightToLeft
)
{
    return E_NOTIMPL;
}
