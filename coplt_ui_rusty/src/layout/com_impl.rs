use std::{
    char::decode_utf16,
    panic::{RefUnwindSafe, UnwindSafe},
    process::Child,
};

use cocom::{ComPtr, HResultE, MakeObject, object::ObjectPtr};
use icu::{
    properties::{
        CodePointMapData, CodePointSetData,
        props::{BidiClass, BidiControl, Script},
    },
    segmenter::{GraphemeClusterSegmenter, LineSegmenter},
};
use windows::Win32::Graphics::DirectWrite::IDWriteFactory7;

use crate::{
    c_available_space,
    col::{NArc, OrderedSet},
    com::*,
    dwrite::DwLayout,
    feb_hr,
    utf16::Utf16Indices,
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
        analyze_scripts(paragraph);
        analyze_break_points(paragraph);
        analyze_graphemes(paragraph);
        analyze_bidi(paragraph);

        return;

        fn analyze_scripts(paragraph: &mut TextParagraphData) {
            let text = &*{ paragraph.m_text };

            let script_ranges = paragraph.script_ranges();
            script_ranges.clear();

            if text.is_empty() {
                return;
            }

            let mut last_i = 0;
            let mut cur_script = Script::Common;

            let map = CodePointMapData::<Script>::new();
            for (i, script) in Utf16Indices::new(text).map(|(i, c)| (i, map.get32(c))) {
                if i == 0 {
                    cur_script = script;
                    continue;
                }
                if script != cur_script {
                    script_ranges.add(TextData_ScriptRange {
                        Start: last_i,
                        Length: i as u32 - last_i,
                        Script: cur_script.to_icu4c_value(),
                    });
                    cur_script = script;
                    last_i = i as u32;
                }
            }
            script_ranges.add(TextData_ScriptRange {
                Start: last_i,
                Length: text.len() as u32 - last_i,
                Script: cur_script.to_icu4c_value(),
            });
        }

        fn analyze_break_points(paragraph: &mut TextParagraphData) {
            let text = &*{ paragraph.m_text };

            let break_points = paragraph.break_points();
            break_points.re_ctor(text.len() as i32);

            if text.is_empty() {
                return;
            }

            for pos in LineSegmenter::new_auto(Default::default())
                .segment_utf16(text)
                .skip(1)
                .map(|a| a - 1)
            {
                break_points.set(pos as i32, true);
            }
        }

        fn analyze_graphemes(paragraph: &mut TextParagraphData) {
            let text = &*{ paragraph.m_text };

            let grapheme_cluster = paragraph.grapheme_cluster();
            grapheme_cluster.clear();
            grapheme_cluster.ensure_cap(text.len() as i32);

            if text.is_empty() {
                return;
            }

            let mut last_pos = 0;
            for pos in GraphemeClusterSegmenter::new().segment_utf16(text).skip(1) {
                for _ in last_pos..pos {
                    grapheme_cluster.add(last_pos as u32);
                }
                last_pos = pos;
            }
            for _ in last_pos..text.len() {
                grapheme_cluster.add(last_pos as u32);
            }
        }

        fn analyze_bidi(paragraph: &mut TextParagraphData) {
            let text = &*{ paragraph.m_text };

            // let script_ranges = paragraph.script_ranges();
            // script_ranges.clear();

            if text.is_empty() {
                return;
            }

            let bidi_class = CodePointMapData::<BidiClass>::new();
            let bidi_control = CodePointSetData::new::<BidiControl>();

            for (i, c) in Utf16Indices::new(text) {
                let class = bidi_class.get32(c);
            }
        }
    }
}
