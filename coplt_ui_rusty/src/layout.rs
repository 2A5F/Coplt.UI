use std::{
    hash::Hash,
    hint::unreachable_unchecked,
    mem::MaybeUninit,
    ops::{Deref, DerefMut, Sub},
    os::raw::c_void,
};

use cocom::{HResult, HResultE};
use concat_idents::concat_idents;
use taffy::{
    BlockContainerStyle, BlockItemStyle, Cache, CacheTree, CheapCloneStr, CollapsibleMarginSet,
    CoreStyle, FlexboxContainerStyle, FlexboxItemStyle, GenericRepetition, GridContainerStyle,
    GridItemStyle, LayoutBlockContainer, LayoutFlexboxContainer, LayoutGridContainer, LayoutInput,
    LayoutOutput, LayoutPartialTree, Point, ResolveOrZero, RoundTree, RunMode, Size,
    TraversePartialTree, TraverseTree, prelude::TaffyZero,
};

use crate::{
    col::{OrderedSet, StrideSlice, map::NativeMap, ordered_set},
    com::{
        self, ChildsData, CommonData, Container, GridName, GridNameType, ILib, LayoutCache,
        NLayoutContext, NodeId, NodeType, RootData, StyleData, TextSpanData, TextSpanStyleData,
    },
    utils::*,
};

mod com_impl;
pub use com_impl::*;

mod text;
pub use text::*;

#[macro_export]
macro_rules! c_option {
    ( #val ; $self:expr => $name:ident  ) => {
        concat_idents!(has_name = Has, $name {
            if $self.has_name {
                concat_idents!(value_name = $name, Value {
                    Some($self.value_name)
                })
            } else {
                None
            }

        })
    };
    ( $self:expr => $name:ident  ) => {
        concat_idents!(has_name = Has, $name {
            if $self.has_name {
                Some($self.$name)
            } else {
                None
            }

        })
    };
}

#[macro_export]
macro_rules! c_available_space {
    ( $self:ident.$name:ident ) => {
        ::concat_idents::concat_idents!(value_name = $name, Value {
            match $self.$name {
                $crate::com::AvailableSpaceType::Definite => {
                    taffy::AvailableSpace::Definite($self.value_name)
                }
                $crate::com::AvailableSpaceType::MinContent => taffy::AvailableSpace::MinContent,
                $crate::com::AvailableSpaceType::MaxContent => taffy::AvailableSpace::MaxContent,
            }
        })
    };
    ( $self:expr => $name:ident ) => {
        ::concat_idents::concat_idents!(value_name = $name, Value {
            match $self.$name {
                $crate::com::AvailableSpaceType::Definite => {
                    taffy::AvailableSpace::Definite($self.value_name)
                }
                $crate::com::AvailableSpaceType::MinContent => taffy::AvailableSpace::MinContent,
                $crate::com::AvailableSpaceType::MaxContent => taffy::AvailableSpace::MaxContent,
            }
        })
    };
}

impl NLayoutContext {
    #[inline(always)]
    pub fn roots(&self) -> *mut NativeMap<NodeId, RootData> {
        self.roots as *mut NativeMap<NodeId, RootData>
    }
}

impl Eq for NodeType {}
impl Ord for NodeType {
    fn cmp(&self, other: &Self) -> std::cmp::Ordering {
        (*self as u8).cmp(&(*other as u8))
    }
}
impl Hash for NodeType {
    fn hash<H: std::hash::Hasher>(&self, state: &mut H) {
        core::mem::discriminant(self).hash(state);
    }
}
impl From<u8> for NodeType {
    #[inline(always)]
    fn from(value: u8) -> Self {
        unsafe { std::mem::transmute(value) }
    }
}

impl NodeId {
    #[inline(always)]
    pub fn index(&self) -> i32 {
        self.Index as i32
    }

    #[inline(always)]
    pub fn typ(&self) -> NodeType {
        unsafe { std::mem::transmute((self.IdAndType & 0xF) as u8) }
    }
}

impl Into<taffy::NodeId> for NodeId {
    #[inline(always)]
    fn into(self) -> taffy::NodeId {
        unsafe { std::mem::transmute(self) }
    }
}

impl From<taffy::NodeId> for NodeId {
    #[inline(always)]
    fn from(value: taffy::NodeId) -> Self {
        unsafe { std::mem::transmute(value) }
    }
}

#[repr(C)]
#[derive(Debug)]
struct SubDoc(*mut NLayoutContext, *mut RootData, *mut com_impl::Layout);

impl SubDoc {
    #[inline(always)]
    pub fn root_data(&self) -> &'static mut RootData {
        unsafe { &mut *self.1 }
    }

    #[inline(always)]
    pub fn ctx(&self) -> &'static mut NLayoutContext {
        unsafe { &mut *self.0 }
    }

    #[inline(always)]
    pub fn childs(&self, id: NodeId) -> &'static mut OrderedSet<NodeId> {
        unsafe { &mut *((&mut self.childs_data(id).m_childs) as *mut _ as *mut OrderedSet<NodeId>) }
    }

    #[inline(always)]
    pub fn common_data(&self, id: NodeId) -> &'static mut CommonData {
        match id.typ() {
            NodeType::Null => panic!("null node"),
            NodeType::View => unsafe { &mut *self.ctx().view_common_data.add(id.index() as usize) },
            NodeType::TextSpan => unsafe {
                &mut *self.ctx().text_span_common_data.add(id.index() as usize)
            },
        }
    }

    #[inline(always)]
    pub fn childs_data(&self, id: NodeId) -> &'static mut ChildsData {
        match id.typ() {
            NodeType::Null => panic!("null node"),
            NodeType::View => unsafe { &mut *self.ctx().view_childs_data.add(id.index() as usize) },
            NodeType::TextSpan => panic!("text span douse not have childs"),
        }
    }

    #[inline(always)]
    pub fn style_data(&self, id: NodeId) -> &'static mut StyleData {
        match id.typ() {
            NodeType::Null => panic!("null node"),
            NodeType::View => unsafe { &mut *self.ctx().view_style_data.add(id.index() as usize) },
            NodeType::TextSpan => panic!("text span douse not have styles"),
        }
    }

    #[inline(always)]
    pub fn text_span_data(&self, id: NodeId) -> &'static mut TextSpanData {
        match id.typ() {
            NodeType::Null => panic!("null node"),
            NodeType::View => panic!("view douse not have text span datas"),
            NodeType::TextSpan => unsafe {
                &mut *self.ctx().text_span_data.add(id.index() as usize)
            },
        }
    }

    #[inline(always)]
    pub fn text_span_style_data(&self, id: NodeId) -> &'static mut TextSpanStyleData {
        match id.typ() {
            NodeType::Null => panic!("null node"),
            NodeType::View => panic!("view douse not have text span styles"),
            NodeType::TextSpan => unsafe {
                &mut *self.ctx().text_span_style_data.add(id.index() as usize)
            },
        }
    }
}

#[inline(always)]
fn get_layout(src: &com::LayoutData) -> taffy::Layout {
    taffy::Layout {
        order: src.Order,
        location: taffy::Point {
            x: src.LocationX,
            y: src.LocationY,
        },
        size: taffy::Size {
            width: src.Width,
            height: src.Height,
        },
        content_size: taffy::Size {
            width: src.ContentWidth,
            height: src.ContentHeight,
        },
        scrollbar_size: taffy::Size {
            width: src.ScrollXSize,
            height: src.ScrollYSize,
        },
        border: taffy::Rect {
            top: src.BorderTopSize,
            right: src.BorderRightSize,
            bottom: src.BorderBottomSize,
            left: src.BorderLeftSize,
        },
        padding: taffy::Rect {
            top: src.PaddingTopSize,
            right: src.PaddingRightSize,
            bottom: src.PaddingBottomSize,
            left: src.PaddingLeftSize,
        },
        margin: taffy::Rect {
            top: src.MarginTopSize,
            right: src.MarginRightSize,
            bottom: src.MarginBottomSize,
            left: src.MarginLeftSize,
        },
    }
}

#[inline(always)]
fn set_layout(dst: &mut com::LayoutData, src: &taffy::Layout) {
    dst.Order = src.order;
    dst.LocationX = src.location.x;
    dst.LocationY = src.location.y;
    dst.Width = src.size.width;
    dst.Height = src.size.height;
    dst.ContentWidth = src.content_size.width;
    dst.ContentHeight = src.content_size.height;
    dst.ScrollXSize = src.scrollbar_size.width;
    dst.ScrollYSize = src.scrollbar_size.height;
    dst.BorderTopSize = src.border.top;
    dst.BorderRightSize = src.border.right;
    dst.BorderBottomSize = src.border.bottom;
    dst.BorderLeftSize = src.border.left;
    dst.PaddingTopSize = src.padding.top;
    dst.PaddingRightSize = src.padding.right;
    dst.PaddingBottomSize = src.padding.bottom;
    dst.PaddingLeftSize = src.padding.left;
    dst.MarginTopSize = src.margin.top;
    dst.MarginRightSize = src.margin.right;
    dst.MarginBottomSize = src.margin.bottom;
    dst.MarginLeftSize = src.margin.left;
}

struct ChildIter(ordered_set::PtrCopyIter<NodeId>);

impl ChildIter {
    #[inline(always)]
    pub fn empty() -> Self {
        static EMPTY: OrderedSet<NodeId> = OrderedSet {
            buckets: std::ptr::null_mut(),
            nodes: std::ptr::null_mut(),
            fast_mode_multiplier: 0,
            cap: 0,
            first: -1,
            last: -1,
            count: 0,
            free_list: -1,
            free_count: -1,
        };
        unsafe { Self(EMPTY.iter_ptr_copy()) }
    }
}

impl Iterator for ChildIter {
    type Item = taffy::NodeId;

    #[inline(always)]
    fn next(&mut self) -> Option<Self::Item> {
        match self.0.next() {
            None => None,
            Some(a) => Some(a.into()),
        }
    }
}

impl TraversePartialTree for SubDoc {
    type ChildIter = ChildIter;

    #[inline(always)]
    fn child_ids(&self, parent_node_id: taffy::NodeId) -> Self::ChildIter {
        ChildIter(self.childs(parent_node_id.into()).iter_ptr_copy())
    }

    #[inline(always)]
    fn child_count(&self, parent_node_id: taffy::NodeId) -> usize {
        self.childs(parent_node_id.into()).count() as usize
    }
}

impl TraverseTree for SubDoc {}

impl LayoutPartialTree for SubDoc {
    type CoreContainerStyle<'a>
        = StyleHandle<'a>
    where
        Self: 'a;

    type CustomIdent = GridName;

    #[inline(always)]
    fn get_core_container_style(&self, node_id: taffy::NodeId) -> Self::CoreContainerStyle<'_> {
        StyleHandle(self, node_id.into())
    }

    #[inline(always)]
    fn set_unrounded_layout(&mut self, node_id: taffy::NodeId, layout: &taffy::Layout) {
        let id = NodeId::from(node_id);
        let dst = self.common_data(id);
        set_layout(&mut dst.UnRoundedLayout, layout);
    }

    fn compute_child_layout(
        &mut self,
        node_id: taffy::NodeId,
        inputs: taffy::LayoutInput,
    ) -> taffy::LayoutOutput {
        let id = NodeId::from(node_id);
        match id.typ() {
            // todo: simple layout for independent text
            NodeType::Null | NodeType::TextSpan => taffy::LayoutOutput::HIDDEN,
            NodeType::View => {
                taffy::compute_cached_layout(self, node_id, inputs, |tree, node_id, inputs| {
                    if inputs.run_mode == taffy::RunMode::PerformHiddenLayout {
                        return taffy::compute_hidden_layout(tree, node_id);
                    }
                    let style = StyleHandle(tree, id);
                    let visible = style.Visible;
                    if let com::Visible::Remove = visible {
                        return taffy::compute_hidden_layout(tree, node_id);
                    }
                    let childs = tree.childs(id);
                    if childs.count() == 0 {
                        return taffy::compute_leaf_layout(
                            inputs,
                            &style,
                            |_, _| 0.0,
                            |_, _| Size::ZERO,
                        );
                    }
                    let container = style.Container;
                    match container {
                        com::Container::Flex => {
                            taffy::compute_flexbox_layout(tree, node_id, inputs)
                        }
                        com::Container::Grid => taffy::compute_grid_layout(tree, node_id, inputs),
                        com::Container::Text => tree.compute_text_layout(id, inputs),
                    }
                })
            }
        }
    }
}

impl RoundTree for SubDoc {
    #[inline(always)]
    fn get_unrounded_layout(&self, node_id: taffy::NodeId) -> taffy::Layout {
        let id = NodeId::from(node_id);
        let data = self.common_data(id);
        get_layout(&data.UnRoundedLayout)
    }

    #[inline(always)]
    fn set_final_layout(&mut self, node_id: taffy::NodeId, layout: &taffy::Layout) {
        let id = NodeId::from(node_id);
        let data = self.common_data(id);
        set_layout(&mut data.FinalLayout, layout);
    }

    #[inline(always)]
    fn should_round(&self, node_id: taffy::NodeId) -> bool {
        let id = NodeId::from(node_id);
        return matches!(id.typ(), NodeType::View);
    }
}

impl CacheTree for SubDoc {
    #[inline(always)]
    fn cache_get(
        &self,
        node_id: taffy::NodeId,
        known_dimensions: taffy::Size<Option<f32>>,
        available_space: taffy::Size<taffy::AvailableSpace>,
        run_mode: taffy::RunMode,
    ) -> Option<taffy::LayoutOutput> {
        let id = NodeId::from(node_id);
        let data = &self.common_data(id).LayoutCache;
        cache_get(data, known_dimensions, available_space, run_mode)
    }

    #[inline(always)]
    fn cache_store(
        &mut self,
        node_id: taffy::NodeId,
        known_dimensions: taffy::Size<Option<f32>>,
        available_space: taffy::Size<taffy::AvailableSpace>,
        run_mode: taffy::RunMode,
        layout_output: taffy::LayoutOutput,
    ) {
        let id = NodeId::from(node_id);
        let data = &mut self.common_data(id).LayoutCache;
        cache_store(
            data,
            known_dimensions,
            available_space,
            run_mode,
            layout_output,
        )
    }

    #[inline(always)]
    fn cache_clear(&mut self, node_id: taffy::NodeId) {
        let id = NodeId::from(node_id);
        let data = &mut self.common_data(id).LayoutCache;
        cache_clear(data);
    }
}

impl LayoutBlockContainer for SubDoc {
    type BlockContainerStyle<'a>
        = StyleHandle<'a>
    where
        Self: 'a;

    type BlockItemStyle<'a>
        = StyleHandle<'a>
    where
        Self: 'a;

    #[inline(always)]
    fn get_block_container_style(&self, node_id: taffy::NodeId) -> Self::BlockContainerStyle<'_> {
        StyleHandle(self, node_id.into())
    }

    #[inline(always)]
    fn get_block_child_style(&self, child_node_id: taffy::NodeId) -> Self::BlockItemStyle<'_> {
        StyleHandle(self, child_node_id.into())
    }
}

impl LayoutFlexboxContainer for SubDoc {
    type FlexboxContainerStyle<'a>
        = StyleHandle<'a>
    where
        Self: 'a;

    type FlexboxItemStyle<'a>
        = StyleHandle<'a>
    where
        Self: 'a;

    #[inline(always)]
    fn get_flexbox_container_style(
        &self,
        node_id: taffy::NodeId,
    ) -> Self::FlexboxContainerStyle<'_> {
        StyleHandle(self, node_id.into())
    }

    #[inline(always)]
    fn get_flexbox_child_style(&self, child_node_id: taffy::NodeId) -> Self::FlexboxItemStyle<'_> {
        StyleHandle(self, child_node_id.into())
    }
}

impl LayoutGridContainer for SubDoc {
    type GridContainerStyle<'a>
        = StyleHandle<'a>
    where
        Self: 'a;

    type GridItemStyle<'a>
        = StyleHandle<'a>
    where
        Self: 'a;

    #[inline(always)]
    fn get_grid_container_style(&self, node_id: taffy::NodeId) -> Self::GridContainerStyle<'_> {
        StyleHandle(self, node_id.into())
    }

    #[inline(always)]
    fn get_grid_child_style(&self, child_node_id: taffy::NodeId) -> Self::GridItemStyle<'_> {
        StyleHandle(self, child_node_id.into())
    }
}

#[derive(Debug, Clone, Copy)]
struct StyleHandle<'a>(&'a SubDoc, NodeId);

macro_rules! common_style {
    [ $self:ident.$s:ident => $i:expr ] => {
         (*(*$self.0).0).$s.add($i as usize)
    };
}

macro_rules! view_style {
    [ $self:ident.$s:ident => $i:expr ] => {
         (*(*$self.0).0).$s.add($i as usize)
    };
}

impl Hash for GridNameType {
    fn hash<H: std::hash::Hasher>(&self, state: &mut H) {
        core::mem::discriminant(self).hash(state);
    }
}

impl Hash for GridName {
    fn hash<H: std::hash::Hasher>(&self, state: &mut H) {
        self.Id.hash(state);
        self.Type.hash(state);
    }
}

impl Default for GridName {
    fn default() -> Self {
        Self {
            Id: Default::default(),
            Type: GridNameType::Name,
        }
    }
}

impl Eq for GridName {}

impl CheapCloneStr for GridName {
    fn with_start(&self) -> Self {
        Self {
            Id: self.Id,
            Type: GridNameType::Start,
        }
    }

    fn with_end(&self) -> Self {
        Self {
            Id: self.Id,
            Type: GridNameType::End,
        }
    }
}

impl<'a> StyleHandle<'a> {
    #[inline(always)]
    pub fn style(&self) -> &StyleData {
        self.0.style_data(self.1)
    }
}

impl<'a> Deref for StyleHandle<'a> {
    type Target = StyleData;

    fn deref(&self) -> &Self::Target {
        self.0.style_data(self.1)
    }
}

macro_rules! c_overflow {
    ( $self:ident.$name:ident ) => {
        match $self.style().$name {
            com::Overflow::Visible => taffy::Overflow::Visible,
            com::Overflow::Clip => taffy::Overflow::Clip,
            com::Overflow::Hidden => taffy::Overflow::Hidden,
        }
    };
}

macro_rules! c_position {
    ( $self:ident.$name:ident ) => {
        match $self.style().$name {
            com::Position::Relative => taffy::Position::Relative,
            com::Position::Absolute => taffy::Position::Absolute,
        }
    };
}

macro_rules! c_length_percentage_auto {
    ( $self:expr => $name:ident ) => {
        concat_idents!(value_name = $name, Value {
            match $self.$name {
                com::LengthType::Fixed => {
                    taffy::LengthPercentageAuto::length($self.value_name)
                }
                com::LengthType::Percent => {
                    taffy::LengthPercentageAuto::percent($self.value_name)
                }
                com::LengthType::Auto => taffy::LengthPercentageAuto::auto(),
            }
        })
    };
}

macro_rules! c_dimension {
    ( $self:expr => $name:ident ) => {
        concat_idents!(value_name = $name, Value {
            match $self.$name {
                com::LengthType::Fixed => {
                    taffy::Dimension::length($self.value_name)
                }
                com::LengthType::Percent => {
                    taffy::Dimension::percent($self.value_name)
                }
                com::LengthType::Auto => taffy::Dimension::auto(),
            }
        })
    };
}

macro_rules! c_length_percentage {
    ( $self:expr => $name:ident ) => {
        concat_idents!(value_name = $name, Value {
            match $self.$name {
                com::LengthType::Fixed => {
                    taffy::LengthPercentage::length($self.value_name)
                }
                com::LengthType::Percent => {
                    taffy::LengthPercentage::percent($self.value_name)
                }
                com::LengthType::Auto => taffy::LengthPercentage::length(0.0),
            }
        })
    };
}

impl<'a> CoreStyle for StyleHandle<'a> {
    type CustomIdent = GridName;

    #[inline(always)]
    fn box_generation_mode(&self) -> taffy::BoxGenerationMode {
        match self.Visible {
            com::Visible::Remove => taffy::BoxGenerationMode::None,
            com::Visible::Visible | com::Visible::Hidden => taffy::BoxGenerationMode::Normal,
        }
    }

    #[inline(always)]
    fn is_block(&self) -> bool {
        false
    }

    #[inline(always)]
    fn is_compressible_replaced(&self) -> bool {
        false
    }

    #[inline(always)]
    fn box_sizing(&self) -> taffy::BoxSizing {
        match self.BoxSizing {
            com::BoxSizing::BorderBox => taffy::BoxSizing::BorderBox,
            com::BoxSizing::ContentBox => taffy::BoxSizing::ContentBox,
        }
    }

    #[inline(always)]
    fn overflow(&self) -> taffy::Point<taffy::Overflow> {
        Point {
            x: c_overflow!(self.OverflowX),
            y: c_overflow!(self.OverflowY),
        }
    }

    #[inline(always)]
    fn scrollbar_width(&self) -> f32 {
        self.ScrollBarSize
    }

    #[inline(always)]
    fn position(&self) -> taffy::Position {
        c_position!(self.Position)
    }

    #[inline(always)]
    fn inset(&self) -> taffy::Rect<taffy::LengthPercentageAuto> {
        taffy::Rect {
            left: c_length_percentage_auto!(self => InsertLeft),
            right: c_length_percentage_auto!(self => InsertRight),
            top: c_length_percentage_auto!(self => InsertTop),
            bottom: c_length_percentage_auto!(self => InsertBottom),
        }
    }

    #[inline(always)]
    fn size(&self) -> taffy::Size<taffy::Dimension> {
        taffy::Size {
            width: c_dimension!(self => Width),
            height: c_dimension!(self => Height),
        }
    }

    #[inline(always)]
    fn min_size(&self) -> taffy::Size<taffy::Dimension> {
        taffy::Size {
            width: c_dimension!(self => MinWidth),
            height: c_dimension!(self => MinHeight),
        }
    }

    #[inline(always)]
    fn max_size(&self) -> taffy::Size<taffy::Dimension> {
        taffy::Size {
            width: c_dimension!(self => MaxWidth),
            height: c_dimension!(self => MaxHeight),
        }
    }

    #[inline(always)]
    fn aspect_ratio(&self) -> Option<f32> {
        if self.HasAspectRatio {
            Some(self.AspectRatioValue)
        } else {
            None
        }
    }

    #[inline(always)]
    fn margin(&self) -> taffy::Rect<taffy::LengthPercentageAuto> {
        taffy::Rect {
            left: c_length_percentage_auto!(self => MarginLeft),
            right: c_length_percentage_auto!(self => MarginRight),
            top: c_length_percentage_auto!(self => MarginTop),
            bottom: c_length_percentage_auto!(self => MarginBottom),
        }
    }

    #[inline(always)]
    fn padding(&self) -> taffy::Rect<taffy::LengthPercentage> {
        taffy::Rect {
            left: c_length_percentage!(self => PaddingLeft),
            right: c_length_percentage!(self => PaddingRight),
            top: c_length_percentage!(self => PaddingTop),
            bottom: c_length_percentage!(self => PaddingBottom),
        }
    }

    #[inline(always)]
    fn border(&self) -> taffy::Rect<taffy::LengthPercentage> {
        taffy::Rect {
            left: c_length_percentage!(self => BorderLeft),
            right: c_length_percentage!(self => BorderRight),
            top: c_length_percentage!(self => BorderTop),
            bottom: c_length_percentage!(self => BorderBottom),
        }
    }
}

impl<'a> BlockContainerStyle for StyleHandle<'a> {
    #[inline(always)]
    fn text_align(&self) -> taffy::TextAlign {
        match self.TextAlign {
            com::TextAlign::Start => taffy::TextAlign::LegacyLeft,
            com::TextAlign::End => taffy::TextAlign::LegacyRight,
            com::TextAlign::Center => taffy::TextAlign::LegacyCenter,
        }
    }
}

impl<'a> BlockItemStyle for StyleHandle<'a> {
    #[inline(always)]
    fn is_table(&self) -> bool {
        false
    }
}

impl<'a> FlexboxContainerStyle for StyleHandle<'a> {
    #[inline(always)]
    fn flex_direction(&self) -> taffy::FlexDirection {
        match self.FlexDirection {
            com::FlexDirection::Column => taffy::FlexDirection::Column,
            com::FlexDirection::Row => taffy::FlexDirection::Row,
            com::FlexDirection::ColumnReverse => taffy::FlexDirection::ColumnReverse,
            com::FlexDirection::RowReverse => taffy::FlexDirection::RowReverse,
        }
    }

    #[inline(always)]
    fn flex_wrap(&self) -> taffy::FlexWrap {
        match self.FlexWrap {
            com::FlexWrap::NoWrap => taffy::FlexWrap::NoWrap,
            com::FlexWrap::Wrap => taffy::FlexWrap::Wrap,
            com::FlexWrap::WrapReverse => taffy::FlexWrap::WrapReverse,
        }
    }

    #[inline(always)]
    fn gap(&self) -> taffy::Size<taffy::LengthPercentage> {
        taffy::Size {
            width: c_length_percentage!(self => GapX),
            height: c_length_percentage!(self => GapY),
        }
    }

    #[inline(always)]
    fn align_content(&self) -> Option<taffy::AlignContent> {
        match self.AlignContent {
            com::AlignType::None => None,
            com::AlignType::Start => Some(taffy::AlignContent::Start),
            com::AlignType::End => Some(taffy::AlignContent::End),
            com::AlignType::FlexStart => Some(taffy::AlignContent::FlexStart),
            com::AlignType::FlexEnd => Some(taffy::AlignContent::FlexEnd),
            com::AlignType::Center => Some(taffy::AlignContent::Center),
            com::AlignType::Baseline => None,
            com::AlignType::Stretch => Some(taffy::AlignContent::Stretch),
            com::AlignType::SpaceBetween => Some(taffy::AlignContent::SpaceBetween),
            com::AlignType::SpaceEvenly => Some(taffy::AlignContent::SpaceEvenly),
            com::AlignType::SpaceAround => Some(taffy::AlignContent::SpaceAround),
        }
    }

    #[inline(always)]
    fn align_items(&self) -> Option<taffy::AlignItems> {
        match self.AlignItems {
            com::AlignType::None => None,
            com::AlignType::Start => Some(taffy::AlignItems::Start),
            com::AlignType::End => Some(taffy::AlignItems::End),
            com::AlignType::FlexStart => Some(taffy::AlignItems::FlexStart),
            com::AlignType::FlexEnd => Some(taffy::AlignItems::FlexEnd),
            com::AlignType::Center => Some(taffy::AlignItems::Center),
            com::AlignType::Baseline => Some(taffy::AlignItems::Baseline),
            com::AlignType::Stretch => Some(taffy::AlignItems::Stretch),
            com::AlignType::SpaceBetween => None,
            com::AlignType::SpaceEvenly => None,
            com::AlignType::SpaceAround => None,
        }
    }

    #[inline(always)]
    fn justify_content(&self) -> Option<taffy::JustifyContent> {
        match self.JustifyContent {
            com::AlignType::None => None,
            com::AlignType::Start => Some(taffy::JustifyContent::Start),
            com::AlignType::End => Some(taffy::JustifyContent::End),
            com::AlignType::FlexStart => Some(taffy::JustifyContent::FlexStart),
            com::AlignType::FlexEnd => Some(taffy::JustifyContent::FlexEnd),
            com::AlignType::Center => Some(taffy::JustifyContent::Center),
            com::AlignType::Baseline => None,
            com::AlignType::Stretch => Some(taffy::JustifyContent::Stretch),
            com::AlignType::SpaceBetween => Some(taffy::JustifyContent::SpaceBetween),
            com::AlignType::SpaceEvenly => Some(taffy::JustifyContent::SpaceEvenly),
            com::AlignType::SpaceAround => Some(taffy::JustifyContent::SpaceAround),
        }
    }
}

impl<'a> FlexboxItemStyle for StyleHandle<'a> {
    #[inline(always)]
    fn flex_basis(&self) -> taffy::Dimension {
        c_dimension!(self => FlexBasis)
    }

    #[inline(always)]
    fn flex_grow(&self) -> f32 {
        self.FlexGrow
    }

    #[inline(always)]
    fn flex_shrink(&self) -> f32 {
        self.FlexShrink
    }

    #[inline(always)]
    fn align_self(&self) -> Option<taffy::AlignSelf> {
        match self.AlignSelf {
            com::AlignType::None => None,
            com::AlignType::Start => Some(taffy::AlignSelf::Start),
            com::AlignType::End => Some(taffy::AlignSelf::End),
            com::AlignType::FlexStart => Some(taffy::AlignSelf::FlexStart),
            com::AlignType::FlexEnd => Some(taffy::AlignSelf::FlexEnd),
            com::AlignType::Center => Some(taffy::AlignSelf::Center),
            com::AlignType::Baseline => Some(taffy::AlignSelf::Baseline),
            com::AlignType::Stretch => Some(taffy::AlignSelf::Stretch),
            com::AlignType::SpaceBetween => None,
            com::AlignType::SpaceEvenly => None,
            com::AlignType::SpaceAround => None,
        }
    }
}

#[inline(always)]
fn to_taffy_track_sizing(item: &com::TrackSizingFunction) -> taffy::TrackSizingFunction {
    taffy::TrackSizingFunction {
        min: match item.Min {
            com::SizingType::Auto => taffy::MinTrackSizingFunction::auto(),
            com::SizingType::Fixed => taffy::MinTrackSizingFunction::length(item.MinValue.Value),
            com::SizingType::Percent => taffy::MinTrackSizingFunction::percent(item.MinValue.Value),
            com::SizingType::Fraction => taffy::MinTrackSizingFunction::auto(),
            com::SizingType::MinContent => taffy::MinTrackSizingFunction::min_content(),
            com::SizingType::MaxContent => taffy::MinTrackSizingFunction::max_content(),
            com::SizingType::FitContent => taffy::MinTrackSizingFunction::auto(),
        },
        max: match item.Max {
            com::SizingType::Auto => taffy::MaxTrackSizingFunction::auto(),
            com::SizingType::Fixed => taffy::MaxTrackSizingFunction::length(item.MaxValue.Value),
            com::SizingType::Percent => taffy::MaxTrackSizingFunction::percent(item.MaxValue.Value),
            com::SizingType::Fraction => taffy::MaxTrackSizingFunction::fr(item.MaxValue.Value),
            com::SizingType::MinContent => taffy::MaxTrackSizingFunction::min_content(),
            com::SizingType::MaxContent => taffy::MaxTrackSizingFunction::max_content(),
            com::SizingType::FitContent => match item.MaxValue.Type {
                com::LengthType::Fixed => {
                    taffy::MaxTrackSizingFunction::fit_content_px(item.MaxValue.Value)
                }
                com::LengthType::Percent => {
                    taffy::MaxTrackSizingFunction::fit_content_percent(item.MaxValue.Value)
                }
                com::LengthType::Auto => taffy::MaxTrackSizingFunction::fit_content_px(1.0),
            },
        },
    }
}

#[derive(Debug, Clone)]
pub struct TrackSizingFunctionIter<'a>(&'a com::NativeList<com::TrackSizingFunction>, i32);

impl<'a> Iterator for TrackSizingFunctionIter<'a> {
    type Item = taffy::TrackSizingFunction;

    #[inline(always)]
    fn next(&mut self) -> Option<Self::Item> {
        if self.1 < self.0.m_size {
            let item = unsafe { &*self.0.m_items.add(self.1 as usize) };
            self.1 += 1;
            Some(to_taffy_track_sizing(item))
        } else {
            None
        }
    }

    #[inline(always)]
    fn size_hint(&self) -> (usize, Option<usize>) {
        let size = (self.0.m_size - self.1) as usize;
        (size, Some(size))
    }
}

impl<'a> ExactSizeIterator for TrackSizingFunctionIter<'a> {}

#[derive(Debug, Clone)]
pub struct TemplateLineNamesIter<'a>(&'a com::NativeList<com::NativeList<GridName>>, i32);

impl<'a> taffy::TemplateLineNames<'a, GridName> for TemplateLineNamesIter<'a> {
    type LineNameSet<'b>
        = LineNameSetIter<'b>
    where
        Self: 'b;
}

impl<'a> Iterator for TemplateLineNamesIter<'a> {
    type Item = LineNameSetIter<'a>;

    #[inline(always)]
    fn next(&mut self) -> Option<Self::Item> {
        if self.1 < self.0.m_size {
            let item = unsafe { &*self.0.m_items.add(self.1 as usize) };
            self.1 += 1;
            Some(LineNameSetIter(item, 0))
        } else {
            None
        }
    }

    #[inline(always)]
    fn size_hint(&self) -> (usize, Option<usize>) {
        let size = (self.0.m_size - self.1) as usize;
        (size, Some(size))
    }
}

impl<'a> ExactSizeIterator for TemplateLineNamesIter<'a> {}

#[derive(Debug, Clone)]
pub struct LineNameSetIter<'a>(&'a com::NativeList<GridName>, i32);

impl<'a> Iterator for LineNameSetIter<'a> {
    type Item = &'a GridName;

    #[inline(always)]
    fn next(&mut self) -> Option<Self::Item> {
        if self.1 < self.0.m_size {
            let item = unsafe { &*self.0.m_items.add(self.1 as usize) };
            self.1 += 1;
            Some(item)
        } else {
            None
        }
    }

    #[inline(always)]
    fn size_hint(&self) -> (usize, Option<usize>) {
        let size = (self.0.m_size - self.1) as usize;
        (size, Some(size))
    }
}

impl<'a> ExactSizeIterator for LineNameSetIter<'a> {}

impl GenericRepetition for com::GridTemplateRepetition {
    type CustomIdent = GridName;

    type RepetitionTrackList<'a>
        = TrackSizingFunctionIter<'a>
    where
        Self: 'a;

    type TemplateLineNames<'a>
        = TemplateLineNamesIter<'a>
    where
        Self: 'a;

    #[inline(always)]
    fn count(&self) -> taffy::RepetitionCount {
        match self.Repetition {
            com::RepetitionType::Count => taffy::RepetitionCount::Count(self.RepetitionValue),
            com::RepetitionType::AutoFill => taffy::RepetitionCount::AutoFill,
            com::RepetitionType::AutoFit => taffy::RepetitionCount::AutoFit,
        }
    }

    #[inline(always)]
    fn track_count(&self) -> u16 {
        self.Tracks.m_size as u16
    }

    #[inline(always)]
    fn tracks(&self) -> Self::RepetitionTrackList<'_> {
        TrackSizingFunctionIter(&self.Tracks, 0)
    }

    #[inline(always)]
    fn lines_names(&self) -> Self::TemplateLineNames<'_> {
        TemplateLineNamesIter(&self.LineIds, 0)
    }
}

impl<'s> GenericRepetition for &'s com::GridTemplateRepetition {
    type CustomIdent = GridName;

    type RepetitionTrackList<'a>
        = TrackSizingFunctionIter<'a>
    where
        Self: 'a;

    type TemplateLineNames<'a>
        = TemplateLineNamesIter<'a>
    where
        Self: 'a;

    #[inline(always)]
    fn count(&self) -> taffy::RepetitionCount {
        match self.Repetition {
            com::RepetitionType::Count => taffy::RepetitionCount::Count(self.RepetitionValue),
            com::RepetitionType::AutoFill => taffy::RepetitionCount::AutoFill,
            com::RepetitionType::AutoFit => taffy::RepetitionCount::AutoFit,
        }
    }

    #[inline(always)]
    fn track_count(&self) -> u16 {
        self.Tracks.m_size as u16
    }

    #[inline(always)]
    fn tracks(&self) -> Self::RepetitionTrackList<'_> {
        TrackSizingFunctionIter(&self.Tracks, 0)
    }

    #[inline(always)]
    fn lines_names(&self) -> Self::TemplateLineNames<'_> {
        TemplateLineNamesIter(&self.LineIds, 0)
    }
}

impl com::GridTemplateComponent {
    #[inline(always)]
    fn to_taffy(
        &self,
    ) -> taffy::GenericGridTemplateComponent<GridName, &'_ com::GridTemplateRepetition> {
        match self.Type {
            com::GridTemplateComponentType::Single => {
                let item = unsafe { &self.Union.Single };
                taffy::GenericGridTemplateComponent::Single(to_taffy_track_sizing(item))
            }
            com::GridTemplateComponentType::Repeat => {
                let item = unsafe { &self.Union.Repeat };
                taffy::GenericGridTemplateComponent::Repeat(item)
            }
        }
    }
}

#[derive(Debug, Clone)]
pub struct TemplateTrackListIter<'a>(&'a com::NativeList<com::GridTemplateComponent>, i32);

impl<'a> Iterator for TemplateTrackListIter<'a> {
    type Item = taffy::GenericGridTemplateComponent<GridName, &'a com::GridTemplateRepetition>;

    #[inline(always)]
    fn next(&mut self) -> Option<Self::Item> {
        if self.1 < self.0.m_size {
            let item = unsafe { &*self.0.m_items.add(self.1 as usize) };
            self.1 += 1;
            Some(item.to_taffy())
        } else {
            None
        }
    }

    #[inline(always)]
    fn size_hint(&self) -> (usize, Option<usize>) {
        let size = (self.0.m_size - self.1) as usize;
        (size, Some(size))
    }
}

impl<'a> ExactSizeIterator for TemplateTrackListIter<'a> {}

#[derive(Debug, Clone)]
pub struct AutoTrackListIter<'a>(&'a com::NativeList<com::TrackSizingFunction>, i32);

impl<'a> AutoTrackListIter<'a> {
    pub fn empty() -> Self {
        static EMPTY: com::NativeList<com::TrackSizingFunction> = com::NativeList {
            m_items: std::ptr::null_mut(),
            m_cap: 0,
            m_size: 0,
        };
        Self(unsafe { &EMPTY }, 0)
    }
}

impl<'a> Iterator for AutoTrackListIter<'a> {
    type Item = taffy::TrackSizingFunction;

    #[inline(always)]
    fn next(&mut self) -> Option<Self::Item> {
        if self.1 < self.0.m_size {
            let item = unsafe { &*self.0.m_items.add(self.1 as usize) };
            self.1 += 1;
            Some(to_taffy_track_sizing(item))
        } else {
            None
        }
    }

    #[inline(always)]
    fn size_hint(&self) -> (usize, Option<usize>) {
        let size = (self.0.m_size - self.1) as usize;
        (size, Some(size))
    }
}

impl<'a> ExactSizeIterator for AutoTrackListIter<'a> {}

#[derive(Debug, Clone)]
pub struct GridTemplateAreasIter<'a>(&'a com::NativeList<com::GridTemplateArea>, i32);

impl<'a> Iterator for GridTemplateAreasIter<'a> {
    type Item = taffy::GridTemplateArea<GridName>;

    #[inline(always)]
    fn next(&mut self) -> Option<Self::Item> {
        if self.1 < self.0.m_size {
            let item = unsafe { &*self.0.m_items.add(self.1 as usize) };
            self.1 += 1;
            Some(taffy::GridTemplateArea {
                name: item.Id,
                row_start: item.RowStart,
                row_end: item.RowEnd,
                column_start: item.ColumnStart,
                column_end: item.ColumnEnd,
            })
        } else {
            None
        }
    }

    #[inline(always)]
    fn size_hint(&self) -> (usize, Option<usize>) {
        let size = (self.0.m_size - self.1) as usize;
        (size, Some(size))
    }
}

impl<'a> ExactSizeIterator for GridTemplateAreasIter<'a> {}

impl<'s> GridContainerStyle for StyleHandle<'s> {
    type Repetition<'a>
        = &'a com::GridTemplateRepetition
    where
        Self: 'a;

    type TemplateTrackList<'a>
        = TemplateTrackListIter<'a>
    where
        Self: 'a;

    type AutoTrackList<'a>
        = AutoTrackListIter<'a>
    where
        Self: 'a;

    type TemplateLineNames<'a>
        = TemplateLineNamesIter<'a>
    where
        Self: 'a;

    type GridTemplateAreas<'a>
        = GridTemplateAreasIter<'a>
    where
        Self: 'a;

    #[inline(always)]
    fn grid_template_rows(&self) -> Option<Self::TemplateTrackList<'_>> {
        Some(TemplateTrackListIter(&self.Grid.val()?.GridTemplateRows, 0))
    }

    #[inline(always)]
    fn grid_template_columns(&self) -> Option<Self::TemplateTrackList<'_>> {
        Some(TemplateTrackListIter(
            &self.Grid.val()?.GridTemplateColumns,
            0,
        ))
    }

    #[inline(always)]
    fn grid_auto_rows(&self) -> Self::AutoTrackList<'_> {
        if let Some(grid) = self.Grid.val() {
            AutoTrackListIter(&grid.GridAutoRows, 0)
        } else {
            AutoTrackListIter::empty()
        }
    }

    #[inline(always)]
    fn grid_auto_columns(&self) -> Self::AutoTrackList<'_> {
        if let Some(grid) = self.Grid.val() {
            AutoTrackListIter(&grid.GridAutoColumns, 0)
        } else {
            AutoTrackListIter::empty()
        }
    }

    #[inline(always)]
    fn grid_template_areas(&self) -> Option<Self::GridTemplateAreas<'_>> {
        Some(GridTemplateAreasIter(
            &self.Grid.val()?.GridTemplateAreas,
            0,
        ))
    }

    #[inline(always)]
    fn grid_template_column_names(&self) -> Option<Self::TemplateLineNames<'_>> {
        Some(TemplateLineNamesIter(
            &self.Grid.val()?.GridTemplateColumnNames,
            0,
        ))
    }

    #[inline(always)]
    fn grid_template_row_names(&self) -> Option<Self::TemplateLineNames<'_>> {
        Some(TemplateLineNamesIter(
            &self.Grid.val()?.GridTemplateRowNames,
            0,
        ))
    }

    #[inline(always)]
    fn grid_auto_flow(&self) -> taffy::GridAutoFlow {
        match self.GridAutoFlow {
            com::GridAutoFlow::Row => taffy::GridAutoFlow::Row,
            com::GridAutoFlow::Column => taffy::GridAutoFlow::Column,
            com::GridAutoFlow::RowDense => taffy::GridAutoFlow::RowDense,
            com::GridAutoFlow::ColumnDense => taffy::GridAutoFlow::ColumnDense,
        }
    }

    #[inline(always)]
    fn gap(&self) -> taffy::Size<taffy::LengthPercentage> {
        taffy::Size {
            width: c_length_percentage!(self => GapX),
            height: c_length_percentage!(self => GapY),
        }
    }

    #[inline(always)]
    fn align_content(&self) -> Option<taffy::AlignContent> {
        match self.AlignContent {
            com::AlignType::None => None,
            com::AlignType::Start => Some(taffy::AlignContent::Start),
            com::AlignType::End => Some(taffy::AlignContent::End),
            com::AlignType::FlexStart => Some(taffy::AlignContent::FlexStart),
            com::AlignType::FlexEnd => Some(taffy::AlignContent::FlexEnd),
            com::AlignType::Center => Some(taffy::AlignContent::Center),
            com::AlignType::Baseline => None,
            com::AlignType::Stretch => Some(taffy::AlignContent::Stretch),
            com::AlignType::SpaceBetween => Some(taffy::AlignContent::SpaceBetween),
            com::AlignType::SpaceEvenly => Some(taffy::AlignContent::SpaceEvenly),
            com::AlignType::SpaceAround => Some(taffy::AlignContent::SpaceAround),
        }
    }

    #[inline(always)]
    fn justify_content(&self) -> Option<taffy::JustifyContent> {
        match self.JustifyContent {
            com::AlignType::None => None,
            com::AlignType::Start => Some(taffy::JustifyContent::Start),
            com::AlignType::End => Some(taffy::JustifyContent::End),
            com::AlignType::FlexStart => Some(taffy::JustifyContent::FlexStart),
            com::AlignType::FlexEnd => Some(taffy::JustifyContent::FlexEnd),
            com::AlignType::Center => Some(taffy::JustifyContent::Center),
            com::AlignType::Baseline => None,
            com::AlignType::Stretch => Some(taffy::JustifyContent::Stretch),
            com::AlignType::SpaceBetween => Some(taffy::JustifyContent::SpaceBetween),
            com::AlignType::SpaceEvenly => Some(taffy::JustifyContent::SpaceEvenly),
            com::AlignType::SpaceAround => Some(taffy::JustifyContent::SpaceAround),
        }
    }

    #[inline(always)]
    fn align_items(&self) -> Option<taffy::AlignItems> {
        match self.AlignItems {
            com::AlignType::None => None,
            com::AlignType::Start => Some(taffy::AlignItems::Start),
            com::AlignType::End => Some(taffy::AlignItems::End),
            com::AlignType::FlexStart => Some(taffy::AlignItems::FlexStart),
            com::AlignType::FlexEnd => Some(taffy::AlignItems::FlexEnd),
            com::AlignType::Center => Some(taffy::AlignItems::Center),
            com::AlignType::Baseline => Some(taffy::AlignItems::Baseline),
            com::AlignType::Stretch => Some(taffy::AlignItems::Stretch),
            com::AlignType::SpaceBetween => None,
            com::AlignType::SpaceEvenly => None,
            com::AlignType::SpaceAround => None,
        }
    }

    #[inline(always)]
    fn justify_items(&self) -> Option<taffy::AlignItems> {
        match self.JustifyItems {
            com::AlignType::None => None,
            com::AlignType::Start => Some(taffy::AlignItems::Start),
            com::AlignType::End => Some(taffy::AlignItems::End),
            com::AlignType::FlexStart => Some(taffy::AlignItems::FlexStart),
            com::AlignType::FlexEnd => Some(taffy::AlignItems::FlexEnd),
            com::AlignType::Center => Some(taffy::AlignItems::Center),
            com::AlignType::Baseline => Some(taffy::AlignItems::Baseline),
            com::AlignType::Stretch => Some(taffy::AlignItems::Stretch),
            com::AlignType::SpaceBetween => None,
            com::AlignType::SpaceEvenly => None,
            com::AlignType::SpaceAround => None,
        }
    }
}

impl com::GridPlacement {
    #[inline(always)]
    pub fn to_taffy(&self) -> taffy::GridPlacement<GridName> {
        match self.Type {
            com::GridPlacementType::Auto => taffy::GridPlacement::Auto,
            com::GridPlacementType::Line => taffy::GridPlacement::Line(self.Value1.into()),
            com::GridPlacementType::NamedLine => taffy::GridPlacement::NamedLine(
                GridName {
                    Id: self.Name,
                    Type: self.NameType,
                },
                self.Value1,
            ),
            com::GridPlacementType::Span => taffy::GridPlacement::Span(self.Value1 as u16),
            com::GridPlacementType::NamedSpan => taffy::GridPlacement::NamedSpan(
                GridName {
                    Id: self.Name,
                    Type: self.NameType,
                },
                self.Value1 as u16,
            ),
        }
    }
}

impl<'a> GridItemStyle for StyleHandle<'a> {
    #[inline(always)]
    fn grid_row(&self) -> taffy::Line<taffy::GridPlacement<Self::CustomIdent>> {
        taffy::Line {
            start: self.GridRowStart.to_taffy(),
            end: self.GridRowEnd.to_taffy(),
        }
    }

    #[inline(always)]
    fn grid_column(&self) -> taffy::Line<taffy::GridPlacement<Self::CustomIdent>> {
        taffy::Line {
            start: self.GridColumnStart.to_taffy(),
            end: self.GridColumnEnd.to_taffy(),
        }
    }

    #[inline(always)]
    fn align_self(&self) -> Option<taffy::AlignSelf> {
        match self.AlignSelf {
            com::AlignType::None => None,
            com::AlignType::Start => Some(taffy::AlignSelf::Start),
            com::AlignType::End => Some(taffy::AlignSelf::End),
            com::AlignType::FlexStart => Some(taffy::AlignSelf::FlexStart),
            com::AlignType::FlexEnd => Some(taffy::AlignSelf::FlexEnd),
            com::AlignType::Center => Some(taffy::AlignSelf::Center),
            com::AlignType::Baseline => Some(taffy::AlignSelf::Baseline),
            com::AlignType::Stretch => Some(taffy::AlignSelf::Stretch),
            com::AlignType::SpaceBetween => None,
            com::AlignType::SpaceEvenly => None,
            com::AlignType::SpaceAround => None,
        }
    }

    #[inline(always)]
    fn justify_self(&self) -> Option<taffy::AlignSelf> {
        match self.JustifySelf {
            com::AlignType::None => None,
            com::AlignType::Start => Some(taffy::AlignSelf::Start),
            com::AlignType::End => Some(taffy::AlignSelf::End),
            com::AlignType::FlexStart => Some(taffy::AlignSelf::FlexStart),
            com::AlignType::FlexEnd => Some(taffy::AlignSelf::FlexEnd),
            com::AlignType::Center => Some(taffy::AlignSelf::Center),
            com::AlignType::Baseline => Some(taffy::AlignSelf::Baseline),
            com::AlignType::Stretch => Some(taffy::AlignSelf::Stretch),
            com::AlignType::SpaceBetween => None,
            com::AlignType::SpaceEvenly => None,
            com::AlignType::SpaceAround => None,
        }
    }
}

impl SubDoc {
    #[inline(always)]
    pub fn compute_text_layout(
        &mut self,
        id: NodeId,
        inputs: taffy::LayoutInput,
    ) -> taffy::LayoutOutput {
        unsafe { (*self.2).compute_text_layout(self, id, inputs) }
    }
}

#[repr(u8)]
#[derive(Debug, Clone, Copy)]
enum CopltLayoutRunMode {
    PerformLayout,
    ComputeSize,
    PerformHiddenLayout,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy)]
enum CopltLayoutSizingMode {
    ContentSize,
    InherentSize,
}

#[repr(u8)]
#[derive(Debug, Clone, Copy)]
enum CopltLayoutRequestedAxis {
    Horizontal,
    Vertical,
    Both,
}

#[repr(C)]
#[derive(Debug, Clone, Copy)]
#[allow(non_snake_case)]
struct CopltLayoutInputs {
    KnownWidth: f32,
    KnownHeight: f32,
    ParentWidth: f32,
    ParentHeight: f32,
    AvailableSpaceWidthValue: f32,
    AvailableSpaceHeightValue: f32,
    HasKnownWidth: bool,
    HasKnownHeight: bool,
    HasParentWidth: bool,
    HasParentHeight: bool,
    AvailableSpaceWidth: com::AvailableSpaceType,
    AvailableSpaceHeight: com::AvailableSpaceType,
    RunMode: CopltLayoutRunMode,
    SizingMode: CopltLayoutSizingMode,
    Axis: CopltLayoutRequestedAxis,
}

impl From<LayoutInput> for CopltLayoutInputs {
    fn from(value: LayoutInput) -> Self {
        Self {
            KnownWidth: value.known_dimensions.width.unwrap_or_default(),
            KnownHeight: value.known_dimensions.height.unwrap_or_default(),
            ParentWidth: value.parent_size.width.unwrap_or_default(),
            ParentHeight: value.parent_size.height.unwrap_or_default(),
            AvailableSpaceWidthValue: value.available_space.width.unwrap_or(0.0),
            AvailableSpaceHeightValue: value.available_space.height.unwrap_or(0.0),
            HasKnownWidth: value.known_dimensions.width.is_some(),
            HasKnownHeight: value.known_dimensions.height.is_some(),
            HasParentWidth: value.parent_size.width.is_some(),
            HasParentHeight: value.parent_size.height.is_some(),
            AvailableSpaceWidth: match value.available_space.width {
                taffy::AvailableSpace::Definite(_) => com::AvailableSpaceType::Definite,
                taffy::AvailableSpace::MinContent => com::AvailableSpaceType::MinContent,
                taffy::AvailableSpace::MaxContent => com::AvailableSpaceType::MaxContent,
            },
            AvailableSpaceHeight: match value.available_space.height {
                taffy::AvailableSpace::Definite(_) => com::AvailableSpaceType::Definite,
                taffy::AvailableSpace::MinContent => com::AvailableSpaceType::MinContent,
                taffy::AvailableSpace::MaxContent => com::AvailableSpaceType::MaxContent,
            },
            RunMode: match value.run_mode {
                RunMode::PerformLayout => CopltLayoutRunMode::PerformLayout,
                RunMode::ComputeSize => CopltLayoutRunMode::ComputeSize,
                RunMode::PerformHiddenLayout => CopltLayoutRunMode::PerformHiddenLayout,
            },
            SizingMode: match value.sizing_mode {
                taffy::SizingMode::ContentSize => CopltLayoutSizingMode::ContentSize,
                taffy::SizingMode::InherentSize => CopltLayoutSizingMode::InherentSize,
            },
            Axis: match value.axis {
                taffy::RequestedAxis::Horizontal => CopltLayoutRequestedAxis::Horizontal,
                taffy::RequestedAxis::Vertical => CopltLayoutRequestedAxis::Vertical,
                taffy::RequestedAxis::Both => CopltLayoutRequestedAxis::Both,
            },
        }
    }
}

impl Into<LayoutInput> for CopltLayoutInputs {
    fn into(self) -> LayoutInput {
        LayoutInput {
            run_mode: match self.RunMode {
                CopltLayoutRunMode::PerformLayout => taffy::RunMode::PerformLayout,
                CopltLayoutRunMode::ComputeSize => taffy::RunMode::ComputeSize,
                CopltLayoutRunMode::PerformHiddenLayout => taffy::RunMode::PerformHiddenLayout,
            },
            sizing_mode: match self.SizingMode {
                CopltLayoutSizingMode::ContentSize => taffy::SizingMode::ContentSize,
                CopltLayoutSizingMode::InherentSize => taffy::SizingMode::InherentSize,
            },
            axis: match self.Axis {
                CopltLayoutRequestedAxis::Horizontal => taffy::RequestedAxis::Horizontal,
                CopltLayoutRequestedAxis::Vertical => taffy::RequestedAxis::Vertical,
                CopltLayoutRequestedAxis::Both => taffy::RequestedAxis::Both,
            },
            known_dimensions: Size {
                width: if self.HasKnownWidth {
                    Some(self.KnownWidth)
                } else {
                    None
                },
                height: if self.HasKnownHeight {
                    Some(self.KnownHeight)
                } else {
                    None
                },
            },
            parent_size: Size {
                width: if self.HasParentWidth {
                    Some(self.ParentWidth)
                } else {
                    None
                },
                height: if self.HasParentHeight {
                    Some(self.ParentHeight)
                } else {
                    None
                },
            },
            available_space: Size {
                width: match self.AvailableSpaceWidth {
                    com::AvailableSpaceType::Definite => {
                        taffy::AvailableSpace::Definite(self.AvailableSpaceWidthValue)
                    }
                    com::AvailableSpaceType::MinContent => taffy::AvailableSpace::MinContent,
                    com::AvailableSpaceType::MaxContent => taffy::AvailableSpace::MaxContent,
                },
                height: match self.AvailableSpaceHeight {
                    com::AvailableSpaceType::Definite => {
                        taffy::AvailableSpace::Definite(self.AvailableSpaceHeightValue)
                    }
                    com::AvailableSpaceType::MinContent => taffy::AvailableSpace::MinContent,
                    com::AvailableSpaceType::MaxContent => taffy::AvailableSpace::MaxContent,
                },
            },
            vertical_margins_are_collapsible: taffy::Line::FALSE,
        }
    }
}

impl Into<LayoutOutput> for com::LayoutOutput {
    fn into(self) -> LayoutOutput {
        LayoutOutput {
            size: Size {
                width: self.Width,
                height: self.Height,
            },
            content_size: Size {
                width: self.ContentWidth,
                height: self.ContentHeight,
            },
            first_baselines: Point {
                x: if self.HasFirstBaselinesX {
                    Some(self.FirstBaselinesX)
                } else {
                    None
                },
                y: if self.HasFirstBaselinesY {
                    Some(self.FirstBaselinesY)
                } else {
                    None
                },
            },
            top_margin: CollapsibleMarginSet {
                positive: self.TopMargin.Positive,
                negative: self.TopMargin.Negative,
            },
            bottom_margin: CollapsibleMarginSet {
                positive: self.TopMargin.Positive,
                negative: self.TopMargin.Negative,
            },
            margins_can_collapse_through: self.MarginsCanCollapseThrough,
        }
    }
}

impl From<LayoutOutput> for com::LayoutOutput {
    fn from(value: LayoutOutput) -> Self {
        Self {
            Width: value.size.width,
            Height: value.size.height,
            ContentWidth: value.content_size.width,
            ContentHeight: value.content_size.height,
            FirstBaselinesX: value.first_baselines.x.unwrap_or_default(),
            FirstBaselinesY: value.first_baselines.y.unwrap_or_default(),
            TopMargin: com::LayoutCollapsibleMarginSet {
                Positive: value.top_margin.positive,
                Negative: value.top_margin.negative,
            },
            BottomMargin: com::LayoutCollapsibleMarginSet {
                Positive: value.bottom_margin.positive,
                Negative: value.bottom_margin.negative,
            },
            HasFirstBaselinesX: value.first_baselines.x.is_some(),
            HasFirstBaselinesY: value.first_baselines.y.is_some(),
            MarginsCanCollapseThrough: value.margins_can_collapse_through,
        }
    }
}
