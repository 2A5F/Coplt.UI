use etagere::{euclid::Size2D, *};

use super::com::*;
use cocom::{ComPtr, MakeObject, impls::ObjectBoxNew, object::Object};

#[unsafe(no_mangle)]
pub extern "C" fn coplt_ui_new_atlas_allocator(
    t: AtlasAllocatorType,
    width: i32,
    height: i32,
    output: *mut *mut IAtlasAllocator,
) {
    match t {
        AtlasAllocatorType::Common => {
            let obj = AtlasAllocator::new(width, height).make_com();
            unsafe { *output = obj.leak() };
        }
        AtlasAllocatorType::Bucketed => {
            let obj = BucketedAtlasAllocator::new(width, height).make_com();
            unsafe { *output = obj.leak() };
        }
    }
}

#[cocom::object(IAtlasAllocator)]
pub struct AtlasAllocator(etagere::AtlasAllocator);

impl AtlasAllocator {
    pub fn new(width: i32, height: i32) -> Self {
        Self(etagere::AtlasAllocator::new(Size2D::new(width, height)))
    }
}

impl impls::IAtlasAllocator for AtlasAllocator {
    fn Clear(&mut self) -> () {
        self.0.clear();
    }

    fn get_IsEmpty(&mut self) -> bool {
        self.0.is_empty()
    }

    fn GetSize(&mut self, out_width: *mut i32, out_height: *mut i32) -> () {
        let size = self.0.size();
        unsafe {
            *out_width = size.width;
            *out_height = size.height;
        }
    }

    fn Allocate(
        &mut self,
        width: i32,
        height: i32,
        out_id: *mut u32,
        out_rect: *mut crate::com::AABB2DI,
    ) -> bool {
        match self.0.allocate(Size2D::new(width, height)) {
            Some(al) => {
                unsafe {
                    *out_id = al.id.serialize();
                    *out_rect = AABB2DI {
                        MinX: al.rectangle.min.x,
                        MinY: al.rectangle.min.y,
                        MaxX: al.rectangle.max.x,
                        MaxY: al.rectangle.max.y,
                    }
                }
                true
            }
            None => false,
        }
    }

    fn Deallocate(&mut self, id: u32) -> () {
        self.0.deallocate(AllocId::deserialize(id));
    }
}

#[cocom::object(IAtlasAllocator)]
pub struct BucketedAtlasAllocator(etagere::BucketedAtlasAllocator);

impl BucketedAtlasAllocator {
    pub fn new(width: i32, height: i32) -> Self {
        Self(etagere::BucketedAtlasAllocator::new(Size2D::new(
            width, height,
        )))
    }
}

impl impls::IAtlasAllocator for BucketedAtlasAllocator {
    fn Clear(&mut self) -> () {
        self.0.clear();
    }

    fn get_IsEmpty(&mut self) -> bool {
        self.0.is_empty()
    }

    fn GetSize(&mut self, out_width: *mut i32, out_height: *mut i32) -> () {
        let size = self.0.size();
        unsafe {
            *out_width = size.width;
            *out_height = size.height;
        }
    }

    fn Allocate(
        &mut self,
        width: i32,
        height: i32,
        out_id: *mut u32,
        out_rect: *mut crate::com::AABB2DI,
    ) -> bool {
        match self.0.allocate(Size2D::new(width, height)) {
            Some(al) => {
                unsafe {
                    *out_id = al.id.serialize();
                    *out_rect = AABB2DI {
                        MinX: al.rectangle.min.x,
                        MinY: al.rectangle.min.y,
                        MaxX: al.rectangle.max.x,
                        MaxY: al.rectangle.max.y,
                    }
                }
                true
            }
            None => false,
        }
    }

    fn Deallocate(&mut self, id: u32) -> () {
        self.0.deallocate(AllocId::deserialize(id));
    }
}
