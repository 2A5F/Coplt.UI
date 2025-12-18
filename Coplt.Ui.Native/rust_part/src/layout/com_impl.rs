use std::{
    char::decode_utf16,
    ops::{Deref, Range},
    os::raw::c_void,
    panic::{AssertUnwindSafe, RefUnwindSafe, UnwindSafe},
    process::Child,
    u32,
};

use cocom::{ComPtr, HResultE, MakeObject, object::ObjectPtr};
use coplt_ui_rust_common::{AGen, MakeGeneratorIter, a_gen};
use icu::{
    properties::{
        CodePointMapData, CodePointSetData,
        props::{BidiClass, BidiControl, Script},
    },
    segmenter::{GraphemeClusterSegmenter, LineSegmenter},
};
use taffy::{LengthPercentage, LengthPercentageAuto, ResolveOrZero, Style};
use windows::Win32::Graphics::DirectWrite::IDWriteFactory7;

use crate::{
    IsZeroLength, c_available_space,
    col::{NArc, OrderedSet},
    com::*,
    dwrite::DwLayout,
    feb_hr,
    icu4c::{UBiDi, UBiDiDirection, UBiDiLevel},
    utf16::Utf16Indices,
};

#[repr(C)]
#[derive(Debug)]
pub struct FontRange {
    pub start: u32,
    pub end: u32,
    pub font_face: ComPtr<IFontFace>,
}

#[derive(Debug)]
#[cocom::object(ILayout)]
pub struct Layout {
    #[cfg(target_os = "windows")]
    pub(crate) inner: DwLayout,
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
    pub(super) fn compute_text_layout(
        &mut self,
        doc: &mut super::SubDocInner,
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
        &mut self,
        doc: &mut super::SubDocInner,
        id: NodeId,
        inputs: taffy::LayoutInput,
        root_style: &StyleData,
    ) {
        let common = doc.common_data(id);
        let childs = doc.childs(id);
        let paragraph = doc.text_paragraph_data(id);
        let style = doc.text_style_data(id);

        if paragraph.is_text_dirty() {
            self.sync_text_info(doc, id, paragraph, root_style, style);
        }
    }

    fn sync_text_info(
        &mut self,
        doc: &mut super::SubDocInner,
        id: NodeId,
        paragraph: &mut TextParagraphData,
        root_style: &StyleData,
        style: &TextStyleData,
    ) {
        analyze_scripts(paragraph);
        analyze_break_points(paragraph);
        analyze_graphemes(paragraph);
        analyze_same_style(doc, id, paragraph, root_style, style);
        analyze_locale(doc, id, paragraph, root_style, style);

        if let Err(e) = analyze_bidi(paragraph, root_style, style) {
            std::panic::panic_any(e);
        }

        if let Err(e) = self
            .inner
            .analyze_fonts(doc, id, paragraph, root_style, style)
        {
            std::panic::panic_any(e);
        }

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
                        End: i as u32,
                        Script: cur_script.to_icu4c_value(),
                    });
                    cur_script = script;
                    last_i = i as u32;
                }
            }
            script_ranges.add(TextData_ScriptRange {
                Start: last_i,
                End: text.len() as u32,
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

        fn analyze_bidi(
            paragraph: &mut TextParagraphData,
            root_style: &StyleData,
            style: &TextStyleData,
        ) -> anyhow::Result<()> {
            let text = &*{ paragraph.m_text };

            let bidi_ranges = paragraph.bidi_ranges();
            bidi_ranges.clear();

            if text.is_empty() {
                return Ok(());
            }

            let mut bidi = UBiDi::new();
            bidi.set_para(
                text,
                // todo text style override
                match root_style.TextDirection {
                    TextDirection::Forward => UBiDiLevel::LeftToRight,
                    TextDirection::Reverse => UBiDiLevel::RightToLeft,
                },
            )?;

            let runs = bidi.count_runs()?;
            for i in 0..runs {
                let run = bidi.get_visual_run(i);
                bidi_ranges.add(TextData_BidiRange {
                    Start: run.logical_start as u32,
                    End: run.logical_start as u32 + run.length as u32,
                    Direction: match run.direction {
                        UBiDiDirection::RightToLeft => BidiDirection::RightToLeft,
                        _ => BidiDirection::LeftToRight,
                    },
                });
            }

            Ok(())
        }

        fn analyze_same_style(
            doc: &mut super::SubDocInner,
            id: NodeId,
            paragraph: &mut TextParagraphData,
            root_style: &StyleData,
            style: &TextStyleData,
        ) {
            let text = &*{ paragraph.m_text };

            let same_style_ranges = paragraph.same_style_ranges();
            same_style_ranges.clear();

            if text.is_empty() {
                return;
            }

            let root_data = SameStyleData::from_style(root_style);

            for (range, _, span) in Layout::iter_child_style_range_with_must_split(
                doc,
                id,
                text,
                style,
                root_data,
                |child_style, base| SameStyleData::new(child_style, base),
                |child_style| {
                    !child_style.InsertLeft().is_zero_length()
                        || !child_style.InsertTop().is_zero_length()
                        || !child_style.InsertRight().is_zero_length()
                        || !child_style.InsertBottom().is_zero_length()
                        || !child_style.MarginLeft().is_zero_length()
                        || !child_style.MarginTop().is_zero_length()
                        || !child_style.MarginRight().is_zero_length()
                        || !child_style.MarginBottom().is_zero_length()
                        || !child_style.PaddingLeft().is_zero_length()
                        || !child_style.PaddingTop().is_zero_length()
                        || !child_style.PaddingRight().is_zero_length()
                        || !child_style.PaddingBottom().is_zero_length()
                },
            ) {
                same_style_ranges.push(TextData_SameStyleRange {
                    Start: range.start,
                    End: range.end,
                    HasFirstSpan: span.is_some(),
                    FirstSpanValue: TextSpanNode {
                        Index: span.unwrap_or_default(),
                    },
                });
            }
        }

        fn analyze_locale(
            doc: &mut super::SubDocInner,
            id: NodeId,
            paragraph: &mut TextParagraphData,
            root_style: &StyleData,
            style: &TextStyleData,
        ) {
            let text = &*{ paragraph.m_text };

            let locale_ranges = paragraph.locale_ranges();
            locale_ranges.clear();

            if text.is_empty() {
                return;
            }

            let root_locale = style.Locale().unwrap_or(root_style.Locale);

            for (range, locale, _) in Layout::iter_child_style_range(
                doc,
                id,
                text,
                style,
                root_locale,
                |child_style, base| child_style.Locale().unwrap_or(*base),
            ) {
                locale_ranges.push(TextData_LocaleRange {
                    Start: range.start,
                    End: range.end,
                    Locale: locale,
                });
            }
        }
    }
}

impl Layout {
    pub fn iter_child_style_range<T: PartialEq + Clone + Unpin>(
        doc: &mut super::SubDocInner,
        id: NodeId,
        text: &[u16],
        style: &TextStyleData,
        root_data: T,
        load_child_data: impl FnMut(&TextStyleData, &T) -> T,
    ) -> impl Iterator<Item = (Range<u32>, T, Option<u32>)> {
        Self::iter_child_style_range_with_must_split(
            doc,
            id,
            text,
            style,
            root_data,
            load_child_data,
            |_| false,
        )
    }

    pub fn iter_child_style_range_with_must_split<T: PartialEq + Clone + Unpin>(
        doc: &mut super::SubDocInner,
        id: NodeId,
        text: &[u16],
        style: &TextStyleData,
        root_data: T,
        mut load_child_data: impl FnMut(&TextStyleData, &T) -> T,
        mut must_split: impl FnMut(&TextStyleData) -> bool,
    ) -> impl Iterator<Item = (Range<u32>, T, Option<u32>)> {
        a_gen(async move |ctx: AGen<(Range<u32>, T, Option<u32>)>| {
            let mut data = root_data.clone();

            let mut first_span = None;
            let mut cur_start = 0;
            let mut cur_end = 0;

            let childs = doc.childs(id);
            for child in childs
                .iter()
                .copied()
                .filter(|child| matches!(child.typ(), NodeType::TextSpan))
            {
                let text_span_data = doc.text_span_data(child);
                let text_span_style = doc.text_style_data(child);

                let start = text_span_data.TextStart;
                let length = text_span_data.TextLength;
                let end = start + length;
                if end < cur_end {
                    continue;
                }
                let start = start.max(cur_end);

                if start > cur_end {
                    if data != root_data {
                        if cur_end != cur_start {
                            ctx.Yield((cur_start..cur_end, data, first_span)).await;
                            cur_start = cur_end;
                            first_span = None;
                        }

                        data = root_data.clone();
                    }
                    cur_end = start;
                }

                let child_data = load_child_data(text_span_style, &root_data);

                if must_split(text_span_style) || child_data != data {
                    if cur_end != cur_start {
                        ctx.Yield((cur_start..cur_end, data, first_span)).await;
                    }
                    cur_start = start;
                    first_span = Some(child.Index);
                    data = child_data;
                }
                cur_end = end;
            }

            if cur_end != cur_start {
                ctx.Yield((cur_start..cur_end, data, first_span)).await;
            }

            let text_len = text.len() as u32;
            if cur_end != text_len {
                ctx.Yield((cur_end..text_len, root_data, None)).await;
            }
        })
        .to_iter()
    }
}

#[derive(Debug, Clone, Copy, PartialEq)]
struct SameStyleData {
    pub font_fallback: *mut IFontFallback,
    pub font_size: f32,
    pub font_weight: FontWeight,
    pub font_width: FontWidth,
    pub font_italic: bool,
    pub font_oblique: f32,
    pub text_orientation: TextOrientation,
}

impl SameStyleData {
    pub fn from_style(style: &StyleData) -> Self {
        Self {
            font_fallback: style.FontFallback,
            font_size: style.FontSize,
            font_weight: style.FontWeight,
            font_width: style.FontWidth,
            font_italic: style.FontItalic,
            font_oblique: style.FontOblique,
            text_orientation: style.TextOrientation,
        }
    }

    pub fn new(style: &TextStyleData, fallback: &Self) -> Self {
        Self {
            font_fallback: style.FontFallback().unwrap_or(fallback.font_fallback),
            font_size: style.FontSize().unwrap_or(fallback.font_size),
            font_weight: style.FontWeight().unwrap_or(fallback.font_weight),
            font_width: style.FontWidth().unwrap_or(fallback.font_width),
            font_italic: style.FontItalic().unwrap_or(fallback.font_italic),
            font_oblique: style.FontOblique().unwrap_or(fallback.font_oblique),
            text_orientation: style.TextOrientation().unwrap_or(fallback.text_orientation),
        }
    }
}
