use std::{
    alloc::Layout,
    hash::Hash,
    mem::ManuallyDrop,
    ops::{Deref, DerefMut},
    sync::atomic,
};

use crate::coplt_alloc::coplt_free;

#[repr(C)]
pub struct NArc<T> {
    ptr: *mut Inner<T>,
}

#[repr(C)]
struct Inner<T> {
    count: u64,
    value: T,
}

impl<T> NArc<T> {
    pub fn val(&self) -> Option<&T> {
        if self.ptr.is_null() {
            None
        } else {
            Some(unsafe { &(*self.ptr).value })
        }
    }
    pub fn val_mut(&mut self) -> Option<&mut T> {
        if self.ptr.is_null() {
            None
        } else {
            Some(unsafe { &mut (*self.ptr).value })
        }
    }
}

impl<T> Drop for NArc<T> {
    fn drop(&mut self) {
        unsafe {
            if self.ptr.is_null() {
                return;
            }
            let count = &mut *(((&mut (*self.ptr).count) as *mut _) as *mut atomic::AtomicU64);
            if count.fetch_sub(1, atomic::Ordering::AcqRel) == 1 {
                if std::mem::needs_drop::<T>() {
                    let data = &mut *(((&mut (*self.ptr).value) as *mut _)
                        as *mut std::mem::ManuallyDrop<T>);
                    ManuallyDrop::drop(data);
                }
                coplt_free(self.ptr);
            }
        }
    }
}

impl<T> Clone for NArc<T> {
    fn clone(&self) -> Self {
        let count =
            unsafe { &mut *(((&mut (*self.ptr).count) as *mut _) as *mut atomic::AtomicU64) };
        count.fetch_add(1, atomic::Ordering::Relaxed);
        Self {
            ptr: self.ptr.clone(),
        }
    }
}

impl<T: PartialEq> PartialEq for NArc<T> {
    fn eq(&self, other: &Self) -> bool {
        self.val().eq(&other.val())
    }
}

impl<T: Eq> Eq for NArc<T> {}

impl<T: std::fmt::Debug> std::fmt::Debug for NArc<T> {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        self.val().fmt(f)
    }
}

impl<T: PartialOrd> PartialOrd for NArc<T> {
    fn partial_cmp(&self, other: &Self) -> Option<std::cmp::Ordering> {
        self.val().partial_cmp(&other.val())
    }

    fn lt(&self, other: &Self) -> bool {
        self.val().lt(&other.val())
    }

    fn le(&self, other: &Self) -> bool {
        self.val().le(&other.val())
    }

    fn gt(&self, other: &Self) -> bool {
        self.val().gt(&other.val())
    }

    fn ge(&self, other: &Self) -> bool {
        self.val().ge(&other.val())
    }
}

impl<T: Ord> Ord for NArc<T> {
    fn cmp(&self, other: &Self) -> std::cmp::Ordering {
        self.val().cmp(&other.val())
    }
}

impl<T: Hash> Hash for NArc<T> {
    fn hash<H: std::hash::Hasher>(&self, state: &mut H) {
        self.val().hash(state);
    }
}
