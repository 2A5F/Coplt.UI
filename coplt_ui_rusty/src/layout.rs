use cocom::{HResult, HResultE};
use taffy::{CoreStyle, LayoutPartialTree, Point, TraversePartialTree};

use crate::{
    col::{OrderedSet, ordered_set},
    com::{self, CommonStyleData, NLayoutContext, NodeLocate, ViewStyleData},
};

#[unsafe(no_mangle)]
pub extern "C" fn coplt_ui_layout_calc(ctx: *mut NLayoutContext) -> HResult {
    unsafe {
        for root_index in (*ctx).roots() {
            let sub_doc = SubDoc(ctx, *root_index);
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

impl SubDoc {
    #[inline(always)]
    pub fn get_childs(&self, id: NodeId) -> &OrderedSet<NodeLocate> {
        match id.typ() {
            NodeType::View => unsafe { &*childs_data![self.view_childs_data => id.index()] },
            NodeType::Text => unreachable!(),
            NodeType::Root => unsafe { &*childs_data![self.root_childs_data => id.index()] },
        }
    }
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

    fn set_unrounded_layout(&mut self, node_id: taffy::NodeId, layout: &taffy::Layout) {
        todo!()
    }

    fn compute_child_layout(
        &mut self,
        node_id: taffy::NodeId,
        inputs: taffy::LayoutInput,
    ) -> taffy::LayoutOutput {
        todo!()
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
    pub fn view_style(&self) -> &ViewStyleData {
        match self.1.1 {
            NodeType::View => unsafe { &*common_style![self.view_style_data => self.1.0] },
            NodeType::Text => unreachable!(),
            NodeType::Root => unsafe { &*common_style![self.root_style_data => self.1.0] },
        }
    }
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
        match self.1.1 {
            NodeType::View | NodeType::Root => {
                matches!(self.view_style().Display, com::Display::Block)
            }
            NodeType::Text => false,
        }
    }

    #[inline(always)]
    fn is_compressible_replaced(&self) -> bool {
        false
    }

    #[inline(always)]
    fn box_sizing(&self) -> taffy::BoxSizing {
        match self.1.1 {
            NodeType::View | NodeType::Root => match self.view_style().BoxSizing {
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
                x: match self.view_style().OverflowX {
                    com::Overflow::Visible => taffy::Overflow::Visible,
                    com::Overflow::Clip => taffy::Overflow::Clip,
                    com::Overflow::Hidden => taffy::Overflow::Hidden,
                },
                y: match self.view_style().OverflowY {
                    com::Overflow::Visible => taffy::Overflow::Visible,
                    com::Overflow::Clip => taffy::Overflow::Clip,
                    com::Overflow::Hidden => taffy::Overflow::Hidden,
                },
            },
            NodeType::Text => Point {
                x: taffy::Overflow::Visible,
                y: taffy::Overflow::Visible,
            },
        }
    }

    #[inline(always)]
    fn scrollbar_width(&self) -> f32 {
        0.0
    }

    #[inline(always)]
    fn position(&self) -> taffy::Position {
        match self.1.1 {
            NodeType::View | NodeType::Root => match self.view_style().Position {
                com::Position::Relative => taffy::Position::Relative,
                com::Position::Absolute => taffy::Position::Absolute,
            },
            NodeType::Text => taffy::Position::Relative,
        }
    }

    #[inline(always)]
    fn inset(&self) -> taffy::Rect<taffy::LengthPercentageAuto> {
        taffy::Style::<Self::CustomIdent>::DEFAULT.inset
    }

    #[inline(always)]
    fn size(&self) -> taffy::Size<taffy::Dimension> {
        taffy::Style::<Self::CustomIdent>::DEFAULT.size
    }

    #[inline(always)]
    fn min_size(&self) -> taffy::Size<taffy::Dimension> {
        taffy::Style::<Self::CustomIdent>::DEFAULT.min_size
    }

    #[inline(always)]
    fn max_size(&self) -> taffy::Size<taffy::Dimension> {
        taffy::Style::<Self::CustomIdent>::DEFAULT.max_size
    }

    #[inline(always)]
    fn aspect_ratio(&self) -> Option<f32> {
        taffy::Style::<Self::CustomIdent>::DEFAULT.aspect_ratio
    }

    #[inline(always)]
    fn margin(&self) -> taffy::Rect<taffy::LengthPercentageAuto> {
        taffy::Style::<Self::CustomIdent>::DEFAULT.margin
    }

    #[inline(always)]
    fn padding(&self) -> taffy::Rect<taffy::LengthPercentage> {
        taffy::Style::<Self::CustomIdent>::DEFAULT.padding
    }

    #[inline(always)]
    fn border(&self) -> taffy::Rect<taffy::LengthPercentage> {
        taffy::Style::<Self::CustomIdent>::DEFAULT.border
    }
}
