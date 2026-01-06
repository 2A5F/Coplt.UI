use std::panic::AssertUnwindSafe;

use cocom::{ComPtr, HResultE};

#[cfg(target_os = "windows")]
use crate::dwrite;
use crate::{c_available_space, com::*, feb_hr};

#[repr(C)]
#[derive(Debug)]
pub struct FontRange {
    pub start: u32,
    pub end: u32,
    pub font_face: ComPtr<IFontFace>,
    pub style_range: u32,
}

#[derive(Debug)]
#[cocom::object(ILayout)]
pub struct Layout {
    #[cfg(target_os = "windows")]
    pub(crate) inner: dwrite::DwLayout,
}

pub(crate) trait LayoutInner {
    fn analyze_fonts(
        &mut self,
        doc: &mut super::SubDocInner,
        id: NodeId,
        paragraph: &mut TextParagraphData,
        root_style: &StyleData,
        style: &TextStyleData,
    ) -> anyhow::Result<()>;
}

impl impls::ILayout for Layout {
    fn Calc(&mut self, ctx: *mut crate::com::NLayoutContext) -> cocom::HResult {
        feb_hr(AssertUnwindSafe(|| {
            for root in unsafe { &mut *(*ctx).roots() }.iter_mut().map(|a| a.1) {
                let mut sub_doc = super::SubDoc {
                    layout: self,
                    inner: super::SubDocInner(ctx, root as *mut _),
                };
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
        }))
    }
}

impl Layout {
    pub fn compute_hidden_layout(
        &mut self,
        doc: &mut super::SubDocInner,
        id: NodeId,
    ) -> taffy::LayoutOutput {
        if let NodeType::View | NodeType::TextParagraph = id.typ() {
            let data = &mut *doc.layout_data(id);
            super::cache_clear(&mut data.LayoutCache);
            super::set_layout(&mut data.UnRoundedLayout, &taffy::Layout::with_order(0));

            if let NodeType::View = id.typ() {
                let childs = doc.childs(id);
                for child in childs.iter() {
                    self.compute_hidden_layout(doc, *child);
                }
            }
        }

        taffy::LayoutOutput::HIDDEN
    }
}
