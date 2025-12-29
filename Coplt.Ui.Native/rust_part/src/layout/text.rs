use std::{collections::HashMap, ops::Range, str::FromStr, u64};

use crate::{
    IsZeroLength,
    com::{self, *},
    icu4c::{self, UBiDi, UBiDiDirection, UBiDiLevel},
    layout::{Layout, LayoutInner, ViewStyleHandle},
    utf16::Utf16Indices,
    utils::UnicodeBufferPushUtf16,
};
use cocom::ComPtr;
use coplt_ui_rust_common::{AGen, MakeGeneratorIter, a_gen, merge_ranges};
use harfrust::{Feature, ShaperData, ShaperInstance, UnicodeBuffer, Variation};
use icu::{
    properties::{CodePointMapData, props::Script},
    segmenter::{GraphemeClusterSegmenter, LineSegmenter},
};
use skrifa::prelude::Size as SkrifaSize;
use skrifa::{FontRef, GlyphId, MetadataProvider};

macro_rules! tag {
    { $a:expr, $b:expr, $c:expr, $d:expr } => {
        const { harfrust::Tag::new(&[$a as u8, $b as u8, $c as u8, $d as u8]) }
    };
    { $str:expr } => {
        const {
            let str = $str.as_bytes();
            harfrust::Tag::new(&[str[0] as u8, str[1] as u8, str[2] as u8, str[3] as u8])
        }
    };
}

use font_face_ops::*;
use taffy::{CoreStyle, MaybeMath, MaybeResolve, Rect, ResolveOrZero, Size, SizingMode};
#[cfg(target_os = "windows")]
mod font_face_ops {
    use super::*;

    pub fn get_font_ref(font_face: &'_ ComPtr<IFontFace>) -> &'_ FontRef<'_> {
        use crate::dwrite::FontFace;
        let font_face = unsafe { font_face.as_object::<FontFace>() };
        font_face.font_ref()
    }

    pub fn get_glyph_type(
        font_face: &ComPtr<IFontFace>,
        glyph: u16,
        not_exists: impl FnOnce() -> GlyphType,
    ) -> GlyphType {
        use crate::dwrite::FontFace;
        let font_face = unsafe { font_face.as_object::<FontFace>() };
        font_face.get_glyph_type(glyph, not_exists)
    }
}

struct RootConstants {
    dir: WritingDirection,
    min_size: Size<Option<f32>>,
    max_size: Size<Option<f32>>,
    margin: Rect<f32>,
    border: Rect<f32>,
    node_outer_size: Size<Option<f32>>,
    node_inner_size: Size<Option<f32>>,
    container_size: Size<f32>,
}

#[inline(always)]
fn compute_constants(
    doc: &mut super::SubDocInner,
    id: NodeId,
    inputs: taffy::LayoutInput,
) -> RootConstants {
    let common = doc.common_data(id);
    let style = doc.style_data(id);
    let childs = doc.childs(id);

    let dir = style.WritingDirection;
    let parent_size = inputs.parent_size;

    let style = ViewStyleHandle(&doc, id);
    inputs.known_dimensions;
    let aspect_ratio = style.aspect_ratio();
    let margin = style
        .margin()
        .resolve_or_zero(parent_size.width, |_, _| 0.0);
    let padding = style
        .padding()
        .resolve_or_zero(parent_size.width, |_, _| 0.0);
    let border = style
        .border()
        .resolve_or_zero(parent_size.width, |_, _| 0.0);
    let padding_border_sum = padding.sum_axes() + border.sum_axes();
    let box_sizing_adjustment = if style.box_sizing() == taffy::BoxSizing::ContentBox {
        padding_border_sum
    } else {
        taffy::Size::ZERO
    };

    let scrollbar_gutter = style.overflow().transpose().map(|overflow| match overflow {
        taffy::Overflow::Scroll => style.scrollbar_width(),
        _ => 0.0,
    });

    let mut content_box_inset = padding + border;
    content_box_inset.right += scrollbar_gutter.x;
    content_box_inset.bottom += scrollbar_gutter.y;

    let node_outer_size = inputs.known_dimensions;
    let node_inner_size = node_outer_size.maybe_sub(content_box_inset.sum_axes());

    RootConstants {
        dir,
        min_size: style
            .min_size()
            .maybe_resolve(parent_size, |_, _| 0.0)
            .maybe_apply_aspect_ratio(aspect_ratio)
            .maybe_add(box_sizing_adjustment),
        max_size: style
            .max_size()
            .maybe_resolve(parent_size, |_, _| 0.0)
            .maybe_apply_aspect_ratio(aspect_ratio)
            .maybe_add(box_sizing_adjustment),
        margin,
        border,
        node_outer_size,
        node_inner_size,
        container_size: Default::default(),
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

        taffy::compute_cached_layout(doc, id.into(), inputs, |doc, _, inputs| {
            if inputs.run_mode == taffy::RunMode::PerformHiddenLayout {
                return self.compute_hidden_layout(doc, id);
            }

            let layout = doc.layout_data(id);
            let style = doc.style_data(id);
            let childs = doc.childs(id);

            if layout.is_layout_dirty(doc) {
                self.check_text_dirty(doc, id);
                layout.LayoutDirtyFrame = u64::MAX;
            }

            let mut constants = compute_constants(doc, id, inputs);

            // todo

            for child in childs.iter() {
                match child.typ() {
                    NodeType::Null | NodeType::TextSpan => continue,
                    NodeType::View => {
                        // todo inline block
                    }
                    NodeType::TextParagraph => {
                        let _r = self.compute_text_paragraph_layout(doc, *child, inputs, style);
                        // todo
                    }
                }
            }

            // todo

            taffy::LayoutOutput::HIDDEN
        })
    }

    fn compute_text_paragraph_layout(
        &mut self,
        doc: &mut super::SubDocInner,
        id: NodeId,
        inputs: taffy::LayoutInput,
        root_style: &StyleData,
    ) -> taffy::LayoutOutput {
        taffy::compute_cached_layout(doc, id.into(), inputs, |doc, _, inputs| {
            let common = doc.common_data(id);
            let childs = doc.childs(id);
            let paragraph = doc.text_paragraph_data(id);
            let style = doc.text_style_data(id);

            let runs = paragraph.run_ranges();

            // todo

            taffy::LayoutOutput::HIDDEN
        })
    }
}

impl Layout {
    fn check_text_dirty(&mut self, doc: &mut super::SubDocInner, id: NodeId) {
        let root_style = doc.style_data(id);
        let childs = doc.childs(id);

        for child in childs.iter() {
            match child.typ() {
                NodeType::Null | NodeType::TextSpan => continue,
                NodeType::View => {
                    // inline block not need
                    continue;
                }
                NodeType::TextParagraph => {
                    let id = *child;
                    let paragraph = doc.text_paragraph_data(id);
                    let style = doc.text_style_data(id);

                    if paragraph.is_text_dirty(doc) {
                        self.sync_text_info(doc, id, paragraph);
                    }
                    if paragraph.is_text_dirty(doc) || paragraph.is_text_style_dirty(doc) {
                        self.sync_styled_info(doc, id, paragraph, root_style, style);
                    }
                    paragraph.TextDirtyFrame = u64::MAX;
                    paragraph.TextStyleDirtyFrame = u64::MAX;
                }
            }
        }
    }

    fn sync_text_info(
        &mut self,
        doc: &mut super::SubDocInner,
        id: NodeId,
        paragraph: &mut TextParagraphData,
    ) {
        analyze_scripts(paragraph);
        analyze_break_points(paragraph);
        analyze_graphemes(paragraph);
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
    }

    fn sync_styled_info(
        &mut self,
        doc: &mut super::SubDocInner,
        id: NodeId,
        paragraph: &mut TextParagraphData,
        root_style: &StyleData,
        style: &TextStyleData,
    ) {
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

        build_runs(paragraph);

        shape(doc, root_style, style, paragraph);

        return;

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
                match style.TextDirection().unwrap_or(root_style.TextDirection) {
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

            for (range, data, span) in Layout::iter_child_style_range_with_must_split(
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
                    ComputedFontSize: data.font_size,
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

        fn build_runs(paragraph: &mut TextParagraphData) {
            let text = &*{ paragraph.m_text };

            let run_ranges = paragraph.run_ranges();
            run_ranges.clear();

            if text.is_empty() {
                return;
            }

            let script_ranges: &[_] = paragraph.script_ranges();
            let bidi_ranges: &[_] = paragraph.bidi_ranges();
            let same_style_ranges: &[_] = paragraph.same_style_ranges();
            let font_ranges: &[_] = paragraph.font_ranges();

            let inputs: [&mut dyn Iterator<Item = (/* index */ u32, Range<u32>)>; _] = [
                &mut script_ranges
                    .iter()
                    .enumerate()
                    .map(|(n, r)| (n as u32, r.Start..r.End)),
                &mut bidi_ranges
                    .iter()
                    .enumerate()
                    .map(|(n, r)| (n as u32, r.Start..r.End)),
                &mut same_style_ranges
                    .iter()
                    .enumerate()
                    .map(|(n, r)| (n as u32, r.Start..r.End)),
                &mut font_ranges
                    .iter()
                    .enumerate()
                    .map(|(n, r)| (n as u32, r.start..r.end)),
            ];
            for (range, [script, bidi, style, font]) in merge_ranges(inputs) {
                run_ranges.push(TextData_RunRange {
                    Start: range.start,
                    End: range.end,
                    ScriptRange: script,
                    BidiRange: bidi,
                    StyleRange: style,
                    FontRange: font,
                    ..Default::default()
                });
            }
        }

        fn shape(
            doc: &mut super::SubDocInner,
            root_style: &StyleData,
            style: &TextStyleData,
            paragraph: &mut TextParagraphData,
        ) {
            let text = &*{ paragraph.m_text };

            let glyph_datas = paragraph.glyph_datas();
            glyph_datas.clear();

            if text.is_empty() {
                return;
            }

            let default_locale = doc.root_data().DefaultLocale.or(doc.ctx().default_locale);

            let text_direction = style.TextDirection().unwrap_or(root_style.TextDirection);
            let writing_direction = style
                .WritingDirection()
                .unwrap_or(root_style.WritingDirection);

            let axis_y = match writing_direction {
                WritingDirection::Horizontal => false,
                WritingDirection::Vertical => true,
            };

            let dir = match (text_direction, writing_direction) {
                (TextDirection::Forward, WritingDirection::Horizontal) => {
                    harfrust::Direction::LeftToRight
                }
                (TextDirection::Forward, WritingDirection::Vertical) => {
                    harfrust::Direction::TopToBottom
                }
                (TextDirection::Reverse, WritingDirection::Horizontal) => {
                    harfrust::Direction::RightToLeft
                }
                (TextDirection::Reverse, WritingDirection::Vertical) => {
                    harfrust::Direction::BottomToTop
                }
            };

            let pt_size = doc.root_data().Dpi / 72.0;

            let fonts_ranges = paragraph.font_ranges();
            let style_ranges = paragraph.same_style_ranges();
            let font_metas: Vec<_> = fonts_ranges
                .iter()
                .map(|font_range| {
                    let style_range = &style_ranges[font_range.style_range as usize];
                    let span_style = style_range.style(doc).map(|s| &*s).unwrap_or(style);

                    let font_size = span_style
                        .FontSize()
                        .or(style.FontSize())
                        .unwrap_or(root_style.FontSize);
                    let font_weight = span_style
                        .FontWeight()
                        .or(style.FontWeight())
                        .unwrap_or(root_style.FontWeight);
                    let font_width = span_style
                        .FontWidth()
                        .or(style.FontWidth())
                        .unwrap_or(root_style.FontWidth);
                    let font_italic = span_style
                        .FontItalic()
                        .or(style.FontItalic())
                        .unwrap_or(root_style.FontItalic);
                    let font_oblique = span_style
                        .FontOblique()
                        .or(style.FontOblique())
                        .unwrap_or(root_style.FontOblique);

                    let font_face = &font_range.font_face;
                    let font = get_font_ref(font_face);
                    let shaper_data = ShaperData::new(font);
                    let setting = [
                        ("wght", font_weight as i32 as f32),
                        ("wdth", font_width.Width * 100.0),
                        ("ital", if font_italic { 1.0 } else { 0.0 }),
                        ("slnt", if font_italic { font_oblique } else { 0.0 }),
                    ];
                    let location = font.axes().location(setting);
                    let variations = [
                        Variation {
                            tag: tag!("wght"),
                            value: font_weight as i32 as f32,
                        },
                        Variation {
                            tag: tag!("wdth"),
                            value: font_width.Width * 100.0,
                        },
                        Variation {
                            tag: tag!("ital"),
                            value: if font_italic { 1.0 } else { 0.0 },
                        },
                        Variation {
                            tag: tag!("slnt"),
                            value: if font_italic { font_oblique } else { 0.0 },
                        },
                    ];
                    let instance = ShaperInstance::from_variations(font, variations);
                    let metrics = font.metrics(SkrifaSize::new(font_size), &location);

                    (shaper_data, font, instance, font_size, location, metrics)
                })
                .collect();
            let shapers: Vec<_> = (0..fonts_ranges.len() as usize)
                .into_iter()
                .map(|i| {
                    let (shaper_data, font, instance, _, _, _) = &font_metas[i];
                    let shaper = shaper_data
                        .shaper(font)
                        .instance(Some(instance))
                        .point_size(Some(pt_size))
                        .build();
                    shaper
                })
                .collect();
            let glyph_metricses: Vec<_> = (0..fonts_ranges.len() as usize)
                .into_iter()
                .map(|i| {
                    let (_, font, _, font_size, location, _) = &font_metas[i];
                    font.glyph_metrics(SkrifaSize::new(*font_size), location)
                })
                .collect();
            let mut glyph_type_caches: Vec<_> = (0..fonts_ranges.len() as usize)
                .into_iter()
                .map(|_| HashMap::<(u32, u32), GlyphType>::new())
                .collect();

            let mut buffer = UnicodeBuffer::new();
            for run_range in paragraph.run_ranges().iter_mut() {
                let font_face = &fonts_ranges[run_range.FontRange as usize].font_face;
                let glyph_type_cache = &mut glyph_type_caches[run_range.FontRange as usize];
                let (_, font, _, font_size, _, metrics) = &font_metas[run_range.FontRange as usize];
                run_range.Ascent = metrics.ascent;
                run_range.Descent = metrics.descent;
                run_range.Leading = metrics.leading;

                let color_glyphs = font.color_glyphs();
                let outline_glyphs = font.outline_glyphs();
                let bitmap_strikes = font.bitmap_strikes();

                let span_style = run_range
                    .get_style_range(paragraph)
                    .style(doc)
                    .map(|s| &*s)
                    .unwrap_or(style);

                let sub_text = &text[run_range.Start as usize..run_range.End as usize];

                buffer.push_utf16(sub_text);
                buffer.set_cluster_level(harfrust::BufferClusterLevel::MonotoneCharacters);
                buffer.set_direction(dir);
                let locale = span_style
                    .Locale()
                    .or(style.Locale())
                    .unwrap_or(default_locale);
                buffer.set_language(
                    locale
                        .to_language()
                        .unwrap_or(harfrust::Language::from_str("en-us").unwrap()),
                );
                let script = run_range.get_script_range(paragraph).Script;
                let script = icu::properties::props::Script::from_icu4c_value(script);
                let script = icu4c::script::get_short_name(script);
                buffer.set_script(harfrust::Script::from_str(script).unwrap());

                // todo from style
                let features = [
                    Feature::new(tag!("rlig"), 1, ..),
                    Feature::new(tag!("calt"), 1, ..),
                    Feature::new(tag!("liga"), 1, ..),
                    Feature::new(tag!("clig"), 1, ..),
                    Feature::new(tag!("locl"), 1, ..),
                    Feature::new(tag!("ccmp"), 1, ..),
                    Feature::new(tag!("mark"), 1, ..),
                    Feature::new(tag!("mkmk"), 1, ..),
                    Feature::new(tag!("kern"), 1, ..),
                ];

                let shaper = &shapers[run_range.FontRange as usize];
                let glyph_gmetrics = &glyph_metricses[run_range.FontRange as usize];
                let glyph_buffer = shaper.shape(buffer, &features);

                let glyph_len = glyph_buffer.len();

                glyph_datas.ensure_cap(glyph_datas.len() + glyph_len as i32);
                run_range.GlyphStart = glyph_datas.len() as u32;

                let glyph_infos = glyph_buffer.glyph_infos();
                let glyph_positions = glyph_buffer.glyph_positions();
                for (glyph_info, glyph_position) in glyph_infos.iter().zip(glyph_positions.iter()) {
                    let mut flags = GlyphDataFlags::None;
                    if glyph_info.unsafe_to_break() {
                        flags |= GlyphDataFlags::UnsafeToBreak;
                    }
                    let glyph_id = GlyphId::new(glyph_info.glyph_id);
                    let glyph_gmetric = glyph_gmetrics.advance_width(glyph_id);
                    let scale = 1.0 / metrics.units_per_em as f32;

                    let typ = match glyph_type_cache
                        .entry((glyph_info.glyph_id, unsafe { f32::to_bits(*font_size) }))
                    {
                        std::collections::hash_map::Entry::Occupied(entry) => *entry.get(),
                        std::collections::hash_map::Entry::Vacant(entry) => *entry.insert({
                            get_glyph_type(font_face, glyph_info.glyph_id as u16, || {
                                if color_glyphs.get(glyph_id).is_some() {
                                    GlyphType::Color
                                } else if outline_glyphs.get(glyph_id).is_some() {
                                    GlyphType::Outline
                                } else if bitmap_strikes
                                    .glyph_for_size(SkrifaSize::new(*font_size), glyph_id)
                                    .is_some()
                                {
                                    GlyphType::Bitmap
                                } else {
                                    GlyphType::Invalid
                                }
                            })
                        }),
                    };

                    glyph_datas.push(GlyphData {
                        Cluster: glyph_info.cluster,
                        Advance: glyph_gmetric.unwrap_or_else(|| {
                            scale
                                * if axis_y {
                                    glyph_position.y_advance as f32
                                } else {
                                    glyph_position.x_advance as f32
                                }
                        }),
                        Offset: scale
                            * if axis_y {
                                glyph_position.x_offset
                            } else {
                                glyph_position.y_offset
                            } as f32,
                        GlyphId: glyph_info.glyph_id as u16,
                        Flags: flags,
                        Type: typ,
                    });
                }

                run_range.GlyphEnd = glyph_datas.len() as u32;

                buffer = glyph_buffer.clear();
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
