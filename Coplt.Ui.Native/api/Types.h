#pragma once
#ifndef COPLT_UI_TYPES_H
#define COPLT_UI_TYPES_H

#include "CoCom.h"

namespace Coplt {

    struct Str16;

    struct Str8;

    template <class T0 /* T */>
    struct NativeArcInner;

    template <class T0 /* T */>
    struct NativeArc;

    template <class T0 /* T */>
    struct NativeList;

    struct GridName;

    struct GridPlacement;

    struct GridTemplateArea;

    struct GridTemplateComponent;

    union GridTemplateComponentUnion;

    struct GridTemplateRepetition;

    struct SizingValue;

    struct TrackSizingFunction;

    struct CWStr;

    struct FFIMap;

    struct FFIOrderedSet;

    struct FontFallbackBuilderCreateInfo;

    struct FontFamilyNameInfo;

    struct LayoutCache;

    struct LayoutCacheEntryLayoutOutput;

    struct LayoutCacheEntrySize;

    struct LayoutCollapsibleMarginSet;

    struct LayoutData;

    struct LayoutOutput;

    struct NFontInfo;

    struct NFontPair;

    struct NLayoutContext;

    struct NNodeIdCtrl;

    struct NString;

    struct FontWidth;

    struct LocaleId;

    struct FontMetrics;

    struct TextRange;

    struct ChildsData;

    struct CommonData;

    struct GridContainerStyle;

    struct RootData;

    struct StyleData;

    struct NodeId;

    struct IFont;

    struct IFontCollection;

    struct IFontFace;

    struct IFontFallback;

    struct IFontFallbackBuilder;

    struct IFontFamily;

    struct IFontManager;

    struct ILayout;

    struct ILib;

    struct IStub;

    struct ITextData;

    struct ITextLayout;

    enum class AlignType : ::Coplt::u8
    {
        None = 0,
        Start = 1,
        End = 2,
        FlexStart = 3,
        FlexEnd = 4,
        Center = 5,
        Baseline = 6,
        Stretch = 7,
        SpaceBetween = 8,
        SpaceEvenly = 9,
        SpaceAround = 10,
    };

    enum class GridNameType : ::Coplt::u8
    {
        Name = 0,
        Start = 1,
        End = 2,
    };

    enum class GridPlacementType : ::Coplt::u8
    {
        Auto = 0,
        Line = 1,
        NamedLine = 2,
        Span = 3,
        NamedSpan = 4,
    };

    enum class GridTemplateComponentType : ::Coplt::u8
    {
        Single = 0,
        Repeat = 1,
    };

    enum class LengthType : ::Coplt::u8
    {
        Fixed = 0,
        Percent = 1,
        Auto = 2,
    };

    enum class RepetitionType : ::Coplt::u8
    {
        Count = 0,
        AutoFill = 1,
        AutoFit = 2,
    };

    enum class SizingType : ::Coplt::u8
    {
        Auto = 0,
        Fixed = 1,
        Percent = 2,
        Fraction = 3,
        MinContent = 4,
        MaxContent = 5,
        FitContent = 6,
    };

    enum class AvailableSpaceType : ::Coplt::u8
    {
        Definite = 0,
        MinContent = 1,
        MaxContent = 2,
    };

    enum class LogLevel : ::Coplt::u8
    {
        Fatal = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4,
        Verbose = 5,
    };

    COPLT_ENUM_FLAGS(LayoutCacheFlags, ::Coplt::u16)
    {
        Empty = 0,
        Final = 1,
        Measure0 = 2,
        Measure1 = 4,
        Measure2 = 8,
        Measure3 = 16,
        Measure4 = 32,
        Measure5 = 64,
        Measure6 = 128,
        Measure7 = 256,
        Measure8 = 512,
    };

    enum class BoxSizing : ::Coplt::u8
    {
        BorderBox = 0,
        ContentBox = 1,
    };

    enum class Container : ::Coplt::u8
    {
        Flex = 0,
        Grid = 1,
        Text = 2,
        Block = 3,
    };

    enum class CursorType : ::Coplt::u8
    {
        Default = 0,
        Pointer = 1,
        ContextMenu = 2,
        Help = 3,
        Progress = 4,
        Wait = 5,
        Cell = 6,
        Crosshair = 7,
        Text = 8,
        VerticalText = 9,
        Alias = 10,
        Copy = 11,
        Move = 12,
        NoDrop = 13,
        NotAllowed = 14,
        Grab = 15,
        Grabbing = 16,
        AllScroll = 17,
        ColResize = 18,
        RowResize = 19,
        NResize = 20,
        EResize = 21,
        SResize = 22,
        WResize = 23,
        NeResize = 24,
        NwResize = 25,
        SeResize = 26,
        SwResize = 27,
        EwResize = 28,
        NsResize = 29,
        NeSwResize = 30,
        NwSeResize = 31,
        ZoomIn = 32,
        ZoomOut = 33,
    };

    enum class FlexDirection : ::Coplt::u8
    {
        Column = 0,
        Row = 1,
        ColumnReverse = 2,
        RowReverse = 3,
    };

    enum class FlexWrap : ::Coplt::u8
    {
        NoWrap = 0,
        Wrap = 1,
        WrapReverse = 2,
    };

    enum class FontWeight : ::Coplt::i32
    {
        None = 0,
        Thin = 100,
        ExtraLight = 200,
        Light = 300,
        SemiLight = 350,
        Normal = 400,
        Medium = 500,
        SemiBold = 600,
        Bold = 700,
        ExtraBold = 800,
        Black = 900,
        ExtraBlack = 950,
    };

    enum class GridAutoFlow : ::Coplt::u8
    {
        Row = 0,
        Column = 1,
        RowDense = 2,
        ColumnDense = 3,
    };

    enum class LineAlign : ::Coplt::u8
    {
        Baseline = 0,
        Start = 1,
        End = 2,
        Center = 3,
    };

    enum class LocaleMode : ::Coplt::u8
    {
        Normal = 0,
        ByScript = 1,
    };

    enum class Overflow : ::Coplt::u8
    {
        Visible = 0,
        Clip = 1,
        Hidden = 2,
    };

    enum class PointerEvents : ::Coplt::u8
    {
        Auto = 0,
        None = 1,
    };

    enum class Position : ::Coplt::u8
    {
        Relative = 0,
        Absolute = 1,
    };

    enum class TextAlign : ::Coplt::u8
    {
        Start = 0,
        End = 1,
        Center = 2,
    };

    enum class TextDirection : ::Coplt::u8
    {
        Forward = 0,
        Reverse = 1,
        LeftToRight = 2,
        RightToLeft = 3,
    };

    enum class TextMode : ::Coplt::u8
    {
        Block = 0,
        Inline = 1,
    };

    enum class TextOrientation : ::Coplt::u8
    {
        Mixed = 0,
        Upright = 1,
        Sideways = 2,
    };

    enum class TextOverflow : ::Coplt::u8
    {
        Clip = 0,
        Ellipsis = 1,
    };

    enum class TextWrap : ::Coplt::u8
    {
        Wrap = 0,
        NoWrap = 1,
    };

    enum class Visible : ::Coplt::u8
    {
        Visible = 0,
        Hidden = 1,
        Remove = 2,
    };

    enum class WhiteSpace : ::Coplt::u8
    {
        Normal = 0,
        NoWrap = 1,
        Pre = 2,
        PreWrap = 3,
        PreLine = 4,
        BreakSpaces = 5,
    };

    enum class WordBreak : ::Coplt::u8
    {
        Auto = 0,
        BreakAll = 1,
        KeepAll = 2,
    };

    enum class WritingDirection : ::Coplt::u8
    {
        Horizontal = 0,
        Vertical = 1,
    };

    enum class CharCategory : ::Coplt::u8
    {
        Unassigned = 0,
        GeneralOtherTypes = 0,
        UppercaseLetter = 1,
        LowercaseLetter = 2,
        TitlecaseLetter = 3,
        ModifierLetter = 4,
        OtherLetter = 5,
        NonSpacingMark = 6,
        EnclosingMark = 7,
        CombiningSpacingMark = 8,
        DecimalDigitNumber = 9,
        LetterNumber = 10,
        OtherNumber = 11,
        SpaceSeparator = 12,
        LineSeparator = 13,
        ParagraphSeparator = 14,
        ControlChar = 15,
        FormatChar = 16,
        PrivateUseChar = 17,
        Surrogate = 18,
        DashPunctuation = 19,
        StartPunctuation = 20,
        EndPunctuation = 21,
        ConnectorPunctuation = 22,
        OtherPunctuation = 23,
        MathSymbol = 24,
        CurrencySymbol = 25,
        ModifierSymbol = 26,
        OtherSymbol = 27,
        InitialPunctuation = 28,
        FinalPunctuation = 29,
    };

    COPLT_ENUM_FLAGS(FontFlags, ::Coplt::i32)
    {
        None = 0,
        Color = 1,
        Monospaced = 2,
    };

    enum class ScriptCode : ::Coplt::i32
    {
        InvalidCode = -1,
        Common = 0,
        Inherited = 1,
        Arabic = 2,
        Armenian = 3,
        Bengali = 4,
        Bopomofo = 5,
        Cherokee = 6,
        Coptic = 7,
        Cyrillic = 8,
        Deseret = 9,
        Devanagari = 10,
        Ethiopic = 11,
        Georgian = 12,
        Gothic = 13,
        Greek = 14,
        Gujarati = 15,
        Gurmukhi = 16,
        Han = 17,
        Hangul = 18,
        Hebrew = 19,
        Hiragana = 20,
        Kannada = 21,
        Katakana = 22,
        Khmer = 23,
        Lao = 24,
        Latin = 25,
        Malayalam = 26,
        Mongolian = 27,
        Myanmar = 28,
        Ogham = 29,
        OldItalic = 30,
        Oriya = 31,
        Runic = 32,
        Sinhala = 33,
        Syriac = 34,
        Tamil = 35,
        Telugu = 36,
        Thaana = 37,
        Thai = 38,
        Tibetan = 39,
        CanadianAboriginal = 40,
        Ucas = 40,
        Yi = 41,
        Tagalog = 42,
        Hanunoo = 43,
        Buhid = 44,
        Tagbanwa = 45,
        Braille = 46,
        Cypriot = 47,
        Limbu = 48,
        LinearB = 49,
        Osmanya = 50,
        Shavian = 51,
        TaiLe = 52,
        Ugaritic = 53,
        KatakanaOrHiragana = 54,
        Buginese = 55,
        Glagolitic = 56,
        Kharoshthi = 57,
        SylotiNagri = 58,
        NewTaiLue = 59,
        Tifinagh = 60,
        OldPersian = 61,
        Balinese = 62,
        Batak = 63,
        Blissymbols = 64,
        Brahmi = 65,
        Cham = 66,
        Cirth = 67,
        OldChurchSlavonicCyrillic = 68,
        DemoticEgyptian = 69,
        HieraticEgyptian = 70,
        EgyptianHieroglyphs = 71,
        Khutsuri = 72,
        SimplifiedHan = 73,
        TraditionalHan = 74,
        PahawhHmong = 75,
        OldHungarian = 76,
        HarappanIndus = 77,
        Javanese = 78,
        KayahLi = 79,
        LatinFraktur = 80,
        LatinGaelic = 81,
        Lepcha = 82,
        LinearA = 83,
        Mandaic = 84,
        Mandaean = 84,
        MayanHieroglyphs = 85,
        MeroiticHieroglyphs = 86,
        Meroitic = 86,
        Nko = 87,
        Orkhon = 88,
        OldPermic = 89,
        PhagsPa = 90,
        Phoenician = 91,
        Miao = 92,
        PhoneticPollard = 92,
        Rongorongo = 93,
        Sarati = 94,
        EstrangeloSyriac = 95,
        WesternSyriac = 96,
        EasternSyriac = 97,
        Tengwar = 98,
        Vai = 99,
        VisibleSpeech = 100,
        Cuneiform = 101,
        UnwrittenLanguages = 102,
        Unknown = 103,
        Carian = 104,
        Japanese = 105,
        Lanna = 106,
        Lycian = 107,
        Lydian = 108,
        OlChiki = 109,
        Rejang = 110,
        Saurashtra = 111,
        SignWriting = 112,
        Sundanese = 113,
        Moon = 114,
        MeiteiMayek = 115,
        ImperialAramaic = 116,
        Avestan = 117,
        Chakma = 118,
        Korean = 119,
        Kaithi = 120,
        Manichaean = 121,
        InscriptionalPahlavi = 122,
        PsalterPahlavi = 123,
        BookPahlavi = 124,
        InscriptionalParthian = 125,
        Samaritan = 126,
        TaiViet = 127,
        MathematicalNotation = 128,
        Symbols = 129,
        Bamum = 130,
        Lisu = 131,
        NakhiGeba = 132,
        OldSouthArabian = 133,
        BassaVah = 134,
        Duployan = 135,
        Elbasan = 136,
        Grantha = 137,
        Kpelle = 138,
        Loma = 139,
        Mende = 140,
        MeroiticCursive = 141,
        OldNorthArabian = 142,
        Nabataean = 143,
        Palmyrene = 144,
        Khudawadi = 145,
        Sindhi = 145,
        WarangCiti = 146,
        Afaka = 147,
        Jurchen = 148,
        Mro = 149,
        Nushu = 150,
        Sharada = 151,
        SoraSompeng = 152,
        Takri = 153,
        Tangut = 154,
        Woleai = 155,
        AnatolianHieroglyphs = 156,
        Khojki = 157,
        Tirhuta = 158,
        CaucasianAlbanian = 159,
        Mahajani = 160,
        Ahom = 161,
        Hatran = 162,
        Modi = 163,
        Multani = 164,
        PauCinHau = 165,
        Siddham = 166,
        Adlam = 167,
        Bhaiksuki = 168,
        Marchen = 169,
        Newa = 170,
        Osage = 171,
        HanWithBopomofo = 172,
        Jamo = 173,
        SymbolsEmoji = 174,
        MasaramGondi = 175,
        Soyombo = 176,
        ZanabazarSquare = 177,
        Dogra = 178,
        GunjalaGondi = 179,
        Makasar = 180,
        Medefaidrin = 181,
        HanifiRohingya = 182,
        Sogdian = 183,
        OldSogdian = 184,
        Elymaic = 185,
        NyiakengPuachueHmong = 186,
        Nandinagari = 187,
        Wancho = 188,
        Chorasmian = 189,
        DivesAkuru = 190,
        KhitanSmallScript = 191,
        Yezidi = 192,
        CyproMinoan = 193,
        OldUyghur = 194,
        Tangsa = 195,
        Toto = 196,
        Vithkuqi = 197,
        Kawi = 198,
        NagMundari = 199,
        ArabicNastaliq = 200,
        Garay = 201,
        GurungKhema = 202,
        KiratRai = 203,
        OlOnal = 204,
        Sunuwar = 205,
        Todhri = 206,
        TuluTigalari = 207,
        BeriaErfe = 208,
        Sidetic = 209,
        TaiYo = 210,
        TolongSiki = 211,
        TraditionalHanWithLatin = 212,
    };

    enum class NodeType : ::Coplt::u8
    {
        View = 0,
        Text = 1,
    };

    struct LayoutCollapsibleMarginSet
    {
        ::Coplt::f32 Positive;
        ::Coplt::f32 Negative;
    };

    template <class T0 /* T */>
    struct NativeList
    {
        T0* m_items;
        ::Coplt::i32 m_cap;
        ::Coplt::i32 m_size;
    };

    struct SizingValue
    {
        ::Coplt::f32 Value;
        ::Coplt::LengthType Type;
    };

    struct LayoutOutput
    {
        ::Coplt::f32 Width;
        ::Coplt::f32 Height;
        ::Coplt::f32 ContentWidth;
        ::Coplt::f32 ContentHeight;
        ::Coplt::f32 FirstBaselinesX;
        ::Coplt::f32 FirstBaselinesY;
        ::Coplt::LayoutCollapsibleMarginSet TopMargin;
        ::Coplt::LayoutCollapsibleMarginSet BottomMargin;
        bool HasFirstBaselinesX;
        bool HasFirstBaselinesY;
        bool MarginsCanCollapseThrough;
    };

    struct GridTemplateRepetition
    {
        ::Coplt::NativeList<::Coplt::TrackSizingFunction> Tracks;
        ::Coplt::NativeList<::Coplt::NativeList<::Coplt::GridName>> LineIds;
        ::Coplt::u16 RepetitionValue;
        ::Coplt::RepetitionType Repetition;
    };

    struct TrackSizingFunction
    {
        ::Coplt::SizingValue MinValue;
        ::Coplt::SizingValue MaxValue;
        ::Coplt::SizingType Min;
        ::Coplt::SizingType Max;
    };

    struct LayoutCacheEntryLayoutOutput
    {
        ::Coplt::f32 KnownDimensionsWidthValue;
        ::Coplt::f32 KnownDimensionsHeightValue;
        ::Coplt::f32 AvailableSpaceWidthValue;
        ::Coplt::f32 AvailableSpaceHeightValue;
        bool HasKnownDimensionsWidth;
        bool HasKnownDimensionsHeight;
        ::Coplt::AvailableSpaceType AvailableSpaceWidth;
        ::Coplt::AvailableSpaceType AvailableSpaceHeight;
        ::Coplt::LayoutOutput Content;
    };

    struct LayoutCacheEntrySize
    {
        ::Coplt::f32 KnownDimensionsWidthValue;
        ::Coplt::f32 KnownDimensionsHeightValue;
        ::Coplt::f32 AvailableSpaceWidthValue;
        ::Coplt::f32 AvailableSpaceHeightValue;
        bool HasKnownDimensionsWidth;
        bool HasKnownDimensionsHeight;
        ::Coplt::AvailableSpaceType AvailableSpaceWidth;
        ::Coplt::AvailableSpaceType AvailableSpaceHeight;
        ::Coplt::f32 ContentWidth;
        ::Coplt::f32 ContentHeight;
    };

    struct Str16
    {
        ::Coplt::char16 const* Data;
        ::Coplt::u32 Size;
    };

    template <class T0 /* T */>
    struct NativeArc
    {
        ::Coplt::NativeArcInner<T0>* m_ptr;
    };

    struct GridName
    {
        ::Coplt::i32 Id;
        ::Coplt::GridNameType Type;
    };

    struct GridPlacement
    {
        ::Coplt::i32 Name;
        ::Coplt::i16 Value1;
        ::Coplt::GridNameType NameType;
        ::Coplt::GridPlacementType Type;
    };

    union GridTemplateComponentUnion
    {
        ::Coplt::TrackSizingFunction Single;
        ::Coplt::GridTemplateRepetition Repeat;
    };

    struct CWStr
    {
        ::Coplt::char16 const* Locale;
    };

    struct FFIMap
    {
        ::Coplt::i32* m_buckets;
        void* m_entries;
        ::Coplt::u64 m_fast_mode_multiplier;
        ::Coplt::i32 m_cap;
        ::Coplt::i32 m_count;
        ::Coplt::i32 m_free_list;
        ::Coplt::i32 m_free_count;
    };

    struct FFIOrderedSet
    {
        ::Coplt::i32* m_buckets;
        void* m_nodes;
        ::Coplt::u64 m_fast_mode_multiplier;
        ::Coplt::i32 m_cap;
        ::Coplt::i32 m_first;
        ::Coplt::i32 m_last;
        ::Coplt::i32 m_count;
        ::Coplt::i32 m_free_list;
        ::Coplt::i32 m_free_count;
    };

    struct LayoutCache
    {
        ::Coplt::LayoutCacheEntryLayoutOutput FinalLayoutEntry;
        ::Coplt::LayoutCacheEntrySize MeasureEntries0;
        ::Coplt::LayoutCacheEntrySize MeasureEntries1;
        ::Coplt::LayoutCacheEntrySize MeasureEntries2;
        ::Coplt::LayoutCacheEntrySize MeasureEntries3;
        ::Coplt::LayoutCacheEntrySize MeasureEntries4;
        ::Coplt::LayoutCacheEntrySize MeasureEntries5;
        ::Coplt::LayoutCacheEntrySize MeasureEntries6;
        ::Coplt::LayoutCacheEntrySize MeasureEntries7;
        ::Coplt::LayoutCacheEntrySize MeasureEntries8;
        ::Coplt::LayoutCacheFlags Flags;
    };

    struct LayoutData
    {
        ::Coplt::u32 Order;
        ::Coplt::f32 LocationX;
        ::Coplt::f32 LocationY;
        ::Coplt::f32 Width;
        ::Coplt::f32 Height;
        ::Coplt::f32 ContentWidth;
        ::Coplt::f32 ContentHeight;
        ::Coplt::f32 ScrollXSize;
        ::Coplt::f32 ScrollYSize;
        ::Coplt::f32 BorderTopSize;
        ::Coplt::f32 BorderRightSize;
        ::Coplt::f32 BorderBottomSize;
        ::Coplt::f32 BorderLeftSize;
        ::Coplt::f32 PaddingTopSize;
        ::Coplt::f32 PaddingRightSize;
        ::Coplt::f32 PaddingBottomSize;
        ::Coplt::f32 PaddingLeftSize;
        ::Coplt::f32 MarginTopSize;
        ::Coplt::f32 MarginRightSize;
        ::Coplt::f32 MarginBottomSize;
        ::Coplt::f32 MarginLeftSize;
    };

    struct FontWidth
    {
        ::Coplt::f32 Width;
    };

    struct LocaleId
    {
        ::Coplt::char16* Name;
    };

    struct FontMetrics
    {
        ::Coplt::f32 Ascent;
        ::Coplt::f32 Descent;
        ::Coplt::f32 Leading;
        ::Coplt::f32 LineHeight;
        ::Coplt::u16 UnitsPerEm;
    };

    struct NodeId
    {
        ::Coplt::u32 Index;
        ::Coplt::u32 IdAndType;
    };

    struct Str8
    {
        ::Coplt::u8 const* Data;
        ::Coplt::u32 Size;
    };

    template <class T0 /* T */>
    struct NativeArcInner
    {
        ::Coplt::u64 m_count;
        T0 m_data;
    };

    struct GridTemplateArea
    {
        ::Coplt::GridName Id;
        ::Coplt::u16 RowStart;
        ::Coplt::u16 RowEnd;
        ::Coplt::u16 ColumnStart;
        ::Coplt::u16 ColumnEnd;
    };

    struct GridTemplateComponent
    {
        ::Coplt::GridTemplateComponentUnion Union;
        ::Coplt::GridTemplateComponentType Type;
    };

    struct FontFallbackBuilderCreateInfo
    {
        bool DisableSystemFallback;
    };

    struct FontFamilyNameInfo
    {
        ::Coplt::Str16 Name;
        ::Coplt::u32 Local;
    };

    struct NFontInfo
    {
        ::Coplt::FontMetrics Metrics;
        ::Coplt::FontWidth Width;
        ::Coplt::FontWeight Weight;
        ::Coplt::FontFlags Flags;
    };

    struct NFontPair
    {
        IFont* Font;
        ::Coplt::NFontInfo* Info;
    };

    struct NLayoutContext
    {
        IFontManager* font_manager;
        ::Coplt::FFIMap* roots;
        ::Coplt::i32* node_buckets;
        ::Coplt::NNodeIdCtrl* node_ctrl;
        ::Coplt::CommonData* node_common_data;
        ::Coplt::ChildsData* node_childs_data;
        ::Coplt::StyleData* node_style_data;
        ::Coplt::i32 node_count;
        bool rounding;
    };

    struct NNodeIdCtrl
    {
        ::Coplt::i32 HashCode;
        ::Coplt::i32 Next;
        ::Coplt::NodeId Key;
    };

    struct NString
    {
        ::Coplt::char16 const* m_str;
        void* m_handle;
        ::Coplt::i32 m_len;
    };

    struct TextRange
    {
        ::Coplt::CWStr Locale;
        ::Coplt::i32 Start;
        ::Coplt::i32 Length;
        ::Coplt::ScriptCode Script;
        ::Coplt::CharCategory Category;
        bool ScriptIsRtl;
    };

    struct ChildsData
    {
        ::Coplt::FFIOrderedSet m_childs;
        ::Coplt::FFIMap m_texts;
        ::Coplt::u32 m_text_id_inc;
        ::Coplt::u64 m_version;
        ::Coplt::u64 m_last_version;
    };

    struct CommonData
    {
        ITextLayout* TextLayoutObject;
        ITextLayout* TextLayoutBelongTo;
        ::Coplt::LayoutData FinalLayout;
        ::Coplt::LayoutData UnRoundedLayout;
        ::Coplt::LayoutCache LayoutCache;
        ::Coplt::u32 LastLayoutVersion;
        ::Coplt::u32 LastTextLayoutVersion;
        ::Coplt::u32 LayoutVersion;
        ::Coplt::u32 TextLayoutVersion;
    };

    struct GridContainerStyle
    {
        ::Coplt::NativeList<::Coplt::GridTemplateComponent> GridTemplateRows;
        ::Coplt::NativeList<::Coplt::GridTemplateComponent> GridTemplateColumns;
        ::Coplt::NativeList<::Coplt::TrackSizingFunction> GridAutoRows;
        ::Coplt::NativeList<::Coplt::TrackSizingFunction> GridAutoColumns;
        ::Coplt::NativeList<::Coplt::GridTemplateArea> GridTemplateAreas;
        ::Coplt::NativeList<::Coplt::NativeList<::Coplt::GridName>> GridTemplateColumnNames;
        ::Coplt::NativeList<::Coplt::NativeList<::Coplt::GridName>> GridTemplateRowNames;
    };

    struct RootData
    {
        ::Coplt::NodeId Node;
        ::Coplt::f32 AvailableSpaceXValue;
        ::Coplt::f32 AvailableSpaceYValue;
        ::Coplt::AvailableSpaceType AvailableSpaceX;
        ::Coplt::AvailableSpaceType AvailableSpaceY;
        bool UseRounding;
    };

    struct StyleData
    {
        ::Coplt::NativeArc<::Coplt::GridContainerStyle> Grid;
        IFontFallback* FontFallback;
        ::Coplt::LocaleId Locale;
        ::Coplt::i32 ZIndex;
        ::Coplt::f32 TextColorR;
        ::Coplt::f32 TextColorG;
        ::Coplt::f32 TextColorB;
        ::Coplt::f32 TextColorA;
        ::Coplt::f32 Opacity;
        ::Coplt::f32 BackgroundColorR;
        ::Coplt::f32 BackgroundColorG;
        ::Coplt::f32 BackgroundColorB;
        ::Coplt::f32 BackgroundColorA;
        ::Coplt::f32 ScrollBarSize;
        ::Coplt::f32 WidthValue;
        ::Coplt::f32 HeightValue;
        ::Coplt::f32 MinWidthValue;
        ::Coplt::f32 MinHeightValue;
        ::Coplt::f32 MaxWidthValue;
        ::Coplt::f32 MaxHeightValue;
        ::Coplt::f32 AspectRatioValue;
        ::Coplt::f32 InsertTopValue;
        ::Coplt::f32 InsertRightValue;
        ::Coplt::f32 InsertBottomValue;
        ::Coplt::f32 InsertLeftValue;
        ::Coplt::f32 MarginTopValue;
        ::Coplt::f32 MarginRightValue;
        ::Coplt::f32 MarginBottomValue;
        ::Coplt::f32 MarginLeftValue;
        ::Coplt::f32 PaddingTopValue;
        ::Coplt::f32 PaddingRightValue;
        ::Coplt::f32 PaddingBottomValue;
        ::Coplt::f32 PaddingLeftValue;
        ::Coplt::f32 BorderTopValue;
        ::Coplt::f32 BorderRightValue;
        ::Coplt::f32 BorderBottomValue;
        ::Coplt::f32 BorderLeftValue;
        ::Coplt::f32 GapXValue;
        ::Coplt::f32 GapYValue;
        ::Coplt::f32 FlexGrow;
        ::Coplt::f32 FlexShrink;
        ::Coplt::f32 FlexBasisValue;
        ::Coplt::f32 TabSizeValue;
        ::Coplt::f32 FontSize;
        ::Coplt::FontWidth FontWidth;
        ::Coplt::f32 FontOblique;
        ::Coplt::FontWeight FontWeight;
        ::Coplt::f32 LineHeightValue;
        ::Coplt::GridPlacement GridRowStart;
        ::Coplt::GridPlacement GridRowEnd;
        ::Coplt::GridPlacement GridColumnStart;
        ::Coplt::GridPlacement GridColumnEnd;
        ::Coplt::Visible Visible;
        ::Coplt::Position Position;
        ::Coplt::Container Container;
        ::Coplt::TextMode TextMode;
        ::Coplt::BoxSizing BoxSizing;
        ::Coplt::CursorType Cursor;
        ::Coplt::PointerEvents PointerEvents;
        ::Coplt::Overflow OverflowX;
        ::Coplt::Overflow OverflowY;
        ::Coplt::LengthType Width;
        ::Coplt::LengthType Height;
        ::Coplt::LengthType MinWidth;
        ::Coplt::LengthType MinHeight;
        ::Coplt::LengthType MaxWidth;
        ::Coplt::LengthType MaxHeight;
        ::Coplt::LengthType InsertTop;
        ::Coplt::LengthType InsertRight;
        ::Coplt::LengthType InsertBottom;
        ::Coplt::LengthType InsertLeft;
        ::Coplt::LengthType MarginTop;
        ::Coplt::LengthType MarginRight;
        ::Coplt::LengthType MarginBottom;
        ::Coplt::LengthType MarginLeft;
        ::Coplt::LengthType PaddingTop;
        ::Coplt::LengthType PaddingRight;
        ::Coplt::LengthType PaddingBottom;
        ::Coplt::LengthType PaddingLeft;
        ::Coplt::LengthType BorderTop;
        ::Coplt::LengthType BorderRight;
        ::Coplt::LengthType BorderBottom;
        ::Coplt::LengthType BorderLeft;
        bool HasAspectRatio;
        ::Coplt::FlexDirection FlexDirection;
        ::Coplt::FlexWrap FlexWrap;
        ::Coplt::GridAutoFlow GridAutoFlow;
        ::Coplt::LengthType GapX;
        ::Coplt::LengthType GapY;
        ::Coplt::AlignType AlignContent;
        ::Coplt::AlignType JustifyContent;
        ::Coplt::AlignType AlignItems;
        ::Coplt::AlignType JustifyItems;
        ::Coplt::AlignType AlignSelf;
        ::Coplt::AlignType JustifySelf;
        ::Coplt::LengthType FlexBasis;
        bool FontItalic;
        bool FontOpticalSizing;
        ::Coplt::TextAlign TextAlign;
        ::Coplt::LineAlign LineAlign;
        ::Coplt::LengthType TabSize;
        ::Coplt::LocaleMode LocaleMode;
        ::Coplt::TextDirection TextDirection;
        ::Coplt::WritingDirection WritingDirection;
        ::Coplt::WhiteSpace WhiteSpace;
        ::Coplt::TextWrap TextWrap;
        ::Coplt::WordBreak WordBreak;
        ::Coplt::TextOrientation TextOrientation;
        ::Coplt::TextOverflow TextOverflow;
        ::Coplt::LengthType LineHeight;
    };

} // namespace Coplt

#endif //COPLT_UI_TYPES_H
