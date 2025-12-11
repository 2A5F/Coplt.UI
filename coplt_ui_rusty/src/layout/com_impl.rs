use std::{
    panic::{RefUnwindSafe, UnwindSafe},
    process::Child,
};

use cocom::{ComPtr, HResultE, MakeObject, object::ObjectPtr};
use icu::segmenter::LineSegmenter;
use windows::Win32::Graphics::DirectWrite::IDWriteFactory7;

use crate::{
    c_available_space,
    col::{NArc, OrderedSet},
    com::*,
    dwrite::DwLayout,
    feb_hr,
};

#[repr(C)]
#[derive(Debug)]
pub struct FontRange {
    start: u32,
    length: u32,
    font_face: ComPtr<IFontFace>,
}

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
        debug_assert!(matches!(id.typ(), NodeType::View));

        let common = doc.common_data(id);
        let style = doc.style_data(id);
        let childs = doc.childs(id);

        // todo

        for child in childs.iter() {
            match child.typ() {
                NodeType::Null | NodeType::TextSpan => continue,
                NodeType::View => {
                    // todo inline block
                }
                NodeType::TextParagraph => {
                    let r = self.compute_text_paragraph_layout(doc, *child, inputs, style);
                    // todo
                }
            }
        }

        // todo

        taffy::LayoutOutput::HIDDEN
    }

    fn compute_text_paragraph_layout(
        &self,
        doc: &mut super::SubDoc,
        id: NodeId,
        inputs: taffy::LayoutInput,
        root_style: &StyleData,
    ) {
        let common = doc.common_data(id);
        let childs = doc.childs(id);
        let paragraph = doc.text_paragraph_data(id);
        let style = doc.text_style_data(id);

        if paragraph.is_text_dirty() {
            Self::sync_text_info(paragraph);
        }
    }

    fn sync_text_info(paragraph: &mut TextParagraphData) {
        let text = &*{ paragraph.m_text };

        {
            let break_points = paragraph.break_points();
            break_points.clear();
            break_points.add_range(
                LineSegmenter::new_auto(Default::default())
                    .segment_utf16(text)
                    .map(|a| a as u32),
            );
        }
    }
}
