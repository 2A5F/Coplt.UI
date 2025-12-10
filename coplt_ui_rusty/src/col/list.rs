use std::{
    i32,
    ops::{Deref, DerefMut},
    ptr::drop_in_place,
    vec::IntoIter,
};

use crate::coplt_alloc::{
    coplt_alloc_array, coplt_free, coplt_realloc_array, coplt_ui_free, coplt_ui_malloc,
    coplt_ui_realloc, coplt_zalloc_array,
};

#[repr(C)]
pub struct NList<T> {
    items: *mut T,
    cap: i32,
    size: i32,
}

impl<T> NList<T> {
    pub fn new() -> Self {
        Self {
            items: std::ptr::null_mut(),
            cap: 0,
            size: 0,
        }
    }

    pub fn with_capacity(cap: i32) -> Self {
        assert!(cap >= 0);
        let items = if cap == 0 {
            std::ptr::null_mut()
        } else {
            unsafe { coplt_alloc_array::<T>(cap as usize) }
        };
        Self {
            items,
            cap,
            size: 0,
        }
    }

    pub unsafe fn new_zeroed(len: i32) -> Self {
        assert!(len >= 0);
        Self {
            items: unsafe { coplt_zalloc_array::<T>(len as usize) },
            cap: len,
            size: len,
        }
    }
}

impl<T: Clone> NList<T> {
    pub fn new_with(len: i32, val: T) -> Self {
        assert!(len >= 0);
        let items = unsafe { coplt_alloc_array::<T>(len as usize) };

        let mut i = 0;
        if len > 0 {
            loop {
                unsafe {
                    let ptr = items.add(i as usize);

                    let next_i = i + 1;
                    if next_i < len {
                        ptr.write(val.clone());
                        i = next_i;
                    } else {
                        ptr.write(val);
                        break;
                    }
                }
            }
        }

        Self {
            items,
            cap: len,
            size: len,
        }
    }
}

impl<T> NList<T> {
    pub fn len(&self) -> i32 {
        self.size
    }

    pub fn cap(&self) -> i32 {
        self.cap
    }

    pub fn at(&self, i: i32) -> *mut T {
        if i as u32 > self.size as u32 {
            std::ptr::null_mut()
        } else {
            unsafe { self.items.add(i as usize) }
        }
    }
}

impl<T> Deref for NList<T> {
    type Target = [T];

    fn deref(&self) -> &Self::Target {
        unsafe { std::slice::from_raw_parts(self.items, self.size as usize) }
    }
}

impl<T> DerefMut for NList<T> {
    fn deref_mut(&mut self) -> &mut Self::Target {
        unsafe { std::slice::from_raw_parts_mut(self.items, self.size as usize) }
    }
}

impl<T> NList<T> {
    pub const DEFAULT_CAPACITY: i32 = 4;

    pub fn set_cap(&mut self, cap: i32) {
        assert!(cap >= self.size);

        if (self.items.is_null()) {
            self.items = if cap == 0 {
                std::ptr::null_mut()
            } else {
                unsafe { coplt_alloc_array(cap as usize) }
            };
        } else if cap != self.cap {
            if cap == 0 {
                unsafe { coplt_ui_free(self.items as _, align_of::<T>()) };
                self.items = std::ptr::null_mut();
            } else {
                self.items = unsafe { coplt_realloc_array(self.items, cap as usize) };
            }
        }
        self.cap = cap;
    }
}

impl<T> NList<T> {
    #[inline]
    fn get_new_cap(&mut self, cap: i32) -> i32 {
        debug_assert!(self.items.is_null() || self.cap < cap);

        let mut new_cap = if self.items.is_null() || self.cap == 0 {
            Self::DEFAULT_CAPACITY
        } else {
            2 * self.cap
        };

        if new_cap as u32 > i32::MAX as u32 {
            new_cap = i32::MAX;
        }

        if new_cap < cap {
            new_cap = cap;
        }

        new_cap
    }

    fn grow(&mut self, cap: i32) {
        let new_cap = self.get_new_cap(cap);
        self.set_cap(new_cap);
    }

    fn grow_for_insertion(&mut self, index_to_insert: i32, insertion_count: i32) {
        debug_assert!(index_to_insert > 0 && index_to_insert <= self.size && insertion_count > 0);

        let required_capacity = self.size + insertion_count;
        let new_capacity = self.get_new_cap(required_capacity);

        if self.items.is_null() {
            self.items = unsafe { coplt_alloc_array(new_capacity as usize) };
        } else if index_to_insert == self.size {
            self.items = unsafe { coplt_realloc_array(self.items, new_capacity as usize) };
        } else {
            let new_items: *mut T = unsafe { coplt_alloc_array(new_capacity as usize) };

            if index_to_insert != 0 {
                unsafe {
                    std::ptr::copy_nonoverlapping(self.items, new_items, index_to_insert as usize)
                };
            }

            unsafe {
                std::ptr::copy_nonoverlapping(
                    self.items.add(index_to_insert as usize),
                    new_items.add(index_to_insert as usize + 1),
                    (self.size - index_to_insert) as usize,
                )
            };

            let old_items = std::mem::replace(&mut self.items, new_items);
            unsafe { coplt_ui_free(old_items as _, align_of::<T>()) };
        }
        self.cap = new_capacity;
    }

    #[inline(never)]
    unsafe fn unsafe_add_with_resize(&mut self) -> *mut T {
        let size = self.size;
        self.grow(size + 1);
        self.size = size + 1;
        unsafe { self.items.add(size as usize) }
    }

    pub unsafe fn unsafe_add(&mut self) -> *mut T {
        if self.items.is_null() || self.size >= self.cap {
            unsafe { self.unsafe_add_with_resize() }
        } else {
            let size = self.size;
            self.size += 1;
            unsafe { self.items.add(size as usize) }
        }
    }
}

impl<T> NList<T> {
    pub fn add(&mut self, val: T) {
        unsafe {
            self.unsafe_add().write(val);
        }
    }

    pub fn add_range(&mut self, iter: impl IntoIterator<Item = T>) {
        let mut iter = iter.into_iter();
        match iter.size_hint() {
            (_, Some(len)) => {
                if len == 0 {
                    return;
                }

                let index = self.size;

                let new_count = (len + self.size as usize) as i32;
                if new_count > self.cap {
                    self.grow(new_count);
                }
                self.size = new_count;

                let target = unsafe { self.items.add(index as usize) };
                for (i, item) in iter.enumerate() {
                    unsafe { target.add(i).write(item) };
                }
                return;
            }
            (len, None) => {
                let new_count = (len + self.size as usize) as i32;
                if new_count > self.cap {
                    self.grow(new_count);
                }
            }
            _ => {}
        }
        for item in iter {
            self.add(item);
        }
    }
}

impl<T: Copy> NList<T> {
    pub fn add_range_copy(&mut self, slice: &[T]) {
        if slice.is_empty() {
            return;
        }

        let index = self.size;

        let new_count = (slice.len() + self.size as usize) as i32;
        if new_count > self.cap {
            self.grow(new_count);
        }
        self.size = new_count;

        (&mut self[index as usize..]).copy_from_slice(slice);
    }
}

impl<T> NList<T> {
    pub fn insert(&mut self, index: i32, item: T) -> Result<(), T> {
        if index as u32 <= self.size as u32 {
            if index == self.size {
                self.add(item);
                return Ok(());
            }
            if self.items.is_null() || self.size == self.cap {
                self.grow_for_insertion(index, 1);
            } else if index < self.size {
                unsafe {
                    std::ptr::copy(
                        self.items.add(index as usize),
                        self.items.add(index as usize + 1),
                        (self.size - index) as usize,
                    );
                }
            }
            unsafe { self.items.add(index as usize).write(item) };
            self.size += 1;
            Ok(())
        } else {
            Err(item)
        }
    }
}

impl<T> NList<T> {
    pub fn remove_at(&mut self, index: i32) -> Option<T> {
        if (index as u32) < (self.size as u32) {
            self.size -= 1;
            let val = unsafe { self.items.add(index as usize).read() };
            if index < self.size {
                unsafe {
                    std::ptr::copy(
                        self.items.add(index as usize + 1),
                        self.items.add(index as usize),
                        (self.size - index) as usize,
                    );
                }
            }
            Some(val)
        } else {
            None
        }
    }
}

impl<T> NList<T> {
    pub fn push(&mut self, val: T) {
        self.add(val);
    }

    pub fn pop(&mut self) -> Option<T> {
        if self.size == 0 {
            return None;
        } else {
            self.size -= 1;
            Some(unsafe { self.items.add(self.size as usize).read() })
        }
    }
}

impl<T> NList<T> {
    pub fn clear(&mut self) {
        if self.items.is_null() || self.size <= 0 {
            return;
        }
        if std::mem::needs_drop::<T>() {
            for i in 0..self.size {
                unsafe {
                    drop_in_place(self.items.add(i as usize));
                }
            }
        }
        self.size = 0;
    }
}

impl<T> Drop for NList<T> {
    fn drop(&mut self) {
        if self.items.is_null() {
            return;
        }
        if std::mem::needs_drop::<T>() {
            self.clear();
        }
        unsafe {
            coplt_ui_free(self.items as _, align_of::<T>());
        }
    }
}
