use core::f32;
use std::{collections::HashMap, fmt::Debug, ops::Range, str::FromStr, u64};

use crate::{
    IsZeroLength,
    col::NList,
    com::{self, *},
    icu4c::{self, UBiDi, UBiDiDirection, UBiDiLevel},
    layout::{Layout, LayoutInner, ViewStyleHandle},
    utf16::Utf16Indices,
    utils::UnicodeBufferPushUtf16,
};
use cocom::ComPtr;
use coplt_ui_rust_common::{
    AGen, Coroutine, Generator, GeneratorToIter, IterableGenerator, a_gen, merge_ranges,
};
use font_types::BoundingBox;
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
use taffy::{
    CoreStyle, LengthPercentageAuto, MaybeMath, MaybeResolve, Point, Rect, ResolveOrZero, Size,
    SizingMode,
    prelude::{TaffyMaxContent, TaffyZero},
};
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

#[derive(Debug, Clone, Copy, Default)]
struct RootConstants {
    dir: taffy::FlexDirection,
    min_size: Size<Option<f32>>,
    max_size: Size<Option<f32>>,
    margin: Rect<f32>,
    border: Rect<f32>,
    node_outer_size: Size<Option<f32>>,
    node_inner_size: Size<Option<f32>>,
    padding_border_sum: Size<f32>,
    container_size: Size<f32>,
}

#[inline(always)]
fn compute_constants(
    doc: &mut super::SubDocInner,
    id: NodeId,
    inputs: &mut taffy::LayoutInput,
) -> RootConstants {
    let common = doc.common_data(id);
    let style = doc.style_data(id);
    let childs = doc.childs(id);

    let dir = match (style.WritingDirection, style.TextDirection) {
        (WritingDirection::Horizontal, TextDirection::Forward) => taffy::FlexDirection::Row,
        (WritingDirection::Horizontal, TextDirection::Reverse) => taffy::FlexDirection::RowReverse,
        (WritingDirection::Vertical, TextDirection::Forward) => taffy::FlexDirection::Column,
        (WritingDirection::Vertical, TextDirection::Reverse) => taffy::FlexDirection::ColumnReverse,
    };
    let parent_size = inputs.parent_size;

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

    let min_size = style
        .min_size()
        .maybe_resolve(parent_size, |_, _| 0.0)
        .maybe_apply_aspect_ratio(aspect_ratio)
        .maybe_add(box_sizing_adjustment);

    let max_size = style
        .max_size()
        .maybe_resolve(parent_size, |_, _| 0.0)
        .maybe_apply_aspect_ratio(aspect_ratio)
        .maybe_add(box_sizing_adjustment);

    let clamped_style_size = style
        .size()
        .maybe_resolve(parent_size, |_, _| 0.0)
        .maybe_apply_aspect_ratio(aspect_ratio)
        .maybe_add(box_sizing_adjustment)
        .maybe_clamp(min_size, max_size);

    let min_max_definite_size = min_size.zip_map(max_size, |min, max| match (min, max) {
        (Some(min), Some(max)) if max <= min => Some(min),
        _ => None,
    });

    inputs.known_dimensions = inputs.known_dimensions.or(min_max_definite_size
        .or(clamped_style_size)
        .maybe_max(padding_border_sum));

    let node_outer_size = inputs.known_dimensions;
    let node_inner_size = node_outer_size.maybe_sub(padding_border_sum);

    RootConstants {
        dir,
        min_size,
        max_size,
        margin,
        border,
        node_outer_size,
        node_inner_size,
        padding_border_sum,
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

        taffy::compute_cached_layout(doc, id.into(), inputs, |doc, _, mut inputs| {
            if inputs.run_mode == taffy::RunMode::PerformHiddenLayout {
                return self.compute_hidden_layout(doc, id);
            }

            let layout = doc.layout_data(id);
            let style = doc.style_data(id);
            let childs = doc.childs(id);

            let mut constants = compute_constants(doc, id, &mut inputs);
            // Short-circuit layout if the container's size is fully determined by the container's size and the run mode
            // is ComputeSize (and thus the container's size is all that we're interested in)
            if inputs.run_mode == taffy::RunMode::ComputeSize {
                if let Size {
                    width: Some(width),
                    height: Some(height),
                } = inputs.known_dimensions
                {
                    return taffy::LayoutOutput::from_outer_size(Size { width, height });
                }
            }

            if layout.is_layout_dirty(doc) {
                self.check_text_dirty(doc, id);
                layout.LayoutDirtyFrame = u64::MAX;
            }

            let inner_size = self.compute_text_layout_inner(doc, id, &constants, &inputs);
            let outer_size = constants
                .node_outer_size
                .unwrap_or(inner_size + constants.padding_border_sum);
            let inner_size = constants.node_inner_size.unwrap_or(inner_size);

            if inputs.run_mode == taffy::RunMode::ComputeSize {
                return taffy::LayoutOutput::from_sizes(outer_size, inner_size);
            }

            let mut line_spans = layout.text_view_data().line_spans();
            let mut lines = layout.text_view_data().lines();

            if lines.len() == 0 {
                return taffy::LayoutOutput::from_sizes(outer_size, inner_size);
            }

            self.perform_text_layout_inner(doc, id, &constants, &inputs, inner_size);

            let last_base_line = lines.last().unwrap().BaseLine;
            let base_line = taffy::Point {
                x: if constants.dir.is_row() {
                    None
                } else {
                    Some(last_base_line)
                },
                y: if constants.dir.is_row() {
                    Some(last_base_line)
                } else {
                    None
                },
            };

            return taffy::LayoutOutput::from_sizes_and_baselines(
                outer_size, inner_size, base_line,
            );
        })
    }

    fn compute_text_layout_inner(
        &mut self,
        doc: &mut super::SubDocInner,
        id: NodeId,
        constants: &RootConstants,
        root_inputs: &taffy::LayoutInput,
    ) -> taffy::Size<f32> {
        let layout_data = doc.layout_data(id);
        let root_style = doc.style_data(id);
        let childs = doc.childs(id);
        let mut line_spans = layout_data.text_view_data().line_spans();
        let mut lines = layout_data.text_view_data().lines();

        let should_output = root_inputs.run_mode == taffy::RunMode::PerformLayout;

        let line_spans = &mut MiList::new_with_clear(should_output, line_spans);
        let lines = &mut MiList::new_with_clear(should_output, lines);

        let available_space = {
            let mut available_space = match (
                constants.node_inner_size.main(constants.dir),
                root_inputs.available_space.main(constants.dir),
            ) {
                (Some(v), _) => v,
                (None, taffy::AvailableSpace::Definite(v)) => {
                    f32::max(0.0, v - constants.padding_border_sum.main(constants.dir))
                }
                (None, taffy::AvailableSpace::MinContent) => f32::max(
                    0.0,
                    // max_size is correct, this may seem strange, but it's available space
                    constants
                        .max_size
                        .main(constants.dir)
                        .map(|a| a - constants.padding_border_sum.main(constants.dir))
                        .unwrap_or_default(),
                ),
                (None, taffy::AvailableSpace::MaxContent) => f32::min(
                    f32::INFINITY,
                    constants
                        .max_size
                        .main(constants.dir)
                        .map(|a| {
                            f32::max(0.0, a - constants.padding_border_sum.main(constants.dir))
                        })
                        .unwrap_or(f32::INFINITY),
                ),
            };
            if available_space.is_nan() {
                available_space = 0.0;
            }
            available_space
        };

        let dir = constants.dir;

        let mut ctx = LineBreakCtx::new(available_space, constants);
        for child in childs.iter() {
            match child.typ() {
                NodeType::Null | NodeType::TextSpan => continue,
                NodeType::View => {
                    // todo inline block
                    continue;
                }
                NodeType::TextParagraph => {
                    let style = doc.text_style_data(*child);
                    let paragraph = doc.text_paragraph_data(*child);
                    let runs = &**paragraph.run_ranges();
                    if runs.is_empty() {
                        continue;
                    }

                    let break_afters = paragraph.break_points();

                    let same_style_ranges = &**paragraph.same_style_ranges();
                    let same_style_constants: Vec<_> = same_style_ranges
                        .iter()
                        .map(|a| {
                            let run_style = a.style(doc).map(|a| &*a).unwrap_or(&*style);
                            let margin = run_style.margin().resolve_or_zero(None, |_, _| 0.0);
                            let padding = run_style.padding().resolve_or_zero(None, |_, _| 0.0);

                            let mp_main_start = margin.main_start(dir) + padding.main_start(dir);
                            let mp_main_end = margin.main_end(dir) + padding.main_end(dir);

                            let mp_cross_start = margin.cross_start(dir) + padding.cross_start(dir);
                            let mp_cross_end = margin.cross_end(dir) + padding.cross_end(dir);

                            let font_size = run_style
                                .FontSize()
                                .or(style.FontSize())
                                .unwrap_or(root_style.FontSize);
                            let line_height = run_style
                                .LineHeight()
                                .or(style.LineHeight())
                                .unwrap_or(root_style.LineHeight())
                                .resolve_to_option(font_size, |_, _| 0.0);
                            let line_height = line_height
                                .map(|line_height| mp_cross_start + line_height + mp_cross_end);

                            let wrap_flags = run_style
                                .WrapFlags()
                                .or(style.WrapFlags())
                                .unwrap_or(root_style.WrapFlags);
                            let allow_newline = wrap_flags.contains(WrapFlags::AllowNewLine);
                            let wrap_in_space = wrap_flags.contains(WrapFlags::WrapInSpace);
                            let allow_wrap = run_style
                                .TextWrap()
                                .or(style.TextWrap())
                                .unwrap_or(root_style.TextWrap)
                                != TextWrap::NoWrap;

                            SameStyleConstants {
                                mp_main_start,
                                mp_main_end,
                                mp_cross_start,
                                mp_cross_end,
                                line_height,
                                allow_newline,
                                wrap_in_space,
                                allow_wrap,
                            }
                        })
                        .collect();

                    for (run_index, run) in runs.iter().enumerate() {
                        let style_range = run.get_style_range(paragraph);
                        let sc = &same_style_constants[run.StyleRange as usize];

                        let mut run_ctx = LineBreakRunCtx::new(
                            child.Index,
                            run_index as u32,
                            run,
                            lines,
                            line_spans,
                            sc,
                        );

                        ctx.apply_run_start(&mut run_ctx);

                        let mut glyph_datas = run.get_glyph_datas(paragraph);
                        while !glyph_datas.is_empty() {
                            let gcs = GlyphClusterSize::calc_next(run, glyph_datas);
                            let is_break_after = break_afters.get(run.Start as i32 + gcs.last_cluster() as i32);
                            ctx.apply_cluster(&mut run_ctx, &gcs, is_break_after);
                            glyph_datas = &glyph_datas[gcs.glyph_count as usize..];
                        }

                        ctx.apply_run_end(&mut run_ctx);
                    }
                }
            }
        }
        ctx.finally(lines, line_spans);

        let mut size = taffy::Size::ZERO;
        size.set_main(constants.dir, ctx.max_main_size);
        size.set_cross(constants.dir, ctx.sum_cross_size);

        return size;
    }

    fn perform_text_layout_inner(
        &mut self,
        doc: &mut super::SubDocInner,
        id: NodeId,
        constants: &RootConstants,
        root_inputs: &taffy::LayoutInput,
        inner_size: Size<f32>,
    ) {
        let layout_data = doc.layout_data(id);
        let root_style = doc.style_data(id);
        let childs = doc.childs(id);
        let mut line_spans = layout_data.text_view_data().line_spans();
        let mut lines = layout_data.text_view_data().lines();

        debug_assert_eq!(root_inputs.run_mode, taffy::RunMode::PerformLayout);
        debug_assert!(line_spans.len() > 0 && lines.len() > 0);

        for line in lines.iter_mut() {
            match root_style.TextAlign {
                TextAlign::Start => {}
                TextAlign::End => {
                    if constants.dir.is_row() {
                        line.X = inner_size.width - line.Width;
                    } else {
                        line.Y = inner_size.height - line.Height;
                    }
                }
                TextAlign::Center => {
                    if constants.dir.is_row() {
                        line.X = (inner_size.width - line.Width) * 0.5;
                    } else {
                        line.Y = (inner_size.height - line.Height) * 0.5;
                    }
                }
            }
            let spans = &mut line_spans[line.SpanStart as usize..line.SpanEnd as usize];
            for span in spans.iter_mut() {
                match root_style.LineAlign {
                    LineAlign::Baseline => {
                        let offset = line.BaseLine - span.BaseLine;
                        if constants.dir.is_row() {
                            span.Y = offset;
                        } else {
                            span.X = offset;
                        }
                    }
                    LineAlign::Start => {}
                    LineAlign::End => {
                        if constants.dir.is_row() {
                            span.Y = line.Height - span.Height;
                        } else {
                            span.X = line.Width - span.Width;
                        }
                    }
                    LineAlign::Center => {
                        if constants.dir.is_row() {
                            span.Y = (line.Height - span.Height) * 0.5;
                        } else {
                            span.X = (line.Width - span.Width) * 0.5;
                        }
                    }
                }
            }
        }
    }
}

#[derive(Debug, Clone, Copy, Default)]
struct LineBreakCursor {
    pub span: u32,
    pub char: u32,
    pub offset: f32,
}

impl LineBreakCursor {
    pub fn new(span: u32, char: u32, offset: f32) -> Self {
        Self { span, char, offset }
    }
}

struct LineBreakRunCtx<'a, 'b> {
    pub node_index: u32,
    pub run_index: u32,
    pub run: &'a TextData_RunRange,
    pub lines: &'a mut MiList<'b, LineData>,
    pub line_spans: &'a mut MiList<'b, LineSpanData>,
    pub style_constants: &'a SameStyleConstants,
}

impl<'a, 'b> LineBreakRunCtx<'a, 'b> {
    pub fn new(
        node_index: u32,
        run_index: u32,
        run: &'a TextData_RunRange,
        lines: &'a mut MiList<'b, LineData>,
        line_spans: &'a mut MiList<'b, LineSpanData>,
        style_constants: &'a SameStyleConstants,
    ) -> Self {
        Self {
            node_index,
            run_index,
            run,
            lines,
            line_spans,
            style_constants,
        }
    }
}

enum MiList<'a, T> {
    Real(&'a mut NList<T>),
    Ignore { len: i32 },
}

impl<'a, T> MiList<'a, T> {
    pub fn new_with_clear(should_output: bool, list: &'a mut NList<T>) -> Self {
        if should_output {
            list.clear();
            Self::Real(list)
        } else {
            Self::Ignore { len: 0 }
        }
    }
    pub fn add(&mut self, val: T) {
        match self {
            MiList::Real(list) => list.add(val),
            MiList::Ignore { len } => *len += 1,
        }
    }
    pub fn len(&self) -> i32 {
        match self {
            MiList::Real(list) => list.len(),
            MiList::Ignore { len } => *len,
        }
    }
}

#[derive(Debug, Clone, Default)]
struct LineBreakCtx {
    pub available_space: f32,
    pub dir: taffy::FlexDirection,

    pub max_main_size: f32,
    pub sum_cross_size: f32,

    pub cursor: LineBreakCursor,

    pub nth_line: u32,
    pub cur_line_start_span: u32,
    pub cur_line_max_line_height: f32,
    pub cur_line_max_base_line: f32,
    pub cur_line_size: f32,

    pub cur_run_line_height: f32,
    pub cur_run_base_line: f32,

    pub cur_span_start: LineBreakCursor,
    pub last_break_point: Option<LineBreakCursor>,
}

impl LineBreakCtx {
    pub fn new(available_space: f32, constants: &RootConstants) -> Self {
        Self {
            available_space,
            dir: constants.dir,
            ..Default::default()
        }
    }

    fn shoud_wrap(&self) -> bool {
        self.cursor.offset > self.available_space
            && self
                .last_break_point
                .map(|a| a.offset > 0.0)
                .unwrap_or(false)
    }

    pub fn apply_cluster(
        &mut self,
        rc: &mut LineBreakRunCtx,
        gcs: &GlyphClusterSize,
        is_break_after: bool,
    ) {
        self.cursor.offset += gcs.size;
        self.cursor.char += gcs.char_count;
        if self.shoud_wrap() {
            self.break_line(rc);
        }
        if is_break_after {
            self.last_break_point = Some(self.cursor);
        }
    }

    pub fn apply_run_start(&mut self, rc: &mut LineBreakRunCtx) {
        let sc = rc.style_constants;
        let line_height = sc
            .line_height
            .unwrap_or_else(|| rc.run.Ascent + rc.run.Descent + rc.run.Leading);
        let base_line = rc.run.Ascent;

        self.cur_run_base_line = base_line;
        self.cur_run_line_height = sc.mp_cross_start + line_height + sc.mp_cross_end;

        self.cursor.offset += sc.mp_main_start;
        self.cursor.char = rc.run.Start;
        if self.shoud_wrap() {
            self.break_line(rc);
        }

        self.cur_line_max_base_line = self.cur_line_max_base_line.max(self.cur_run_base_line);
        self.cur_line_max_line_height = self.cur_line_max_line_height.max(self.cur_run_line_height);
    }

    pub fn apply_run_end(&mut self, rc: &mut LineBreakRunCtx) {
        self.cursor.offset += rc.style_constants.mp_main_end;
        if self.cursor.char != self.cur_span_start.char {
            debug_assert!(self.cursor.char >= self.cur_span_start.char);
            let start = self.cur_span_start.offset;
            let size = self.cursor.offset - self.cur_span_start.offset;
            let line_height = self.cur_run_line_height;
            let base_line = self.cur_run_base_line;
            rc.line_spans.add(LineSpanData {
                X: if self.dir.is_row() { start } else { 0.0 },
                Y: if self.dir.is_row() { 0.0 } else { start },
                Width: if self.dir.is_row() { size } else { line_height },
                Height: if self.dir.is_row() { line_height } else { size },
                BaseLine: base_line,
                NthLine: self.nth_line,
                NodeIndex: rc.node_index,
                RunRange: rc.run_index,
                Start: self.cur_span_start.char,
                End: self.cursor.char,
                Type: LineSpanType::Text,
            });
            self.cur_line_size += size;
            self.cur_span_start = self.cursor;
        }

        self.cur_run_base_line = 0.0;
        self.cur_run_line_height = 0.0;
    }

    fn break_line(&mut self, rc: &mut LineBreakRunCtx) {
        if let Some(last_break_point) = self.last_break_point {
            if last_break_point.char != self.cur_span_start.char {
                if last_break_point.char > self.cur_span_start.char {
                    let start = self.cur_span_start.offset;
                    let size = last_break_point.offset - self.cur_span_start.offset;
                    let line_height = self.cur_run_line_height;
                    let base_line = self.cur_run_base_line;
                    rc.line_spans.add(LineSpanData {
                        X: if self.dir.is_row() { start } else { 0.0 },
                        Y: if self.dir.is_row() { 0.0 } else { start },
                        Width: if self.dir.is_row() { size } else { line_height },
                        Height: if self.dir.is_row() { line_height } else { size },
                        BaseLine: base_line,
                        NthLine: self.nth_line,
                        NodeIndex: rc.node_index,
                        RunRange: rc.run_index,
                        Start: self.cur_span_start.char,
                        End: last_break_point.char,
                        Type: LineSpanType::Text,
                    });
                    self.cur_line_size += size;
                    self.cur_span_start = last_break_point;
                } else {
                    todo!()
                }
            }
        }
        // force break line; newline
        else if self.cursor.char != self.cur_span_start.char {
            debug_assert!(self.cursor.char >= self.cur_span_start.char);
            let start = self.cur_span_start.offset;
            let size = self.cursor.offset - self.cur_span_start.offset;
            let line_height = self.cur_run_line_height;
            let base_line = self.cur_run_base_line;
            rc.line_spans.add(LineSpanData {
                X: if self.dir.is_row() { start } else { 0.0 },
                Y: if self.dir.is_row() { 0.0 } else { start },
                Width: if self.dir.is_row() { size } else { line_height },
                Height: if self.dir.is_row() { line_height } else { size },
                BaseLine: base_line,
                NthLine: self.nth_line,
                NodeIndex: rc.node_index,
                RunRange: rc.run_index,
                Start: self.cur_span_start.char,
                End: self.cursor.char,
                Type: LineSpanType::Text,
            });
            self.cur_line_size += size;
            self.cur_span_start = self.cursor;
        }

        debug_assert!(rc.line_spans.len() > 0);

        let span_start = self.cur_line_start_span;
        let span_end = rc.line_spans.len() as u32;
        let size = self.cur_line_size;
        let line_height = self.cur_line_max_line_height;
        let cross_start = self.sum_cross_size;
        rc.lines.add(LineData {
            X: if self.dir.is_row() { 0.0 } else { cross_start },
            Y: if self.dir.is_row() { cross_start } else { 0.0 },
            Width: if self.dir.is_row() { size } else { line_height },
            Height: if self.dir.is_row() { line_height } else { size },
            BaseLine: self.cur_line_max_base_line,
            NthLine: self.nth_line,
            SpanStart: span_start,
            SpanEnd: span_end,
        });
        self.cur_line_size = 0.0;
        self.nth_line += 1;
        self.sum_cross_size += line_height;
        self.max_main_size = self.max_main_size.max(size);
        self.cur_line_start_span = span_end;
        if let Some(last_break_point) = self.last_break_point {
            self.cursor.offset -= last_break_point.offset;
        } else {
            self.cursor.offset = 0.0;
        }
        self.last_break_point = None;
        self.cur_span_start.offset = 0.0;
        self.cur_line_max_base_line = self.cur_run_base_line;
        self.cur_line_max_line_height = self.cur_run_line_height;
    }

    fn finally(&mut self, lines: &mut MiList<LineData>, line_spans: &mut MiList<LineSpanData>) {
        if self.cur_line_start_span < line_spans.len() as u32 {
            let span_start = self.cur_line_start_span;
            let span_end = line_spans.len() as u32;
            let size = self.cur_line_size;
            let line_height = self.cur_line_max_line_height;
            let cross_start = self.sum_cross_size;
            lines.add(LineData {
                X: if self.dir.is_row() { 0.0 } else { cross_start },
                Y: if self.dir.is_row() { cross_start } else { 0.0 },
                Width: if self.dir.is_row() { size } else { line_height },
                Height: if self.dir.is_row() { line_height } else { size },
                BaseLine: self.cur_line_max_base_line,
                NthLine: self.nth_line,
                SpanStart: span_start,
                SpanEnd: span_end,
            });
            self.nth_line += 1;
            self.sum_cross_size += line_height;
            self.max_main_size = self.max_main_size.max(size);
        }
    }
}

#[derive(Debug, Clone, Copy, Default)]
struct SameStyleConstants {
    pub mp_main_start: f32,
    pub mp_main_end: f32,
    pub mp_cross_start: f32,
    pub mp_cross_end: f32,
    pub line_height: Option<f32>,
    pub allow_newline: bool,
    pub wrap_in_space: bool,
    pub allow_wrap: bool,
}

#[derive(Debug, Clone, Copy, Default)]
struct GlyphClusterSize {
    pub cluster: u32,
    pub char_count: u32,
    pub glyph_count: u32,
    pub size: f32,
}

impl GlyphClusterSize {
    pub fn calc_next(run: &TextData_RunRange, rem_glyph_datas: &[GlyphData]) -> Self {
        let mut iter = rem_glyph_datas.iter();
        let first = match iter.next() {
            Some(a) => a,
            None => return Default::default(),
        };
        let mut glyph_count = 1;
        let mut size = first.Advance + first.Offset;
        let cluster = first.Cluster;
        while let Some(item) = iter.next().filter(|a| a.Cluster == cluster) {
            glyph_count += 1;
            size += item.Advance + item.Offset;
        }
        let char_count = if glyph_count >= rem_glyph_datas.len() {
            run.End - cluster - run.Start
        } else {
            rem_glyph_datas[glyph_count].Cluster - cluster
        };
        Self {
            cluster,
            char_count,
            glyph_count: glyph_count as u32,
            size,
        }
    }

    pub fn last_cluster(&self) -> u32 {
        if self.char_count == 0 {
            0
        } else {
            self.cluster + self.char_count - 1
        }
    }
}

impl Layout {
    fn check_text_dirty(&mut self, doc: &mut super::SubDocInner, id: NodeId) {
        let root_style = doc.style_data(id);
        let childs = doc.childs(id);

        for child in childs.iter() {
            match child.typ() {
                NodeType::Null | NodeType::TextSpan | NodeType::View => continue,
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

            // todo rewrite script merge

            let map = CodePointMapData::<Script>::new();
            for (i, script) in Utf16Indices::new(text).map(|(i, c)| (i, map.get32(c))) {
                if i == 0 {
                    cur_script = script;
                    continue;
                }
                if matches!(script, Script::Common | Script::Inherited | Script::Unknown) {
                    continue;
                }
                if script != cur_script {
                    if matches!(
                        cur_script,
                        Script::Common | Script::Inherited | Script::Unknown
                    ) {
                        cur_script = script;
                        continue;
                    }
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
            )
            .iter()
            {
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
            )
            .iter()
            {
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
            for (range, [script, bidi, style, font]) in merge_ranges(inputs).iter() {
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

            let text_direction = root_style.TextDirection;
            let writing_direction = root_style.WritingDirection;

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
                run_range.Descent = -metrics.descent;
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
    pub fn iter_child_style_range<'a, T: PartialEq + Clone + Unpin>(
        doc: &mut super::SubDocInner,
        id: NodeId,
        text: &[u16],
        style: &TextStyleData,
        root_data: T,
        load_child_data: impl FnMut(&TextStyleData, &T) -> T,
    ) -> Generator<'a, impl Coroutine<Yield = (Range<u32>, T, Option<u32>)>> {
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

    pub fn iter_child_style_range_with_must_split<'a, T: PartialEq + Clone + Unpin>(
        doc: &mut super::SubDocInner,
        id: NodeId,
        text: &[u16],
        style: &TextStyleData,
        root_data: T,
        mut load_child_data: impl FnMut(&TextStyleData, &T) -> T,
        mut must_split: impl FnMut(&TextStyleData) -> bool,
    ) -> Generator<'a, impl Coroutine<Yield = (Range<u32>, T, Option<u32>)>> {
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
    pub line_height: LengthPercentageAuto,
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
            line_height: style.LineHeight(),
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
            line_height: style.LineHeight().unwrap_or(fallback.line_height),
        }
    }
}
