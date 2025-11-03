#include "TextLayout.h"

#include "Layout.h"

using namespace Coplt;

void TextLayout::ReBuild()
{
    m_paragraph_datas.clear();
    m_paragraph_datas.resize(m_paragraphs.size(), TextLayoutCalc::ParagraphData{});
    for (int i = 0; i < m_paragraphs.size(); ++i)
    {
        m_paragraph_datas[i].m_src = Rc(new TextLayoutCalc::TextAnalysisSource(&m_paragraph_datas[i]));
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
    return E_NOTIMPL;
}

HRESULT TextLayoutCalc::TextAnalysisSource::GetTextBeforePosition(
    UINT32 textPosition, const WCHAR** textString, UINT32* textLength
)
{
    return E_NOTIMPL;
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
