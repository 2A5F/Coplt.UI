#include "TextLayout.h"

#include "Layout.h"

using namespace Coplt;

// HRESULT TextAnalysisSource::QueryInterface(const IID& riid, void** ppvObject)
// {
//     if (!ppvObject)
//         return E_INVALIDARG;
//
//     if (riid == __uuidof(IUnknown))
//     {
//         *ppvObject = static_cast<IUnknown*>(this);
//     }
//     else if (riid == __uuidof(IDWriteTextAnalysisSource))
//     {
//         *ppvObject = static_cast<IDWriteTextAnalysisSource*>(this);
//     }
//     else
//     {
//         *ppvObject = nullptr;
//         return E_NOINTERFACE;
//     }
//
//     AddRef();
//     return S_OK;
// }
//
// ULONG TextAnalysisSource::AddRef()
// {
//     return Impl_AddRef();
// }
//
// ULONG TextAnalysisSource::Release()
// {
//     return Impl_Release();
// }
//
// HRESULT TextAnalysisSource::GetTextAtPosition(UINT32 textPosition, const WCHAR** textString, UINT32* textLength)
// {
//     return E_NOTIMPL;
// }
//
// HRESULT TextAnalysisSource::GetTextBeforePosition(UINT32 textPosition, const WCHAR** textString, UINT32* textLength)
// {
//     return E_NOTIMPL;
// }
//
// DWRITE_READING_DIRECTION TextAnalysisSource::GetParagraphReadingDirection()
// {
//     return DWRITE_READING_DIRECTION_LEFT_TO_RIGHT;
// }
//
// HRESULT TextAnalysisSource::GetLocaleName(UINT32 textPosition, UINT32* textLength, const WCHAR** localeName)
// {
//     return E_NOTIMPL;
// }
//
// HRESULT TextAnalysisSource::GetNumberSubstitution(UINT32 textPosition, UINT32* textLength,
//                                                   IDWriteNumberSubstitution** numberSubstitution)
// {
//     return E_NOTIMPL;
// }
