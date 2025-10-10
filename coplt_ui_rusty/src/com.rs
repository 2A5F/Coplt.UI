#![allow(unused)]
#![allow(non_snake_case)]

#[repr(i32)]
pub enum AvailableSpaceType {
    Definite = 0,
    MinContent = 1,
    MaxContent = 2,
}

#[repr(u8)]
pub enum LogLevel {
    Fatal = 0,
    Error = 1,
    Warning = 2,
    Info = 3,
    Debug = 4,
    Verbose = 5,
}

#[repr(u8)]
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

#[repr(u32)]
pub enum BorderRadiusMode {
    Circle = 0,
    Parabola = 1,
    Cosine = 2,
    Cubic = 3,
}

#[repr(u8)]
pub enum BoxSizing {
    BorderBox = 0,
    ContentBox = 1,
}

#[repr(u8)]
pub enum CursorType {
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
}

#[repr(u8)]
pub enum Display {
    Flex = 0,
    Grid = 1,
    Block = 2,
    Inline = 3,
    None = 4,
}

#[repr(u8)]
pub enum FontStyle {
    Normal = 0,
    Italic = 1,
    Oblique = 2,
}

#[repr(i32)]
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
pub enum LengthType {
    Fixed = 0,
    Percent = 1,
    Auto = 2,
}

#[repr(u8)]
pub enum Overflow {
    Visible = 0,
    Clip = 1,
    Hidden = 2,
    Scroll = 3,
}

#[repr(u8)]
pub enum PointerEvents {
    Auto = 0,
    None = 1,
}

#[repr(u8)]
pub enum Position {
    Relative = 0,
    Absolute = 1,
}

#[repr(u32)]
pub enum SamplerType {
    LinearClamp = 0,
    LinearWrap = 1,
    PointClamp = 2,
    PointWrap = 3,
}

#[repr(u8)]
pub enum TextAlign {
    Auto = 0,
    Left = 1,
    Right = 2,
    Center = 3,
}

#[repr(i32)]
pub enum FontFlags {
    None = 0,
    Color = 1,
    Monospaced = 2,
}

#[repr(C)]
pub struct Str16 {
    pub Data: *const u16,
    pub Size: u32,
}

#[repr(C)]
pub struct Str8 {
    pub Data: *const u8,
    pub Size: u32,
}

#[repr(C)]
pub struct FontFamilyNameInfo {
    pub Name: Str16,
    pub Local: u32,
}

#[repr(C)]
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
pub struct LayoutCollapsibleMarginSet {
    pub Positive: f32,
    pub Negative: f32,
}

#[repr(C)]
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
pub struct NFontInfo {
    pub Metrics: FontMetrics,
    pub Width: FontWidth,
    pub Weight: FontWeight,
    pub Style: FontStyle,
    pub Flags: FontFlags,
}

#[repr(C)]
pub struct NFontPair {
    pub Font: *mut IFont,
    pub Info: *mut NFontInfo,
}

#[repr(C)]
pub struct UiNodeData {
    pub Object: *mut (),
    pub Style: StyleData,
    pub Layout: LayoutData,
    pub FinalLayout: LayoutData,
    pub LayoutCache: LayoutCache,
}

#[repr(C)]
pub struct FontWidth {
    pub Width: f32,
}

#[repr(C)]
pub struct StyleData {
    pub Image: *mut (),
    pub ZIndex: i32,
    pub Opacity: f32,
    pub ColorR: f32,
    pub ColorG: f32,
    pub ColorB: f32,
    pub ColorA: f32,
    pub ImageTintR: f32,
    pub ImageTintG: f32,
    pub ImageTintB: f32,
    pub ImageTintA: f32,
    pub ScrollbarWidth: f32,
    pub InsertTopValue: f32,
    pub InsertRightValue: f32,
    pub InsertBottomValue: f32,
    pub InsertLeftValue: f32,
    pub WidthValue: f32,
    pub HeightValue: f32,
    pub MinWidthValue: f32,
    pub MinHeightValue: f32,
    pub MaxWidthValue: f32,
    pub MaxHeightValue: f32,
    pub AspectRatioValue: f32,
    pub MarginTopValue: f32,
    pub MarginRightValue: f32,
    pub MarginBottomValue: f32,
    pub MarginLeftValue: f32,
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
    pub BorderMode: BorderRadiusMode,
    pub BackgroundSampler: SamplerType,
    pub Display: Display,
    pub BoxSizing: BoxSizing,
    pub OverflowX: Overflow,
    pub OverflowY: Overflow,
    pub Position: Position,
    pub InsertTop: LengthType,
    pub InsertRight: LengthType,
    pub InsertBottomV: LengthType,
    pub InsertLeft: LengthType,
    pub Width: LengthType,
    pub Height: LengthType,
    pub MinWidth: LengthType,
    pub MinHeight: LengthType,
    pub MaxMinWidth: LengthType,
    pub MaxMinHeight: LengthType,
    pub HasAspectRatio: bool,
    pub MarginTop: LengthType,
    pub MarginRight: LengthType,
    pub MarginBottomV: LengthType,
    pub MarginLeft: LengthType,
    pub PaddingTop: LengthType,
    pub PaddingRight: LengthType,
    pub PaddingBottomV: LengthType,
    pub PaddingLeft: LengthType,
    pub BorderTop: LengthType,
    pub BorderRight: LengthType,
    pub BorderBottomV: LengthType,
    pub BorderLeft: LengthType,
    pub AlignItems: AlignType,
    pub AlignSelf: AlignType,
    pub JustifyItems: AlignType,
    pub JustifySelf: AlignType,
    pub AlignContent: AlignType,
    pub JustifyContent: AlignType,
    pub GapX: LengthType,
    pub GapY: LengthType,
    pub TextAlign: TextAlign,
    pub TextSize: LengthType,
    pub Cursor: CursorType,
    pub PointerEvents: PointerEvents,
}

#[repr(C)]
pub struct FontMetrics {
    pub Ascent: f32,
    pub Descent: f32,
    pub Leading: f32,
    pub LineHeight: f32,
    pub UnitsPerEm: u16,
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
pub struct IFontFamily {
}

impl IFontFamily {
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
