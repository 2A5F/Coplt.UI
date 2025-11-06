#pragma once

#include <span>
#include <icu.h>
#include <dwrite_3.h>

#include "../Com.h"
#include "../TextLayout.h"
#include "../Layout.h"

namespace Coplt
{
    namespace TextLayoutCalc
    {
        struct ParagraphData;
    }

    struct Layout;

    struct TextLayout final : BaseTextLayout<TextLayout>
    {
        LayoutCalc::CtxNodeRef m_node;
        std::vector<TextLayoutCalc::ParagraphData> m_paragraph_datas{};

        void ReBuild(Layout* layout, LayoutCalc::CtxNodeRef node);

        COPLT_IMPL_START
        COPLT_IMPL_END
    };

    namespace TextLayoutCalc
    {
        // ReSharper disable once CppPolymorphicClassWithNonVirtualPublicDestructor
        struct TextAnalysisSource final : IDWriteTextAnalysisSource1, RefCount<TextAnalysisSource>
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
            HRESULT GetVerticalGlyphOrientation(
                UINT32 textPosition, UINT32* textLength,
                DWRITE_VERTICAL_GLYPH_ORIENTATION* glyphOrientation, UINT8* bidiLevel
            ) override;
        };

        // ReSharper disable once CppPolymorphicClassWithNonVirtualPublicDestructor
        struct TextAnalysisSink final : IDWriteTextAnalysisSink1, RefCount<TextAnalysisSink>
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
            HRESULT SetGlyphOrientation(
                UINT32 textPosition, UINT32 textLength, DWRITE_GLYPH_ORIENTATION_ANGLE glyphOrientationAngle,
                UINT8 adjustedBidiLevel, BOOL isSideways, BOOL isRightToLeft
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

        struct FontRange
        {
            u32 Start;
            u32 Length;
            Rc<IDWriteFontFace5> Font;
            f32 Scale;
            NodeId Scope;
        };

        struct Run
        {
            u32 Start;
            u32 Length;
            u32 ScriptRangeIndex;
            u32 BidiRangeIndex;
            u32 FontRangeIndex;

            u32 GlyphStartIndex;
            u32 ActualGlyphCount;
        };

        struct ParagraphData
        {
            Layout* m_layout{};
            TextLayout* m_text_layout{};

            explicit ParagraphData(TextLayout* text_layout);

            u32 m_index{};

            Rc<TextAnalysisSource> m_src{};
            Rc<TextAnalysisSink> m_sink{};

            f32 m_single_line_width{};
            f32 m_single_line_height{};

            std::vector<ScriptRange> m_script_ranges{};
            std::vector<BidiRange> m_bidi_ranges{};
            std::vector<DWRITE_LINE_BREAKPOINT> m_line_breakpoints{};
            std::vector<FontRange> m_font_ranges{};
            std::vector<Run> m_runs{};

            std::vector<u16> m_cluster_map{};
            std::vector<DWRITE_SHAPING_TEXT_PROPERTIES> m_text_props{};
            std::vector<u16> m_glyph_indices{};
            std::vector<DWRITE_SHAPING_GLYPH_PROPERTIES> m_glyph_props{};
            std::vector<f32> m_glyph_advances{};
            std::vector<DWRITE_GLYPH_OFFSET> m_glyph_offsets{};

            void ReBuild();

            std::vector<BaseTextLayoutStorage::Paragraph>& GetTextLayoutParagraphs() const;
            BaseTextLayoutStorage::Paragraph& GetParagraph() const;
            std::span<BaseTextLayoutStorage::Item> GetItems() const;
            std::span<BaseTextLayoutStorage::Item> GetItems(u32 Start, u32 Length) const;

            LayoutCalc::CtxNodeRef GetScope(const BaseTextLayoutStorage::ScopeRange& range) const;

            void AnalyzeFonts();
            void CollectRuns();
            void AnalyzeGlyphs();

            void CalcSingleLineSize();
        };
    }
} // namespace Coplt
