pub mod arc;
pub mod enumerator;
pub mod hash;
pub mod hash_helpers;
pub mod map;
pub mod ordered_set;

pub use arc::NArc;
pub use enumerator::*;
pub use hash::*;
pub use ordered_set::OrderedSet;

use crate::com::NodeId;

impl GetHashCode for NodeId {
    fn get_hash_code(&self) -> i32 {
        (self.Index ^ self.IdAndType) as i32
    }
}

impl Eq for NodeId {}

#[repr(C)]
pub enum InsertResult<T> {
    None,
    AddNew,
    Overwrite(T),
}

#[repr(C)]
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct StrideSlice<T> {
    ptr: *mut T,
    len: u32,
    stride: u32,
}

impl<T> StrideSlice<T> {
    pub fn new(ptr: *mut T, len: u32, stride: u32) -> Self {
        Self { ptr, len, stride }
    }

    pub fn iter(&self) -> StrideSliceIter<T> {
        StrideSliceIter {
            ptr: self.ptr,
            i: 0,
            len: self.len,
            stride: self.stride,
        }
    }
}

#[repr(C)]
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct StrideSliceIter<T> {
    ptr: *mut T,
    i: u32,
    len: u32,
    stride: u32,
}

impl<T> Iterator for StrideSliceIter<T> {
    type Item = *mut T;

    fn next(&mut self) -> Option<Self::Item> {
        if self.i >= self.len {
            None
        } else {
            Some(unsafe { (self.ptr as *mut u8).add((self.i * self.stride) as usize) as *mut T })
        }
    }

    fn size_hint(&self) -> (usize, Option<usize>) {
        let size = (self.len - self.i) as usize;
        (size, Some(size))
    }
}
