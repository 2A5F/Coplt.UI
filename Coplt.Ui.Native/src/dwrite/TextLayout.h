#pragma once

#include <span>
#include <icu.h>
#include <dwrite_3.h>

#include <hb.h>

#include "../Com.h"
#include "../TextLayout.h"
#include "../Layout.h"

namespace Coplt::LayoutCalc
{
    struct Layout;
}

namespace Coplt::LayoutCalc::Texts
{
    struct ParagraphData;

    struct TextLayoutCache_Final : CacheEntryBase
    {
        LayoutOutput Output;
        // todo other data
    };

    struct TextLayoutCache_Measure : CacheEntryBase
    {
        f32 Width;
        f32 Height;

        Size<f32> Size() const
        {
            return {.Width = Width, .Height = Height};
        }
    };

    struct TextLayoutCache
    {
        TextLayoutCache_Final Final;
        TextLayoutCache_Measure Measure[9];
        LayoutCacheFlags Flags;

        std::optional<LayoutOutput> GetOutput(const LayoutInputs& inputs);

        void StoreFinal(const LayoutInputs& inputs, LayoutOutput output);
        void StoreMeasure(const LayoutInputs& inputs, f32 width, f32 height);
        void Clear();
    };

    struct TextLayout final : BaseTextLayout<TextLayout>
    {
        CtxNodeRef m_node;
        std::vector<ParagraphData> m_paragraph_datas{};

        void ReBuild(Layout* layout, CtxNodeRef node);

        COPLT_IMPL_START
        COPLT_IMPL_END

        void Compute(LayoutOutput& out, const LayoutInputs& inputs, CtxNodeRef node);
        LayoutOutput Compute(const LayoutInputs& inputs);
    };

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
        // item range only alive when IsInlineBlock is true
        u32 ItemStart;
        u32 ItemLength;
        bool IsInlineBlock;
    };

    struct SameStyleRange
    {
        u32 Start;
        u32 Length;
        u32 FirstScope;
    };

    struct Run
    {
        u32 Start;
        u32 Length;
        u32 ScriptRangeIndex;
        u32 BidiRangeIndex;
        u32 FontRangeIndex;
        u32 StyleRangeIndex;

        u32 GlyphStartIndex;
        u32 ActualGlyphCount;

        bool HasSingleLineSize;
        f32 SingleLineWidth;
        f32 SingleLineHeight;

        bool IsInlineBlock(const ParagraphData& data) const;
        std::optional<Size<f32>> SingleLineSize(const ParagraphData& data);
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
        std::vector<DWRITE_LINE_BREAKPOINT> m_line_breakpoints{};
        std::vector<FontRange> m_font_ranges{};
        std::vector<SameStyleRange> m_same_style_ranges{};
        std::vector<Run> m_runs{};

        std::vector<u16> m_cluster_map{};
        std::vector<DWRITE_SHAPING_TEXT_PROPERTIES> m_text_props{};
        std::vector<u16> m_glyph_indices{};
        std::vector<DWRITE_SHAPING_GLYPH_PROPERTIES> m_glyph_props{};
        std::vector<f32> m_glyph_advances{};
        std::vector<DWRITE_GLYPH_OFFSET> m_glyph_offsets{};

        TextLayoutCache m_cache{};

        void ReBuild();

        std::vector<Paragraph>& GetTextLayoutParagraphs() const;
        Paragraph& GetParagraph() const;
        std::span<TextItem> GetItems() const;
        std::span<TextItem> GetItems(u32 Start, u32 Length) const;

        CtxNodeRef GetScope(const TextItem& item) const;
        CtxNodeRef GetScope(const TextScopeRange& range) const;
        CtxNodeRef GetScope(const SameStyleRange& range) const;

        void AnalyzeFonts();
        void AnalyzeStyles();
        void CollectRuns();
        void AnalyzeGlyphsFirst();

        LayoutOutput Compute(
            TextLayout& layout, LayoutRunMode RunMode,
            Size<AvailableSpace> AvailableSpace, Size<std::optional<f32>> KnownSize
        );
    };
} // namespace Coplt
