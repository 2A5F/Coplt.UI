#![allow(unused)]
#![allow(non_snake_case)]
#![allow(non_camel_case_types)]

use cocom::{Guid, HResult, Interface, IUnknown, IWeak};

#[cocom::interface("32b30623-411e-4fd5-a009-ae7e9ed88e78")]
pub trait IAtlasAllocator : IUnknown {
    fn Clear(&mut self) -> ();
    fn get_IsEmpty(&mut self) -> bool;
    fn GetSize(&mut self, out_width: *mut i32, out_height: *mut i32) -> ();
    fn Allocate(&mut self, width: i32, height: i32, out_id: *mut u32, out_rect: *mut AABB2D) -> bool;
    fn Deallocate(&mut self, id: u32) -> ();
    fn GetRect(&mut self, id: u32, out_rect: *mut AABB2D) -> ();
}

#[cocom::interface("09c443bc-9736-4aac-8117-6890555005ff")]
pub trait IFont : IUnknown {
    fn get_Info(&self) -> *const NFontInfo;
    fn CreateFace(&self, /* out */ face: *mut *mut IFontFace, manager: *mut IFontManager) -> HResult;
}

#[cocom::interface("e56d9271-e6fd-4def-b03a-570380e0d560")]
pub trait IFontCollection : IUnknown {
    fn GetFamilies(&self, /* out */ count: *mut u32) -> *const *mut IFontFamily;
    fn ClearNativeFamiliesCache(&mut self) -> ();
    fn FindDefaultFamily(&mut self) -> u32;
}

#[cocom::interface("09c443bc-9736-4aac-8117-6890555005ff")]
pub trait IFontFace : IUnknown {
    fn get_Id(&self) -> u64;
    fn get_Info(&self) -> *const NFontInfo;
    fn Equals(&self, other: *mut IFontFace) -> bool;
    fn HashCode(&self) -> i32;
    fn GetFamilyNames(&self, ctx: *mut core::ffi::c_void, add: unsafe extern "C" fn(*mut core::ffi::c_void, *mut u16, i32, *mut u16, i32) -> core::ffi::c_void) -> HResult;
    fn GetFaceNames(&self, ctx: *mut core::ffi::c_void, add: unsafe extern "C" fn(*mut core::ffi::c_void, *mut u16, i32, *mut u16, i32) -> core::ffi::c_void) -> HResult;
}

#[cocom::interface("b0dbb428-eca1-4784-b27f-629bddf93ea4")]
pub trait IFontFallback : IUnknown {
}

#[cocom::interface("9b4e9893-0ea4-456b-bf54-9563db70eff0")]
pub trait IFontFallbackBuilder : IUnknown {
    fn Build(&mut self, ff: *mut *mut IFontFallback) -> HResult;
    fn Add(&mut self, name: *const u16, length: i32, exists: *mut bool) -> HResult;
    fn AddLocaled(&mut self, locale: *const u16, name: *const u16, name_length: i32, exists: *mut bool) -> HResult;
}

#[cocom::interface("f8009d34-9417-4b87-b23b-b7885d27aeab")]
pub trait IFontFamily : IUnknown {
    fn GetLocalNames(&self, /* out */ length: *mut u32) -> *const Str16;
    fn GetNames(&self, /* out */ length: *mut u32) -> *const FontFamilyNameInfo;
    fn ClearNativeNamesCache(&mut self) -> ();
    fn GetFonts(&mut self, /* out */ length: *mut u32, /* out */ pair: *mut *const NFontPair) -> HResult;
    fn ClearNativeFontsCache(&mut self) -> ();
}

#[cocom::interface("15a9651e-4fa2-48f3-9291-df0f9681a7d1")]
pub trait IFontManager : IUnknown {
    fn SetAssocUpdate(&mut self, Data: *mut core::ffi::c_void, OnDrop: unsafe extern "C" fn(*mut core::ffi::c_void) -> core::ffi::c_void, OnAdd: unsafe extern "C" fn(*mut core::ffi::c_void, *mut IFontFace, u64) -> core::ffi::c_void, OnExpired: unsafe extern "C" fn(*mut core::ffi::c_void, *mut IFontFace, u64) -> core::ffi::c_void) -> u64;
    fn RemoveAssocUpdate(&mut self, AssocUpdateId: u64) -> ();
    fn SetExpireFrame(&mut self, FrameCount: u64) -> ();
    fn SetExpireTime(&mut self, TimeTicks: u64) -> ();
    fn GetCurrentFrame(&self) -> u64;
    fn Update(&mut self, CurrentTime: u64) -> ();
    fn FontFaceToId(&mut self, Face: *mut IFontFace) -> u64;
    fn IdToFontFace(&mut self, Id: u64) -> *mut IFontFace;
}

#[cocom::interface("f1e64bf0-ffb9-42ce-be78-31871d247883")]
pub trait ILayout : IUnknown {
    fn Calc(&mut self, ctx: *mut NLayoutContext) -> HResult;
}

#[cocom::interface("778be1fe-18f2-4aa5-8d1f-52d83b132cff")]
pub trait ILib : IUnknown {
    fn SetLogger(&mut self, obj: *mut core::ffi::c_void, logger: unsafe extern "C" fn(*mut core::ffi::c_void, LogLevel, StrKind, i32, *mut core::ffi::c_void) -> core::ffi::c_void, is_enabled: unsafe extern "C" fn(*mut core::ffi::c_void, LogLevel) -> u8, drop: unsafe extern "C" fn(*mut core::ffi::c_void) -> core::ffi::c_void) -> ();
    fn ClearLogger(&mut self) -> ();
    fn GetCurrentErrorMessage(&mut self) -> Str8;
    fn CreateFontManager(&mut self, fm: *mut *mut IFontManager) -> HResult;
    fn GetSystemFontCollection(&mut self, fc: *mut *mut IFontCollection) -> HResult;
    fn GetSystemFontFallback(&mut self, ff: *mut *mut IFontFallback) -> HResult;
    fn CreateFontFallbackBuilder(&mut self, ffb: *mut *mut IFontFallbackBuilder, info: *const FontFallbackBuilderCreateInfo) -> HResult;
    fn CreateLayout(&mut self, layout: *mut *mut ILayout) -> HResult;
    fn SplitTexts(&mut self, ranges: *mut NativeList<TextRange>, chars: *const u16, len: i32) -> HResult;
}

#[cocom::interface("dac7a459-b942-4a96-b7d6-ee5c74eca806")]
pub trait IPath : IUnknown {
    fn CalcAABB(&mut self, out_aabb: *mut AABB2DF) -> ();
}

#[cocom::interface("ee1c5b1d-b22d-446a-9eef-128cec82e6c0")]
pub trait IPathBuilder : IUnknown {
    fn Build(&mut self, path: *mut *mut IPath) -> HResult;
    fn Reserve(&mut self, Endpoints: i32, CtrlPoints: i32) -> ();
    fn Batch(&mut self, cmds: *const PathBuilderCmd, num_cmds: i32) -> ();
    fn Close(&mut self) -> ();
    fn MoveTo(&mut self, x: f32, y: f32) -> ();
    fn LineTo(&mut self, x: f32, y: f32) -> ();
    fn QuadraticBezierTo(&mut self, ctrl_x: f32, ctrl_y: f32, to_x: f32, to_y: f32) -> ();
    fn CubicBezierTo(&mut self, ctrl0_x: f32, ctrl0_y: f32, ctrl1_x: f32, ctrl1_y: f32, to_x: f32, to_y: f32) -> ();
    fn Arc(&mut self, center_x: f32, center_y: f32, radii_x: f32, radii_y: f32, sweep_angle: f32, x_rotation: f32) -> ();
}

#[cocom::interface("a998ec87-868d-4320-a30a-638c291f5562")]
pub trait IStub : IUnknown {
    fn Some(&mut self, a: NodeType, b: *mut RootData, c: *mut NString) -> ();
}

#[cocom::interface("acf5d52e-a656-4c00-a528-09aa4d86b2b2")]
pub trait ITessellator : IUnknown {
    fn Fill(&mut self, path: *mut IPath, options: *mut TessFillOptions) -> HResult;
    fn Stroke(&mut self, path: *mut IPath, options: *mut TessStrokeOptions) -> HResult;
}

#[cocom::interface("bd0c7402-1de8-4547-860d-c78fd70ff203")]
pub trait ITextData : IUnknown {
}

#[cocom::interface("f558ba07-1f1d-4c32-8229-134271b17083")]
pub trait ITextLayout : IUnknown {
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum StrKind {
    Str8 = 0,
    Str16 = 1,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum FillRule {
    EvenOdd = 0,
    NonZero = 1,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum LineCap {
    Butt = 0,
    Square = 1,
    Round = 2,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum LineJoin {
    Miter = 0,
    MiterClip = 1,
    Round = 2,
    Bevel = 3,
}

#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum PathBuilderCmdType {
    Close = 0,
    MoveTo = 1,
    LineTo = 2,
    QuadraticBezierTo = 3,
    CubicBezierTo = 4,
    Arc = 5,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum Orientation {
    Horizontal = 0,
    Vertical = 1,
}

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
    Trace = 0,
    Debug = 1,
    Info = 2,
    Warn = 3,
    Error = 4,
    Fatal = 5,
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
pub enum WordBreak {
    Auto = 0,
    BreakAll = 1,
    KeepAll = 2,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum WrapFlags {
    None = 0,
    AllowNewLine = 1,
    WrapInSpace = 2,
    TrimStart = 4,
    TrimEnd = 8,
    Trim = 12,
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
pub struct AABB2D {
    pub MinX: u32,
    pub MinY: u32,
    pub MaxX: u32,
    pub MaxY: u32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct AABB2DF {
    pub MinX: f32,
    pub MinY: f32,
    pub MaxX: f32,
    pub MaxY: f32,
}

#[repr(C)]
#[derive(Clone, Copy)]
pub union PathBuilderCmd {
    pub Type: PathBuilderCmdType,
    pub XTo: PathBuilderCmdXToPoint,
    pub QuadraticBezierTo: PathBuilderCmdQuadraticBezierTo,
    pub CubicBezierTo: PathBuilderCmdCubicBezierTo,
    pub Arc: PathBuilderCmdArc,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct PathBuilderCmdArc {
    pub Type: PathBuilderCmdType,
    pub CenterX: f32,
    pub CenterY: f32,
    pub RadiiX: f32,
    pub RadiiY: f32,
    pub SweepAngle: f32,
    pub XRotation: f32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct PathBuilderCmdCubicBezierTo {
    pub Type: PathBuilderCmdType,
    pub Ctrl0X: f32,
    pub Ctrl0Y: f32,
    pub Ctrl1X: f32,
    pub Ctrl1Y: f32,
    pub ToX: f32,
    pub ToY: f32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct PathBuilderCmdQuadraticBezierTo {
    pub Type: PathBuilderCmdType,
    pub CtrlX: f32,
    pub CtrlY: f32,
    pub ToX: f32,
    pub ToY: f32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct PathBuilderCmdXToPoint {
    pub Type: PathBuilderCmdType,
    pub X: f32,
    pub Y: f32,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct TessFillOptions {
    pub ToLerance: f32,
    pub FillRule: FillRule,
    pub SweepOrientation: Orientation,
    pub HandleIntersections: bool,
}

#[repr(C)]
#[derive(Clone, Copy, Debug, PartialEq, PartialOrd)]
pub struct TessStrokeOptions {
    pub ToLerance: f32,
    pub LineWidth: f32,
    pub MiterLimit: f32,
    pub StartCap: LineCap,
    pub EndCap: LineCap,
    pub LineJoin: LineJoin,
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
    pub m_entries: *mut core::ffi::c_void,
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
    pub m_nodes: *mut core::ffi::c_void,
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
pub struct FontFallbackBuilderCreateInfo {
    pub DisableSystemFallback: bool,
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
    pub m_handle: *mut core::ffi::c_void,
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
    pub WrapFlags: WrapFlags,
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

pub mod details {
    pub use cocom::details::*;
    use super::*;

    #[repr(C)]
    #[derive(Debug)]
    pub struct VitualTable_IAtlasAllocator {
        b: <IUnknown as Interface>::VitualTable,

        pub f_Clear: unsafe extern "C" fn(this: *const IAtlasAllocator) -> (),
        pub f_get_IsEmpty: unsafe extern "C" fn(this: *const IAtlasAllocator) -> bool,
        pub f_GetSize: unsafe extern "C" fn(this: *const IAtlasAllocator, out_width: *mut i32, out_height: *mut i32) -> (),
        pub f_Allocate: unsafe extern "C" fn(this: *const IAtlasAllocator, width: i32, height: i32, out_id: *mut u32, out_rect: *mut AABB2D) -> bool,
        pub f_Deallocate: unsafe extern "C" fn(this: *const IAtlasAllocator, id: u32) -> (),
        pub f_GetRect: unsafe extern "C" fn(this: *const IAtlasAllocator, id: u32, out_rect: *mut AABB2D) -> (),
    }

    #[repr(C)]
    #[derive(Debug)]
    pub struct VitualTable_IFont {
        b: <IUnknown as Interface>::VitualTable,

        pub f_get_Info: unsafe extern "C" fn(this: *const IFont) -> *const NFontInfo,
        pub f_CreateFace: unsafe extern "C" fn(this: *const IFont, /* out */ face: *mut *mut IFontFace, manager: *mut IFontManager) -> HResult,
    }

    #[repr(C)]
    #[derive(Debug)]
    pub struct VitualTable_IFontCollection {
        b: <IUnknown as Interface>::VitualTable,

        pub f_GetFamilies: unsafe extern "C" fn(this: *const IFontCollection, /* out */ count: *mut u32) -> *const *mut IFontFamily,
        pub f_ClearNativeFamiliesCache: unsafe extern "C" fn(this: *const IFontCollection) -> (),
        pub f_FindDefaultFamily: unsafe extern "C" fn(this: *const IFontCollection) -> u32,
    }

    #[repr(C)]
    #[derive(Debug)]
    pub struct VitualTable_IFontFace {
        b: <IUnknown as Interface>::VitualTable,

        pub f_get_Id: unsafe extern "C" fn(this: *const IFontFace) -> u64,
        pub f_get_Info: unsafe extern "C" fn(this: *const IFontFace) -> *const NFontInfo,
        pub f_Equals: unsafe extern "C" fn(this: *const IFontFace, other: *mut IFontFace) -> bool,
        pub f_HashCode: unsafe extern "C" fn(this: *const IFontFace) -> i32,
        pub f_GetFamilyNames: unsafe extern "C" fn(this: *const IFontFace, ctx: *mut core::ffi::c_void, add: unsafe extern "C" fn(*mut core::ffi::c_void, *mut u16, i32, *mut u16, i32) -> core::ffi::c_void) -> HResult,
        pub f_GetFaceNames: unsafe extern "C" fn(this: *const IFontFace, ctx: *mut core::ffi::c_void, add: unsafe extern "C" fn(*mut core::ffi::c_void, *mut u16, i32, *mut u16, i32) -> core::ffi::c_void) -> HResult,
    }

    #[repr(C)]
    #[derive(Debug)]
    pub struct VitualTable_IFontFallback {
        b: <IUnknown as Interface>::VitualTable,

    }

    #[repr(C)]
    #[derive(Debug)]
    pub struct VitualTable_IFontFallbackBuilder {
        b: <IUnknown as Interface>::VitualTable,

        pub f_Build: unsafe extern "C" fn(this: *const IFontFallbackBuilder, ff: *mut *mut IFontFallback) -> HResult,
        pub f_Add: unsafe extern "C" fn(this: *const IFontFallbackBuilder, name: *const u16, length: i32, exists: *mut bool) -> HResult,
        pub f_AddLocaled: unsafe extern "C" fn(this: *const IFontFallbackBuilder, locale: *const u16, name: *const u16, name_length: i32, exists: *mut bool) -> HResult,
    }

    #[repr(C)]
    #[derive(Debug)]
    pub struct VitualTable_IFontFamily {
        b: <IUnknown as Interface>::VitualTable,

        pub f_GetLocalNames: unsafe extern "C" fn(this: *const IFontFamily, /* out */ length: *mut u32) -> *const Str16,
        pub f_GetNames: unsafe extern "C" fn(this: *const IFontFamily, /* out */ length: *mut u32) -> *const FontFamilyNameInfo,
        pub f_ClearNativeNamesCache: unsafe extern "C" fn(this: *const IFontFamily) -> (),
        pub f_GetFonts: unsafe extern "C" fn(this: *const IFontFamily, /* out */ length: *mut u32, /* out */ pair: *mut *const NFontPair) -> HResult,
        pub f_ClearNativeFontsCache: unsafe extern "C" fn(this: *const IFontFamily) -> (),
    }

    #[repr(C)]
    #[derive(Debug)]
    pub struct VitualTable_IFontManager {
        b: <IUnknown as Interface>::VitualTable,

        pub f_SetAssocUpdate: unsafe extern "C" fn(this: *const IFontManager, Data: *mut core::ffi::c_void, OnDrop: unsafe extern "C" fn(*mut core::ffi::c_void) -> core::ffi::c_void, OnAdd: unsafe extern "C" fn(*mut core::ffi::c_void, *mut IFontFace, u64) -> core::ffi::c_void, OnExpired: unsafe extern "C" fn(*mut core::ffi::c_void, *mut IFontFace, u64) -> core::ffi::c_void) -> u64,
        pub f_RemoveAssocUpdate: unsafe extern "C" fn(this: *const IFontManager, AssocUpdateId: u64) -> (),
        pub f_SetExpireFrame: unsafe extern "C" fn(this: *const IFontManager, FrameCount: u64) -> (),
        pub f_SetExpireTime: unsafe extern "C" fn(this: *const IFontManager, TimeTicks: u64) -> (),
        pub f_GetCurrentFrame: unsafe extern "C" fn(this: *const IFontManager) -> u64,
        pub f_Update: unsafe extern "C" fn(this: *const IFontManager, CurrentTime: u64) -> (),
        pub f_FontFaceToId: unsafe extern "C" fn(this: *const IFontManager, Face: *mut IFontFace) -> u64,
        pub f_IdToFontFace: unsafe extern "C" fn(this: *const IFontManager, Id: u64) -> *mut IFontFace,
    }

    #[repr(C)]
    #[derive(Debug)]
    pub struct VitualTable_ILayout {
        b: <IUnknown as Interface>::VitualTable,

        pub f_Calc: unsafe extern "C" fn(this: *const ILayout, ctx: *mut NLayoutContext) -> HResult,
    }

    #[repr(C)]
    #[derive(Debug)]
    pub struct VitualTable_ILib {
        b: <IUnknown as Interface>::VitualTable,

        pub f_SetLogger: unsafe extern "C" fn(this: *const ILib, obj: *mut core::ffi::c_void, logger: unsafe extern "C" fn(*mut core::ffi::c_void, LogLevel, StrKind, i32, *mut core::ffi::c_void) -> core::ffi::c_void, is_enabled: unsafe extern "C" fn(*mut core::ffi::c_void, LogLevel) -> u8, drop: unsafe extern "C" fn(*mut core::ffi::c_void) -> core::ffi::c_void) -> (),
        pub f_ClearLogger: unsafe extern "C" fn(this: *const ILib) -> (),
        pub f_GetCurrentErrorMessage: unsafe extern "C" fn(this: *const ILib) -> Str8,
        pub f_CreateFontManager: unsafe extern "C" fn(this: *const ILib, fm: *mut *mut IFontManager) -> HResult,
        pub f_GetSystemFontCollection: unsafe extern "C" fn(this: *const ILib, fc: *mut *mut IFontCollection) -> HResult,
        pub f_GetSystemFontFallback: unsafe extern "C" fn(this: *const ILib, ff: *mut *mut IFontFallback) -> HResult,
        pub f_CreateFontFallbackBuilder: unsafe extern "C" fn(this: *const ILib, ffb: *mut *mut IFontFallbackBuilder, info: *const FontFallbackBuilderCreateInfo) -> HResult,
        pub f_CreateLayout: unsafe extern "C" fn(this: *const ILib, layout: *mut *mut ILayout) -> HResult,
        pub f_SplitTexts: unsafe extern "C" fn(this: *const ILib, ranges: *mut NativeList<TextRange>, chars: *const u16, len: i32) -> HResult,
    }

    #[repr(C)]
    #[derive(Debug)]
    pub struct VitualTable_IPath {
        b: <IUnknown as Interface>::VitualTable,

        pub f_CalcAABB: unsafe extern "C" fn(this: *const IPath, out_aabb: *mut AABB2DF) -> (),
    }

    #[repr(C)]
    #[derive(Debug)]
    pub struct VitualTable_IPathBuilder {
        b: <IUnknown as Interface>::VitualTable,

        pub f_Build: unsafe extern "C" fn(this: *const IPathBuilder, path: *mut *mut IPath) -> HResult,
        pub f_Reserve: unsafe extern "C" fn(this: *const IPathBuilder, Endpoints: i32, CtrlPoints: i32) -> (),
        pub f_Batch: unsafe extern "C" fn(this: *const IPathBuilder, cmds: *const PathBuilderCmd, num_cmds: i32) -> (),
        pub f_Close: unsafe extern "C" fn(this: *const IPathBuilder) -> (),
        pub f_MoveTo: unsafe extern "C" fn(this: *const IPathBuilder, x: f32, y: f32) -> (),
        pub f_LineTo: unsafe extern "C" fn(this: *const IPathBuilder, x: f32, y: f32) -> (),
        pub f_QuadraticBezierTo: unsafe extern "C" fn(this: *const IPathBuilder, ctrl_x: f32, ctrl_y: f32, to_x: f32, to_y: f32) -> (),
        pub f_CubicBezierTo: unsafe extern "C" fn(this: *const IPathBuilder, ctrl0_x: f32, ctrl0_y: f32, ctrl1_x: f32, ctrl1_y: f32, to_x: f32, to_y: f32) -> (),
        pub f_Arc: unsafe extern "C" fn(this: *const IPathBuilder, center_x: f32, center_y: f32, radii_x: f32, radii_y: f32, sweep_angle: f32, x_rotation: f32) -> (),
    }

    #[repr(C)]
    #[derive(Debug)]
    pub struct VitualTable_IStub {
        b: <IUnknown as Interface>::VitualTable,

        pub f_Some: unsafe extern "C" fn(this: *const IStub, a: NodeType, b: *mut RootData, c: *mut NString) -> (),
    }

    #[repr(C)]
    #[derive(Debug)]
    pub struct VitualTable_ITessellator {
        b: <IUnknown as Interface>::VitualTable,

        pub f_Fill: unsafe extern "C" fn(this: *const ITessellator, path: *mut IPath, options: *mut TessFillOptions) -> HResult,
        pub f_Stroke: unsafe extern "C" fn(this: *const ITessellator, path: *mut IPath, options: *mut TessStrokeOptions) -> HResult,
    }

    #[repr(C)]
    #[derive(Debug)]
    pub struct VitualTable_ITextData {
        b: <IUnknown as Interface>::VitualTable,

    }

    #[repr(C)]
    #[derive(Debug)]
    pub struct VitualTable_ITextLayout {
        b: <IUnknown as Interface>::VitualTable,

    }
}

pub mod impls {
    pub use cocom::impls::*;
    use cocom::{Guid, HResult};

    pub trait IAtlasAllocator : IUnknown {
        fn Clear(&mut self) -> ();
        fn get_IsEmpty(&mut self) -> bool;
        fn GetSize(&mut self, out_width: *mut i32, out_height: *mut i32) -> ();
        fn Allocate(&mut self, width: i32, height: i32, out_id: *mut u32, out_rect: *mut super::AABB2D) -> bool;
        fn Deallocate(&mut self, id: u32) -> ();
        fn GetRect(&mut self, id: u32, out_rect: *mut super::AABB2D) -> ();
    }

    pub trait IFont : IUnknown {
        fn get_Info(& self) -> *const super::NFontInfo;
        fn CreateFace(& self, /* out */ face: *mut *mut super::IFontFace, manager: *mut super::IFontManager) -> HResult;
    }

    pub trait IFontCollection : IUnknown {
        fn GetFamilies(& self, /* out */ count: *mut u32) -> *const *mut super::IFontFamily;
        fn ClearNativeFamiliesCache(&mut self) -> ();
        fn FindDefaultFamily(&mut self) -> u32;
    }

    pub trait IFontFace : IUnknown {
        fn get_Id(& self) -> u64;
        fn get_Info(& self) -> *const super::NFontInfo;
        fn Equals(& self, other: *mut super::IFontFace) -> bool;
        fn HashCode(& self) -> i32;
        fn GetFamilyNames(& self, ctx: *mut core::ffi::c_void, add: unsafe extern "C" fn(*mut core::ffi::c_void, *mut u16, i32, *mut u16, i32) -> core::ffi::c_void) -> HResult;
        fn GetFaceNames(& self, ctx: *mut core::ffi::c_void, add: unsafe extern "C" fn(*mut core::ffi::c_void, *mut u16, i32, *mut u16, i32) -> core::ffi::c_void) -> HResult;
    }

    pub trait IFontFallback : IUnknown {
    }

    pub trait IFontFallbackBuilder : IUnknown {
        fn Build(&mut self, ff: *mut *mut super::IFontFallback) -> HResult;
        fn Add(&mut self, name: *const u16, length: i32, exists: *mut bool) -> HResult;
        fn AddLocaled(&mut self, locale: *const u16, name: *const u16, name_length: i32, exists: *mut bool) -> HResult;
    }

    pub trait IFontFamily : IUnknown {
        fn GetLocalNames(& self, /* out */ length: *mut u32) -> *const super::Str16;
        fn GetNames(& self, /* out */ length: *mut u32) -> *const super::FontFamilyNameInfo;
        fn ClearNativeNamesCache(&mut self) -> ();
        fn GetFonts(&mut self, /* out */ length: *mut u32, /* out */ pair: *mut *const super::NFontPair) -> HResult;
        fn ClearNativeFontsCache(&mut self) -> ();
    }

    pub trait IFontManager : IUnknown {
        fn SetAssocUpdate(&mut self, Data: *mut core::ffi::c_void, OnDrop: unsafe extern "C" fn(*mut core::ffi::c_void) -> core::ffi::c_void, OnAdd: unsafe extern "C" fn(*mut core::ffi::c_void, *mut super::IFontFace, u64) -> core::ffi::c_void, OnExpired: unsafe extern "C" fn(*mut core::ffi::c_void, *mut super::IFontFace, u64) -> core::ffi::c_void) -> u64;
        fn RemoveAssocUpdate(&mut self, AssocUpdateId: u64) -> ();
        fn SetExpireFrame(&mut self, FrameCount: u64) -> ();
        fn SetExpireTime(&mut self, TimeTicks: u64) -> ();
        fn GetCurrentFrame(& self) -> u64;
        fn Update(&mut self, CurrentTime: u64) -> ();
        fn FontFaceToId(&mut self, Face: *mut super::IFontFace) -> u64;
        fn IdToFontFace(&mut self, Id: u64) -> *mut super::IFontFace;
    }

    pub trait ILayout : IUnknown {
        fn Calc(&mut self, ctx: *mut super::NLayoutContext) -> HResult;
    }

    pub trait ILib : IUnknown {
        fn SetLogger(&mut self, obj: *mut core::ffi::c_void, logger: unsafe extern "C" fn(*mut core::ffi::c_void, super::LogLevel, super::StrKind, i32, *mut core::ffi::c_void) -> core::ffi::c_void, is_enabled: unsafe extern "C" fn(*mut core::ffi::c_void, super::LogLevel) -> u8, drop: unsafe extern "C" fn(*mut core::ffi::c_void) -> core::ffi::c_void) -> ();
        fn ClearLogger(&mut self) -> ();
        fn GetCurrentErrorMessage(&mut self) -> super::Str8;
        fn CreateFontManager(&mut self, fm: *mut *mut super::IFontManager) -> HResult;
        fn GetSystemFontCollection(&mut self, fc: *mut *mut super::IFontCollection) -> HResult;
        fn GetSystemFontFallback(&mut self, ff: *mut *mut super::IFontFallback) -> HResult;
        fn CreateFontFallbackBuilder(&mut self, ffb: *mut *mut super::IFontFallbackBuilder, info: *const super::FontFallbackBuilderCreateInfo) -> HResult;
        fn CreateLayout(&mut self, layout: *mut *mut super::ILayout) -> HResult;
        fn SplitTexts(&mut self, ranges: *mut super::NativeList<super::TextRange>, chars: *const u16, len: i32) -> HResult;
    }

    pub trait IPath : IUnknown {
        fn CalcAABB(&mut self, out_aabb: *mut super::AABB2DF) -> ();
    }

    pub trait IPathBuilder : IUnknown {
        fn Build(&mut self, path: *mut *mut super::IPath) -> HResult;
        fn Reserve(&mut self, Endpoints: i32, CtrlPoints: i32) -> ();
        fn Batch(&mut self, cmds: *const super::PathBuilderCmd, num_cmds: i32) -> ();
        fn Close(&mut self) -> ();
        fn MoveTo(&mut self, x: f32, y: f32) -> ();
        fn LineTo(&mut self, x: f32, y: f32) -> ();
        fn QuadraticBezierTo(&mut self, ctrl_x: f32, ctrl_y: f32, to_x: f32, to_y: f32) -> ();
        fn CubicBezierTo(&mut self, ctrl0_x: f32, ctrl0_y: f32, ctrl1_x: f32, ctrl1_y: f32, to_x: f32, to_y: f32) -> ();
        fn Arc(&mut self, center_x: f32, center_y: f32, radii_x: f32, radii_y: f32, sweep_angle: f32, x_rotation: f32) -> ();
    }

    pub trait IStub : IUnknown {
        fn Some(&mut self, a: super::NodeType, b: *mut super::RootData, c: *mut super::NString) -> ();
    }

    pub trait ITessellator : IUnknown {
        fn Fill(&mut self, path: *mut super::IPath, options: *mut super::TessFillOptions) -> HResult;
        fn Stroke(&mut self, path: *mut super::IPath, options: *mut super::TessStrokeOptions) -> HResult;
    }

    pub trait ITextData : IUnknown {
    }

    pub trait ITextLayout : IUnknown {
    }
}
