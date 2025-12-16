use std::ops::Range;

use tuples::TupleUniformMap;

pub trait GetRange {
    fn get_range(&self) -> Range<usize>;
}

#[derive(Debug)]
pub struct MergeRangeIter<Inputs> {
    inputs: Inputs,
}

pub fn merge_span<Inputs, Mapper>(inputs: Inputs, mapper: Mapper) -> MergeRangeIter<Inputs>
where
    Mapper: tuples::TupleUniformMapperBy<Inputs, Range<usize>, usize>,
{
    MergeRangeIter { inputs }
}
