use std::panic::{RefUnwindSafe, UnwindSafe};

use cocom::{HResultE, MakeObject, object::ObjectPtr};
use windows::Win32::Graphics::DirectWrite::IDWriteFactory7;

use crate::{c_available_space, com::*, dwrite::DwLayout, feb_hr};

#[derive(Debug)]
#[cocom::object(ILayout)]
pub struct Layout {
    #[cfg(target_os = "windows")]
    pub(crate) inner: DwLayout,
}

pub(crate) trait LayoutInner {
    // todo
}

impl impls::ILayout for Layout {
    fn Calc(&mut self, ctx: *mut crate::com::NLayoutContext) -> cocom::HResult {
        let this = self as *mut _;
        feb_hr(|| unsafe {
            for root in unsafe { &mut *(*ctx).roots() }.iter_mut().map(|a| a.1) {
                let mut sub_doc = super::SubDoc(ctx, root as *mut _, this);
                let root_data = *sub_doc.root_data();
                let available_space = taffy::Size {
                    width: c_available_space!(root_data.AvailableSpaceX),
                    height: c_available_space!(root_data.AvailableSpaceY),
                };
                let root_id = root.Node.into();
                taffy::compute_root_layout(&mut sub_doc, root_id, available_space);
                if root_data.UseRounding {
                    taffy::round_layout(&mut sub_doc, root_id);
                }
            }

            Ok(HResultE::Ok.into())
        })
    }
}

impl Layout {
    pub(super) fn compute_text_layout(
        &self,
        doc: &mut super::SubDoc,
        id: NodeId,
        inputs: taffy::LayoutInput,
    ) -> taffy::LayoutOutput {
        let data = doc.common_data(id);
        let style = doc.style_data(id);

        // todo

        taffy::LayoutOutput::HIDDEN
    }
}
