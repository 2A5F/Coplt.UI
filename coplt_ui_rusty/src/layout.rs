use std::hint::unreachable_unchecked;

use cocom::{HResult, HResultE};
use concat_idents::concat_idents;
use taffy::{
    BlockContainerStyle, BlockItemStyle, Cache, CacheTree, CoreStyle, FlexboxContainerStyle,
    FlexboxItemStyle, GridContainerStyle, LayoutBlockContainer, LayoutFlexboxContainer,
    LayoutOutput, LayoutPartialTree, Point, RoundTree, TraversePartialTree, TraverseTree,
    prelude::TaffyZero,
};

use crate::{
    col::{OrderedSet, ordered_set},
    com::{
        self, CommonLayoutData, CommonStyleData, ContainerStyleData, NLayoutContext, NodeLocate,
        RootData,
    },
};

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

macro_rules! c_available_space {
    ( $self:ident.$name:ident ) => {
        concat_idents!(value_name = $name, Value {
            match $self.$name {
                com::AvailableSpaceType::Definite => {
                    taffy::AvailableSpace::Definite($self.value_name)
                }
                com::AvailableSpaceType::MinContent => taffy::AvailableSpace::MinContent,
                com::AvailableSpaceType::MaxContent => taffy::AvailableSpace::MaxContent,
            }
        })
    };
    ( $self:expr => $name:ident ) => {
        concat_idents!(value_name = $name, Value {
            match $self.$name {
                com::AvailableSpaceType::Definite => {
                    taffy::AvailableSpace::Definite($self.value_name)
                }
                com::AvailableSpaceType::MinContent => taffy::AvailableSpace::MinContent,
                com::AvailableSpaceType::MaxContent => taffy::AvailableSpace::MaxContent,
            }
        })
    };
}

#[unsafe(no_mangle)]
pub extern "C" fn coplt_ui_layout_calc(ctx: *mut NLayoutContext) -> HResult {
    unsafe {
        for root_index in (*ctx).roots() {
            let mut sub_doc = SubDoc(ctx, *root_index);
            let root_data = *sub_doc.root_data();
            let available_space = taffy::Size {
                width: c_available_space!(root_data.AvailableSpaceX),
                height: c_available_space!(root_data.AvailableSpaceY),
            };
            let root_id = NodeId::new(*root_index, NodeType::Root).into();
            taffy::compute_root_layout(&mut sub_doc, root_id, available_space);
            if root_data.UseRounding {
                taffy::round_layout(&mut sub_doc, root_id);
            }
        }

        HResultE::Ok.into()
    }
}

impl NLayoutContext {
    #[inline(always)]
    pub fn roots(&self) -> &mut [i32] {
        unsafe { std::slice::from_raw_parts_mut(self.roots, self.root_count as usize) }
    }
}

#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
enum NodeType {
    View = 0,
    Text = 1,
    Root = 2,
}

impl From<u8> for NodeType {
    #[inline(always)]
    fn from(value: u8) -> Self {
        match value {
            0 => Self::View,
            1 => Self::Text,
            2 => Self::Root,
            _ => unreachable!(),
        }
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
struct NodeId(i32, NodeType);

impl NodeId {
    #[inline(always)]
    pub fn new(index: i32, typ: NodeType) -> Self {
        Self(index, typ)
    }

    #[inline(always)]
    pub fn index(&self) -> i32 {
        self.0
    }

    #[inline(always)]
    pub fn typ(&self) -> NodeType {
        self.1
    }
}

impl Into<taffy::NodeId> for NodeId {
    #[inline(always)]
    fn into(self) -> taffy::NodeId {
        taffy::NodeId::new(self.0 as u64 | (self.1 as u64) << 32)
    }
}

impl From<taffy::NodeId> for NodeId {
    #[inline(always)]
    fn from(value: taffy::NodeId) -> Self {
        let u = u64::from(value);
        let index = (u & 0xFF_FF_FF_FF) as i32;
        let typ = ((u >> 32) & 0xFF_FF_FF_FF) as i32;
        Self::new(index, (typ as u8).into())
    }
}

#[derive(Debug)]
struct SubDoc(*mut NLayoutContext, i32);

macro_rules! childs_data {
    [ $self:ident.$s:ident => $i:expr ] => {
         &mut (*((*$self.0).$s.add($i as usize))).m_childs as *mut _ as *mut OrderedSet<NodeLocate>
    };
}

macro_rules! common_layout {
    [ $self:ident.$s:ident => $i:expr ] => {
         (*$self.0).$s.add($i as usize)
    };
}

impl SubDoc {
    #[inline(always)]
    pub fn root_data(&self) -> &RootData {
        unsafe { &*(*self.0).root_root_data.add(self.1 as usize) }
    }

    #[inline(always)]
    pub fn get_childs(&self, id: NodeId) -> &OrderedSet<NodeLocate> {
        match id.typ() {
            NodeType::View => unsafe { &*childs_data![self.view_childs_data => id.index()] },
            NodeType::Text => unreachable!(),
            NodeType::Root => unsafe { &*childs_data![self.root_childs_data => id.index()] },
        }
    }

    #[inline(always)]
    pub fn common_layout(&self, id: NodeId) -> &CommonLayoutData {
        match id.typ() {
            NodeType::View => unsafe { &*common_layout![self.view_layout_data => id.index()] },
            NodeType::Text => unsafe { &*common_layout![self.text_layout_data => id.index()] },
            NodeType::Root => unsafe { &*common_layout![self.root_layout_data => id.index()] },
        }
    }

    #[inline(always)]
    pub fn common_layout_mut(&mut self, id: NodeId) -> &mut CommonLayoutData {
        match id.typ() {
            NodeType::View => unsafe { &mut *common_layout![self.view_layout_data => id.index()] },
            NodeType::Text => unsafe { &mut *common_layout![self.text_layout_data => id.index()] },
            NodeType::Root => unsafe { &mut *common_layout![self.root_layout_data => id.index()] },
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

struct ChildIter(ordered_set::PtrCopyIter<NodeLocate>);

impl Iterator for ChildIter {
    type Item = taffy::NodeId;

    #[inline(always)]
    fn next(&mut self) -> Option<Self::Item> {
        match self.0.next() {
            None => None,
            Some(a) => {
                let id = NodeId::new(a.Index, ((a.Id.VersionAndType & 0b1111) as u8).into());
                Some(id.into())
            }
        }
    }
}

impl TraversePartialTree for SubDoc {
    type ChildIter = ChildIter;

    #[inline(always)]
    fn child_ids(&self, parent_node_id: taffy::NodeId) -> Self::ChildIter {
        let id = NodeId::from(parent_node_id);
        let childs = self.get_childs(id);
        ChildIter(childs.iter_ptr_copy())
    }

    #[inline(always)]
    fn child_count(&self, parent_node_id: taffy::NodeId) -> usize {
        let id = NodeId::from(parent_node_id);
        let childs = self.get_childs(id);
        childs.count() as usize
    }
}

impl TraverseTree for SubDoc {}

impl LayoutPartialTree for SubDoc {
    type CoreContainerStyle<'a>
        = StyleHandle<'a>
    where
        Self: 'a;

    type CustomIdent = String;

    #[inline(always)]
    fn get_core_container_style(&self, node_id: taffy::NodeId) -> Self::CoreContainerStyle<'_> {
        StyleHandle(self, node_id.into())
    }

    #[inline(always)]
    fn set_unrounded_layout(&mut self, node_id: taffy::NodeId, layout: &taffy::Layout) {
        let id = NodeId::from(node_id);
        let dst = self.common_layout_mut(id);
        set_layout(&mut dst.Layout, layout);
    }

    fn compute_child_layout(
        &mut self,
        node_id: taffy::NodeId,
        inputs: taffy::LayoutInput,
    ) -> taffy::LayoutOutput {
        taffy::compute_cached_layout(self, node_id, inputs, |tree, node_id, inputs| {
            let id = NodeId::from(node_id);
            match id.typ() {
                NodeType::View | NodeType::Root => {
                    let container = StyleHandle(tree, id).container_style().Container;
                    match container {
                        com::Container::Flex => {
                            taffy::compute_flexbox_layout(tree, node_id, inputs)
                        }
                        com::Container::Grid => todo!(),
                        com::Container::Text => todo!(),
                        com::Container::Block => taffy::compute_block_layout(tree, node_id, inputs),
                    }
                }
                NodeType::Text => todo!(),
            }
        })
    }
}

impl RoundTree for SubDoc {
    #[inline(always)]
    fn get_unrounded_layout(&self, node_id: taffy::NodeId) -> taffy::Layout {
        let id = NodeId::from(node_id);
        let data = self.common_layout(id);
        get_layout(&data.Layout)
    }

    #[inline(always)]
    fn set_final_layout(&mut self, node_id: taffy::NodeId, layout: &taffy::Layout) {
        let id = NodeId::from(node_id);
        let data = self.common_layout_mut(id);
        set_layout(&mut data.FinalLayout, layout);
    }
}

impl CacheTree for SubDoc {
    fn cache_get(
        &self,
        node_id: taffy::NodeId,
        known_dimensions: taffy::Size<Option<f32>>,
        available_space: taffy::Size<taffy::AvailableSpace>,
        run_mode: taffy::RunMode,
    ) -> Option<taffy::LayoutOutput> {
        let id = NodeId::from(node_id);
        let data = &self.common_layout(id).LayoutCache;
        match run_mode {
            taffy::RunMode::PerformLayout => {
                if !data.HasFinalLayoutEntry {
                    return None;
                }
                let entry_known_dimensions = taffy::Size {
                    width: c_option!(#val; data.FinalLayoutEntry => KnownDimensionsWidth),
                    height: c_option!(#val; data.FinalLayoutEntry => KnownDimensionsHeight),
                };
                let entry_available_space = taffy::Size {
                    width: c_available_space!(data.FinalLayoutEntry => AvailableSpaceWidth),
                    height: c_available_space!(data.FinalLayoutEntry => AvailableSpaceHeight),
                };
                let c = {
                    let cached_size = taffy::Size {
                        width: data.FinalLayoutEntry.Content.Width,
                        height: data.FinalLayoutEntry.Content.Height,
                    };
                    (known_dimensions.width == entry_known_dimensions.width
                        || known_dimensions.width == Some(cached_size.width))
                        && (known_dimensions.height == entry_known_dimensions.height
                            || known_dimensions.height == Some(cached_size.height))
                        && (known_dimensions.width.is_some()
                            || entry_available_space
                                .width
                                .is_roughly_equal(available_space.width))
                        && (known_dimensions.height.is_some()
                            || entry_available_space
                                .height
                                .is_roughly_equal(available_space.height))
                };
                if !c {
                    return None;
                }
                Some(taffy::LayoutOutput {
                    size: taffy::Size {
                        width: data.FinalLayoutEntry.Content.Width,
                        height: data.FinalLayoutEntry.Content.Height,
                    },
                    content_size: taffy::Size {
                        width: data.FinalLayoutEntry.Content.ContentWidth,
                        height: data.FinalLayoutEntry.Content.ContentHeight,
                    },
                    first_baselines: taffy::Point {
                        x: c_option!(data.FinalLayoutEntry.Content => FirstBaselinesX),
                        y: c_option!(data.FinalLayoutEntry.Content => FirstBaselinesY),
                    },
                    top_margin: taffy::CollapsibleMarginSet {
                        positive: data.FinalLayoutEntry.Content.TopMargin.Positive,
                        negative: data.FinalLayoutEntry.Content.TopMargin.Negative,
                    },
                    bottom_margin: taffy::CollapsibleMarginSet {
                        positive: data.FinalLayoutEntry.Content.BottomMargin.Positive,
                        negative: data.FinalLayoutEntry.Content.BottomMargin.Negative,
                    },
                    margins_can_collapse_through: data
                        .FinalLayoutEntry
                        .Content
                        .MarginsCanCollapseThrough,
                })
            }
            taffy::RunMode::ComputeSize => {
                let check = |entry: &com::LayoutCacheEntrySize| {
                    let cached_size = taffy::Size {
                        width: entry.ContentWidth,
                        height: entry.ContentHeight,
                    };
                    let entry_known_dimensions = taffy::Size {
                        width: c_option!(#val; entry => KnownDimensionsWidth),
                        height: c_option!(#val; entry => KnownDimensionsHeight),
                    };
                    let entry_available_space = taffy::Size {
                        width: c_available_space!(entry => AvailableSpaceWidth),
                        height: c_available_space!(entry => AvailableSpaceHeight),
                    };
                    let c = (known_dimensions.width == entry_known_dimensions.width
                        || known_dimensions.width == Some(cached_size.width))
                        && (known_dimensions.height == entry_known_dimensions.height
                            || known_dimensions.height == Some(cached_size.height))
                        && (known_dimensions.width.is_some()
                            || entry_available_space
                                .width
                                .is_roughly_equal(available_space.width))
                        && (known_dimensions.height.is_some()
                            || entry_available_space
                                .height
                                .is_roughly_equal(available_space.height));
                    if c {
                        return Some(LayoutOutput::from_outer_size(cached_size));
                    }
                    None
                };
                macro_rules! check {
                    ( $n:tt ) => {
                        concat_idents!(has_name = HasMeasureEntries, $n {
                            if data.has_name {
                                concat_idents!(val_name = MeasureEntries, $n {
                                    if let Some(r) = check(&data.val_name) {
                                        return Some(r);
                                    }
                                })
                            }
                        })
                    };
                }
                check!(0);
                check!(1);
                check!(2);
                check!(3);
                check!(4);
                check!(5);
                check!(6);
                check!(7);
                check!(8);
                None
            }
            taffy::RunMode::PerformHiddenLayout => None,
        }
    }

    fn cache_store(
        &mut self,
        node_id: taffy::NodeId,
        known_dimensions: taffy::Size<Option<f32>>,
        available_space: taffy::Size<taffy::AvailableSpace>,
        run_mode: taffy::RunMode,
        layout_output: taffy::LayoutOutput,
    ) {
        let id = NodeId::from(node_id);
        let data = &mut self.common_layout_mut(id).LayoutCache;
        match run_mode {
            taffy::RunMode::PerformLayout => {
                data.IsEmpty = false;
                data.HasFinalLayoutEntry = true;
                data.FinalLayoutEntry = com::LayoutCacheEntryLayoutOutput {
                    KnownDimensionsWidthValue: known_dimensions.width.unwrap_or_default(),
                    KnownDimensionsHeightValue: known_dimensions.height.unwrap_or_default(),
                    AvailableSpaceWidthValue: match available_space.width {
                        taffy::AvailableSpace::Definite(v) => v,
                        _ => 0.0,
                    },
                    AvailableSpaceHeightValue: match available_space.height {
                        taffy::AvailableSpace::Definite(v) => v,
                        _ => 0.0,
                    },
                    HasKnownDimensionsWidth: known_dimensions.width.is_some(),
                    HasKnownDimensionsHeight: known_dimensions.height.is_some(),
                    AvailableSpaceWidth: match available_space.width {
                        taffy::AvailableSpace::Definite(_) => com::AvailableSpaceType::Definite,
                        taffy::AvailableSpace::MinContent => com::AvailableSpaceType::MinContent,
                        taffy::AvailableSpace::MaxContent => com::AvailableSpaceType::MaxContent,
                    },
                    AvailableSpaceHeight: match available_space.height {
                        taffy::AvailableSpace::Definite(_) => com::AvailableSpaceType::Definite,
                        taffy::AvailableSpace::MinContent => com::AvailableSpaceType::MinContent,
                        taffy::AvailableSpace::MaxContent => com::AvailableSpaceType::MaxContent,
                    },
                    Content: com::LayoutOutput {
                        Width: layout_output.size.width,
                        Height: layout_output.size.height,
                        ContentWidth: layout_output.content_size.width,
                        ContentHeight: layout_output.content_size.height,
                        FirstBaselinesX: layout_output.first_baselines.x.unwrap_or_default(),
                        FirstBaselinesY: layout_output.first_baselines.y.unwrap_or_default(),
                        TopMargin: com::LayoutCollapsibleMarginSet {
                            Positive: layout_output.top_margin.positive,
                            Negative: layout_output.top_margin.negative,
                        },
                        BottomMargin: com::LayoutCollapsibleMarginSet {
                            Positive: layout_output.bottom_margin.positive,
                            Negative: layout_output.bottom_margin.negative,
                        },
                        HasFirstBaselinesX: layout_output.first_baselines.x.is_some(),
                        HasFirstBaselinesY: layout_output.first_baselines.y.is_some(),
                        MarginsCanCollapseThrough: layout_output.margins_can_collapse_through,
                    },
                }
            }
            taffy::RunMode::ComputeSize => {
                data.IsEmpty = false;
                let i = Cache::compute_cache_slot(known_dimensions, available_space);
                let items = unsafe {
                    std::slice::from_raw_parts_mut(&mut data.MeasureEntries0 as *mut _, 9)
                };
                let has = unsafe {
                    std::slice::from_raw_parts_mut(&mut data.HasMeasureEntries0 as *mut _, 9)
                };
                has[i] = true;
                items[i] = com::LayoutCacheEntrySize {
                    KnownDimensionsWidthValue: known_dimensions.width.unwrap_or_default(),
                    KnownDimensionsHeightValue: known_dimensions.height.unwrap_or_default(),
                    AvailableSpaceWidthValue: match available_space.width {
                        taffy::AvailableSpace::Definite(v) => v,
                        _ => 0.0,
                    },
                    AvailableSpaceHeightValue: match available_space.height {
                        taffy::AvailableSpace::Definite(v) => v,
                        _ => 0.0,
                    },
                    HasKnownDimensionsWidth: known_dimensions.width.is_some(),
                    HasKnownDimensionsHeight: known_dimensions.height.is_some(),
                    AvailableSpaceWidth: match available_space.width {
                        taffy::AvailableSpace::Definite(_) => com::AvailableSpaceType::Definite,
                        taffy::AvailableSpace::MinContent => com::AvailableSpaceType::MinContent,
                        taffy::AvailableSpace::MaxContent => com::AvailableSpaceType::MaxContent,
                    },
                    AvailableSpaceHeight: match available_space.height {
                        taffy::AvailableSpace::Definite(_) => com::AvailableSpaceType::Definite,
                        taffy::AvailableSpace::MinContent => com::AvailableSpaceType::MinContent,
                        taffy::AvailableSpace::MaxContent => com::AvailableSpaceType::MaxContent,
                    },
                    ContentWidth: layout_output.size.width,
                    ContentHeight: layout_output.size.height,
                }
            }
            taffy::RunMode::PerformHiddenLayout => {}
        }
    }

    fn cache_clear(&mut self, node_id: taffy::NodeId) {
        let id = NodeId::from(node_id);
        let data = &mut self.common_layout_mut(id).LayoutCache;
        if data.IsEmpty {
            return;
        }
        data.IsEmpty = true;
        data.HasFinalLayoutEntry = false;
        data.HasMeasureEntries0 = false;
        data.HasMeasureEntries1 = false;
        data.HasMeasureEntries2 = false;
        data.HasMeasureEntries3 = false;
        data.HasMeasureEntries4 = false;
        data.HasMeasureEntries5 = false;
        data.HasMeasureEntries6 = false;
        data.HasMeasureEntries7 = false;
        data.HasMeasureEntries8 = false;
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

impl<'a> StyleHandle<'a> {
    #[inline(always)]
    pub fn common_style(&self) -> &CommonStyleData {
        match self.1.1 {
            NodeType::View => unsafe { &*common_style![self.view_common_style_data => self.1.0] },
            NodeType::Text => unsafe { &*common_style![self.text_common_style_data => self.1.0] },
            NodeType::Root => unsafe { &*common_style![self.root_common_style_data => self.1.0] },
        }
    }
    #[inline(always)]
    pub fn container_style(&self) -> &ContainerStyleData {
        match self.1.1 {
            NodeType::View => unsafe {
                &*common_style![self.view_container_style_data => self.1.0]
            },
            NodeType::Text => unreachable!(),
            NodeType::Root => unsafe {
                &*common_style![self.root_container_style_data => self.1.0]
            },
        }
    }
}

macro_rules! c_overflow {
    ( $self:ident.$name:ident ) => {
        match $self.container_style().$name {
            com::Overflow::Visible => taffy::Overflow::Visible,
            com::Overflow::Clip => taffy::Overflow::Clip,
            com::Overflow::Hidden => taffy::Overflow::Hidden,
        }
    };
}

macro_rules! c_position {
    ( $self:ident.$name:ident ) => {
        match $self.common_style().$name {
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
    type CustomIdent = String;

    #[inline(always)]
    fn box_generation_mode(&self) -> taffy::BoxGenerationMode {
        match self.common_style().Visible {
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
        match self.1.1 {
            NodeType::View | NodeType::Root => match self.container_style().BoxSizing {
                com::BoxSizing::BorderBox => taffy::BoxSizing::BorderBox,
                com::BoxSizing::ContentBox => taffy::BoxSizing::ContentBox,
            },
            NodeType::Text => taffy::BoxSizing::ContentBox,
        }
    }

    #[inline(always)]
    fn overflow(&self) -> taffy::Point<taffy::Overflow> {
        match self.1.1 {
            NodeType::View | NodeType::Root => Point {
                x: c_overflow!(self.OverflowX),
                y: c_overflow!(self.OverflowY),
            },
            NodeType::Text => Point {
                x: taffy::Overflow::Visible,
                y: taffy::Overflow::Visible,
            },
        }
    }

    #[inline(always)]
    fn scrollbar_width(&self) -> f32 {
        match self.1.1 {
            NodeType::View => self.container_style().ScrollBarSize,
            _ => 0.0,
        }
    }

    #[inline(always)]
    fn position(&self) -> taffy::Position {
        c_position!(self.Position)
    }

    #[inline(always)]
    fn inset(&self) -> taffy::Rect<taffy::LengthPercentageAuto> {
        taffy::Rect {
            left: c_length_percentage_auto!(self.common_style() => InsertLeft),
            right: c_length_percentage_auto!(self.common_style() => InsertRight),
            top: c_length_percentage_auto!(self.common_style() => InsertTop),
            bottom: c_length_percentage_auto!(self.common_style() => InsertBottom),
        }
    }

    #[inline(always)]
    fn size(&self) -> taffy::Size<taffy::Dimension> {
        match self.1.1 {
            NodeType::View | NodeType::Root => taffy::Size {
                width: c_dimension!(self.container_style() => Width),
                height: c_dimension!(self.container_style() => Height),
            },
            NodeType::Text => taffy::Size::auto(),
        }
    }

    #[inline(always)]
    fn min_size(&self) -> taffy::Size<taffy::Dimension> {
        match self.1.1 {
            NodeType::View | NodeType::Root => taffy::Size {
                width: c_dimension!(self.container_style() => MinWidth),
                height: c_dimension!(self.container_style() => MinHeight),
            },
            NodeType::Text => taffy::Size::auto(),
        }
    }

    #[inline(always)]
    fn max_size(&self) -> taffy::Size<taffy::Dimension> {
        match self.1.1 {
            NodeType::View | NodeType::Root => taffy::Size {
                width: c_dimension!(self.container_style() => MaxWidth),
                height: c_dimension!(self.container_style() => MaxHeight),
            },
            NodeType::Text => taffy::Size::auto(),
        }
    }

    #[inline(always)]
    fn aspect_ratio(&self) -> Option<f32> {
        match self.1.1 {
            NodeType::View | NodeType::Root => {
                if self.container_style().HasAspectRatio {
                    Some(self.container_style().AspectRatioValue)
                } else {
                    None
                }
            }
            NodeType::Text => None,
        }
    }

    #[inline(always)]
    fn margin(&self) -> taffy::Rect<taffy::LengthPercentageAuto> {
        taffy::Rect {
            left: c_length_percentage_auto!(self.common_style() => MarginLeft),
            right: c_length_percentage_auto!(self.common_style() => MarginRight),
            top: c_length_percentage_auto!(self.common_style() => MarginTop),
            bottom: c_length_percentage_auto!(self.common_style() => MarginBottom),
        }
    }

    #[inline(always)]
    fn padding(&self) -> taffy::Rect<taffy::LengthPercentage> {
        match self.1.1 {
            NodeType::View | NodeType::Root => taffy::Rect {
                left: c_length_percentage!(self.container_style() => PaddingLeft),
                right: c_length_percentage!(self.container_style() => PaddingRight),
                top: c_length_percentage!(self.container_style() => PaddingTop),
                bottom: c_length_percentage!(self.container_style() => PaddingBottom),
            },
            NodeType::Text => taffy::Rect::zero(),
        }
    }

    #[inline(always)]
    fn border(&self) -> taffy::Rect<taffy::LengthPercentage> {
        match self.1.1 {
            NodeType::View | NodeType::Root => taffy::Rect {
                left: c_length_percentage!(self.common_style() => BorderLeft),
                right: c_length_percentage!(self.common_style() => BorderRight),
                top: c_length_percentage!(self.common_style() => BorderTop),
                bottom: c_length_percentage!(self.common_style() => BorderBottom),
            },
            NodeType::Text => taffy::Rect::zero(),
        }
    }
}

impl<'a> BlockContainerStyle for StyleHandle<'a> {
    #[inline(always)]
    fn text_align(&self) -> taffy::TextAlign {
        match self.1.1 {
            NodeType::View | NodeType::Root => match self.container_style().TextAlign {
                com::TextAlign::Auto => taffy::TextAlign::Auto,
                com::TextAlign::Left => taffy::TextAlign::LegacyLeft,
                com::TextAlign::Right => taffy::TextAlign::LegacyRight,
                com::TextAlign::Center => taffy::TextAlign::LegacyCenter,
            },
            NodeType::Text => taffy::TextAlign::Auto,
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
    fn flex_direction(&self) -> taffy::FlexDirection {
        match self.1.1 {
            NodeType::View | NodeType::Root => match self.container_style().FlexDirection {
                com::FlexDirection::Column => taffy::FlexDirection::Column,
                com::FlexDirection::Row => taffy::FlexDirection::Row,
                com::FlexDirection::ColumnReverse => taffy::FlexDirection::ColumnReverse,
                com::FlexDirection::RowReverse => taffy::FlexDirection::RowReverse,
            },
            NodeType::Text => taffy::FlexDirection::Column,
        }
    }

    fn flex_wrap(&self) -> taffy::FlexWrap {
        match self.1.1 {
            NodeType::View | NodeType::Root => match self.container_style().FlexWrap {
                com::FlexWrap::NoWrap => taffy::FlexWrap::NoWrap,
                com::FlexWrap::Wrap => taffy::FlexWrap::Wrap,
                com::FlexWrap::WrapReverse => taffy::FlexWrap::WrapReverse,
            },
            NodeType::Text => taffy::FlexWrap::NoWrap,
        }
    }

    fn gap(&self) -> taffy::Size<taffy::LengthPercentage> {
        match self.1.1 {
            NodeType::View | NodeType::Root => taffy::Size {
                width: c_length_percentage!(self.container_style() => GapX),
                height: c_length_percentage!(self.container_style() => GapY),
            },
            NodeType::Text => taffy::Size::<taffy::LengthPercentage>::ZERO,
        }
    }

    fn align_content(&self) -> Option<taffy::AlignContent> {
        match self.common_style().AlignContent {
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

    fn align_items(&self) -> Option<taffy::AlignItems> {
        match self.common_style().AlignItems {
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

    fn justify_content(&self) -> Option<taffy::JustifyContent> {
        match self.common_style().JustifyContent {
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
        c_dimension!(self.common_style() => FlexBasis)
    }

    #[inline(always)]
    fn flex_grow(&self) -> f32 {
        self.common_style().FlexGrow
    }

    #[inline(always)]
    fn flex_shrink(&self) -> f32 {
        self.common_style().FlexShrink
    }

    #[inline(always)]
    fn align_self(&self) -> Option<taffy::AlignSelf> {
        match self.common_style().AlignSelf {
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
