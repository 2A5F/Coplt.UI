#pragma once

#include <dwrite_3.h>

#include "../Com.h"
#include "../Arc.h"
#include "../List.h"
#include "../Text.h"
#include "../TextLayout.h"

namespace Coplt
{
    namespace TextLayoutCalc
    {
        struct ParagraphData;
    }

    struct TextLayout final : BaseTextLayout<TextLayout>
    {
        std::vector<TextLayoutCalc::ParagraphData> m_paragraph_datas{};

        void ReBuild();

        COPLT_IMPL_START
        COPLT_IMPL_END
    };

    namespace TextLayoutCalc
    {
        // ReSharper disable once CppPolymorphicClassWithNonVirtualPublicDestructor
        struct TextAnalysisSource final : IDWriteTextAnalysisSource, RefCount<TextAnalysisSource>
        {
            ParagraphData* m_paragraph_data{};

            explicit TextAnalysisSource(ParagraphData* paragraph_data);

            HRESULT QueryInterface(const IID& riid, void** ppvObject) override;
            ULONG AddRef() override;
            ULONG Release() override;
            HRESULT GetTextAtPosition(UINT32 textPosition, const WCHAR** textString, UINT32* textLength) override;
            HRESULT GetTextBeforePosition(UINT32 textPosition, const WCHAR** textString, UINT32* textLength) override;
            DWRITE_READING_DIRECTION GetParagraphReadingDirection() override;
            HRESULT GetLocaleName(UINT32 textPosition, UINT32* textLength, const WCHAR** localeName) override;
            HRESULT GetNumberSubstitution(
                UINT32 textPosition, UINT32* textLength, IDWriteNumberSubstitution** numberSubstitution
            ) override;
        };

        struct ParagraphData
        {
            Rc<TextAnalysisSource> m_src{};
        };
    }
} // namespace Coplt
