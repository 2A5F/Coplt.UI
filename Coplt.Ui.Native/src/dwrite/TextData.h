#pragma once

#include <dwrite_3.h>

#include "../Com.h"
#include "../Arc.h"
#include "../List.h"
#include "../Text.h"

namespace Coplt
{
    struct TextDataObj final : ComImpl<TextDataObj, ITextData>
    {
        COPLT_IMPL_START
        COPLT_IMPL_END
    };

    // ReSharper disable once CppPolymorphicClassWithNonVirtualPublicDestructor
    struct SliceTextAnalysisSource final : IDWriteTextAnalysisSource
    {
        char16 const* text;
        u32 len;

        explicit SliceTextAnalysisSource(char16 const* text, const u32 len) : text(text), len(len)
        {
        }

        HRESULT GetTextAtPosition(UINT32 textPosition, const WCHAR** textString, UINT32* textLength) override
        {
            if (textPosition >= len)
            {
                *textString = nullptr;
                *textLength = 0;
                return S_OK;
            }
            *textString = text + textPosition;
            *textLength = len - textPosition;
            return S_OK;
        }

        HRESULT GetTextBeforePosition(UINT32 textPosition, const WCHAR** textString, UINT32* textLength) override
        {
            if (textPosition >= len || textPosition == 0)
            {
                *textString = nullptr;
                *textLength = 0;
                return S_OK;
            }
            *textString = text;
            *textLength = textPosition;
            return S_OK;
        }

        HRESULT QueryInterface(const IID& riid, void** ppvObject) override { return E_NOINTERFACE; }
        ULONG AddRef() override { throw Exception(); }
        ULONG Release() override { throw Exception(); }

        DWRITE_READING_DIRECTION GetParagraphReadingDirection() override { throw Exception(); }

        HRESULT GetLocaleName(
            UINT32 textPosition, UINT32* textLength, const WCHAR** localeName
        ) override
        {
            *textLength = 0;
            *localeName = nullptr;
            return S_OK;
        }

        HRESULT GetNumberSubstitution(
            UINT32 textPosition, UINT32* textLength, IDWriteNumberSubstitution** numberSubstitution
        ) override
        {
            *textLength = 0;
            *numberSubstitution = nullptr;
            return S_OK;
        }
    };
} // namespace Coplt
