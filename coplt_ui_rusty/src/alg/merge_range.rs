use std::ops::Range;

pub trait GetRangeIter {
    fn get_range_iter(&self) -> &dyn Iterator<Item = Range<usize>>;
}

pub struct MergeRangeIter<'a, const N: usize> {
    inputs: [&'a dyn Iterator<Item = Range<usize>>; N],
}

pub fn merge_span<const N: usize>(
    inputs: [&'_ dyn Iterator<Item = Range<usize>>; N],
) -> MergeRangeIter<'_, N> {
    MergeRangeIter { inputs }
}
