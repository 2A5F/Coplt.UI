use std::hint::unreachable_unchecked;

use cocom::{HResult, HResultE};
use concat_idents::concat_idents;
use taffy::{CoreStyle, LayoutPartialTree, Point, RoundTree, TraversePartialTree, TraverseTree};

use crate::{
    col::{OrderedSet, ordered_set},
    com::{self, CommonLayoutData, CommonStyleData, NLayoutContext, NodeLocate, RootData},
};

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
        todo!()
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
}

macro_rules! c_overflow {
    ( $self:ident.$name:ident ) => {
        match $self.common_style().$name {
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
    ( $self:ident.$name:ident ) => {
        concat_idents!(value_name = $name, Value {
            match $self.common_style().$name {
                com::LengthType::Fixed => {
                    taffy::LengthPercentageAuto::length($self.common_style().value_name)
                }
                com::LengthType::Percent => {
                    taffy::LengthPercentageAuto::percent($self.common_style().value_name)
                }
                com::LengthType::Auto => taffy::LengthPercentageAuto::auto(),
            }
        })
    };
}

macro_rules! c_dimension {
    ( $self:ident.$name:ident ) => {
        concat_idents!(value_name = $name, Value {
            match $self.common_style().$name {
                com::LengthType::Fixed => {
                    taffy::Dimension::length($self.common_style().value_name)
                }
                com::LengthType::Percent => {
                    taffy::Dimension::percent($self.common_style().value_name)
                }
                com::LengthType::Auto => taffy::Dimension::auto(),
            }
        })
    };
}

macro_rules! c_length_percentage {
    ( $self:ident.$name:ident ) => {
        concat_idents!(value_name = $name, Value {
            match $self.common_style().$name {
                com::LengthType::Fixed => {
                    taffy::LengthPercentage::length($self.common_style().value_name)
                }
                com::LengthType::Percent => {
                    taffy::LengthPercentage::percent($self.common_style().value_name)
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
            com::VisibleMode::Remove => taffy::BoxGenerationMode::None,
            com::VisibleMode::Visible | com::VisibleMode::Hidden => {
                taffy::BoxGenerationMode::Normal
            }
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
            NodeType::View | NodeType::Root => match self.common_style().BoxSizing {
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
            NodeType::View => self.common_style().ScrollBarSize,
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
            left: c_length_percentage_auto!(self.InsertLeft),
            right: c_length_percentage_auto!(self.InsertRight),
            top: c_length_percentage_auto!(self.InsertTop),
            bottom: c_length_percentage_auto!(self.InsertBottom),
        }
    }

    #[inline(always)]
    fn size(&self) -> taffy::Size<taffy::Dimension> {
        match self.1.1 {
            NodeType::View | NodeType::Root => taffy::Size {
                width: c_dimension!(self.Width),
                height: c_dimension!(self.Height),
            },
            NodeType::Text => taffy::Size::auto(),
        }
    }

    #[inline(always)]
    fn min_size(&self) -> taffy::Size<taffy::Dimension> {
        match self.1.1 {
            NodeType::View | NodeType::Root => taffy::Size {
                width: c_dimension!(self.MinWidth),
                height: c_dimension!(self.MinHeight),
            },
            NodeType::Text => taffy::Size::auto(),
        }
    }

    #[inline(always)]
    fn max_size(&self) -> taffy::Size<taffy::Dimension> {
        match self.1.1 {
            NodeType::View | NodeType::Root => taffy::Size {
                width: c_dimension!(self.MaxWidth),
                height: c_dimension!(self.MaxHeight),
            },
            NodeType::Text => taffy::Size::auto(),
        }
    }

    #[inline(always)]
    fn aspect_ratio(&self) -> Option<f32> {
        match self.1.1 {
            NodeType::View | NodeType::Root => {
                if self.common_style().HasAspectRatio {
                    Some(self.common_style().AspectRatioValue)
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
            left: c_length_percentage_auto!(self.MarginLeft),
            right: c_length_percentage_auto!(self.MarginRight),
            top: c_length_percentage_auto!(self.MarginTop),
            bottom: c_length_percentage_auto!(self.MarginBottom),
        }
    }

    #[inline(always)]
    fn padding(&self) -> taffy::Rect<taffy::LengthPercentage> {
        match self.1.1 {
            NodeType::View | NodeType::Root => taffy::Rect {
                left: c_length_percentage!(self.PaddingLeft),
                right: c_length_percentage!(self.PaddingRight),
                top: c_length_percentage!(self.PaddingTop),
                bottom: c_length_percentage!(self.PaddingBottom),
            },
            NodeType::Text => taffy::Rect::zero(),
        }
    }

    #[inline(always)]
    fn border(&self) -> taffy::Rect<taffy::LengthPercentage> {
        match self.1.1 {
            NodeType::View | NodeType::Root => taffy::Rect {
                left: c_length_percentage!(self.BorderLeft),
                right: c_length_percentage!(self.BorderRight),
                top: c_length_percentage!(self.BorderTop),
                bottom: c_length_percentage!(self.BorderBottom),
            },
            NodeType::Text => taffy::Rect::zero(),
        }
    }
}
