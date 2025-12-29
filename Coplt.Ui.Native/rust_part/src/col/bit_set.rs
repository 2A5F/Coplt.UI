use crate::coplt_alloc::{coplt_alloc_array, coplt_free, coplt_zalloc_array};

#[repr(C)]
#[derive(Debug)]
pub struct NBitSet {
    items: *mut u64,
    size: i32,
}

impl NBitSet {
    pub fn new(size: i32) -> Self {
        assert!(size >= 0);

        let cap = (size + 63) / 64;
        if cap == 0 {
            Self {
                items: std::ptr::null_mut(),
                size,
            }
        } else {
            Self {
                items: unsafe { coplt_zalloc_array(cap as usize) },
                size,
            }
        }
    }

    pub fn re_ctor(&mut self, size: i32) {
        assert!(size >= 0);

        let old_cap = (self.size + 63) / 64;
        let cap = (size + 63) / 64;

        if old_cap == cap {
            if cap == 0 {
                return;
            }
            unsafe { std::slice::from_raw_parts_mut(self.items, cap as usize) }.fill(0);
        } else {
            unsafe { coplt_free(self.items) };
            self.items = unsafe { coplt_zalloc_array(cap as usize) };
            self.size = size;
        }
    }

    pub fn re_ctor_no_clear(&mut self, size: i32) {
        assert!(size >= 0);

        let old_cap = (self.size + 63) / 64;
        let cap = (size + 63) / 64;

        if old_cap == cap {
            return;
        } else {
            unsafe { coplt_free(self.items) };
            self.items = unsafe { coplt_alloc_array(cap as usize) };
            self.size = size;
        }
    }
}

impl Drop for NBitSet {
    fn drop(&mut self) {
        if self.items.is_null() {
            return;
        }
        unsafe { coplt_free(self.items) };
        self.items = std::ptr::null_mut();
        self.size = 0;
    }
}

impl NBitSet {
    pub fn len(&self) -> i32 {
        self.size
    }

    pub fn get(&self, index: i32) -> bool {
        if index as u32 > self.size as u32 {
            return false;
        }
        let (q, r) = (index / 64, index % 64);
        unsafe { (*self.items.add(q as usize)) & (1u64 << r) != 0 }
    }

    pub fn set(&self, index: i32, value: bool) {
        if index as u32 > self.size as u32 {
            return;
        }
        let (q, r) = (index / 64, index % 64);
        if value {
            unsafe { (*self.items.add(q as usize)) |= (1u64 << r) };
        } else {
            unsafe { (*self.items.add(q as usize)) &= !(1u64 << r) };
        }
    }
}
