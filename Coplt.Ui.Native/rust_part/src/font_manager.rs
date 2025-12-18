#![allow(unused)]
#![allow(non_snake_case)]
#![allow(non_camel_case_types)]

use std::{
    collections::HashMap,
    mem::MaybeUninit,
    os::raw::c_void,
    ptr::NonNull,
    sync::{
        RwLock,
        atomic::{AtomicU64, Ordering},
    },
};

use dashmap::DashMap;

use crate::dwrite::FontFace;

use super::com::*;
use cocom::{
    ComPtr, MakeObject, MakeObjectWeak,
    impls::ObjectBoxNew,
    object::{Object, ObjectPtr},
};

#[unsafe(no_mangle)]
pub extern "C" fn coplt_ui_new_font_manager(
    frame_source: /* move */ *mut IFrameSource,
    output: *mut *mut IFontManager,
) {
    let obj = FontManager::new(unsafe { ComPtr::new(NonNull::new_unchecked(frame_source)) })
        .make_com_weak();
    unsafe { *output = obj.leak() };
}

#[derive(Debug)]
struct AssocUpdate {
    data: *mut c_void,
    on_drop: unsafe extern "C" fn(*mut c_void) -> (),
    on_add: unsafe extern "C" fn(*mut c_void, *mut IFontFace, u64) -> (),
    on_expired: unsafe extern "C" fn(*mut c_void, *mut IFontFace, u64) -> (),
}

unsafe impl Send for AssocUpdate {}
unsafe impl Sync for AssocUpdate {}

impl AssocUpdate {
    fn on_drop(&self) {
        unsafe {
            if 0 != std::mem::transmute::<_, usize>(self.on_drop) {
                (self.on_drop)(self.data);
            }
        }
    }

    fn on_add(&self, face: *mut IFontFace, id: u64) {
        unsafe {
            if 0 != std::mem::transmute::<_, usize>(self.on_add) {
                (self.on_add)(self.data, face, id);
            }
        }
    }

    fn on_expired(&self, face: *mut IFontFace, id: u64) {
        unsafe {
            if 0 != std::mem::transmute::<_, usize>(self.on_expired) {
                (self.on_expired)(self.data, face, id);
            }
        }
    }
}

impl Drop for AssocUpdate {
    fn drop(&mut self) {
        self.on_drop();
    }
}

#[cocom::object(IFontManager)]
#[derive(Debug)]
pub struct FontManager {
    frame_source: ComPtr<IFrameSource>,

    expire_frame: u64,
    expre_time: u64,

    op_lock: RwLock<()>,

    assoc_updates: HashMap<u64, AssocUpdate>,
    assoc_id_inc: u64,

    id_to_faces: DashMap<u64, ComPtr<IFontFace>>,
}

impl FontManager {
    pub fn new(frame_source: ComPtr<IFrameSource>) -> Self {
        Self {
            frame_source,

            expire_frame: 180,
            expre_time: 30000000,

            assoc_updates: HashMap::new(),
            assoc_id_inc: 0,

            id_to_faces: DashMap::new(),

            op_lock: RwLock::new(()),
        }
    }
}

unsafe impl Send for FontManager {}
unsafe impl Sync for FontManager {}

impl impls::IWeak for FontManager {}

impl impls::IFontManager for FontManager {
    fn SetAssocUpdate(
        &mut self,
        Data: *mut core::ffi::c_void,
        OnDrop: unsafe extern "C" fn(*mut core::ffi::c_void) -> (),
        OnAdd: unsafe extern "C" fn(*mut core::ffi::c_void, *mut IFontFace, u64) -> (),
        OnExpired: unsafe extern "C" fn(*mut core::ffi::c_void, *mut IFontFace, u64) -> (),
    ) -> u64 {
        let lock_guard = self.op_lock.write().unwrap();
        let id = self.assoc_id_inc;
        self.assoc_id_inc += 1;
        self.assoc_updates.insert(
            id,
            AssocUpdate {
                data: Data,
                on_drop: OnDrop,
                on_add: OnAdd,
                on_expired: OnExpired,
            },
        );
        drop(lock_guard);
        id
    }

    fn RemoveAssocUpdate(&mut self, AssocUpdateId: u64) -> () {
        let lock_guard = self.op_lock.write().unwrap();
        self.assoc_updates.remove(&AssocUpdateId);
        drop(lock_guard);
    }

    fn GetFrameSource(&mut self) -> *mut IFrameSource {
        self.frame_source.ptr().as_ptr()
    }

    fn SetExpireFrame(&mut self, FrameCount: u64) -> () {
        let lock_guard = self.op_lock.write().unwrap();
        self.expire_frame = FrameCount;
        drop(lock_guard);
    }

    fn SetExpireTime(&mut self, TimeTicks: u64) -> () {
        let lock_guard = self.op_lock.write().unwrap();
        self.expre_time = TimeTicks;
        drop(lock_guard);
    }

    fn Collect(&mut self) -> () {
        let lock_guard = self.op_lock.read().unwrap();
        let mut sft: MaybeUninit<FrameTime> = MaybeUninit::uninit();
        unsafe { self.frame_source.Get(sft.as_mut_ptr()) };
        let sft = unsafe { sft.assume_init() };
        self.id_to_faces.retain(|id, face| {
            if face.get_RefCount() != 1 {
                return false;
            }
            let fft = unsafe { *face.get_FrameTime() };
            if sft.NthFrame - fft.NthFrame < self.expire_frame {
                return false;
            }
            if sft.TimeTicks - fft.TimeTicks < self.expre_time {
                return false;
            }
            for au in self.assoc_updates.values() {
                au.on_expired(face.ptr().as_ptr(), *id);
            }
            true
        });
        drop(lock_guard);
    }

    fn Add(&mut self, Face: *mut IFontFace) -> () {
        let lock_guard = self.op_lock.read().unwrap();
        let face = unsafe {
            (*Face).AddRef();
            ComPtr::new(NonNull::new_unchecked(Face))
        };
        let id = face.get_Id();
        match self.id_to_faces.entry(id) {
            dashmap::Entry::Occupied(_) => {}
            dashmap::Entry::Vacant(entry) => {
                let r = entry.insert(face.clone()).clone();
                for au in self.assoc_updates.values() {
                    au.on_add(r.ptr().as_ptr(), id);
                }
            }
        };
        drop(lock_guard);
    }

    fn GetOrAdd(
        &mut self,
        Id: u64,
        Data: *mut core::ffi::c_void,
        OnAdd: unsafe extern "C" fn(*mut core::ffi::c_void, u64) -> *mut crate::com::IFontFace,
    ) -> *mut crate::com::IFontFace {
        let lock_guard = self.op_lock.read().unwrap();
        let r = match self.id_to_faces.entry(Id) {
            dashmap::Entry::Occupied(entry) => entry.get().clone(),
            dashmap::Entry::Vacant(entry) => {
                let r = entry
                    .insert(unsafe { ComPtr::new(NonNull::new_unchecked(OnAdd(Data, Id))) })
                    .clone();
                for au in self.assoc_updates.values() {
                    au.on_add(r.ptr().as_ptr(), Id);
                }
                r
            }
        };
        drop(lock_guard);
        r.leak()
    }

    fn Get(&mut self, Id: u64) -> *mut IFontFace {
        let lock_guard = self.op_lock.read().unwrap();
        let r = match self.id_to_faces.get(&Id) {
            Some(face) => {
                let face = &*face;
                face.AddRef();
                face.ptr().as_ptr()
            }
            None => std::ptr::null_mut(),
        };
        drop(lock_guard);
        r
    }
}

impl FontManager {
    pub fn get_or_add(
        &mut self,
        id: u64,
        on_add: impl FnOnce() -> anyhow::Result<ComPtr<IFontFace>>,
    ) -> anyhow::Result<ComPtr<IFontFace>> {
        let lock_guard = self.op_lock.read().unwrap();
        let r = match self.id_to_faces.entry(id) {
            dashmap::Entry::Occupied(entry) => entry.get().clone(),
            dashmap::Entry::Vacant(entry) => {
                let r = entry.insert(on_add()?).clone();
                for au in self.assoc_updates.values() {
                    au.on_add(r.ptr().as_ptr(), id);
                }
                r
            }
        };
        drop(lock_guard);
        Ok(r)
    }
}
