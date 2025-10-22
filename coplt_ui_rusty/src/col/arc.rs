use std::{
    alloc::Layout,
    hash::Hash,
    mem::ManuallyDrop,
    ops::{Deref, DerefMut},
    sync::atomic,
};

#[repr(C)]
pub struct NArc<T> {
    ptr: *mut Inner<T>,
}

#[repr(C)]
struct Inner<T> {
    count: u64,
    value: T,
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
                std::alloc::dealloc(self.ptr as *mut u8, Layout::new::<Inner<T>>());
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

impl<T> Deref for NArc<T> {
    type Target = T;

    fn deref(&self) -> &Self::Target {
        unsafe { &(*self.ptr).value }
    }
}

impl<T> DerefMut for NArc<T> {
    fn deref_mut(&mut self) -> &mut Self::Target {
        unsafe { &mut (*self.ptr).value }
    }
}

impl<T: PartialEq> PartialEq for NArc<T> {
    fn eq(&self, other: &Self) -> bool {
        (**self).eq(&**self)
    }
}

impl<T: Eq> Eq for NArc<T> {}

impl<T: std::fmt::Debug> std::fmt::Debug for NArc<T> {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        (&**self).fmt(f)
    }
}

impl<T: PartialOrd> PartialOrd for NArc<T> {
    fn partial_cmp(&self, other: &Self) -> Option<std::cmp::Ordering> {
        (**self).partial_cmp(&**self)
    }

    fn lt(&self, other: &Self) -> bool {
        (**self).lt(&**self)
    }

    fn le(&self, other: &Self) -> bool {
        (**self).le(&**self)
    }

    fn gt(&self, other: &Self) -> bool {
        (**self).gt(&**self)
    }

    fn ge(&self, other: &Self) -> bool {
        (**self).ge(&**self)
    }
}

impl<T: Ord> Ord for NArc<T> {
    fn cmp(&self, other: &Self) -> std::cmp::Ordering {
        (**self).cmp(&**self)
    }
}

impl<T: Hash> Hash for NArc<T> {
    fn hash<H: std::hash::Hasher>(&self, state: &mut H) {
        (**self).hash(state)
    }
}
