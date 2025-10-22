#pragma once
#ifndef COPLT_UI_TYPES_H
#define COPLT_UI_TYPES_H

#include "CoCom.h"

namespace Coplt {

    struct Str16;

    struct Str8;

    template <class T0 /* T */>
    struct NativeBox;

    template <class T0 /* T */>
    struct NativeList;

    struct GridPlacement;

    struct GridTemplateArea;

    struct GridTemplateComponent;

    union GridTemplateComponentUnion;

    struct GridTemplateRepetition;

    struct SizingValue;

    struct TrackSizingFunction;

    template <class T0 /* T */>
    struct FFIOrderedSetNode;

    template <class T0 /* T */>
    struct FFIOrderedSet;

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

    struct FontWidth;

    struct FontMetrics;

    struct ChildsData;

    struct CommonLayoutData;

    struct CommonStyleData;

    struct GridContainerStyleData;

    struct GridContainerStyleInner;

    struct RootData;

    struct TextStyleData;

    struct NodeId;

    struct NodeLocate;

    struct IFont;

    struct IFontCollection;

    struct IFontFace;

    struct IFontFamily;

    struct ILayout;

    struct ILib;

    struct IStub;

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

    enum class AvailableSpaceType : ::Coplt::i32
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

    enum class FontStyle : ::Coplt::u8
    {
        Normal = 0,
        Italic = 1,
        Oblique = 2,
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

    enum class Overflow : ::Coplt::u8
    {
        Visible = 0,
        Clip = 1,
        Hidden = 2,
    };

    enum class Position : ::Coplt::u8
    {
        Relative = 0,
        Absolute = 1,
    };

    enum class TextAlign : ::Coplt::u8
    {
        Auto = 0,
        Left = 1,
        Right = 2,
        Center = 3,
    };

    enum class Visible : ::Coplt::u8
    {
        Visible = 0,
        Hidden = 1,
        Remove = 2,
    };

    COPLT_ENUM_FLAGS(FontFlags, ::Coplt::i32)
    {
        None = 0,
        Color = 1,
        Monospaced = 2,
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
        ::Coplt::NativeList<::Coplt::NativeList<::Coplt::i32>> LineIds;
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
    struct NativeBox
    {
        T0* m_ptr;
    };

    struct GridPlacement
    {
        ::Coplt::i32 Value0;
        ::Coplt::i16 Value1;
        ::Coplt::GridPlacementType Type;
    };

    union GridTemplateComponentUnion
    {
        ::Coplt::TrackSizingFunction Single;
        ::Coplt::GridTemplateRepetition Repeat;
    };

    template <class T0 /* T */>
    struct FFIOrderedSet
    {
        ::Coplt::i32* m_buckets;
        ::Coplt::FFIOrderedSetNode<T0>* m_nodes;
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
        bool HasFinalLayoutEntry;
        bool HasMeasureEntries0;
        bool HasMeasureEntries1;
        bool HasMeasureEntries2;
        bool HasMeasureEntries3;
        bool HasMeasureEntries4;
        bool HasMeasureEntries5;
        bool HasMeasureEntries6;
        bool HasMeasureEntries7;
        bool HasMeasureEntries8;
        bool IsEmpty;
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
        ::Coplt::u32 Id;
        ::Coplt::u32 VersionAndType;
    };

    struct Str8
    {
        ::Coplt::u8 const* Data;
        ::Coplt::u32 Size;
    };

    struct GridTemplateArea
    {
        ::Coplt::i32 Id;
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

    template <class T0 /* T */>
    struct FFIOrderedSetNode
    {
        ::Coplt::i32 HashCode;
        ::Coplt::i32 Next;
        ::Coplt::i32 OrderNext;
        ::Coplt::i32 OrderPrev;
        T0 Value;
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
        ::Coplt::FontStyle Style;
        ::Coplt::FontFlags Flags;
    };

    struct NFontPair
    {
        IFont* Font;
        ::Coplt::NFontInfo* Info;
    };

    struct NLayoutContext
    {
        ::Coplt::i32* roots;
        ::Coplt::i32* view_buckets;
        ::Coplt::i32* text_buckets;
        ::Coplt::i32* root_buckets;
        ::Coplt::NNodeIdCtrl* view_ctrl;
        ::Coplt::NNodeIdCtrl* text_ctrl;
        ::Coplt::NNodeIdCtrl* root_ctrl;
        ::Coplt::CommonLayoutData* view_layout_data;
        ::Coplt::CommonLayoutData* text_layout_data;
        ::Coplt::CommonLayoutData* root_layout_data;
        ::Coplt::CommonStyleData* view_common_style_data;
        ::Coplt::CommonStyleData* text_common_style_data;
        ::Coplt::CommonStyleData* root_common_style_data;
        ::Coplt::ChildsData* view_childs_data;
        void* _pad_0;
        ::Coplt::ChildsData* root_childs_data;
        ::Coplt::GridContainerStyleData* view_grid_container_style_data;
        void* _pad_1;
        ::Coplt::GridContainerStyleData* root_grid_container_style_data;
        ::Coplt::TextStyleData* text_style_data;
        ::Coplt::RootData* root_root_data;
        ::Coplt::i32 root_count;
        ::Coplt::i32 view_count;
        ::Coplt::i32 text_count;
        bool rounding;
    };

    struct NNodeIdCtrl
    {
        ::Coplt::i32 HashCode;
        ::Coplt::i32 Next;
        ::Coplt::NodeId Key;
    };

    struct ChildsData
    {
        ::Coplt::FFIOrderedSet<::Coplt::NodeLocate> m_childs;
    };

    struct CommonLayoutData
    {
        ::Coplt::LayoutData Layout;
        ::Coplt::LayoutData FinalLayout;
        ::Coplt::LayoutCache LayoutCache;
    };

    struct CommonStyleData
    {
        ::Coplt::i32 ZIndex;
        ::Coplt::f32 Opacity;
        ::Coplt::f32 ScrollBarSize;
        ::Coplt::f32 BoxColorR;
        ::Coplt::f32 BoxColorG;
        ::Coplt::f32 BoxColorB;
        ::Coplt::f32 BoxColorA;
        ::Coplt::f32 InsertTopValue;
        ::Coplt::f32 InsertRightValue;
        ::Coplt::f32 InsertBottomValue;
        ::Coplt::f32 InsertLeftValue;
        ::Coplt::f32 WidthValue;
        ::Coplt::f32 HeightValue;
        ::Coplt::f32 MinWidthValue;
        ::Coplt::f32 MinHeightValue;
        ::Coplt::f32 MaxWidthValue;
        ::Coplt::f32 MaxHeightValue;
        ::Coplt::f32 AspectRatioValue;
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
        ::Coplt::Visible Visible;
        ::Coplt::Container Container;
        ::Coplt::BoxSizing BoxSizing;
        ::Coplt::Overflow OverflowX;
        ::Coplt::Overflow OverflowY;
        ::Coplt::Position Position;
        ::Coplt::LengthType InsertTop;
        ::Coplt::LengthType InsertRight;
        ::Coplt::LengthType InsertBottom;
        ::Coplt::LengthType InsertLeft;
        ::Coplt::LengthType Width;
        ::Coplt::LengthType Height;
        ::Coplt::LengthType MinWidth;
        ::Coplt::LengthType MinHeight;
        ::Coplt::LengthType MaxWidth;
        ::Coplt::LengthType MaxHeight;
        bool HasAspectRatio;
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
        ::Coplt::AlignType AlignItems;
        ::Coplt::AlignType AlignSelf;
        ::Coplt::AlignType JustifyItems;
        ::Coplt::AlignType JustifySelf;
        ::Coplt::AlignType AlignContent;
        ::Coplt::AlignType JustifyContent;
        ::Coplt::LengthType GapX;
        ::Coplt::LengthType GapY;
        ::Coplt::FlexDirection FlexDirection;
        ::Coplt::FlexWrap FlexWrap;
        ::Coplt::LengthType FlexBasis;
        ::Coplt::GridAutoFlow GridAutoFlow;
        ::Coplt::TextAlign TextAlign;
        ::Coplt::GridPlacement GridRowStart;
        ::Coplt::GridPlacement GridRowEnd;
        ::Coplt::GridPlacement GridColumnStart;
        ::Coplt::GridPlacement GridColumnEnd;
    };

    struct GridContainerStyleData
    {
        ::Coplt::NativeBox<::Coplt::GridContainerStyleInner> Inner;
    };

    struct GridContainerStyleInner
    {
        ::Coplt::NativeList<::Coplt::GridTemplateComponent> GridTemplateRows;
        ::Coplt::NativeList<::Coplt::GridTemplateComponent> GridTemplateColumns;
        ::Coplt::NativeList<::Coplt::TrackSizingFunction> GridAutoRows;
        ::Coplt::NativeList<::Coplt::TrackSizingFunction> GridAutoColumns;
        ::Coplt::NativeList<::Coplt::GridTemplateArea> GridTemplateAreas;
        ::Coplt::NativeList<::Coplt::NativeList<::Coplt::i32>> GridTemplateColumnNames;
        ::Coplt::NativeList<::Coplt::NativeList<::Coplt::i32>> GridTemplateRowNames;
    };

    struct RootData
    {
        ::Coplt::f32 AvailableSpaceXValue;
        ::Coplt::f32 AvailableSpaceYValue;
        ::Coplt::AvailableSpaceType AvailableSpaceX;
        ::Coplt::AvailableSpaceType AvailableSpaceY;
        bool UseRounding;
    };

    struct TextStyleData
    {
        ::Coplt::f32 TextColorR;
        ::Coplt::f32 TextColorG;
        ::Coplt::f32 TextColorB;
        ::Coplt::f32 TextColorA;
        ::Coplt::f32 TextSizeValue;
        ::Coplt::LengthType TextSize;
    };

    struct NodeLocate
    {
        ::Coplt::NodeId Id;
        ::Coplt::i32 Index;
    };

} // namespace Coplt

#endif //COPLT_UI_TYPES_H
