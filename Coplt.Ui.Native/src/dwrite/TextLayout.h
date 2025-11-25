#pragma once

#include <span>
#include <generator>
#include <icu.h>
#include <dwrite_3.h>

#include "../Com.h"
#include "../TextLayout.h"
#include "../Layout.h"
#include "../Utils.h"
#include "../Harfpp.h"

namespace Coplt
{
    struct LoggerData;
    struct DWriteFontFace;
}

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

    struct HBFontKey
    {
        Rc<DWriteFontFace> Font{};
        u32 FontSize{};
        u32 FontWidth{};
        i32 FontOblique_x100{};
        FontWeight FontWeight{};
        bool FontItalic{};

        HBFontKey() = default;

        explicit HBFontKey(
            Rc<DWriteFontFace> font,
            const StyleData& style
        )
            : Font(std::move(font)),
              FontSize(std::round(style.FontSize)),
              FontWidth(std::round(style.FontWidth.Width)),
              FontOblique_x100(std::round(style.FontOblique * 100)),
              FontWeight(style.FontWeight),
              FontItalic(style.FontItalic)
        {
        }

        bool operator==(const HBFontKey& other) const
        {
            return
                Font.get() == other.Font.get()
                && FontSize == other.FontSize
                && FontWidth == other.FontWidth
                && FontOblique_x100 == other.FontOblique_x100
                && FontWeight == other.FontWeight
                && FontItalic == other.FontItalic;
        }

        i32 GetHashCode() const
        {
            return HashValues(
                reinterpret_cast<usize>(Font.get()),
                FontSize,
                FontWidth,
                FontOblique_x100,
                static_cast<i32>(FontWeight),
                FontItalic
            );
        }
    };

    struct HBFontValue
    {
        Harf::HFont Font{};

        HBFontValue() = default;

        explicit HBFontValue(Harf::HFont font)
            : Font(std::move(font))
        {
        }
    };

    // ReSharper disable once CppPolymorphicClassWithNonVirtualPublicDestructor
    struct OneSpaceTextAnalysisSource final : IDWriteTextAnalysisSource1, RefCount<OneSpaceTextAnalysisSource>
    {
        explicit OneSpaceTextAnalysisSource() = default;

        HRESULT QueryInterface(const IID& riid, void** ppvObject) override
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

        ULONG AddRef() override { return Impl_AddRef(); }
        ULONG Release() override { return Impl_Release(); }

        HRESULT GetTextAtPosition(UINT32 textPosition, const WCHAR** textString, UINT32* textLength) override
        {
            if (textPosition != 0)
            {
                *textString = nullptr;
                *textLength = 0;
            }
            else
            {
                *textString = L" ";
                *textLength = 1;
            }
            return S_OK;
        }

        HRESULT GetTextBeforePosition(UINT32 textPosition, const WCHAR** textString, UINT32* textLength) override
        {
            if (textPosition != 1)
            {
                *textString = nullptr;
                *textLength = 0;
            }
            else
            {
                *textString = L" ";
                *textLength = 1;
            }
            return S_OK;
        }

        DWRITE_READING_DIRECTION GetParagraphReadingDirection() override { return DWRITE_READING_DIRECTION_LEFT_TO_RIGHT; }

        HRESULT GetLocaleName(UINT32 textPosition, UINT32* textLength, const WCHAR** localeName) override
        {
            *textLength = 0;
            *localeName = nullptr;
            return S_OK;
        }

        HRESULT GetNumberSubstitution(
            UINT32 textPosition, UINT32* textLength, IDWriteNumberSubstitution** numberSubstitution
        ) override { return E_NOTIMPL; }

        HRESULT GetVerticalGlyphOrientation(
            UINT32 textPosition, UINT32* textLength,
            DWRITE_VERTICAL_GLYPH_ORIENTATION* glyphOrientation, UINT8* bidiLevel
        ) override { return E_NOTIMPL; }
    };

    struct TextLayout final : BaseTextLayout<TextLayout>
    {
        CtxNodeRef m_node{};
        Layout* m_layout{}; // todo:  init when ctor

        std::vector<ParagraphData> m_paragraph_datas{};
        Map<HBFontKey, HBFontValue> m_hb_font_cache{};

        Rc<DWriteFontFace> m_fallback_undef_font{};
        Rc<OneSpaceTextAnalysisSource> m_one_space_analysis_source{};
        void ReBuild(Layout* layout, CtxNodeRef node);

        COPLT_IMPL_START
        COPLT_IMPL_END

        const Rc<DWriteFontFace>& GetFallbackUndefFont();

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
        Rc<DWriteFontFace> Font;
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

    struct RunBreakLineIter;

    struct RunBreakLineCtx
    {
        u32 NthLine{};
        f32 CurrentLineOffset{};
        f32 AvailableSpace{};
    };

    struct Run
    {
        u32 Start;
        u32 Length;
        u32 ScriptRangeIndex;
        u32 BidiRangeIndex;
        u32 FontRangeIndex;
        u32 StyleRangeIndex;

        u32 ClusterStartIndex;
        u32 GlyphStartIndex;
        u32 ActualGlyphCount;

        bool HasLineInfo;
        ParagraphLineInfo LineInfo;

        std::span<const u16> ClusterMap(const ParagraphData& data) const;
        std::span<const DWRITE_SHAPING_TEXT_PROPERTIES> TextProps(const ParagraphData& data) const;
        std::span<const u16> GlyphIndices(const ParagraphData& data) const;
        std::span<const DWRITE_SHAPING_GLYPH_PROPERTIES> GlyphProps(const ParagraphData& data) const;
        std::span<const f32> GlyphAdvances(const ParagraphData& data) const;
        std::span<const DWRITE_GLYPH_OFFSET> GlyphOffsets(const ParagraphData& data) const;

        bool IsInlineBlock(const ParagraphData& data) const;
        const ParagraphLineInfo& GetLineInfo(const ParagraphData& data);
        void SplitSpans(std::vector<ParagraphSpan>& spans, const ParagraphData& data, const StyleData& style);
        #ifdef _DEBUG
        std::vector<ParagraphLineSpan> BreakLines(
            const ParagraphData& data, const StyleData& style, RunBreakLineCtx& ctx
        ) const;
        #else
        std::generator<ParagraphLineSpan> BreakLines(
            const ParagraphData& data, const StyleData& style, RunBreakLineCtx& ctx
        ) const;
        #endif
    };

    struct ParagraphData
    {
        Layout* m_layout{};
        TextLayout* m_text_layout{};

        explicit ParagraphData(TextLayout* text_layout);

        const LoggerData& Logger() const;

        u32 m_index{};

        Rc<TextAnalysisSource> m_src{};
        Rc<TextAnalysisSink> m_sink{};

        std::vector<char16> m_chars{};
        std::vector<ScriptRange> m_script_ranges{};
        std::vector<BidiRange> m_bidi_ranges{};
        std::vector<DWRITE_LINE_BREAKPOINT> m_line_breakpoints{};
        std::vector<FontRange> m_font_ranges{};
        std::vector<FontRange> m_font_ranges_tmp{};
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

        void CollectChars();
        void AnalyzeFonts();
        void AnalyzeStyles();
        void CollectRuns();
        void AnalyzeGlyphsFirst();
        // void AnalyzeGlyphsCarets();

        LayoutOutput Compute(
            TextLayout& layout, LayoutRunMode RunMode, LayoutRequestedAxis Axis,
            const Size<AvailableSpace>& AvailableSpace, const Size<std::optional<f32>>& KnownSize
        );
    };
} // namespace Coplt
