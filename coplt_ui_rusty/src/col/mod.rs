pub mod enumerator;
pub mod hash;
pub mod hash_helpers;
pub mod ordered_set;

pub use enumerator::*;
pub use hash::*;
pub use ordered_set::OrderedSet;

use crate::com::NodeId;

impl GetHashCode for NodeId {
    fn get_hash_code(&self) -> i32 {
        (self.Id ^ self.VersionAndType) as i32
    }
}

impl PartialEq for NodeId {
    fn eq(&self, other: &Self) -> bool {
        self.Id == other.Id && self.VersionAndType == other.VersionAndType
    }
}

impl Eq for NodeId {}
