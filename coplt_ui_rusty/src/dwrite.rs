use std::{
    ffi::c_void,
    mem::MaybeUninit,
    panic::{RefUnwindSafe, UnwindSafe},
    path::Path,
    ptr::NonNull,
};

use crate::{com::*, feb_hr};
use cocom::{
    ComPtr, ComWeak, HResult, HResultE, MakeObject, MakeObjectWeak,
    impls::{ObjectBox, ObjectBoxNew},
    object::{Object, ObjectPtr},
    pmp,
};
use dashmap::DashMap;
use harfrust::FontRef;
use read_fonts::collections::int_set::Domain;
use windows::{
    Win32::{
        Foundation::{CloseHandle, GENERIC_READ, HANDLE},
        Graphics::DirectWrite::{
            IDWriteFactory7, IDWriteFontFace5, IDWriteFontFallback1, IDWriteFontFileStream,
            IDWriteLocalFontFileLoader, IDWriteLocalizedStrings,
        },
        Storage::FileSystem::{
            CreateFileW, FILE_ATTRIBUTE_NORMAL, FILE_FLAGS_AND_ATTRIBUTES, FILE_SHARE_READ,
            FILE_SHARE_WRITE, GetFileSizeEx, OPEN_EXISTING,
        },
        System::Memory::{
            CreateFileMappingW, FILE_MAP_READ, MEMORY_MAPPED_VIEW_ADDRESS, MapViewOfFile,
            PAGE_READONLY, UnmapViewOfFile,
        },
    },
    core::BOOL,
};
use windows_core::{Free, HSTRING, HStringBuilder, Interface, PCWSTR};

#[derive(Debug)]
pub struct Handle(HANDLE);

impl Drop for Handle {
    fn drop(&mut self) {
        unsafe {
            self.0.free();
        }
    }
}

#[cocom::object(IFontFace)]
pub struct FontFace {
    dw_face: IDWriteFontFace5,
    font_tables: DashMap<font_types::Tag, Option<TableHandle>>,
    frame_source: ComPtr<IFrameSource>,
    manager: ComWeak<IFontManager>,
    frame_time: FrameTime,
    info: NFontInfo,
    file: FontFile,
    font_ref: FontRef<'static>,
}

unsafe impl Send for FontFace {}
unsafe impl Sync for FontFace {}
impl UnwindSafe for FontFace {}
impl RefUnwindSafe for FontFace {}

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
    feb_hr(|| unsafe {
        let face = FontFace::new((*face).clone(), manager)?;
        *out = face.leak();
        Ok(HResultE::Ok.into())
    })
}

impl FontFace {
    pub fn new(
        face: IDWriteFontFace5,
        manager: *mut IFontManager,
    ) -> anyhow::Result<ObjectPtr<Self>> {
        unsafe {
            let file = FontFile::load(&face)?;
            let font_ref = file.get_font_ref(&face)?;
            Ok(Object::inplace(|this: *mut Self| {
                pmp!(this; .font_tables).write(DashMap::new());

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
                pmp!(this; .file).write(file);
                pmp!(this; .font_ref).write(font_ref);
            }))
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

#[derive(Debug)]
pub enum FontFile {
    Mmap {
        file: Handle,
        mmap: Handle,
        view: MEMORY_MAPPED_VIEW_ADDRESS,
        size: i64,
    },
    Stream {
        stream: IDWriteFontFileStream,
        size: u64,
        fragment_start: *mut c_void,
        fragment_context: *mut c_void,
    },
}

impl Drop for FontFile {
    fn drop(&mut self) {
        match self {
            FontFile::Mmap { view, .. } => unsafe {
                UnmapViewOfFile(*view);
            },
            FontFile::Stream {
                stream,
                fragment_context,
                ..
            } => unsafe {
                stream.ReleaseFileFragment(*fragment_context);
            },
        }
    }
}

impl FontFile {
    pub fn load(face: &IDWriteFontFace5) -> anyhow::Result<Self> {
        unsafe {
            let face_ref = face.GetFontFaceReference()?;
            let file = face_ref.GetFontFile()?;
            let mut font_file_reference_key = std::ptr::null_mut();
            let mut font_file_reference_key_size = 0;
            file.GetReferenceKey(
                &mut font_file_reference_key,
                &mut font_file_reference_key_size,
            )?;
            let loader = file.GetLoader()?;
            if let Ok(loader) = loader.cast::<IDWriteLocalFontFileLoader>() {
                let path_len = loader.GetFilePathLengthFromKey(
                    font_file_reference_key,
                    font_file_reference_key_size,
                )?;
                let mut path = vec![0; path_len as usize + 1];
                loader.GetFilePathFromKey(
                    font_file_reference_key,
                    font_file_reference_key_size,
                    &mut path,
                )?;
                FontFile::new_mmap(PCWSTR::from_raw(path.as_mut_ptr()))
            } else {
                let stream = loader
                    .CreateStreamFromKey(font_file_reference_key, font_file_reference_key_size)?;
                let file_size = stream.GetFileSize()?;

                let mut fragment_start = std::ptr::null_mut();
                let mut fragment_context = std::ptr::null_mut();
                stream.ReadFileFragment(
                    &mut fragment_start,
                    0,
                    file_size,
                    &mut fragment_context,
                )?;

                Ok(Self::Stream {
                    stream,
                    size: file_size,
                    fragment_start,
                    fragment_context,
                })
            }
        }
    }

    pub fn new_mmap(path: impl windows_core::Param<windows_core::PCWSTR>) -> anyhow::Result<Self> {
        unsafe {
            let file = Handle(CreateFileW(
                path,
                GENERIC_READ.0,
                FILE_SHARE_READ | FILE_SHARE_WRITE,
                None,
                OPEN_EXISTING,
                FILE_ATTRIBUTE_NORMAL,
                None,
            )?);
            let mut file_size = 0;
            GetFileSizeEx(file.0, &mut file_size)?;
            let mmap = Handle(CreateFileMappingW(file.0, None, PAGE_READONLY, 0, 0, None)?);
            let view = MapViewOfFile(mmap.0, FILE_MAP_READ, 0, 0, 0);
            Ok(Self::Mmap {
                file,
                mmap,
                view,
                size: file_size,
            })
        }
    }
}

impl FontFile {
    pub unsafe fn get_font_ref(&self, face: &IDWriteFontFace5) -> anyhow::Result<FontRef<'static>> {
        unsafe {
            let index = face.GetIndex();
            match self {
                FontFile::Mmap { view, size, .. } => Ok(FontRef::from_index(
                    unsafe { std::slice::from_raw_parts(view.Value as _, *size as usize) },
                    index,
                )?),
                FontFile::Stream {
                    size,
                    fragment_start,
                    ..
                } => Ok(FontRef::from_index(
                    unsafe { std::slice::from_raw_parts(*fragment_start as _, *size as usize) },
                    index,
                )?),
            }
        }
    }
}

#[derive(Debug)]
struct TableHandle {
    face: IDWriteFontFace5,
    table_context: *mut c_void,
    table_data: *mut c_void,
    table_size: u32,
}

unsafe impl Send for TableHandle {}
unsafe impl Sync for TableHandle {}
impl UnwindSafe for TableHandle {}
impl RefUnwindSafe for TableHandle {}

impl Drop for TableHandle {
    fn drop(&mut self) {
        unsafe {
            self.face.ReleaseFontTable(self.table_context);
        }
    }
}

impl<'a> read_fonts::TableProvider<'a> for FontFace {
    fn data_for_tag(&self, tag: font_types::Tag) -> Option<read_fonts::FontData<'a>> {
        fn make_font_data(table: &TableHandle) -> read_fonts::FontData<'static> {
            unsafe {
                read_fonts::FontData::new(std::slice::from_raw_parts(
                    table.table_data as *const u8,
                    table.table_size as usize,
                ))
            }
        }
        match self.font_tables.entry(tag) {
            dashmap::Entry::Occupied(entry) => entry.get().as_ref().map(make_font_data),
            dashmap::Entry::Vacant(entry) => entry
                .insert((|| {
                    let mut table_data = std::ptr::null_mut();
                    let mut table_size = 0;
                    let mut table_context = std::ptr::null_mut();
                    let mut exists = false.into();
                    unsafe {
                        self.dw_face
                            .TryGetFontTable(
                                tag.to_u32(),
                                &mut table_data,
                                &mut table_size,
                                &mut table_context,
                                &mut exists,
                            )
                            .ok()?
                    };
                    if !exists.as_bool() {
                        return None;
                    }
                    Some(TableHandle {
                        face: self.dw_face.clone(),
                        table_context,
                        table_data,
                        table_size,
                    })
                })())
                .as_ref()
                .map(make_font_data),
        }
    }
}

impl FontFace {
    pub fn font_ref<'a>(&'a self) -> &'a FontRef<'a> {
        &self.font_ref
    }
}

#[unsafe(no_mangle)]
pub extern "C" fn coplt_ui_dwrite_create_layout(
    factory: *const IDWriteFactory7,
    out: *mut *mut ILayout,
) -> HResult {
    feb_hr(|| unsafe {
        let layout = crate::layout::Layout::new(DwLayout::new((*factory).clone())?);
        *out = layout.leak();
        Ok(HResultE::Ok.into())
    })
}

#[derive(Debug)]
pub struct DwLayout {
    dw_factory: IDWriteFactory7,
    system_font_fallback: IDWriteFontFallback1,
}

impl DwLayout {
    pub fn new(dw_factory: IDWriteFactory7) -> anyhow::Result<Self> {
        unsafe {
            let font_fallback = dw_factory.GetSystemFontFallback()?;
            let system_font_fallback = font_fallback.cast()?;
            Ok(Self {
                dw_factory,
                system_font_fallback,
            })
        }
    }
}

impl crate::layout::Layout {
    pub fn new(dw: DwLayout) -> ObjectPtr<Self> {
        Self { inner: dw }.make_object()
    }
}

impl crate::layout::LayoutInner for DwLayout {}

impl DwLayout {
    
}
