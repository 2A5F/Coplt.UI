use std::{
    panic::{RefUnwindSafe, UnwindSafe},
    process::Child,
};

use cocom::{ComPtr, HResultE, MakeObject, object::ObjectPtr};
use windows::Win32::Graphics::DirectWrite::IDWriteFactory7;

use crate::{
    c_available_space,
    col::{NArc, OrderedSet},
    com::*,
    dwrite::DwLayout,
    feb_hr,
};

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
        let common = doc.common_data(id);
        let childs_data = doc.childs_data(id);
        let style = doc.style_data(id);
        let childs = doc.childs(id);

        if common.m_text_data.m_ptr.is_null() || common.LastLayoutVersion != common.LayoutVersion {
            self.sync_text_data(doc, common, childs_data, style, childs);
        }
        debug_assert!(!common.m_text_data.m_ptr.is_null());
        let text_data = common.text_data().val_mut().unwrap();

        // todo

        taffy::LayoutOutput::HIDDEN
    }

    fn sync_text_data(
        &self,
        doc: &mut super::SubDoc,
        common: &mut CommonData,
        childs_data: &mut ChildsData,
        style: &mut StyleData,
        childs: &mut OrderedSet<NodeId>,
    ) {
        if common.m_text_data.m_ptr.is_null() {
            *common.text_data() = unsafe { NArc::new_zeroed() };
        }
        let text_data = common.text_data().val_mut().unwrap();

        for child in childs
            .iter()
            .filter(|child| matches!(child.typ(), NodeType::TextSpan))
        {
            let text_span_common = doc.common_data(*child);
            let text_span_data = doc.text_span_data(*child);
            let text_span_style = doc.text_span_style_data(*child);
            // todo
        }
    }
}

#[repr(C)]
#[derive(Debug)]
pub struct FontRange {
    start: u32,
    length: u32,
    font_face: ComPtr<IFontFace>,
}
