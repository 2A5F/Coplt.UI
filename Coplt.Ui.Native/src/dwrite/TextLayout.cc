#include "TextLayout.h"

#include <icu.h>
#include <span>
#include <fmt/xchar.h>

#include "../lib.h"
#include "../Algorithm.h"
#include "../Text.h"
#include "Layout.h"
#include "Error.h"
#include "BaseFontFallback.h"
#include "Utils.h"

using namespace Coplt;

TextLayoutCalc::ParagraphData::ParagraphData(TextLayout* text_layout)
    : m_text_layout(text_layout)
{
}

void TextLayoutCalc::ParagraphData::ReBuild()
{
    if (!m_src) m_src = Rc(new TextAnalysisSource(this));
    if (!m_sink) m_sink = Rc(new TextAnalysisSink(this));
    m_script_ranges.clear();
    m_bidi_ranges.clear();
    m_line_breakpoints.clear();
    m_font_ranges.clear();
}

std::vector<BaseTextLayoutStorage::Paragraph>& TextLayoutCalc::ParagraphData::GetTextLayoutParagraphs() const
{
    return m_text_layout->m_paragraphs;
}

BaseTextLayoutStorage::Paragraph& TextLayoutCalc::ParagraphData::GetParagraph() const
{
    return GetTextLayoutParagraphs()[m_index];
}

std::span<BaseTextLayoutStorage::Item> TextLayoutCalc::ParagraphData::GetItems() const
{
    const auto& paragraph = GetParagraph();
    return GetItems(paragraph.ItemStart, paragraph.ItemLength);
}

std::span<BaseTextLayoutStorage::Item> TextLayoutCalc::ParagraphData::GetItems(const u32 Start, const u32 Length) const
{
    return std::span(m_text_layout->m_items.data() + Start, Length);
}

LayoutCalc::CtxNodeRef TextLayoutCalc::ParagraphData::GetScope(const BaseTextLayoutStorage::ScopeRange& range) const
{
    const auto root_scope = m_text_layout->m_node;
    return
        range.Scope == -1
            ? root_scope
            : LayoutCalc::CtxNodeRef(root_scope.ctx, m_text_layout->m_scopes[range.Scope]);
}

void TextLayout::ReBuild(Layout* layout, LayoutCalc::CtxNodeRef node)
{
    m_node = node;
    m_paragraph_datas.resize(m_paragraphs.size(), TextLayoutCalc::ParagraphData(this));
    for (int i = 0; i < m_paragraphs.size(); ++i)
    {
        auto& paragraph = m_paragraphs[i];
        if (paragraph.Type == ParagraphType::Block) continue;
        auto& data = m_paragraph_datas[i];
        data.m_index = i;
        data.m_layout = layout;
        data.ReBuild();
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
    }

    m_node = {};
}

void TextLayoutCalc::ParagraphData::AnalyzeFonts()
{
    const auto& system_font_fallback = m_layout->m_system_font_fallback;

    const auto& paragraph = GetParagraph();
    for (const auto& range : paragraph.ScopeRanges)
    {
        const auto scope = GetScope(range);
        const auto& style = scope.StyleData();
        const auto font_fall_back =
            style.FontFallback == nullptr
                ? system_font_fallback.get()
                : static_cast<BaseFontFallback*>(style.FontFallback)->m_fallback.get();
        COPLT_DEBUG_ASSERT(font_fall_back != nullptr, "font_fall_back should not be null");

        const auto font_weight = to_dwrite(style.FontWeight);
        const auto font_style = to_dwrite(style.FontStyle);
        const auto font_stretch = to_dwrite(style.FontStretch);

        auto text_start = range.LogicTextStart;
        auto text_length = range.LogicTextLength;

        for (;;)
        {
            u32 mapped_length{};
            Rc<IDWriteFont> mapped_font{};
            f32 scale{};

            if (const auto hr = font_fall_back->MapCharacters(
                m_src.get(), text_start, text_length,
                nullptr, nullptr,
                font_weight, font_style, font_stretch,
                &mapped_length, mapped_font.put(), &scale
            ); FAILED(hr))
                throw ComException(hr, "Failed to map characters");

            // not find, retry use system font fallback
            if (mapped_font == nullptr && font_fall_back != system_font_fallback)
            {
                if (const auto hr = system_font_fallback->MapCharacters(
                    m_src.get(), text_start, text_length,
                    nullptr, nullptr,
                    font_weight, font_style, font_stretch,
                    &mapped_length, mapped_font.put(), &scale
                ); FAILED(hr))
                    throw ComException(hr, "Failed to map characters");
            }

#ifdef _DEBUG
            // if (mapped_font)
            // {
            //     u32 len{};
            //     const char16* local{};
            //     m_src->GetLocaleName(text_start, &len, &local);
            //
            //     const auto name = GetFontFamilyName(mapped_font);
            //     m_layout->m_lib->m_logger.Log(LogLevel::Debug, fmt::format(L"{} :: {}", local, name));
            // }
#endif

            m_font_ranges.push_back(FontRange{
                .Start = text_start,
                .Length = mapped_length,
                .Font = std::move(mapped_font),
                .Scale = scale,
            });

            if (mapped_length >= text_length) break;
            text_start += mapped_length;
            text_length -= mapped_length;
        }
    }
}

TextLayoutCalc::TextAnalysisSource::TextAnalysisSource(ParagraphData* paragraph_data)
    : m_paragraph_data(paragraph_data)
{
}

HRESULT TextLayoutCalc::TextAnalysisSource::QueryInterface(const IID& riid, void** ppvObject)
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
    else
    {
        *ppvObject = nullptr;
        return E_NOINTERFACE;
    }

    AddRef();
    return S_OK;
}

ULONG TextLayoutCalc::TextAnalysisSource::AddRef()
{
    return Impl_AddRef();
}

ULONG TextLayoutCalc::TextAnalysisSource::Release()
{
    return Impl_Release();
}

HRESULT TextLayoutCalc::TextAnalysisSource::GetTextAtPosition(
    UINT32 textPosition, const WCHAR** textString, UINT32* textLength
)
{
    auto layout = m_paragraph_data->m_text_layout;
    const auto item_index = layout->SearchItem(m_paragraph_data->m_index, textPosition);
    if (item_index >= 0)
    {
        const auto item = &layout->m_items[item_index];
        const auto local_offset = textPosition - item->LogicTextStart;
        if (local_offset <= item->LogicTextLength)
        {
            *textString = GetText(layout->m_node.ctx, item) + local_offset;
            *textLength = item->LogicTextLength - local_offset;
            return S_OK;
        }
    }
    *textString = nullptr;
    *textLength = 0;
    return S_OK;
}

HRESULT TextLayoutCalc::TextAnalysisSource::GetTextBeforePosition(
    UINT32 textPosition, const WCHAR** textString, UINT32* textLength
)
{
    auto layout = m_paragraph_data->m_text_layout;
    const auto item_index = layout->SearchItem(m_paragraph_data->m_index, textPosition);
    if (item_index >= 0)
    {
        const auto item = &layout->m_items[item_index];
        const auto local_offset = textPosition - item->LogicTextStart;
        if (local_offset <= item->LogicTextLength)
        {
            *textString = GetText(layout->m_node.ctx, item);
            *textLength = local_offset;
            return S_OK;
        }
        if (item_index > 0)
        {
            const auto pre_item = &layout->m_items[item_index - 1];
            const auto pre_local_offset = textPosition - pre_item->LogicTextStart;
            if (pre_local_offset <= item->LogicTextLength)
            {
                *textString = GetText(layout->m_node.ctx, pre_item);
                *textLength = pre_local_offset;
                return S_OK;
            }
        }
    }
    *textString = nullptr;
    *textLength = 0;
    return S_OK;
}

DWRITE_READING_DIRECTION TextLayoutCalc::TextAnalysisSource::GetParagraphReadingDirection()
{
    return DWRITE_READING_DIRECTION_LEFT_TO_RIGHT;
}

HRESULT TextLayoutCalc::TextAnalysisSource::GetLocaleName(
    UINT32 textPosition, UINT32* textLength, const WCHAR** localeName
)
{
    const auto layout = m_paragraph_data->m_text_layout;
    const auto& paragraph = m_paragraph_data->GetParagraph();
    const auto scope_range_index = Algorithm::BinarySearch(
        paragraph.ScopeRanges.data(), paragraph.ScopeRanges.size(), textPosition,
        [](const TextLayout::ScopeRange& item, const u32 pos)
        {
            if (pos < item.LogicTextStart) return 1;
            if (pos >= item.LogicTextStart + item.LogicTextLength) return -1;
            return 0;
        });
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
            if (paragraph.Type == TextLayout::ParagraphType::Block)
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
                });
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

HRESULT TextLayoutCalc::TextAnalysisSource::GetNumberSubstitution(
    UINT32 textPosition, UINT32* textLength, IDWriteNumberSubstitution** numberSubstitution
)
{
    return E_NOTIMPL;
}

TextLayoutCalc::TextAnalysisSink::TextAnalysisSink(ParagraphData* paragraph_data)
    : m_paragraph_data(paragraph_data)
{
}

HRESULT TextLayoutCalc::TextAnalysisSink::QueryInterface(const IID& riid, void** ppvObject)
{
    if (!ppvObject)
        return E_INVALIDARG;

    if (riid == __uuidof(IUnknown))
    {
        *ppvObject = static_cast<IUnknown*>(this);
    }
    else if (riid == __uuidof(IDWriteTextAnalysisSource))
    {
        *ppvObject = static_cast<IDWriteTextAnalysisSink*>(this);
    }
    else
    {
        *ppvObject = nullptr;
        return E_NOINTERFACE;
    }

    AddRef();
    return S_OK;
}

ULONG TextLayoutCalc::TextAnalysisSink::AddRef()
{
    return Impl_AddRef();
}

ULONG TextLayoutCalc::TextAnalysisSink::Release()
{
    return Impl_Release();
}

HRESULT TextLayoutCalc::TextAnalysisSink::SetScriptAnalysis(
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

    m_paragraph_data->m_script_ranges.push_back(ScriptRange{
        .Start = textPosition,
        .Length = textLength,
        .Analysis = *scriptAnalysis,
        .Script = u_script,
        .Locale = locale,
    });
    return S_OK;
}

HRESULT TextLayoutCalc::TextAnalysisSink::SetLineBreakpoints(
    UINT32 textPosition, UINT32 textLength, const DWRITE_LINE_BREAKPOINT* lineBreakpoints
)
{
    auto& line_breakpoints = m_paragraph_data->m_line_breakpoints;
    line_breakpoints.insert(line_breakpoints.end(), lineBreakpoints, lineBreakpoints + textLength);
    return S_OK;
}

HRESULT TextLayoutCalc::TextAnalysisSink::SetBidiLevel(
    UINT32 textPosition, UINT32 textLength, UINT8 explicitLevel, UINT8 resolvedLevel
)
{
    m_paragraph_data->m_bidi_ranges.push_back(BidiRange{
        .Start = textPosition,
        .Length = textLength,
        .ExplicitLevel = explicitLevel,
        .ResolvedLevel = resolvedLevel,
    });
    return S_OK;
}

HRESULT TextLayoutCalc::TextAnalysisSink::SetNumberSubstitution(
    UINT32 textPosition, UINT32 textLength, IDWriteNumberSubstitution* numberSubstitution
)
{
    return E_NOTIMPL;
}
