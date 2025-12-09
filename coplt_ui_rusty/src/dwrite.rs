use std::{ffi::c_void, mem::MaybeUninit, ptr::NonNull};

use crate::{com::*, feb_hr};
use cocom::{
    ComPtr, ComWeak, HResult, HResultE, MakeObject, MakeObjectWeak,
    impls::{ObjectBox, ObjectBoxNew},
    object::{Object, ObjectPtr},
    pmp,
};
use windows::Win32::Graphics::DirectWrite::{IDWriteFontFace5, IDWriteLocalizedStrings};

#[repr(C)]
#[derive(Debug)]
#[cocom::object(IFontFace)]
pub struct FontFace {
    dw_face: IDWriteFontFace5,
    frame_source: ComPtr<IFrameSource>,
    manager: ComWeak<IFontManager>,
    frame_time: FrameTime,
    info: NFontInfo,
}

unsafe extern "C" {
    #[allow(improper_ctypes)]
    fn coplt_ui_dwrite_get_font_face_info(face: *mut c_void, info: *mut NFontInfo);
}

#[unsafe(no_mangle)]
pub extern "C" fn coplt_ui_dwrite_create_font_face(
    face: *const IDWriteFontFace5,
    manager: *mut IFontManager,
    out: *mut *mut IFontFace,
) -> HResult {
    unsafe {
        let face = FontFace::new((*face).clone(), manager);
        *out = face.leak();
        HResultE::Ok.into()
    }
}

impl FontFace {
    pub fn new(face: IDWriteFontFace5, manager: *mut IFontManager) -> ObjectPtr<Self> {
        unsafe {
            Object::inplace(|this: *mut Self| {
                let frame_source = /*move*/ (*manager).GetFrameSource();
                (*frame_source).Get(pmp!(this; .frame_time));

                coplt_ui_dwrite_get_font_face_info(
                    std::mem::transmute_copy(&face),
                    pmp!(this; .info),
                );

                pmp!(this; .dw_face).write(face);
                pmp!(this; .frame_source).write(ComPtr::new(NonNull::new_unchecked(
                    /*move*/ frame_source,
                )));
                pmp!(this; .manager).write(ComWeak::downgrade(NonNull::new_unchecked(
                    /*clone*/ manager,
                )));
            })
        }
    }
}

impl impls::IFontFace for FontFace {
    fn get_Id(&self) -> u64 {
        unsafe { std::mem::transmute_copy(&self.dw_face) }
    }

    fn get_RefCount(&self) -> u32 {
        unsafe {
            let obj = Object::<Self>::FromValue(self as *const _ as _);
            Object::GetStrongCount(obj)
        }
    }

    fn get_FrameTime(&self) -> *const crate::com::FrameTime {
        &self.frame_time
    }

    fn GetFrameSource(&self) -> *mut crate::com::IFrameSource {
        unsafe { self.frame_source.ptr().as_mut() }
    }

    fn GetFontManager(&self) -> *mut crate::com::IFontManager {
        self.manager.upgrade().map(|a| a.leak()).unwrap_or_default()
    }

    fn get_Info(&self) -> *const crate::com::NFontInfo {
        &self.info
    }

    fn Equals(&self, other: *mut crate::com::IFontFace) -> bool {
        unsafe {
            let other = Object::<Self>::GetObject(other);
            self.dw_face.Equals(unsafe { &(*other).dw_face }).as_bool()
        }
    }

    fn HashCode(&self) -> i32 {
        let face: u64 = unsafe { std::mem::transmute_copy(&self.dw_face) };
        (face as i32) ^ ((face >> 32) as i32)
    }

    fn GetFamilyNames(
        &self,
        ctx: *mut core::ffi::c_void,
        add: unsafe extern "C" fn(*mut core::ffi::c_void, *mut u16, i32, *mut u16, i32) -> (),
    ) -> cocom::HResult {
        feb_hr(|| unsafe {
            let names = self.dw_face.GetFamilyNames()?;
            Self::get_names(&names, ctx, add)?;
            Ok((cocom::HResultE::Ok.into()))
        })
    }

    fn GetFaceNames(
        &self,
        ctx: *mut core::ffi::c_void,
        add: unsafe extern "C" fn(*mut core::ffi::c_void, *mut u16, i32, *mut u16, i32) -> (),
    ) -> cocom::HResult {
        feb_hr(|| unsafe {
            let names = self.dw_face.GetFaceNames()?;
            Self::get_names(&names, ctx, add)?;
            Ok((cocom::HResultE::Ok.into()))
        })
    }
}

impl FontFace {
    pub fn get_names(
        names: &IDWriteLocalizedStrings,
        ctx: *mut c_void,
        add: unsafe extern "C" fn(*mut core::ffi::c_void, *mut u16, i32, *mut u16, i32) -> (),
    ) -> anyhow::Result<()> {
        unsafe {
            let count = names.GetCount();
            let mut locale = vec![];
            let mut string = vec![];
            for i in 0..count {
                let locale_len = names.GetLocaleNameLength(i)?;
                if locale_len as usize + 1 > locale.len() {
                    locale.resize(locale_len as usize + 1, 0);
                }
                names.GetLocaleName(i, &mut locale)?;

                let string_len = names.GetStringLength(i)?;
                if string_len as usize + 1 > string.len() {
                    string.resize(string_len as usize + 1, 0);
                }
                names.GetString(i, &mut string)?;

                add(
                    ctx,
                    locale.as_mut_ptr(),
                    locale_len as i32,
                    string.as_mut_ptr(),
                    string_len as i32,
                );
            }
            Ok(())
        }
    }
}
