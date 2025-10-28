#![allow(unused)]
#![allow(non_snake_case)]

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum AlignType {
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
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum GridNameType {
    Name = 0,
    Start = 1,
    End = 2,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum GridPlacementType {
    Auto = 0,
    Line = 1,
    NamedLine = 2,
    Span = 3,
    NamedSpan = 4,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum GridTemplateComponentType {
    Single = 0,
    Repeat = 1,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum LengthType {
    Fixed = 0,
    Percent = 1,
    Auto = 2,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum RepetitionType {
    Count = 0,
    AutoFill = 1,
    AutoFit = 2,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum SizingType {
    Auto = 0,
    Fixed = 1,
    Percent = 2,
    Fraction = 3,
    MinContent = 4,
    MaxContent = 5,
    FitContent = 6,
}

#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum AvailableSpaceType {
    Definite = 0,
    MinContent = 1,
    MaxContent = 2,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum LogLevel {
    Fatal = 0,
    Error = 1,
    Warning = 2,
    Info = 3,
    Debug = 4,
    Verbose = 5,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum BoxSizing {
    BorderBox = 0,
    ContentBox = 1,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum Container {
    Flex = 0,
    Grid = 1,
    Text = 2,
    Block = 3,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum FlexDirection {
    Column = 0,
    Row = 1,
    ColumnReverse = 2,
    RowReverse = 3,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum FlexWrap {
    NoWrap = 0,
    Wrap = 1,
    WrapReverse = 2,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum FontStyle {
    Normal = 0,
    Italic = 1,
    Oblique = 2,
}

#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum FontWeight {
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
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum GridAutoFlow {
    Row = 0,
    Column = 1,
    RowDense = 2,
    ColumnDense = 3,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum Overflow {
    Visible = 0,
    Clip = 1,
    Hidden = 2,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum Position {
    Relative = 0,
    Absolute = 1,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum TextAlign {
    Auto = 0,
    Left = 1,
    Right = 2,
    Center = 3,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum Visible {
    Visible = 0,
    Hidden = 1,
    Remove = 2,
}

#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum FontFlags {
    None = 0,
    Color = 1,
    Monospaced = 2,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum NodeType {
    View = 0,
    Text = 1,
    Root = 2,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct Str16 {
    pub Data: *const u16,
    pub Size: u32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct Str8 {
    pub Data: *const u8,
    pub Size: u32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct NativeArcInner<T0 /* T */> {
    pub m_count: u64,
    pub m_data: T0,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct NativeArc<T0 /* T */> {
    pub m_ptr: *mut NativeArcInner<T0>,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct NativeList<T0 /* T */> {
    pub m_items: *mut T0,
    pub m_cap: i32,
    pub m_size: i32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct GridName {
    pub Id: i32,
    pub Type: GridNameType,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct GridPlacement {
    pub Name: i32,
    pub Value1: i16,
    pub NameType: GridNameType,
    pub Type: GridPlacementType,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct GridTemplateArea {
    pub Id: GridName,
    pub RowStart: u16,
    pub RowEnd: u16,
    pub ColumnStart: u16,
    pub ColumnEnd: u16,
}

#[repr(C)]
#[derive(Clone, Copy)]
pub struct GridTemplateComponent {
    pub Union: GridTemplateComponentUnion,
    pub Type: GridTemplateComponentType,
}

#[repr(C)]
#[derive(Clone, Copy)]
pub union GridTemplateComponentUnion {
    pub Single: TrackSizingFunction,
    pub Repeat: GridTemplateRepetition,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct GridTemplateRepetition {
    pub Tracks: NativeList<TrackSizingFunction>,
    pub LineIds: NativeList<NativeList<GridName>>,
    pub RepetitionValue: u16,
    pub Repetition: RepetitionType,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct SizingValue {
    pub Value: f32,
    pub Type: LengthType,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct TrackSizingFunction {
    pub MinValue: SizingValue,
    pub MaxValue: SizingValue,
    pub Min: SizingType,
    pub Max: SizingType,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct FFIOrderedSetNode<T0 /* T */> {
    pub HashCode: i32,
    pub Next: i32,
    pub OrderNext: i32,
    pub OrderPrev: i32,
    pub Value: T0,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct FFIOrderedSet<T0 /* T */> {
    pub m_buckets: *mut i32,
    pub m_nodes: *mut FFIOrderedSetNode<T0>,
    pub m_fast_mode_multiplier: u64,
    pub m_cap: i32,
    pub m_first: i32,
    pub m_last: i32,
    pub m_count: i32,
    pub m_free_list: i32,
    pub m_free_count: i32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct FontFamilyNameInfo {
    pub Name: Str16,
    pub Local: u32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct LayoutCache {
    pub FinalLayoutEntry: LayoutCacheEntryLayoutOutput,
    pub MeasureEntries0: LayoutCacheEntrySize,
    pub MeasureEntries1: LayoutCacheEntrySize,
    pub MeasureEntries2: LayoutCacheEntrySize,
    pub MeasureEntries3: LayoutCacheEntrySize,
    pub MeasureEntries4: LayoutCacheEntrySize,
    pub MeasureEntries5: LayoutCacheEntrySize,
    pub MeasureEntries6: LayoutCacheEntrySize,
    pub MeasureEntries7: LayoutCacheEntrySize,
    pub MeasureEntries8: LayoutCacheEntrySize,
    pub HasFinalLayoutEntry: bool,
    pub HasMeasureEntries0: bool,
    pub HasMeasureEntries1: bool,
    pub HasMeasureEntries2: bool,
    pub HasMeasureEntries3: bool,
    pub HasMeasureEntries4: bool,
    pub HasMeasureEntries5: bool,
    pub HasMeasureEntries6: bool,
    pub HasMeasureEntries7: bool,
    pub HasMeasureEntries8: bool,
    pub IsEmpty: bool,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct LayoutCacheEntryLayoutOutput {
    pub KnownDimensionsWidthValue: f32,
    pub KnownDimensionsHeightValue: f32,
    pub AvailableSpaceWidthValue: f32,
    pub AvailableSpaceHeightValue: f32,
    pub HasKnownDimensionsWidth: bool,
    pub HasKnownDimensionsHeight: bool,
    pub AvailableSpaceWidth: AvailableSpaceType,
    pub AvailableSpaceHeight: AvailableSpaceType,
    pub Content: LayoutOutput,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct LayoutCacheEntrySize {
    pub KnownDimensionsWidthValue: f32,
    pub KnownDimensionsHeightValue: f32,
    pub AvailableSpaceWidthValue: f32,
    pub AvailableSpaceHeightValue: f32,
    pub HasKnownDimensionsWidth: bool,
    pub HasKnownDimensionsHeight: bool,
    pub AvailableSpaceWidth: AvailableSpaceType,
    pub AvailableSpaceHeight: AvailableSpaceType,
    pub ContentWidth: f32,
    pub ContentHeight: f32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct LayoutCollapsibleMarginSet {
    pub Positive: f32,
    pub Negative: f32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct LayoutData {
    pub Order: u32,
    pub LocationX: f32,
    pub LocationY: f32,
    pub Width: f32,
    pub Height: f32,
    pub ContentWidth: f32,
    pub ContentHeight: f32,
    pub ScrollXSize: f32,
    pub ScrollYSize: f32,
    pub BorderTopSize: f32,
    pub BorderRightSize: f32,
    pub BorderBottomSize: f32,
    pub BorderLeftSize: f32,
    pub PaddingTopSize: f32,
    pub PaddingRightSize: f32,
    pub PaddingBottomSize: f32,
    pub PaddingLeftSize: f32,
    pub MarginTopSize: f32,
    pub MarginRightSize: f32,
    pub MarginBottomSize: f32,
    pub MarginLeftSize: f32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct LayoutOutput {
    pub Width: f32,
    pub Height: f32,
    pub ContentWidth: f32,
    pub ContentHeight: f32,
    pub FirstBaselinesX: f32,
    pub FirstBaselinesY: f32,
    pub TopMargin: LayoutCollapsibleMarginSet,
    pub BottomMargin: LayoutCollapsibleMarginSet,
    pub HasFirstBaselinesX: bool,
    pub HasFirstBaselinesY: bool,
    pub MarginsCanCollapseThrough: bool,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct NFontInfo {
    pub Metrics: FontMetrics,
    pub Width: FontWidth,
    pub Weight: FontWeight,
    pub Style: FontStyle,
    pub Flags: FontFlags,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct NFontPair {
    pub Font: *mut IFont,
    pub Info: *mut NFontInfo,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct NLayoutContext {
    pub roots: *mut i32,
    pub view_buckets: *mut i32,
    pub text_buckets: *mut i32,
    pub root_buckets: *mut i32,
    pub view_ctrl: *mut NNodeIdCtrl,
    pub text_ctrl: *mut NNodeIdCtrl,
    pub root_ctrl: *mut NNodeIdCtrl,
    pub view_container_layout_data: *mut ContainerLayoutData,
    pub _pad_container_layout_data: *mut (),
    pub root_container_layout_data: *mut ContainerLayoutData,
    pub view_common_style_data: *mut CommonStyleData,
    pub text_common_style_data: *mut CommonStyleData,
    pub root_common_style_data: *mut CommonStyleData,
    pub view_childs_data: *mut ChildsData,
    pub _pad_childs_data: *mut (),
    pub root_childs_data: *mut ChildsData,
    pub view_container_style_data: *mut ContainerStyleData,
    pub _pad_container_style_data: *mut (),
    pub root_container_style_data: *mut ContainerStyleData,
    pub text_data: *mut TextData,
    pub root_root_data: *mut RootData,
    pub root_count: i32,
    pub view_count: i32,
    pub text_count: i32,
    pub rounding: bool,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct NNodeIdCtrl {
    pub HashCode: i32,
    pub Next: i32,
    pub Key: NodeId,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct FontWidth {
    pub Width: f32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct FontMetrics {
    pub Ascent: f32,
    pub Descent: f32,
    pub Leading: f32,
    pub LineHeight: f32,
    pub UnitsPerEm: u16,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct ChildsData {
    pub m_childs: FFIOrderedSet<NodeLocate>,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct CommonStyleData {
    pub ZIndex: i32,
    pub Opacity: f32,
    pub BackgroundColorR: f32,
    pub BackgroundColorG: f32,
    pub BackgroundColorB: f32,
    pub BackgroundColorA: f32,
    pub InsertTopValue: f32,
    pub InsertRightValue: f32,
    pub InsertBottomValue: f32,
    pub InsertLeftValue: f32,
    pub MarginTopValue: f32,
    pub MarginRightValue: f32,
    pub MarginBottomValue: f32,
    pub MarginLeftValue: f32,
    pub FlexGrow: f32,
    pub FlexShrink: f32,
    pub FlexBasisValue: f32,
    pub GridRowStart: GridPlacement,
    pub GridRowEnd: GridPlacement,
    pub GridColumnStart: GridPlacement,
    pub GridColumnEnd: GridPlacement,
    pub Visible: Visible,
    pub Position: Position,
    pub InsertTop: LengthType,
    pub InsertRight: LengthType,
    pub InsertBottom: LengthType,
    pub InsertLeft: LengthType,
    pub MarginTop: LengthType,
    pub MarginRight: LengthType,
    pub MarginBottom: LengthType,
    pub MarginLeft: LengthType,
    pub AlignSelf: AlignType,
    pub JustifySelf: AlignType,
    pub FlexBasis: LengthType,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct ContainerLayoutData {
    pub TextLayoutObject: *mut ITextLayout,
    pub FinalLayout: LayoutData,
    pub Layout: LayoutData,
    pub LayoutCache: LayoutCache,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct ContainerStyleData {
    pub Grid: NativeArc<GridContainerStyle>,
    pub FontFallback: *mut IFontFallback,
    pub ScrollBarSize: f32,
    pub WidthValue: f32,
    pub HeightValue: f32,
    pub MinWidthValue: f32,
    pub MinHeightValue: f32,
    pub MaxWidthValue: f32,
    pub MaxHeightValue: f32,
    pub AspectRatioValue: f32,
    pub PaddingTopValue: f32,
    pub PaddingRightValue: f32,
    pub PaddingBottomValue: f32,
    pub PaddingLeftValue: f32,
    pub BorderTopValue: f32,
    pub BorderRightValue: f32,
    pub BorderBottomValue: f32,
    pub BorderLeftValue: f32,
    pub GapXValue: f32,
    pub GapYValue: f32,
    pub TextColorR: f32,
    pub TextColorG: f32,
    pub TextColorB: f32,
    pub TextColorA: f32,
    pub TextSizeValue: f32,
    pub TabSizeValue: f32,
    pub Container: Container,
    pub BoxSizing: BoxSizing,
    pub OverflowX: Overflow,
    pub OverflowY: Overflow,
    pub Width: LengthType,
    pub Height: LengthType,
    pub MinWidth: LengthType,
    pub MinHeight: LengthType,
    pub MaxWidth: LengthType,
    pub MaxHeight: LengthType,
    pub PaddingTop: LengthType,
    pub PaddingRight: LengthType,
    pub PaddingBottom: LengthType,
    pub PaddingLeft: LengthType,
    pub BorderTop: LengthType,
    pub BorderRight: LengthType,
    pub BorderBottom: LengthType,
    pub BorderLeft: LengthType,
    pub HasAspectRatio: bool,
    pub FlexDirection: FlexDirection,
    pub FlexWrap: FlexWrap,
    pub GridAutoFlow: GridAutoFlow,
    pub GapX: LengthType,
    pub GapY: LengthType,
    pub AlignContent: AlignType,
    pub JustifyContent: AlignType,
    pub AlignItems: AlignType,
    pub JustifyItems: AlignType,
    pub TextAlign: TextAlign,
    pub TextSize: LengthType,
    pub TabSize: LengthType,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct GridContainerStyle {
    pub GridTemplateRows: NativeList<GridTemplateComponent>,
    pub GridTemplateColumns: NativeList<GridTemplateComponent>,
    pub GridAutoRows: NativeList<TrackSizingFunction>,
    pub GridAutoColumns: NativeList<TrackSizingFunction>,
    pub GridTemplateAreas: NativeList<GridTemplateArea>,
    pub GridTemplateColumnNames: NativeList<NativeList<GridName>>,
    pub GridTemplateRowNames: NativeList<NativeList<GridName>>,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct RootData {
    pub AvailableSpaceXValue: f32,
    pub AvailableSpaceYValue: f32,
    pub AvailableSpaceX: AvailableSpaceType,
    pub AvailableSpaceY: AvailableSpaceType,
    pub UseRounding: bool,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct TextData {
    pub m_text: NativeList<u16>,
    pub m_version: u64,
    pub m_inner_version: u64,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct NodeId {
    pub Id: u32,
    pub VersionAndType: u32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct NodeLocate {
    pub Id: NodeId,
    pub Index: i32,
}

#[repr(C)]
pub struct IFont {
}

impl IFont {
}

#[repr(C)]
pub struct IFontCollection {
}

impl IFontCollection {
}

#[repr(C)]
pub struct IFontFace {
}

impl IFontFace {
}

#[repr(C)]
pub struct IFontFallback {
}

impl IFontFallback {
}

#[repr(C)]
pub struct IFontFamily {
}

impl IFontFamily {
}

#[repr(C)]
pub struct ILayout {
}

impl ILayout {
}

#[repr(C)]
pub struct ILib {
}

impl ILib {
}

#[repr(C)]
pub struct IStub {
}

impl IStub {
}

#[repr(C)]
pub struct ITextLayout {
}

impl ITextLayout {
}
