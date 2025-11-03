#include "TextLayout.h"

#include <icu.h>

#include "../Text.h"
#include "Layout.h"
#include "Error.h"

using namespace Coplt;

TextLayoutCalc::ParagraphData::ParagraphData(TextLayout* text_layout)
    : m_text_layout(text_layout)
{
}

void TextLayoutCalc::ParagraphData::ReBuild()
{
    if (!m_src) m_src = Rc(new TextAnalysisSource(this));
    if (!m_sink) m_sink = Rc(new TextAnalysisSink(this));
    m_bidi_ranges.clear();
}

std::vector<BaseTextLayoutStorage::Paragraph>& TextLayoutCalc::ParagraphData::GetParagraphs() const
{
    return m_text_layout->m_paragraphs;
}

void TextLayout::ReBuild(Layout* layout, NLayoutContext* ctx)
{
    m_ctx = ctx;
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
    }

    m_ctx = nullptr;
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
    if (item_index < 0)
    {
        *textString = nullptr;
        *textLength = 0;
    }
    else
    {
        const auto item = &layout->m_items[item_index];
        *textString = GetText(layout->m_ctx, item);
        *textLength = item->Length;
    }
    return S_OK;
}

HRESULT TextLayoutCalc::TextAnalysisSource::GetTextBeforePosition(
    UINT32 textPosition, const WCHAR** textString, UINT32* textLength
)
{
    auto layout = m_paragraph_data->m_text_layout;
    const auto item_index = layout->SearchItem(m_paragraph_data->m_index, textPosition);
    if (item_index < 1)
    {
        *textString = nullptr;
        *textLength = 0;
    }
    else
    {
        const auto item = &layout->m_items[item_index - 1];
        *textString = GetText(layout->m_ctx, item);
        *textLength = item->Length;
    }
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
    return E_NOTIMPL;
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
    return E_NOTIMPL;
}

HRESULT TextLayoutCalc::TextAnalysisSink::SetBidiLevel(
    UINT32 textPosition, UINT32 textLength, UINT8 explicitLevel, UINT8 resolvedLevel
)
{
    m_paragraph_data->m_bidi_ranges.push_back(BidiRange{
        .Start = textPosition,
        .Length = textLength,
        . ExplicitLevel = explicitLevel,
        . ResolvedLevel = resolvedLevel,
    });
    return S_OK;
}

HRESULT TextLayoutCalc::TextAnalysisSink::SetNumberSubstitution(
    UINT32 textPosition, UINT32 textLength, IDWriteNumberSubstitution* numberSubstitution
)
{
    return E_NOTIMPL;
}
