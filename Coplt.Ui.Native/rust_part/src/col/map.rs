use std::ptr::drop_in_place;

use crate::col::iter::EnumeratorIterator;
use crate::col::{GetHashCode, InsertResult, enumerator, hash_helpers};
use crate::coplt_alloc::*;

const START_OF_FREE_LIST: i32 = -3;

#[repr(C)]
#[derive(Debug)]
pub struct NativeMap<TKey, TValue> {
    buckets: *mut i32,
    entries: *mut Entry<TKey, TValue>,
    fast_mode_multiplier: u64,
    cap: i32,
    count: i32,
    free_list: i32,
    free_count: i32,
}

impl<TKey, TValue> Drop for NativeMap<TKey, TValue> {
    fn drop(&mut self) {
        if self.buckets.is_null() {
            return;
        }
        unsafe {
            if std::mem::needs_drop::<TKey>() || std::mem::needs_drop::<TValue>() {
                for (k, v) in self.iter_mut() {
                    drop_in_place(k);
                    drop_in_place(v);
                }
            }
            coplt_free(self.buckets);
            coplt_free(self.entries);
            self.buckets = std::ptr::null_mut();
            self.entries = std::ptr::null_mut();
            self.fast_mode_multiplier = 0;
            self.cap = 0;
            self.count = 0;
            self.free_list = -1;
            self.free_count = -1;
        }
    }
}

#[repr(C)]
#[derive(Debug)]
struct Entry<TKey, TValue> {
    hash_code: i32,
    next: i32,
    key: TKey,
    value: TValue,
}

impl<TKey, TValue> NativeMap<TKey, TValue> {
    pub fn count(&self) -> i32 {
        self.count - self.free_count
    }
    pub fn capacity(&self) -> i32 {
        self.cap
    }
}

impl<TKey, TValue> NativeMap<TKey, TValue> {
    fn initialize(&mut self, capacity: i32) -> i32 {
        let size = hash_helpers::get_prime(capacity);
        self.free_list = -1;
        self.buckets = unsafe { coplt_alloc_array(size as usize) };
        self.entries = unsafe { coplt_alloc_array(size as usize) };
        self.fast_mode_multiplier = hash_helpers::get_fast_mod_multiplier(size as u32);
        self.cap = size;
        size
    }

    fn get_bucket(&self, hash_code: i32) -> *mut i32 {
        unsafe {
            self.buckets.add(hash_helpers::fast_mod(
                hash_code as u32,
                self.cap as u32,
                self.fast_mode_multiplier,
            ) as usize)
        }
    }

    fn resize(&mut self) {
        self.resize_with(hash_helpers::expand_prime(self.count));
    }

    fn resize_with(&mut self, new_size: i32) {
        debug_assert!(!self.entries.is_null());
        debug_assert!(new_size >= self.cap);

        unsafe {
            self.entries = coplt_realloc_array(self.entries, new_size as usize);

            coplt_free(self.buckets);
            self.buckets = coplt_zalloc_array(new_size as usize);

            let count = self.count;
            self.fast_mode_multiplier = hash_helpers::get_fast_mod_multiplier(new_size as u32);
            for i in 0..count {
                let entry = self.entries.add(i as usize);
                if (*entry).next >= -1 {
                    let bucket = self.get_bucket((*entry).hash_code);
                    (*entry).next = (*bucket) - 1;
                    (*bucket) = i + 1;
                }
            }

            self.cap = new_size;
        }
    }
}

impl<TKey: GetHashCode + PartialEq, TValue> NativeMap<TKey, TValue> {
    fn try_insert(&mut self, key: TKey, value: TValue, overwrite: bool) -> InsertResult<TValue> {
        unsafe {
            if self.buckets.is_null() {
                self.initialize(0);
            }
            debug_assert!(!self.buckets.is_null());

            let mut entries = self.entries;
            debug_assert!(!entries.is_null());

            let hash_code = key.get_hash_code();

            let mut collision_count = 0u32;
            let mut bucket = self.get_bucket(hash_code);
            let mut i = (*bucket) - 1; // Value in _buckets is 1-based

            while (i as u32) < (self.cap as u32) {
                let entry = entries.add(i as usize);
                if (*entry).hash_code == hash_code && (*entry).key == key {
                    if overwrite {
                        return InsertResult::Overwrite(std::mem::replace(
                            &mut (*entry).value,
                            value,
                        ));
                    }
                    return InsertResult::None;
                }

                i = (*entry).next;

                collision_count += 1;
                if collision_count > self.cap as u32 {
                    panic!("Concurrent operations are not supported")
                }
            }

            let index = if self.free_count > 0 {
                let index = self.free_list;
                debug_assert!(
                    START_OF_FREE_LIST - (*entries.add(self.free_list as usize)).next >= -1,
                    "shouldn't overflow because `next` cannot underflow"
                );
                self.free_list = START_OF_FREE_LIST - (*entries.add(self.free_list as usize)).next;
                self.free_count -= 1;
                index
            } else {
                let count = self.count;
                if count == self.cap {
                    self.resize();
                    bucket = self.get_bucket(hash_code);
                }
                let index = count;
                self.count = count + 1;
                entries = self.entries;
                index
            };

            let entry = entries.add(index as usize);
            entry.write(Entry {
                hash_code,
                next: (*bucket) - 1, // Value in _buckets is 1-based
                key,
                value,
            });
            (*bucket) = index + 1; // Value in _buckets is 1-based

            InsertResult::AddNew
        }
    }
}

impl<TKey, TValue> NativeMap<TKey, TValue> {
    fn find_value<Q: GetHashCode + PartialEq<TKey>>(&self, key: &Q) -> Option<i32> {
        if self.buckets.is_null() {
            return None;
        }

        unsafe {
            let entries = self.entries;
            debug_assert!(!entries.is_null());

            let hash_code = key.get_hash_code();
            let mut i = *self.get_bucket(hash_code);
            let mut collision_count = 0u32;

            i -= 1;
            loop {
                if i as u32 >= self.cap as u32 {
                    return None;
                }

                let entry = &*entries.add(i as usize);
                if entry.hash_code == hash_code && *key == entry.key {
                    return Some(i);
                }

                i = entry.next;

                collision_count += 1;
                if collision_count > self.cap as u32 {
                    panic!("Concurrent operations are not supported")
                }
            }
        }
    }
}

impl<TKey: GetHashCode + PartialEq, TValue> NativeMap<TKey, TValue> {
    pub fn try_add(&mut self, key: TKey, value: TValue) -> bool {
        matches!(self.try_insert(key, value, false), InsertResult::AddNew)
    }

    pub fn set(&mut self, key: TKey, value: TValue) -> bool {
        matches!(self.try_insert(key, value, true), InsertResult::AddNew)
    }
}

impl<TKey, TValue> NativeMap<TKey, TValue> {
    pub fn has<Q: GetHashCode + PartialEq<TKey>>(&self, key: &Q) -> bool {
        self.find_value(key).is_some()
    }

    pub fn get<Q: GetHashCode + PartialEq<TKey>>(&self, key: &Q) -> Option<&TValue> {
        self.find_value(key)
            .map(|i| unsafe { &(*self.entries.add(i as usize)).value })
    }

    pub fn get_mut<Q: GetHashCode + PartialEq<TKey>>(&mut self, key: &Q) -> Option<&mut TValue> {
        self.find_value(key)
            .map(|i| unsafe { &mut (*self.entries.add(i as usize)).value })
    }

    pub fn remove<Q: GetHashCode + PartialEq<TKey>>(&mut self, key: &Q) -> Option<(TKey, TValue)> {
        if self.buckets.is_null() {
            return None;
        }

        let entries = self.entries;
        debug_assert!(!entries.is_null());

        unsafe {
            let mut collision_count = 0u32;
            let hash_code = key.get_hash_code();
            let mut bucket = self.get_bucket(hash_code);
            let mut last = -1;
            let mut i = *bucket - 1;
            while i >= 0 {
                let entry = entries.add(i as usize);
                if (*entry).hash_code == hash_code && *key == (*entry).key {
                    if last < 0 {
                        *bucket = (*entry).next - 1;
                    } else {
                        (*entries.add(last as usize)).next = (*entry).next;
                    }

                    debug_assert!(
                        START_OF_FREE_LIST - self.free_list < 0,
                        "shouldn't underflow because max hashtable length is MaxPrimeArrayLength = 0x7FEFFFFD(2146435069) _freelist underflow threshold 2147483646"
                    );
                    (*entry).next = START_OF_FREE_LIST - self.free_list;

                    let key = std::ptr::read(&(*entry).key);
                    let value = std::ptr::read(&(*entry).value);

                    self.free_list = i;
                    self.free_count += 1;
                    return Some((key, value));
                }

                last = i;
                i = (*entry).next;

                collision_count += 1;
                if collision_count > self.cap as u32 {
                    panic!("Concurrent operations are not supported");
                }
            }
        }

        None
    }

    pub fn clear(&mut self) {
        let count = self.count;
        if count <= 0 {
            return;
        }
        debug_assert!(!self.buckets.is_null());
        debug_assert!(!self.entries.is_null());

        unsafe {
            if std::mem::needs_drop::<TKey>() || std::mem::needs_drop::<TValue>() {
                for (k, v) in self.iter_mut() {
                    drop_in_place(k);
                    drop_in_place(v);
                }
            }

            std::slice::from_raw_parts_mut(self.buckets, self.cap as usize).fill(0);
            self.count = 0;
            self.free_list = -1;
            self.free_count = 0;
            std::slice::from_raw_parts_mut(
                self.entries as *mut u8,
                count as usize * size_of::<Entry<TKey, TValue>>(),
            )
            .fill(0);
        }
    }
}

pub struct Enumerator<'a, TKey, TValue> {
    map: &'a NativeMap<TKey, TValue>,
    cur: *mut Entry<TKey, TValue>,
    index: i32,
}

pub struct MutEnumerator<'a, TKey, TValue> {
    map: &'a mut NativeMap<TKey, TValue>,
    cur: *mut Entry<TKey, TValue>,
    index: i32,
}

impl<TKey, TValue> NativeMap<TKey, TValue> {
    pub fn get_enumerator(&self) -> Enumerator<'_, TKey, TValue> {
        Enumerator {
            map: self,
            cur: std::ptr::null_mut(),
            index: 0,
        }
    }
    pub fn get_enumerator_mut(&mut self) -> MutEnumerator<'_, TKey, TValue> {
        MutEnumerator {
            map: self,
            cur: std::ptr::null_mut(),
            index: 0,
        }
    }
}

impl<'a, TKey, TValue> enumerator::Enumerator for Enumerator<'a, TKey, TValue> {
    type Item = (&'a TKey, &'a TValue);

    fn move_next(&mut self) -> bool {
        while (self.index as u32) < (self.map.count as u32) {
            unsafe {
                let entry = self.map.entries.add(self.index as usize);
                self.index += 1;

                if (*entry).next >= -1 {
                    self.cur = entry;
                    return true;
                }
            }
        }
        self.index = self.map.count + 1;
        self.cur = std::ptr::null_mut();
        false
    }

    fn current(&self) -> (&'a TKey, &'a TValue) {
        let entry = unsafe { &*self.cur };
        (&entry.key, &entry.value)
    }
}

impl<'a, TKey, TValue> enumerator::Enumerator for MutEnumerator<'a, TKey, TValue> {
    type Item = (&'a mut TKey, &'a mut TValue);

    fn move_next(&mut self) -> bool {
        while (self.index as u32) < (self.map.count as u32) {
            unsafe {
                let entry = self.map.entries.add(self.index as usize);
                self.index += 1;

                if (*entry).next >= -1 {
                    self.cur = entry;
                    return true;
                }
            }
        }
        self.index = self.map.count + 1;
        self.cur = std::ptr::null_mut();
        false
    }

    fn current(&self) -> (&'a mut TKey, &'a mut TValue) {
        let entry = unsafe { &mut *self.cur };
        (&mut entry.key, &mut entry.value)
    }
}

impl<TKey, TValue> NativeMap<TKey, TValue> {
    pub fn iter(&self) -> EnumeratorIterator<Enumerator<'_, TKey, TValue>> {
        EnumeratorIterator(self.get_enumerator())
    }
    pub fn iter_mut(&mut self) -> EnumeratorIterator<MutEnumerator<'_, TKey, TValue>> {
        EnumeratorIterator(self.get_enumerator_mut())
    }
}
