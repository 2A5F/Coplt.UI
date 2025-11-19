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

#[repr(u8)]
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

#[repr(u16)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum LayoutCacheFlags {
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
pub enum LineAlign {
    Baseline = 0,
    Start = 1,
    End = 2,
    Center = 3,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum LocaleMode {
    Normal = 0,
    ByScript = 1,
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
pub enum PointerEvents {
    Auto = 0,
    None = 1,
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
    Start = 0,
    End = 1,
    Center = 2,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum TextDirection {
    Forward = 0,
    Reverse = 1,
    LeftToRight = 2,
    RightToLeft = 3,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum TextMode {
    Block = 0,
    Inline = 1,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum TextOrientation {
    Mixed = 0,
    Upright = 1,
    Sideways = 2,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum TextOverflow {
    Clip = 0,
    Ellipsis = 1,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum TextWrap {
    Wrap = 0,
    NoWrap = 1,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum Visible {
    Visible = 0,
    Hidden = 1,
    Remove = 2,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum WhiteSpaceMerge {
    Keep = 0,
    Merge = 1,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum WhiteSpaceWrap {
    WrapAll = 0,
    WrapLine = 1,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum WordBreak {
    Auto = 0,
    BreakAll = 1,
    KeepAll = 2,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum WritingDirection {
    Horizontal = 0,
    Vertical = 1,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum CharCategory {
    Unassigned = 0,
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
}

#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum FontFlags {
    None = 0,
    Color = 1,
    Monospaced = 2,
}

#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum ScriptCode {
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
    MayanHieroglyphs = 85,
    MeroiticHieroglyphs = 86,
    Nko = 87,
    Orkhon = 88,
    OldPermic = 89,
    PhagsPa = 90,
    Phoenician = 91,
    Miao = 92,
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
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum NodeType {
    View = 0,
    Text = 1,
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
pub struct CWStr {
    pub Locale: *const u16,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct FFIMap {
    pub m_buckets: *mut i32,
    pub m_entries: *mut (),
    pub m_fast_mode_multiplier: u64,
    pub m_cap: i32,
    pub m_count: i32,
    pub m_free_list: i32,
    pub m_free_count: i32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct FFIOrderedSet {
    pub m_buckets: *mut i32,
    pub m_nodes: *mut (),
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
    pub Flags: LayoutCacheFlags,
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
    pub font_manager: *mut IFontManager,
    pub roots: *mut FFIMap,
    pub node_buckets: *mut i32,
    pub node_ctrl: *mut NNodeIdCtrl,
    pub node_common_data: *mut CommonData,
    pub node_childs_data: *mut ChildsData,
    pub node_style_data: *mut StyleData,
    pub node_count: i32,
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
pub struct NString {
    pub m_str: *const u16,
    pub m_handle: *mut (),
    pub m_len: i32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct FontWidth {
    pub Width: f32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct LocaleId {
    pub Name: *mut u16,
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
pub struct TextRange {
    pub Locale: CWStr,
    pub Start: i32,
    pub Length: i32,
    pub Script: ScriptCode,
    pub Category: CharCategory,
    pub ScriptIsRtl: bool,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct ChildsData {
    pub m_childs: FFIOrderedSet,
    pub m_texts: FFIMap,
    pub m_text_id_inc: u32,
    pub m_version: u64,
    pub m_last_version: u64,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct CommonData {
    pub TextLayoutObject: *mut ITextLayout,
    pub TextLayoutBelongTo: *mut ITextLayout,
    pub FinalLayout: LayoutData,
    pub UnRoundedLayout: LayoutData,
    pub LayoutCache: LayoutCache,
    pub LastLayoutVersion: u32,
    pub LastTextLayoutVersion: u32,
    pub LayoutVersion: u32,
    pub TextLayoutVersion: u32,
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
    pub Node: NodeId,
    pub AvailableSpaceXValue: f32,
    pub AvailableSpaceYValue: f32,
    pub AvailableSpaceX: AvailableSpaceType,
    pub AvailableSpaceY: AvailableSpaceType,
    pub UseRounding: bool,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct StyleData {
    pub Grid: NativeArc<GridContainerStyle>,
    pub FontFallback: *mut IFontFallback,
    pub Locale: LocaleId,
    pub ZIndex: i32,
    pub TextColorR: f32,
    pub TextColorG: f32,
    pub TextColorB: f32,
    pub TextColorA: f32,
    pub Opacity: f32,
    pub BackgroundColorR: f32,
    pub BackgroundColorG: f32,
    pub BackgroundColorB: f32,
    pub BackgroundColorA: f32,
    pub ScrollBarSize: f32,
    pub WidthValue: f32,
    pub HeightValue: f32,
    pub MinWidthValue: f32,
    pub MinHeightValue: f32,
    pub MaxWidthValue: f32,
    pub MaxHeightValue: f32,
    pub AspectRatioValue: f32,
    pub InsertTopValue: f32,
    pub InsertRightValue: f32,
    pub InsertBottomValue: f32,
    pub InsertLeftValue: f32,
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
    pub FlexGrow: f32,
    pub FlexShrink: f32,
    pub FlexBasisValue: f32,
    pub TabSizeValue: f32,
    pub FontSize: f32,
    pub FontWidth: FontWidth,
    pub FontOblique: f32,
    pub FontWeight: FontWeight,
    pub LineHeightValue: f32,
    pub GridRowStart: GridPlacement,
    pub GridRowEnd: GridPlacement,
    pub GridColumnStart: GridPlacement,
    pub GridColumnEnd: GridPlacement,
    pub Visible: Visible,
    pub Position: Position,
    pub Container: Container,
    pub TextMode: TextMode,
    pub BoxSizing: BoxSizing,
    pub Cursor: CursorType,
    pub PointerEvents: PointerEvents,
    pub OverflowX: Overflow,
    pub OverflowY: Overflow,
    pub Width: LengthType,
    pub Height: LengthType,
    pub MinWidth: LengthType,
    pub MinHeight: LengthType,
    pub MaxWidth: LengthType,
    pub MaxHeight: LengthType,
    pub InsertTop: LengthType,
    pub InsertRight: LengthType,
    pub InsertBottom: LengthType,
    pub InsertLeft: LengthType,
    pub MarginTop: LengthType,
    pub MarginRight: LengthType,
    pub MarginBottom: LengthType,
    pub MarginLeft: LengthType,
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
    pub AlignSelf: AlignType,
    pub JustifySelf: AlignType,
    pub FlexBasis: LengthType,
    pub FontItalic: bool,
    pub FontOpticalSizing: bool,
    pub TextAlign: TextAlign,
    pub LineAlign: LineAlign,
    pub TabSize: LengthType,
    pub LocaleMode: LocaleMode,
    pub TextDirection: TextDirection,
    pub WritingDirection: WritingDirection,
    pub WhiteSpaceMerge: WhiteSpaceMerge,
    pub WhiteSpaceWrap: WhiteSpaceWrap,
    pub TextWrap: TextWrap,
    pub WordBreak: WordBreak,
    pub TextOrientation: TextOrientation,
    pub TextOverflow: TextOverflow,
    pub LineHeight: LengthType,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct NodeId {
    pub Index: u32,
    pub IdAndType: u32,
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
pub struct IFontManager {
}

impl IFontManager {
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
pub struct ITextData {
}

impl ITextData {
}

#[repr(C)]
pub struct ITextLayout {
}

impl ITextLayout {
}
