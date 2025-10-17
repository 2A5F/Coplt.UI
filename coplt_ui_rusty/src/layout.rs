use cocom::{HResult, HResultE};

use crate::com::NLayoutContext;

#[unsafe(no_mangle)]
pub extern "C" fn coplt_ui_layout_calc(ctx: *mut NLayoutContext) -> HResult {
    unsafe { layout_calc(&mut *ctx) }
}

fn layout_calc(_ctx: &mut NLayoutContext) -> HResult {
    HResultE::Ok.into()
}
