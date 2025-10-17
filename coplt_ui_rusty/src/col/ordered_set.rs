use std::{
    alloc::Layout,
    mem::ManuallyDrop,
};

use crate::col::{Enumerator, EnumeratorIter, GetHashCode};

use super::hash_helpers::*;

const START_OF_FREE_LIST: i32 = -3;

#[repr(C)]
#[derive(Debug)]
pub struct OrderedSet<T> {
    pub buckets: *mut i32,
    pub nodes: *mut OrderedSetNode<T>,
    pub fast_mode_multiplier: u64,
    pub cap: i32,
    pub first: i32,
    pub last: i32,
    pub count: i32,
    pub free_list: i32,
    pub free_count: i32,
}

#[repr(C)]
#[derive(Debug, Clone, Copy)]
pub struct OrderedSetNode<T> {
    pub hash_code: i32,
    pub next: i32,
    pub order_next: i32,
    pub order_prev: i32,
    pub value: T,
}

impl<T> Drop for OrderedSet<T> {
    fn drop(&mut self) {
        unsafe {
            self.drop_items();
            if self.buckets.is_null() {
                std::alloc::dealloc(self.buckets as *mut u8, std::alloc::Layout::new::<i32>());
                self.buckets = std::ptr::null_mut();
            }
            if self.nodes.is_null() {
                std::alloc::dealloc(
                    self.nodes as *mut u8,
                    std::alloc::Layout::new::<OrderedSetNode<T>>(),
                );
                self.nodes = std::ptr::null_mut();
            }
        }
    }
}

impl<T> OrderedSet<T> {
    pub fn leak(&mut self) {
        self.buckets = std::ptr::null_mut();
        self.nodes = std::ptr::null_mut();
    }
}

impl<T> OrderedSet<T> {
    unsafe fn drop_items(&mut self) {
        unsafe {
            if std::mem::needs_drop::<T>() {
                for item in self.iter_mut() {
                    let item = item as *mut _ as *mut ManuallyDrop<T>;
                    item.drop_in_place();
                }
            }
        }
    }
}

impl<T: GetHashCode + PartialEq> OrderedSet<T> {
    fn initialize(&mut self, capacity: i32) -> i32 {
        let size = get_prime(capacity);

        self.first = -1;
        self.last = -1;
        self.free_list = -1;
        self.buckets = unsafe {
            std::alloc::alloc_zeroed(Layout::from_size_align_unchecked(
                size as usize * size_of::<i32>(),
                align_of::<i32>(),
            )) as *mut _
        };
        self.nodes = unsafe {
            std::alloc::alloc_zeroed(Layout::from_size_align_unchecked(
                size as usize * size_of::<OrderedSetNode<T>>(),
                align_of::<OrderedSetNode<T>>(),
            )) as *mut _
        };
        self.fast_mode_multiplier = get_fast_mod_multiplier(size as u32);
        self.cap = size;

        size
    }

    #[inline]
    unsafe fn get_bucket_ref(&self, hash_code: i32) -> *mut i32 {
        let buckets = self.buckets;
        let i = fast_mod(hash_code as u32, self.cap as u32, self.fast_mode_multiplier);
        unsafe { buckets.add(i as usize) }
    }

    fn resize(&mut self) {
        self.resize_with_new_size(expand_prime(self.count));
    }

    fn resize_with_new_size(&mut self, new_size: i32) {
        debug_assert!(!self.nodes.is_null());
        debug_assert!(new_size >= self.cap);

        unsafe {
            self.nodes = std::alloc::realloc(
                self.nodes as *mut _,
                Layout::new::<OrderedSetNode<T>>(),
                new_size as usize,
            ) as *mut _;
            std::alloc::dealloc(self.buckets as *mut _, Layout::new::<i32>());
            self.buckets = std::alloc::alloc_zeroed(Layout::from_size_align_unchecked(
                new_size as usize * size_of::<i32>(),
                align_of::<i32>(),
            )) as *mut _;

            let count = self.count;
            self.fast_mode_multiplier = get_fast_mod_multiplier(new_size as u32);
            for i in 0..count {
                let node = &mut *self.nodes.add(i as usize);
                if node.next >= -1 {
                    let bucket = &mut *self.get_bucket_ref(node.hash_code);
                    node.next = *bucket - 1;
                    *bucket = i + 1;
                }
            }

            self.cap = new_size;
        }
    }

    fn find_item_index<U: GetHashCode + PartialEq<T>>(&self, item: &U) -> i32 {
        let buckets = self.buckets;
        if buckets.is_null() {
            return -1;
        }
        let nodes = self.nodes;
        debug_assert!(!nodes.is_null());

        let mut collision_count = 0u32;
        let hash_code = item.get_hash_code();
        let mut i = *unsafe { &*self.get_bucket_ref(hash_code) } - 1;
        while i >= 0 {
            let node = unsafe { &*nodes.add(i as usize) };
            if node.hash_code == hash_code && *item == node.value {
                return i; // found
            }
            i = node.next;

            collision_count += 1;
            if collision_count > self.cap as u32 {
                panic!("Concurrent operations are not supported")
            }
        }

        -1
    }

    fn add_if_not_present(&mut self, item: T, location: &mut i32, insert_first: bool) -> bool {
        if self.buckets.is_null() {
            self.initialize(0);
        }
        debug_assert!(!self.buckets.is_null());
        debug_assert!(!self.nodes.is_null());

        let mut buckets = self.buckets;
        let mut nodes = self.nodes;

        let hash_code = item.get_hash_code();
        let mut collision_count = 0u32;
        let mut bucket = unsafe { &mut *self.get_bucket_ref(hash_code) };

        let mut i = *bucket - 1;
        while i >= 0 {
            let node = unsafe { &mut *nodes.add(i as usize) };
            if node.hash_code == hash_code && item == node.value {
                *location = i;
                return false;
            }
            i = node.next;

            collision_count += 1;
            if collision_count > self.cap as u32 {
                panic!("Concurrent operations are not supported");
            }
        }

        let index = if self.free_count > 0 {
            let index = self.free_count;
            self.free_count -= 1;
            debug_assert!(
                START_OF_FREE_LIST - unsafe { &*nodes.add(self.free_list as usize) }.next >= -1,
                "shouldn't overflow because `next` cannot underflow"
            );
            self.free_list =
                START_OF_FREE_LIST - unsafe { &*nodes.add(self.free_list as usize) }.next;
            index
        } else {
            let count = self.count;
            if count == self.cap {
                self.resize();
                bucket = unsafe { &mut *self.get_bucket_ref(hash_code) };
            }
            let index = count;
            self.count = count + 1;
            buckets = self.buckets;
            nodes = self.nodes;
            index
        };

        let node = unsafe { &mut *nodes.add(index as usize) };
        node.hash_code = hash_code;
        node.next = *bucket - 1;
        node.value = item;
        *bucket = index + 1;
        *location = index;

        if insert_first {
            let first = self.first;
            node.order_prev = -1;
            node.order_next = first;
            if self.last == -1 {
                self.last = index;
            }
            if first != -1 {
                let next = unsafe { &mut *nodes.add(first as usize) };
                next.order_prev = index;
            }
            self.first = index;
        } else {
            let last = self.last;
            node.order_next = -1;
            node.order_prev = last;
            if self.first == -1 {
                self.first = index;
            }
            if last != -1 {
                let prev = unsafe { &mut *nodes.add(last as usize) };
                prev.order_next = index;
            }
            self.last = index;
        }

        true
    }

    fn add_or_get_return_node(&mut self, item: T, location: &mut i32) -> &mut OrderedSetNode<T> {
        self.add_if_not_present(item, location, false);
        unsafe { &mut *self.nodes.add(*location as usize) }
    }

    fn remove_order_only(&mut self, node: &mut OrderedSetNode<T>) {
        let nodes = self.nodes;
        debug_assert!(!nodes.is_null());

        if (node.order_prev == -1) {
            self.first = node.order_next;
        } else {
            unsafe { &mut *nodes.add(node.order_prev as usize) }.order_next = node.order_next;
        }
        if (node.order_next == -1) {
            self.last = node.order_prev;
        } else {
            unsafe { &mut *nodes.add(node.order_next as usize) }.order_prev = node.order_prev;
        }
    }
}

impl<T: GetHashCode + PartialEq> OrderedSet<T> {
    pub fn contains<U: GetHashCode + PartialEq<T>>(&self, item: &U) -> bool {
        self.find_item_index(item) >= 0
    }

    pub fn get<U: GetHashCode + PartialEq<T>>(&self, item: &U) -> Option<&T> {
        if !self.buckets.is_null() {
            let index = self.find_item_index(item);
            if index >= 0 {
                return Some(unsafe { &(*self.nodes.add(index as usize)).value });
            }
        }

        None
    }

    pub fn get_mut<U: GetHashCode + PartialEq<T>>(&mut self, item: &U) -> Option<&mut T> {
        if !self.buckets.is_null() {
            let index = self.find_item_index(item);
            if index >= 0 {
                return Some(unsafe { &mut (*self.nodes.add(index as usize)).value });
            }
        }

        None
    }

    pub fn add(&mut self, item: T) -> bool {
        let mut location = 0i32;
        self.add_if_not_present(item, &mut location, false)
    }

    pub fn add_or_get(&mut self, item: T) -> &mut T {
        let mut location = 0i32;
        &mut self.add_or_get_return_node(item, &mut location).value
    }

    pub fn add_first(&mut self, item: T) -> bool {
        let mut location = 0i32;
        self.add_if_not_present(item, &mut location, true)
    }

    pub fn remove<U: GetHashCode + PartialEq<T>>(&mut self, item: &U) -> Option<T> {
        if self.buckets.is_null() {
            return None;
        }

        let buckets = self.buckets;
        let nodes = self.nodes;
        debug_assert!(!nodes.is_null());

        let mut collision_count = 0u32;
        let mut last = -1;

        let hash_code = item.get_hash_code();

        let mut bucket = unsafe { &mut *self.get_bucket_ref(hash_code) };
        let mut i = *bucket - 1;

        while i >= 0 {
            let node = unsafe { &mut *nodes.add(i as usize) };
            if node.hash_code == hash_code && *item == node.value {
                if last < 0 {
                    *bucket = node.next + 1;
                } else {
                    unsafe { &mut *nodes.add(last as usize) }.next = node.next;
                }

                debug_assert!(
                    (START_OF_FREE_LIST - self.free_list) < 0,
                    "shouldn't underflow because max hashtable length is MaxPrimeArrayLength = 0x7FEFFFFD(2146435069) _freelist underflow threshold 2147483646"
                );
                node.next = START_OF_FREE_LIST - self.free_list;

                let value = unsafe { std::ptr::read((&mut node.value) as *mut T) };

                self.free_list = i;
                self.free_count += 1;

                self.remove_order_only(node);

                return Some(value);
            }

            last = i;
            i = node.next;

            collision_count += 1;
            if collision_count > self.cap as u32 {
                panic!("Concurrent operations are not supported")
            }
        }

        None
    }
}

impl<T> OrderedSet<T> {
    pub fn count(&self) -> i32 {
        self.count - self.free_count
    }

    pub fn capacity(&self) -> i32 {
        self.cap
    }

    pub fn clear(&mut self) {
        let count = self.count();
        if count > 0 {
            unsafe { self.drop_items() };

            unsafe { std::slice::from_raw_parts_mut(self.buckets, self.cap as usize) }.fill(0);

            self.count = 0;
            self.free_count = 0;
            self.last = -1;
            self.first = -1;
            self.free_list = -1;
        }
    }

    pub fn iter(&self) -> impl Iterator<Item = &T> {
        RefEnumerator {
            this: self,
            cur: -1,
        }
        .iter()
    }

    pub fn iter_mut(&mut self) -> impl Iterator<Item = &mut T> {
        MutEnumerator {
            this: self,
            cur: -1,
        }
        .iter()
    }
}

use enumerator::*;
mod enumerator {
    pub use super::*;

    #[derive(Debug)]
    pub struct RefEnumerator<'a, T> {
        pub this: &'a OrderedSet<T>,
        pub cur: i32,
    }

    impl<'a, T> Enumerator for RefEnumerator<'a, T> {
        type Item = &'a T;

        fn move_next(&mut self) -> bool {
            if self.this.nodes.is_null() {
                return false;
            }
            if self.cur < 0 {
                if self.this.first == -1 {
                    return false;
                }
                self.cur = self.this.first;
            } else {
                let cur = unsafe { &*self.this.nodes.add(self.cur as usize) };
                if cur.order_next == -1 {
                    return false;
                }
                self.cur = cur.order_next;
            }
            true
        }

        fn current(&self) -> Self::Item {
            &unsafe { &*self.this.nodes.add(self.cur as usize) }.value
        }
    }

    #[derive(Debug)]
    pub struct MutEnumerator<'a, T> {
        pub this: &'a mut OrderedSet<T>,
        pub cur: i32,
    }

    impl<'a, T> Enumerator for MutEnumerator<'a, T> {
        type Item = &'a mut T;

        fn move_next(&mut self) -> bool {
            if self.this.nodes.is_null() {
                return false;
            }
            if self.cur < 0 {
                if self.this.first == -1 {
                    return false;
                }
                self.cur = self.this.first;
            } else {
                let cur = unsafe { &*self.this.nodes.add(self.cur as usize) };
                if cur.order_next == -1 {
                    return false;
                }
                self.cur = cur.order_next;
            }
            true
        }

        fn current(&self) -> Self::Item {
            &mut unsafe { &mut *self.this.nodes.add(self.cur as usize) }.value
        }
    }
}

impl<T> OrderedSet<T> {
    pub fn first(&self) -> Option<&T> {
        if (self.first >= 0) {
            Some(&unsafe { &*self.nodes.add(self.first as usize) }.value)
        } else {
            None
        }
    }
    pub fn first_mut(&mut self) -> Option<&mut T> {
        if (self.first >= 0) {
            Some(&mut unsafe { &mut *self.nodes.add(self.first as usize) }.value)
        } else {
            None
        }
    }
    pub fn last(&self) -> Option<&T> {
        if (self.last >= 0) {
            Some(&unsafe { &*self.nodes.add(self.last as usize) }.value)
        } else {
            None
        }
    }
    pub fn last_mut(&mut self) -> Option<&mut T> {
        if (self.last >= 0) {
            Some(&mut unsafe { &mut *self.nodes.add(self.last as usize) }.value)
        } else {
            None
        }
    }
}

impl<T: GetHashCode + PartialEq> OrderedSet<T> {
    pub fn prev<U: GetHashCode + PartialEq<T>>(&self, item: &U) -> Option<&T> {
        let index = self.find_item_index(item);
        if (index >= 0) {
            let node = unsafe { &*self.nodes.add(index as usize) };
            if node.order_prev >= 0 {
                return Some(&unsafe { &*self.nodes.add(node.order_prev as usize) }.value);
            }
        }
        None
    }
    pub fn prev_mut<U: GetHashCode + PartialEq<T>>(&mut self, item: &U) -> Option<&mut T> {
        let index = self.find_item_index(item);
        if (index >= 0) {
            let node = unsafe { &*self.nodes.add(index as usize) };
            if node.order_prev >= 0 {
                return Some(&mut unsafe { &mut *self.nodes.add(node.order_prev as usize) }.value);
            }
        }
        None
    }
    pub fn next<U: GetHashCode + PartialEq<T>>(&self, item: &U) -> Option<&T> {
        let index = self.find_item_index(item);
        if (index >= 0) {
            let node = unsafe { &*self.nodes.add(index as usize) };
            if node.order_next >= 0 {
                return Some(&unsafe { &*self.nodes.add(node.order_next as usize) }.value);
            }
        }
        None
    }
    pub fn next_mut<U: GetHashCode + PartialEq<T>>(&mut self, item: &U) -> Option<&mut T> {
        let index = self.find_item_index(item);
        if (index >= 0) {
            let node = unsafe { &*self.nodes.add(index as usize) };
            if node.order_next >= 0 {
                return Some(&mut unsafe { &mut *self.nodes.add(node.order_next as usize) }.value);
            }
        }
        None
    }

    pub fn set_next(&mut self, item: T, next: T) -> bool {
        let mut this_index = -1;
        let mut next_index = -1;
        let this_node =
            unsafe { &mut *(self as *mut Self) }.add_or_get_return_node(item, &mut this_index);
        let next_node =
            unsafe { &mut *(self as *mut Self) }.add_or_get_return_node(next, &mut next_index);
        if this_index == next_index {
            return false;
        }
        if this_node.order_next == next_index {
            return false;
        }
        self.remove_order_only(next_node);
        let old_next = this_node.order_next;
        this_node.order_next = next_index;
        next_node.order_next = old_next;
        if old_next == -1 {
            self.last = next_index;
        } else {
            unsafe { &mut *self.nodes.add(old_next as usize) }.order_prev = next_index;
        }
        next_node.order_prev = this_index;
        true
    }

    pub fn set_prev(&mut self, item: T, next: T) -> bool {
        let mut this_index = -1;
        let mut prev_index = -1;
        let this_node =
            unsafe { &mut *(self as *mut Self) }.add_or_get_return_node(item, &mut this_index);
        let prev_node =
            unsafe { &mut *(self as *mut Self) }.add_or_get_return_node(next, &mut prev_index);
        if this_index == prev_index {
            return false;
        }
        if this_node.order_prev == prev_index {
            return false;
        }
        self.remove_order_only(prev_node);
        let old_prev = this_node.order_prev;
        this_node.order_prev = prev_index;
        prev_node.order_prev = old_prev;
        if old_prev == -1 {
            self.first = prev_index;
        } else {
            unsafe { &mut *self.nodes.add(old_prev as usize) }.order_next = prev_index;
        }
        prev_node.order_next = this_index;
        true
    }
}
