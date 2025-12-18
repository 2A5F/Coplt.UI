use std::{
    ffi::c_void,
    mem::MaybeUninit,
    panic::{RefUnwindSafe, UnwindSafe},
    path::Path,
    ptr::NonNull,
};

use crate::{
    c_option,
    col::NList,
    com::*,
    feb_hr,
    font_manager::FontManager,
    layout::{FontRange, SubDocInner},
};
use cocom::{
    ComPtr, ComWeak, HResult, HResultE, MakeObject, MakeObjectWeak,
    impls::{ObjectBox, ObjectBoxNew},
    object::{Object, ObjectPtr},
    pmp,
};
use dashmap::DashMap;
use harfrust::FontRef;
use read_fonts::collections::int_set::Domain;
use windows::Win32::{
    Foundation::{GENERIC_READ, HANDLE},
    Graphics::DirectWrite::{
        DWRITE_FONT_AXIS_TAG_ITALIC, DWRITE_FONT_AXIS_TAG_SLANT, DWRITE_FONT_AXIS_TAG_WEIGHT,
        DWRITE_FONT_AXIS_TAG_WIDTH, DWRITE_FONT_AXIS_VALUE, DWRITE_READING_DIRECTION,
        DWRITE_READING_DIRECTION_BOTTOM_TO_TOP, DWRITE_READING_DIRECTION_LEFT_TO_RIGHT,
        DWRITE_READING_DIRECTION_RIGHT_TO_LEFT, DWRITE_READING_DIRECTION_TOP_TO_BOTTOM,
        IDWriteFactory7, IDWriteFontFace5, IDWriteFontFallback1, IDWriteFontFileStream,
        IDWriteLocalFontFileLoader, IDWriteLocalizedStrings, IDWriteTextAnalysisSource,
        IDWriteTextAnalysisSource_Impl,
    },
    Storage::FileSystem::{
        CreateFileW, FILE_ATTRIBUTE_NORMAL, FILE_SHARE_READ, FILE_SHARE_WRITE, GetFileSizeEx,
        OPEN_EXISTING,
    },
    System::Memory::{
        CreateFileMappingW, FILE_MAP_READ, MEMORY_MAPPED_VIEW_ADDRESS, MapViewOfFile,
        PAGE_READONLY, UnmapViewOfFile,
    },
};
use windows_core::{Free, HSTRING, HStringBuilder, Interface, PCWSTR, implement};

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

    pub fn get(
        face: IDWriteFontFace5,
        manager: *mut IFontManager,
    ) -> anyhow::Result<ComPtr<IFontFace>> {
        unsafe {
            let ptr_manager = manager;
            let manager = Object::<FontManager>::GetObject(manager);
            let id: usize = std::mem::transmute_copy(&face);
            (*manager).get_or_add(id as u64, || Ok(FontFace::new(face, ptr_manager)?.to_com()))
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

#[implement(IDWriteTextAnalysisSource)]
struct OneSpaceTextAnalysisSource;

impl IDWriteTextAnalysisSource_Impl for OneSpaceTextAnalysisSource_Impl {
    fn GetTextAtPosition(
        &self,
        textposition: u32,
        textstring: *mut *mut u16,
        textlength: *mut u32,
    ) -> windows_core::Result<()> {
        unsafe {
            if textposition == 0 {
                static DATA: [u16; 2] = [0x20, 0];
                *textstring = (&DATA).as_ptr() as _;
                *textlength = 1;
            } else {
                *textstring = std::ptr::null_mut();
                *textlength = 0;
            }
        }
        Ok(())
    }

    fn GetTextBeforePosition(
        &self,
        textposition: u32,
        textstring: *mut *mut u16,
        textlength: *mut u32,
    ) -> windows_core::Result<()> {
        unsafe {
            if textposition == 1 {
                static DATA: [u16; 2] = [0x20, 0];
                *textstring = (&DATA).as_ptr() as _;
                *textlength = 1;
            } else {
                *textstring = std::ptr::null_mut();
                *textlength = 0;
            }
        }
        Ok(())
    }

    fn GetParagraphReadingDirection(
        &self,
    ) -> windows::Win32::Graphics::DirectWrite::DWRITE_READING_DIRECTION {
        windows::Win32::Graphics::DirectWrite::DWRITE_READING_DIRECTION_LEFT_TO_RIGHT
    }

    fn GetLocaleName(
        &self,
        textposition: u32,
        textlength: *mut u32,
        localename: *mut *mut u16,
    ) -> windows_core::Result<()> {
        unsafe {
            *textlength = 0;
            *localename = std::ptr::null_mut();
        }
        Ok(())
    }

    fn GetNumberSubstitution(
        &self,
        textposition: u32,
        textlength: *mut u32,
        numbersubstitution: windows_core::OutRef<
            windows::Win32::Graphics::DirectWrite::IDWriteNumberSubstitution,
        >,
    ) -> windows_core::Result<()> {
        unsafe {
            *textlength = 0;
        }
        Ok(())
    }
}

#[derive(Debug)]
pub struct DwLayout {
    pub dw_factory: IDWriteFactory7,
    pub system_font_fallback: IDWriteFontFallback1,
    pub undef_font: Option<IDWriteFontFace5>,
}

impl DwLayout {
    pub fn new(dw_factory: IDWriteFactory7) -> anyhow::Result<Self> {
        unsafe {
            let font_fallback = dw_factory.GetSystemFontFallback()?;
            let system_font_fallback = font_fallback.cast()?;
            Ok(Self {
                dw_factory,
                system_font_fallback,
                undef_font: None,
            })
        }
    }

    pub fn get_undef_font(&mut self, doc: &SubDocInner) -> anyhow::Result<&IDWriteFontFace5> {
        if self.undef_font.is_none() {
            let fm = unsafe { &(*doc.ctx().font_manager) };
            let ostas: IDWriteTextAnalysisSource = OneSpaceTextAnalysisSource.into();
            let mut mapped_len = 0;
            let mut scale = 0.0;
            let mut mapped_font = None;
            unsafe {
                self.system_font_fallback.MapCharacters(
                    &ostas,
                    0,
                    1,
                    None,
                    None,
                    &[],
                    &mut mapped_len,
                    &mut scale,
                    &mut mapped_font,
                )?;
            }

            self.undef_font = mapped_font;
        }
        Ok((self.undef_font.as_ref().unwrap()))
    }
}

impl crate::layout::Layout {
    pub fn new(dw: DwLayout) -> ObjectPtr<Self> {
        Self { inner: dw }.make_object()
    }
}

unsafe extern "C" {
    fn coplt_ui_dwrite_get_font_fallback(
        obj: NonNull<IFontFallback>,
        out: *mut IDWriteFontFallback1,
    );
}

fn get_dwrite_ffb(obj: NonNull<IFontFallback>) -> IDWriteFontFallback1 {
    unsafe {
        let mut dfb = MaybeUninit::uninit();
        coplt_ui_dwrite_get_font_fallback(obj, dfb.as_mut_ptr());
        dfb.assume_init()
    }
}

impl crate::layout::LayoutInner for DwLayout {
    fn analyze_fonts(
        &mut self,
        doc: &mut SubDocInner,
        id: NodeId,
        paragraph: &mut TextParagraphData,
        root_style: &StyleData,
        style: &TextStyleData,
    ) -> anyhow::Result<()> {
        let text = &*{ paragraph.m_text };

        let same_style_ranges: &[_] = &*paragraph.same_style_ranges();
        let font_ranges = paragraph.font_ranges();
        font_ranges.clear();

        if text.is_empty() {
            return Ok(());
        }

        let fm = doc.ctx().font_manager;

        let text_direction = style.TextDirection().unwrap_or(root_style.TextDirection);
        let writing_direction = style
            .WritingDirection()
            .unwrap_or(root_style.WritingDirection);

        let dir = match (text_direction, writing_direction) {
            (TextDirection::Forward, WritingDirection::Horizontal) => {
                DWRITE_READING_DIRECTION_LEFT_TO_RIGHT
            }
            (TextDirection::Forward, WritingDirection::Vertical) => {
                DWRITE_READING_DIRECTION_TOP_TO_BOTTOM
            }
            (TextDirection::Reverse, WritingDirection::Horizontal) => {
                DWRITE_READING_DIRECTION_RIGHT_TO_LEFT
            }
            (TextDirection::Reverse, WritingDirection::Vertical) => {
                DWRITE_READING_DIRECTION_BOTTOM_TO_TOP
            }
        };

        let tas: IDWriteTextAnalysisSource = TextAnalysisSource {
            text,
            locale_ranges: &paragraph.locale_ranges(),
            dir,
        }
        .into();

        for ssr in same_style_ranges {
            let span_style = c_option!(#val; ssr => FirstSpan)
                .map(|span| &*span.text_style_data(doc))
                .unwrap_or(style);

            let font_fallback = span_style.FontFallback().unwrap_or(root_style.FontFallback);
            let font_fallback = NonNull::new(font_fallback)
                .map(get_dwrite_ffb)
                .unwrap_or_else(|| self.system_font_fallback.clone());

            let font_weight = span_style.FontWeight().unwrap_or(root_style.FontWeight);
            let font_width = span_style.FontWidth().unwrap_or(root_style.FontWidth);
            let font_italic = span_style.FontItalic().unwrap_or(root_style.FontItalic);
            let font_oblique = span_style.FontOblique().unwrap_or(root_style.FontOblique);

            let axis_values: [DWRITE_FONT_AXIS_VALUE; _] = [
                DWRITE_FONT_AXIS_VALUE {
                    axisTag: DWRITE_FONT_AXIS_TAG_WEIGHT,
                    value: font_weight as i32 as f32,
                },
                DWRITE_FONT_AXIS_VALUE {
                    axisTag: DWRITE_FONT_AXIS_TAG_WIDTH,
                    value: font_width.Width * 100.0,
                },
                DWRITE_FONT_AXIS_VALUE {
                    axisTag: DWRITE_FONT_AXIS_TAG_ITALIC,
                    value: if font_italic { 1.0 } else { 0.0 },
                },
                DWRITE_FONT_AXIS_VALUE {
                    axisTag: DWRITE_FONT_AXIS_TAG_SLANT,
                    value: if font_italic { font_oblique } else { 0.0 },
                },
            ];

            unsafe {
                let mut start = ssr.Start;
                let mut end = ssr.End;
                while start < end {
                    let mut scale = 0.0;
                    let mut mapped_length = 0;
                    let mut mapped_fontface = None;
                    font_fallback.MapCharacters(
                        &tas,
                        start,
                        end - start,
                        None,
                        None,
                        &axis_values,
                        &mut mapped_length,
                        &mut scale,
                        &mut mapped_fontface,
                    )?;
                    if mapped_length == 0 {
                        break;
                    }

                    let font_face = mapped_fontface
                        .map_or_else(|| self.get_undef_font(doc).cloned(), Ok)
                        .and_then(|face| FontFace::get(face, fm))?;

                    font_ranges.push(FontRange {
                        start,
                        end: start + mapped_length,
                        font_face,
                    });

                    start += mapped_length;
                }
            }
        }

        Ok(())
    }
}

#[implement(IDWriteTextAnalysisSource)]
struct TextAnalysisSource<'a> {
    text: &'a [u16],
    locale_ranges: &'a [TextData_LocaleRange],
    dir: DWRITE_READING_DIRECTION,
}

impl<'a> IDWriteTextAnalysisSource_Impl for TextAnalysisSource_Impl<'a> {
    fn GetTextAtPosition(
        &self,
        textposition: u32,
        textstring: *mut *mut u16,
        textlength: *mut u32,
    ) -> windows_core::Result<()> {
        unsafe {
            if textposition >= self.text.len() as u32 {
                *textstring = std::ptr::null_mut();
                *textlength = 0;
            } else {
                *textstring = (self.text.as_ptr() as *mut u16).add(textposition as usize);
                *textlength = self.text.len() as u32 - textposition;
            }
        }
        Ok(())
    }

    fn GetTextBeforePosition(
        &self,
        textposition: u32,
        textstring: *mut *mut u16,
        textlength: *mut u32,
    ) -> windows_core::Result<()> {
        unsafe {
            if textposition >= self.text.len() as u32 {
                *textstring = std::ptr::null_mut();
                *textlength = 0;
            } else {
                *textstring = self.text.as_ptr() as *mut u16;
                *textlength = textposition;
            }
        }
        Ok(())
    }

    fn GetParagraphReadingDirection(&self) -> DWRITE_READING_DIRECTION {
        self.dir
    }

    fn GetLocaleName(
        &self,
        textposition: u32,
        textlength: *mut u32,
        localename: *mut *mut u16,
    ) -> windows_core::Result<()> {
        if let Ok(pos) = self
            .locale_ranges
            .binary_search_by(TextData_LocaleRange::search_pos(textposition))
        {
            let range = &self.locale_ranges[pos];
            unsafe {
                *textlength = range.End - textposition;
                *localename = range.Locale.Name;
            }
        } else {
            unsafe {
                *textlength = 0;
                *localename = std::ptr::null_mut();
            }
        }
        Ok(())
    }

    fn GetNumberSubstitution(
        &self,
        textposition: u32,
        textlength: *mut u32,
        numbersubstitution: windows_core::OutRef<
            windows::Win32::Graphics::DirectWrite::IDWriteNumberSubstitution,
        >,
    ) -> windows_core::Result<()> {
        unsafe {
            *textlength = 0;
        }
        Ok(())
    }
}
