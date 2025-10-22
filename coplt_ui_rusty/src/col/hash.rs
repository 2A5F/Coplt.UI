pub trait GetHashCode {
    fn get_hash_code(&self) -> i32;
}

macro_rules! impl_primitive{
    { $($n:tt),* $(,)? } => {
        $(
            impl GetHashCode for $n {
                fn get_hash_code(&self) -> i32 {
                    *self as i32
                }
            }
        )*
    };
}

impl_primitive! { bool, i8, u8, i16, u16, i32, u32 }

impl GetHashCode for i64 {
    fn get_hash_code(&self) -> i32 {
        *self as i32 & (*self >> 32) as i32
    }
}

impl GetHashCode for u64 {
    fn get_hash_code(&self) -> i32 {
        *self as i32 & (*self >> 32) as i32
    }
}

impl GetHashCode for isize {
    fn get_hash_code(&self) -> i32 {
        if size_of::<isize>() == 4 {
            return *self as i32;
        }
        *self as i32 & (*self >> 32) as i32
    }
}

impl GetHashCode for usize {
    fn get_hash_code(&self) -> i32 {
        if size_of::<usize>() == 4 {
            return *self as i32;
        }
        *self as i32 & (*self >> 32) as i32
    }
}

impl GetHashCode for f32 {
    fn get_hash_code(&self) -> i32 {
        if *self == 0.0 {
            return 0;
        }
        unsafe { f32::to_bits(*self).cast_signed() }
    }
}

impl GetHashCode for f64 {
    fn get_hash_code(&self) -> i32 {
        if *self == 0.0 {
            return 0;
        }
        let v: i64 = unsafe { f64::to_bits(*self).cast_signed() };
        v.get_hash_code()
    }
}
