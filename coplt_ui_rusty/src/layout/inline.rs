use std::alloc::Layout;

use crate::{col::NArc, com::NativeArc};

pub use super::*;

#[derive(Debug)]
pub struct TextLayoutObject {}

impl TextLayoutObject {
    pub fn new() -> Self {
        Self {}
    }
}
