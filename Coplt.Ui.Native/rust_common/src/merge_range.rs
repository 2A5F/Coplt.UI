#![allow(unused)]

use std::ops::Range;

use crate::*;

pub fn merge_span<const N: usize>(
    inputs: [&'_ mut dyn Iterator<Item = Range<usize>>; N],
) -> impl Iterator<Item = Range<usize>> {
    a_gen(|ctx| gen_merge_span(ctx, inputs)).to_iter()
}

async fn gen_merge_span<const N: usize>(
    ctx: AGen<Range<usize>>,
    inputs: [&'_ mut dyn Iterator<Item = Range<usize>>; N],
) -> Option<()> {
    let mut states = [const { State::None }; N];
    let mut last_pos = 0;

    let (min_pos, min_range) = (0..N)
        .into_iter()
        .filter_map(|n| {
            let ref mut state = states[n];
            if let State::None = state {
                let input = &mut *inputs[n];
                match input.next() {
                    Some(v) => *state = State::Some(v),
                    None => *state = State::End,
                }
            }
            if let State::End = state {
                None
            } else {
                Some((n, unsafe { state.some().clone() }))
            }
        })
        .min_by_key(|a| a.1.start)?;

    debug_assert!(min_pos >= last_pos);

    let off = min_pos - last_pos;
    if off > 0 {
        last_pos = min_pos;
        ctx.Yield(Range {
            start: min_pos,
            end: min_pos + off,
        })
        .await;
    }

    // todo
    todo!()
}

enum State {
    None,
    Some(Range<usize>),
    End,
}

impl State {
    unsafe fn some(&mut self) -> &mut Range<usize> {
        match self {
            State::Some(range) => range,
            _ => unreachable!(),
        }
    }
    fn try_some(&mut self) -> Option<&mut Range<usize>> {
        match self {
            State::Some(range) => Some(range),
            _ => None,
        }
    }
}
