#![allow(unused)]
#![allow(non_snake_case)]

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
pub struct FontWidth {
    pub Width: f32,
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
