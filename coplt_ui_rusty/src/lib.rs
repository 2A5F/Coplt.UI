#![allow(dead_code)]
#![allow(unused)]

use std::fmt::Debug;

mod coplt_alloc {
    use core::alloc::GlobalAlloc;

    unsafe extern "C" {
        fn coplt_ui_malloc(size: usize, align: usize) -> *mut u8;
        fn coplt_ui_free(ptr: *mut u8, align: usize);
        fn coplt_ui_zalloc(size: usize, align: usize) -> *mut u8;
        fn coplt_ui_realloc(ptr: *mut u8, new_size: usize, align: usize) -> *mut u8;
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

mod col;
mod com;
mod layout;

mod com_impl {
    use std::ops::{Deref, DerefMut};

    use crate::{col::NArc, com::NativeArc};

    use super::*;

    unsafe impl<T> Send for com::NativeList<T> {}
    unsafe impl<T> Sync for com::NativeList<T> {}

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
}
