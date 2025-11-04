#pragma once

#include <icu.h>
#include <dwrite_3.h>

#include "../Com.h"
#include "../TextLayout.h"

namespace Coplt
{
    namespace TextLayoutCalc
    {
        struct ParagraphData;
    }

    struct Layout;

    struct TextLayout final : BaseTextLayout<TextLayout>
    {
        NLayoutContext* m_ctx;
        std::vector<TextLayoutCalc::ParagraphData> m_paragraph_datas{};

        void ReBuild(Layout* layout, NLayoutContext* ctx);

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

        // ReSharper disable once CppPolymorphicClassWithNonVirtualPublicDestructor
        struct TextAnalysisSink final : IDWriteTextAnalysisSink, RefCount<TextAnalysisSink>
        {
            ParagraphData* m_paragraph_data{};

            explicit TextAnalysisSink(ParagraphData* paragraph_data);

            HRESULT QueryInterface(const IID& riid, void** ppvObject) override;
            ULONG AddRef() override;
            ULONG Release() override;
            HRESULT SetScriptAnalysis(
                UINT32 textPosition, UINT32 textLength, const DWRITE_SCRIPT_ANALYSIS* scriptAnalysis
            ) override;
            HRESULT SetLineBreakpoints(
                UINT32 textPosition, UINT32 textLength, const DWRITE_LINE_BREAKPOINT* lineBreakpoints
            ) override;
            HRESULT SetBidiLevel(
                UINT32 textPosition, UINT32 textLength, UINT8 explicitLevel, UINT8 resolvedLevel
            ) override;
            HRESULT SetNumberSubstitution(
                UINT32 textPosition, UINT32 textLength, IDWriteNumberSubstitution* numberSubstitution
            ) override;
        };

        struct ScriptRange
        {
            u32 Start;
            u32 Length;
            DWRITE_SCRIPT_ANALYSIS Analysis;
            UScriptCode Script;
            const char16* Locale;
        };

        struct BidiRange
        {
            u32 Start;
            u32 Length;
            u8 ExplicitLevel;
            u8 ResolvedLevel;
        };

        struct ParagraphData
        {
            Layout* m_layout{};
            TextLayout* m_text_layout{};

            explicit ParagraphData(TextLayout* text_layout);

            u32 m_index{};

            Rc<TextAnalysisSource> m_src{};
            Rc<TextAnalysisSink> m_sink{};

            std::vector<ScriptRange> m_script_ranges{};
            std::vector<BidiRange> m_bidi_ranges{};

            void ReBuild();

            std::vector<BaseTextLayoutStorage::Paragraph>& GetTextLayoutParagraphs() const;
            BaseTextLayoutStorage::Paragraph& GetParagraph() const;
        };
    }
} // namespace Coplt
