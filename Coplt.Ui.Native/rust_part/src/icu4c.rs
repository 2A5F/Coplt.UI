use std::ffi::{CStr, c_char, c_schar, c_void};

#[repr(transparent)]
#[derive(Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub struct UErrorCode(std::ffi::c_int);

impl UErrorCode {
    fn is_ok(&self) -> bool {
        self.0 == 0
    }

    fn is_failed(&self) -> bool {
        self.0 > 0
    }

    fn get_name(&self) -> &'static CStr {
        unsafe extern "C" {
            fn u_errorName(_: UErrorCode) -> *const c_schar;
        }
        unsafe { CStr::from_ptr(u_errorName(*self)) }
    }
}

impl std::fmt::Debug for UErrorCode {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.debug_tuple("UErrorCode")
            .field(&self.0)
            .field(&self.get_name())
            .finish()
    }
}

impl std::fmt::Display for UErrorCode {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.write_fmt(format_args!(
            "UErrorCode({})",
            self.get_name().to_string_lossy()
        ))
    }
}

impl From<UErrorCode> for anyhow::Error {
    fn from(value: UErrorCode) -> Self {
        anyhow::Error::msg(value)
    }
}

#[repr(transparent)]
#[derive(Debug)]
pub struct UBiDi(*mut c_void);

impl UBiDi {
    pub fn new() -> Self {
        unsafe extern "C" {
            fn ubidi_open() -> *mut c_void;
        }

        Self(unsafe { ubidi_open() })
    }
}

impl Drop for UBiDi {
    fn drop(&mut self) {
        unsafe extern "C" {
            fn ubidi_close(_: *mut c_void);
        }
        unsafe {
            ubidi_close(self.0);
        }
    }
}

#[repr(u8)]
#[non_exhaustive]
#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub enum UBiDiLevel {
    LeftToRight = 0,
    RightToLeft = 1,
    DefaultLeftToRight = 0xfe,
    DefaultRightToLeft = 0xff,
}

#[repr(i32)]
#[non_exhaustive]
#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub enum UBiDiDirection {
    LeftToRight,
    RightToLeft,
    Mixed,
    Neutral,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub struct UParagraphInfo {
    pub start: i32,
    pub limit: i32,
    pub level: UBiDiLevel,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub struct UVisualRunInfo {
    pub logical_start: i32,
    pub length: i32,
    pub direction: UBiDiDirection,
}

impl UBiDi {
    pub fn set_para(&mut self, text: &[u16], para_level: UBiDiLevel) -> Result<(), UErrorCode> {
        unsafe extern "C" {
            fn ubidi_setPara(
                _: *mut c_void,
                text: *const u16,
                length: i32,
                paraLevel: UBiDiLevel,
                embeddingLevels: *mut c_void,
                pErrorCode: *mut UErrorCode,
            );
        }

        let mut status = UErrorCode(0);

        unsafe {
            ubidi_setPara(
                self.0,
                text.as_ptr(),
                text.len() as i32,
                para_level,
                std::ptr::null_mut(),
                &mut status,
            );
        }

        if status.is_ok() { Ok(()) } else { Err(status) }
    }

    pub fn get_direction(&self) -> UBiDiDirection {
        unsafe extern "C" {
            fn ubidi_getDirection(_: *mut c_void) -> UBiDiDirection;
        }
        unsafe { ubidi_getDirection(self.0) }
    }

    pub fn get_processed_length(&self) -> i32 {
        unsafe extern "C" {
            fn ubidi_getProcessedLength(_: *mut c_void) -> i32;
        }
        unsafe { ubidi_getProcessedLength(self.0) }
    }

    pub fn count_paragraphs(&self) -> i32 {
        unsafe extern "C" {
            fn ubidi_countParagraphs(_: *mut c_void) -> i32;
        }
        unsafe { ubidi_countParagraphs(self.0) }
    }

    pub fn get_paragraph_by_index(&self, para_index: i32) -> Result<UParagraphInfo, UErrorCode> {
        unsafe extern "C" {
            fn ubidi_getParagraphByIndex(
                _: *mut c_void,
                paraIndex: i32,
                pParaStart: *mut i32,
                pParaLimit: *mut i32,
                pParaLevel: *mut UBiDiLevel,
                pErrorCode: *mut UErrorCode,
            );
        }
        let mut start = 0;
        let mut limit = 0;
        let mut level = UBiDiLevel::LeftToRight;
        let mut err = UErrorCode(0);
        unsafe {
            ubidi_getParagraphByIndex(
                self.0, para_index, &mut start, &mut limit, &mut level, &mut err,
            )
        };
        if err.is_failed() {
            Err(err)
        } else {
            Ok(UParagraphInfo {
                start,
                limit,
                level,
            })
        }
    }

    pub fn get_result_length(&self) -> i32 {
        unsafe extern "C" {
            fn ubidi_getResultLength(_: *mut c_void) -> i32;
        }
        unsafe { ubidi_getResultLength(self.0) }
    }

    pub fn get_level_at(&self, char_index: i32) -> UBiDiLevel {
        unsafe extern "C" {
            fn ubidi_getLevelAt(_: *mut c_void, charIndex: i32) -> UBiDiLevel;
        }
        unsafe { ubidi_getLevelAt(self.0, char_index) }
    }

    pub fn get_levels(&mut self) -> Result<&[UBiDiLevel], UErrorCode> {
        unsafe extern "C" {
            fn ubidi_getLevels(_: *mut c_void, err: *mut UErrorCode) -> *const UBiDiLevel;
        }

        let len = self.get_processed_length();
        let mut err = UErrorCode(0);
        let levels = unsafe { ubidi_getLevels(self.0, &mut err) };
        if err.is_failed() {
            Err(err)
        } else {
            Ok(unsafe { std::slice::from_raw_parts(levels, len as usize) })
        }
    }

    pub fn count_runs(&mut self) -> Result<i32, UErrorCode> {
        unsafe extern "C" {
            fn ubidi_countRuns(_: *mut c_void, err: *mut UErrorCode) -> i32;
        }
        let mut err = UErrorCode(0);
        let count = unsafe { ubidi_countRuns(self.0, &mut err) };
        if err.is_failed() { Err(err) } else { Ok(count) }
    }

    pub fn get_visual_run(&mut self, run_index: i32) -> UVisualRunInfo {
        unsafe extern "C" {
            fn ubidi_getVisualRun(
                _: *mut c_void,
                runIndex: i32,
                pLogicalStart: *mut i32,
                pLength: *mut i32,
            ) -> UBiDiDirection;
        }
        let mut logical_start = 0;
        let mut length = 0;
        let direction =
            unsafe { ubidi_getVisualRun(self.0, run_index, &mut logical_start, &mut length) };
        UVisualRunInfo {
            logical_start,
            length,
            direction,
        }
    }
}

pub mod script {
    use crate::icu4c::UErrorCode;
    use std::ffi::{CStr, c_char};

    pub fn get_script(codepoint: u32) -> Result<icu::properties::props::Script, UErrorCode> {
        unsafe extern "C" {
            fn uscript_getScript(codepoint: u32, err: *mut UErrorCode) -> i32;
        }

        let mut err = UErrorCode(0);

        let r = unsafe { uscript_getScript(codepoint, &mut err) };

        if err.is_failed() {
            Err(err)
        } else {
            Ok(icu::properties::props::Script::from_icu4c_value(r as _))
        }
    }

    pub fn get_short_name(script: icu::properties::props::Script) -> &'static str {
        unsafe extern "C" {
            fn uscript_getShortName(script: i32) -> *const c_char;
        }

        unsafe {
            let str = uscript_getShortName(script.to_icu4c_value() as i32);
            let c_str = CStr::from_ptr(str);
            return str::from_utf8_unchecked(c_str.to_bytes());
        }
    }
}
