#![allow(dead_code)]
#![allow(unused)]

use std::ffi::c_char;
use std::fmt::Debug;
use std::panic::UnwindSafe;

mod coplt_alloc {
    use core::alloc::GlobalAlloc;

    unsafe extern "C" {
        pub fn coplt_ui_malloc(size: usize, align: usize) -> *mut u8;
        pub fn coplt_ui_free(ptr: *mut u8, align: usize);
        pub fn coplt_ui_zalloc(size: usize, align: usize) -> *mut u8;
        pub fn coplt_ui_realloc(ptr: *mut u8, new_size: usize, align: usize) -> *mut u8;
    }

    pub unsafe fn coplt_free<T>(ptr: *mut T) {
        unsafe { coplt_ui_free(ptr as *mut u8, align_of::<T>()) }
    }

    pub unsafe fn coplt_alloc_array<T>(size: usize) -> *mut T {
        (unsafe { coplt_ui_malloc(size_of::<T>() * size, align_of::<T>()) }) as *mut T
    }

    pub unsafe fn coplt_zalloc_array<T>(size: usize) -> *mut T {
        (unsafe { coplt_ui_zalloc(size_of::<T>() * size, align_of::<T>()) }) as *mut T
    }

    pub unsafe fn coplt_realloc_array<T>(old: *mut T, new_size: usize) -> *mut T {
        (unsafe { coplt_ui_realloc(old as *mut u8, size_of::<T>() * new_size, align_of::<T>()) })
            as *mut T
    }

    #[global_allocator]
    static GLOBAL_ALLOC: CopltAlloc = CopltAlloc;

    struct CopltAlloc;

    unsafe impl GlobalAlloc for CopltAlloc {
        unsafe fn alloc(&self, layout: std::alloc::Layout) -> *mut u8 {
            unsafe { coplt_ui_malloc(layout.size(), layout.align()) }
        }

        unsafe fn dealloc(&self, ptr: *mut u8, layout: std::alloc::Layout) {
            unsafe { coplt_ui_free(ptr, layout.align()) }
        }

        unsafe fn alloc_zeroed(&self, layout: std::alloc::Layout) -> *mut u8 {
            unsafe { coplt_ui_zalloc(layout.size(), layout.align()) }
        }

        unsafe fn realloc(
            &self,
            ptr: *mut u8,
            layout: std::alloc::Layout,
            new_size: usize,
        ) -> *mut u8 {
            unsafe { coplt_ui_realloc(ptr, new_size, layout.align()) }
        }
    }
}

use cocom::{ComPtr, HResult, HResultE, object};
use coplt_alloc::*;

mod atlas;
mod col;
mod com;
#[cfg(target_os = "windows")]
mod dwrite;
mod font_manager;
mod icu4c;
mod layout;
mod unicode_utils;
mod utf16;
mod utils;

#[cfg(target_os = "windows")]
use dwrite::FontFace;
use taffy::{LengthPercentage, LengthPercentageAuto, ResolveOrZero};

mod error_message {
    #[repr(C)]
    pub struct RustString {
        pub p_rust_string_data: *const u8,
        pub rust_string_len: usize,
    }
    unsafe extern "C" {
        pub fn coplt_ui_set_current_error_message(msg: *const RustString);
    }
}

pub fn set_current_error_message(msg: String) {
    use error_message::*;

    unsafe {
        let str = RustString {
            rust_string_len: msg.len(),
            p_rust_string_data: msg.leak().as_ptr(),
        };
        coplt_ui_set_current_error_message(&str);
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn coplt_ui_get_user_ui_default_locale_impl(len: *mut usize) -> *const c_char {
    match sys_locale::get_locale() {
        Some(str) => unsafe {
            *len = str.len();
            str.leak().as_mut_ptr() as *const c_char
        },
        None => std::ptr::null_mut(),
    }
}

pub fn feb_hr(f: impl FnOnce() -> anyhow::Result<cocom::HResult> + UnwindSafe) -> cocom::HResult {
    match std::panic::catch_unwind(f) {
        Ok(ok) => match ok {
            Ok(ok) => ok,
            Err(e) => {
                let msg = format!("{e:?}");
                set_current_error_message(msg);
                cocom::HResultE::Fail.into()
            }
        },
        Err(mut err) => {
            if let Some(err) = err.downcast_mut::<HResult>() {
                return *err;
            } else if let Some(err) = err.downcast_mut::<HResultE>() {
                return (*err).into();
            } else if let Some(e) = err.downcast_mut::<anyhow::Error>() {
                let msg = format!("{e:?}");
                set_current_error_message(msg);
                cocom::HResultE::Fail.into()
            } else {
                // cannot process
                std::panic::resume_unwind(err)
            }
        }
    }
}

pub trait IsZeroLength {
    fn is_zero_length(&self) -> bool;
}

impl<T: IsZeroLength> IsZeroLength for Option<T> {
    fn is_zero_length(&self) -> bool {
        match self {
            Some(v) => v.is_zero_length(),
            None => true,
        }
    }
}

impl IsZeroLength for LengthPercentageAuto {
    fn is_zero_length(&self) -> bool {
        self.resolve_or_zero(None, |_, _| 0.0) <= 0.0
    }
}

impl IsZeroLength for LengthPercentage {
    fn is_zero_length(&self) -> bool {
        self.resolve_or_zero(None, |_, _| 0.0) <= 0.0
    }
}

mod com_impl {
    use std::{
        ffi::c_void,
        fmt::Display,
        ops::{Deref, DerefMut},
        str::FromStr,
    };

    use etagere::euclid::Rect;
    use taffy::{
        CoreStyle, LengthPercentage, LengthPercentageAuto, ResolveOrZero,
        prelude::{TaffyAuto, TaffyZero},
    };

    use crate::{
        col::{NArc, NBitSet, NList},
        com::{
            AvailableSpaceType, ChildsData, CommonData, CursorType, FontWeight, FontWidth,
            GlyphData, IFontFallback, LayoutCache, LayoutCacheEntryLayoutOutput,
            LayoutCacheEntrySize, LayoutCacheFlags, LayoutCollapsibleMarginSet, LayoutData,
            LayoutResult, LineAlign, LocaleId, NString, NativeArc, NativeList, PointerEvents,
            StyleData, TextAlign, TextData_BidiRange, TextData_FontRange, TextData_LocaleRange,
            TextData_RunRange, TextData_SameStyleRange, TextData_ScriptRange, TextDirection,
            TextOrientation, TextOverflow, TextParagraphData, TextSpanData, TextSpanNode,
            TextStyleData, TextStyleOverride, TextWrap, WordBreak, WrapFlags, WritingDirection,
        },
        layout::FontRange,
    };

    use super::*;

    unsafe impl<T> Send for com::NativeList<T> {}
    unsafe impl<T> Sync for com::NativeList<T> {}

    unsafe impl<T> Send for col::OrderedSet<T> {}
    unsafe impl<T> Sync for col::OrderedSet<T> {}

    impl Debug for com::GridTemplateComponent {
        fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
            let mut a = f.debug_tuple("GridTemplateComponent");
            unsafe {
                match self.Type {
                    com::GridTemplateComponentType::Single => a.field(&self.Union.Single),
                    com::GridTemplateComponentType::Repeat => a.field(&self.Union.Repeat),
                }
            }
            .finish()
        }
    }

    impl PartialEq for com::GridTemplateComponent {
        fn eq(&self, other: &Self) -> bool {
            unsafe {
                self.Type == other.Type
                    && match self.Type {
                        com::GridTemplateComponentType::Single => {
                            self.Union.Single == other.Union.Single
                        }
                        com::GridTemplateComponentType::Repeat => {
                            self.Union.Repeat == other.Union.Repeat
                        }
                    }
            }
        }
    }

    impl PartialOrd for com::GridTemplateComponent {
        fn partial_cmp(&self, other: &Self) -> Option<std::cmp::Ordering> {
            unsafe {
                match self.Type.partial_cmp(&other.Type) {
                    Some(core::cmp::Ordering::Equal) => match self.Type {
                        com::GridTemplateComponentType::Single => {
                            self.Union.Single.partial_cmp(&other.Union.Single)
                        }
                        com::GridTemplateComponentType::Repeat => {
                            self.Union.Repeat.partial_cmp(&other.Union.Repeat)
                        }
                    },
                    ord => return ord,
                }
            }
        }
    }

    impl<T> NativeArc<T> {
        pub fn as_n_arc(&self) -> &NArc<T> {
            unsafe { std::mem::transmute(self) }
        }
        pub fn as_n_arc_mut(&mut self) -> &mut NArc<T> {
            unsafe { std::mem::transmute(self) }
        }
    }

    impl<T> Deref for NativeArc<T> {
        type Target = NArc<T>;

        fn deref(&self) -> &Self::Target {
            self.as_n_arc()
        }
    }

    impl<T> DerefMut for NativeArc<T> {
        fn deref_mut(&mut self) -> &mut Self::Target {
            self.as_n_arc_mut()
        }
    }

    impl LocaleId {
        pub fn or(self, other: Self) -> Self {
            if self.Name.is_null() { other } else { self }
        }

        pub fn get_utf16(self) -> Option<&'static [u16]> {
            if self.Name.is_null() {
                None
            } else {
                Some(unsafe { std::slice::from_raw_parts(self.Name as _, self.Length) })
            }
        }
        pub fn get_utf8(self) -> Option<&'static [u8]> {
            if self.Name.is_null() {
                None
            } else {
                Some(unsafe {
                    std::slice::from_raw_parts(self.Name.add(self.Length * 2 + 2), self.Length)
                })
            }
        }
        pub fn get_str(self) -> Option<&'static str> {
            Some(unsafe { std::str::from_utf8_unchecked(self.get_utf8()?) })
        }
        pub fn to_language(self) -> Option<harfrust::Language> {
            self.get_str()
                .and_then(|str| harfrust::Language::from_str(str).ok())
        }
    }

    impl Deref for NString {
        type Target = [u16];

        fn deref(&self) -> &Self::Target {
            unsafe { std::slice::from_raw_parts(self.m_str, self.m_len as usize) }
        }
    }

    impl LayoutData {
        pub fn is_layout_dirty(&self, doc: &layout::SubDocInner) -> bool {
            self.LayoutDirtyFrame == doc.current_frame()
        }
    }

    impl Default for LayoutResult {
        fn default() -> Self {
            Self {
                Order: 0,
                LocationX: 0.0,
                LocationY: 0.0,
                Width: 0.0,
                Height: 0.0,
                ContentWidth: 0.0,
                ContentHeight: 0.0,
                ScrollXSize: 0.0,
                ScrollYSize: 0.0,
                BorderTopSize: 0.0,
                BorderRightSize: 0.0,
                BorderBottomSize: 0.0,
                BorderLeftSize: 0.0,
                PaddingTopSize: 0.0,
                PaddingRightSize: 0.0,
                PaddingBottomSize: 0.0,
                PaddingLeftSize: 0.0,
                MarginTopSize: 0.0,
                MarginRightSize: 0.0,
                MarginBottomSize: 0.0,
                MarginLeftSize: 0.0,
            }
        }
    }

    impl Default for LayoutCache {
        fn default() -> Self {
            Self {
                FinalLayoutEntry: Default::default(),
                MeasureEntries0: Default::default(),
                MeasureEntries1: Default::default(),
                MeasureEntries2: Default::default(),
                MeasureEntries3: Default::default(),
                MeasureEntries4: Default::default(),
                MeasureEntries5: Default::default(),
                MeasureEntries6: Default::default(),
                MeasureEntries7: Default::default(),
                MeasureEntries8: Default::default(),
                Flags: LayoutCacheFlags::empty(),
            }
        }
    }

    impl Default for LayoutCacheEntryLayoutOutput {
        fn default() -> Self {
            Self {
                KnownDimensionsWidthValue: Default::default(),
                KnownDimensionsHeightValue: Default::default(),
                AvailableSpaceWidthValue: Default::default(),
                AvailableSpaceHeightValue: Default::default(),
                HasKnownDimensionsWidth: Default::default(),
                HasKnownDimensionsHeight: Default::default(),
                AvailableSpaceWidth: AvailableSpaceType::Definite,
                AvailableSpaceHeight: AvailableSpaceType::Definite,
                Content: Default::default(),
            }
        }
    }

    impl Default for LayoutCacheEntrySize {
        fn default() -> Self {
            Self {
                KnownDimensionsWidthValue: Default::default(),
                KnownDimensionsHeightValue: Default::default(),
                AvailableSpaceWidthValue: Default::default(),
                AvailableSpaceHeightValue: Default::default(),
                HasKnownDimensionsWidth: Default::default(),
                HasKnownDimensionsHeight: Default::default(),
                AvailableSpaceWidth: AvailableSpaceType::Definite,
                AvailableSpaceHeight: AvailableSpaceType::Definite,
                ContentWidth: Default::default(),
                ContentHeight: Default::default(),
            }
        }
    }

    impl Default for com::LayoutOutput {
        fn default() -> Self {
            Self {
                Width: Default::default(),
                Height: Default::default(),
                ContentWidth: Default::default(),
                ContentHeight: Default::default(),
                FirstBaselinesX: Default::default(),
                FirstBaselinesY: Default::default(),
                TopMargin: Default::default(),
                BottomMargin: Default::default(),
                HasFirstBaselinesX: Default::default(),
                HasFirstBaselinesY: Default::default(),
                MarginsCanCollapseThrough: Default::default(),
            }
        }
    }

    impl Default for LayoutCollapsibleMarginSet {
        fn default() -> Self {
            Self {
                Positive: Default::default(),
                Negative: Default::default(),
            }
        }
    }

    impl Default for TextData_RunRange {
        fn default() -> Self {
            Self {
                Start: Default::default(),
                End: Default::default(),
                ScriptRange: Default::default(),
                BidiRange: Default::default(),
                StyleRange: Default::default(),
                FontRange: Default::default(),
                GlyphStart: Default::default(),
                GlyphEnd: Default::default(),
                Ascent: Default::default(),
                Descent: Default::default(),
                Leading: Default::default(),
            }
        }
    }

    macro_rules! GetTextStyle {
        { $src:expr => $name:ident { $get:ident } } => {
            if $src.Override.contains(TextStyleOverride::$name) {
                Some($get!{ $src => $name })
            } else {
                None
            }
        };
        { $src:expr => $name:ident } => {
            if $src.Override.contains(TextStyleOverride::$name) {
                Some($src.$name)
            } else {
                None
            }
        };
        { fn $name:ident -> $type:ty $({ $get:ident })? } => {
            pub fn $name(&self) -> Option<$type> {
                GetTextStyle!(self => $name $({ $get })?)
            }
        };
        { impl $struct:ty { $(fn $name:ident -> $type:ty $({ $get:ident })?;)* } } => {
            #[allow(non_snake_case)]
            impl $struct {
                $(GetTextStyle! { fn $name -> $type $({ $get })? })*
            }
        };
    }

    #[allow(non_snake_case)]
    impl StyleData {
        pub fn LineHeight(&self) -> LengthPercentageAuto {
            c_length_percentage_auto!(self => LineHeight)
        }
    }

    GetTextStyle! {
        impl TextStyleData
        {
            fn FontFallback -> *mut IFontFallback;
            fn Locale -> LocaleId;
            fn TextColorR -> f32;
            fn TextColorG -> f32;
            fn TextColorB -> f32;
            fn TextColorA -> f32;
            fn Opacity -> f32;
            fn BackgroundColorR -> f32;
            fn BackgroundColorG -> f32;
            fn BackgroundColorB -> f32;
            fn BackgroundColorA -> f32;
            fn InsertLeft -> LengthPercentageAuto { c_length_percentage_auto };
            fn InsertTop -> LengthPercentageAuto { c_length_percentage_auto };
            fn InsertRight -> LengthPercentageAuto { c_length_percentage_auto };
            fn InsertBottom -> LengthPercentageAuto { c_length_percentage_auto };
            fn MarginLeft -> LengthPercentageAuto { c_length_percentage_auto };
            fn MarginTop -> LengthPercentageAuto { c_length_percentage_auto };
            fn MarginRight -> LengthPercentageAuto { c_length_percentage_auto };
            fn MarginBottom -> LengthPercentageAuto { c_length_percentage_auto };
            fn PaddingLeft -> LengthPercentage { c_length_percentage };
            fn PaddingTop -> LengthPercentage { c_length_percentage };
            fn PaddingRight -> LengthPercentage { c_length_percentage };
            fn PaddingBottom -> LengthPercentage { c_length_percentage };
            fn TabSize -> LengthPercentageAuto { c_length_percentage_auto };
            fn FontSize -> f32;
            fn FontWidth -> FontWidth;
            fn FontOblique -> f32;
            fn FontWeight -> FontWeight;
            fn LineHeight -> LengthPercentageAuto { c_length_percentage_auto };
            fn Cursor -> CursorType;
            fn PointerEvents -> PointerEvents;
            fn FontItalic -> bool;
            fn FontOpticalSizing -> bool;
            fn WrapFlags -> WrapFlags;
            fn TextWrap -> TextWrap;
            fn WordBreak -> WordBreak;
            fn TextOrientation -> TextOrientation;
        }
    }

    impl TextStyleData {
        #[inline(always)]
        pub fn margin(&self) -> taffy::Rect<LengthPercentageAuto> {
            taffy::Rect {
                left: self.MarginLeft().unwrap_or(LengthPercentageAuto::AUTO),
                right: self.MarginRight().unwrap_or(LengthPercentageAuto::AUTO),
                top: self.MarginTop().unwrap_or(LengthPercentageAuto::AUTO),
                bottom: self.MarginBottom().unwrap_or(LengthPercentageAuto::AUTO),
            }
        }
        #[inline(always)]
        pub fn padding(&self) -> taffy::Rect<LengthPercentage> {
            taffy::Rect {
                left: self.PaddingLeft().unwrap_or(LengthPercentage::ZERO),
                right: self.PaddingRight().unwrap_or(LengthPercentage::ZERO),
                top: self.PaddingTop().unwrap_or(LengthPercentage::ZERO),
                bottom: self.PaddingBottom().unwrap_or(LengthPercentage::ZERO),
            }
        }
    }

    impl TextSpanData {}

    impl TextParagraphData {
        pub fn is_text_dirty(&self, doc: &layout::SubDocInner) -> bool {
            self.TextDirtyFrame == doc.current_frame()
        }

        pub fn is_text_style_dirty(&self, doc: &layout::SubDocInner) -> bool {
            self.TextStyleDirtyFrame == doc.current_frame()
        }

        pub fn break_points(&mut self) -> &'static mut NBitSet {
            unsafe { std::mem::transmute(&mut self.m_break_points) }
        }

        pub fn grapheme_cluster(&mut self) -> &'static mut NList<u32> {
            unsafe { std::mem::transmute(&mut self.m_grapheme_cluster) }
        }

        pub fn script_ranges(&mut self) -> &'static mut NList<TextData_ScriptRange> {
            unsafe { std::mem::transmute(&mut self.m_script_ranges) }
        }

        pub fn bidi_ranges(&mut self) -> &'static mut NList<TextData_BidiRange> {
            unsafe { std::mem::transmute(&mut self.m_bidi_ranges) }
        }

        pub fn same_style_ranges(&mut self) -> &'static mut NList<TextData_SameStyleRange> {
            unsafe { std::mem::transmute(&mut self.m_same_style_ranges) }
        }

        pub fn locale_ranges(&mut self) -> &'static mut NList<TextData_LocaleRange> {
            unsafe { std::mem::transmute(&mut self.m_locale_ranges) }
        }

        pub fn font_ranges(&mut self) -> &'static mut NList<FontRange> {
            debug_assert_eq!(size_of::<FontRange>(), size_of::<TextData_FontRange>());

            unsafe { std::mem::transmute(&mut self.m_font_ranges) }
        }

        pub fn run_ranges(&mut self) -> &'static mut NList<TextData_RunRange> {
            unsafe { std::mem::transmute(&mut self.m_run_ranges) }
        }

        pub fn glyph_datas(&mut self) -> &'static mut NList<GlyphData> {
            unsafe { std::mem::transmute(&mut self.m_glyph_datas) }
        }
    }

    impl PartialEq<TextData_LocaleRange> for u32 {
        fn eq(&self, other: &TextData_LocaleRange) -> bool {
            *self >= other.Start && *self < other.End
        }
    }

    impl PartialOrd<TextData_LocaleRange> for u32 {
        fn partial_cmp(&self, other: &TextData_LocaleRange) -> Option<std::cmp::Ordering> {
            Some(TextData_LocaleRange::cmp_pos(*self, other))
        }

        fn lt(&self, other: &TextData_LocaleRange) -> bool {
            *self < other.Start
        }

        fn le(&self, other: &TextData_LocaleRange) -> bool {
            *self < other.End
        }

        fn gt(&self, other: &TextData_LocaleRange) -> bool {
            *self >= other.End
        }

        fn ge(&self, other: &TextData_LocaleRange) -> bool {
            *self >= other.Start
        }
    }

    impl TextData_LocaleRange {
        pub fn cmp_pos(pos: u32, this: &TextData_LocaleRange) -> std::cmp::Ordering {
            if pos < this.Start {
                std::cmp::Ordering::Less
            } else if pos >= this.End {
                std::cmp::Ordering::Greater
            } else {
                std::cmp::Ordering::Equal
            }
        }

        pub fn search_pos(
            pos: u32,
        ) -> impl for<'a> Fn(&'a TextData_LocaleRange) -> std::cmp::Ordering {
            move |this| Self::cmp_pos(pos, this)
        }
    }

    impl TextData_SameStyleRange {
        pub fn first_span(&self) -> Option<TextSpanNode> {
            if self.HasFirstSpan {
                Some(self.FirstSpanValue)
            } else {
                None
            }
        }
        pub fn style(&self, doc: &mut layout::SubDocInner) -> Option<&'static mut TextStyleData> {
            self.first_span().map(|span| span.text_style_data(doc))
        }
    }

    impl TextData_RunRange {
        pub fn get_font_range(&self, paragraph: &mut TextParagraphData) -> &'static mut FontRange {
            &mut paragraph.font_ranges()[self.FontRange as usize]
        }
        pub fn get_style_range(
            &self,
            paragraph: &mut TextParagraphData,
        ) -> &'static mut TextData_SameStyleRange {
            &mut paragraph.same_style_ranges()[self.StyleRange as usize]
        }
        pub fn get_script_range(
            &self,
            paragraph: &mut TextParagraphData,
        ) -> &'static mut TextData_ScriptRange {
            &mut paragraph.script_ranges()[self.ScriptRange as usize]
        }
        pub fn get_glyph_datas(&self, paragraph: &mut TextParagraphData) -> &'static [GlyphData] {
            &paragraph.glyph_datas()[self.GlyphStart as usize..self.GlyphEnd as usize]
        }
    }
}
