#![allow(unused)]

use std::ops::Range;

use crate::*;

pub fn merge_ranges<const N: usize>(
    inputs: [&'_ mut dyn Iterator<Item = (/* index */ u32, Range<u32>)>; N],
) -> impl Iterator<Item = (Range<u32>, /* index */ [u32; N])> {
    a_gen(move |ctx| gen_merge_ranges(ctx, inputs)).to_iter()
}

async fn gen_merge_ranges<const N: usize>(
    ctx: AGen<(Range<u32>, /* index */ [u32; N])>,
    inputs: [&'_ mut dyn Iterator<Item = (/* index */ u32, Range<u32>)>; N],
) -> Option<()> {
    let mut states = [const { State::None }; N];
    let mut last_pos = (0..N)
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
                Some(unsafe { state.some().1.start })
            }
        })
        .min()?;

    loop {
        let next_pos = (0..N)
            .into_iter()
            .filter_map(|n| {
                let ref mut state = states[n];
                let range = state.try_some()?;
                while range.1.end <= last_pos {
                    match inputs[n].next() {
                        Some(v) => *range = v,
                        None => {
                            *state = State::End;
                            return None;
                        }
                    }
                }
                if range.1.start > last_pos {
                    Some(range.1.start)
                } else {
                    Some(range.1.end)
                }
            })
            .min()?;

        if next_pos > last_pos {
            let range = Range {
                start: last_pos,
                end: next_pos,
            };
            last_pos = next_pos;
            ctx.Yield((range, State::get_indexes(&states))).await;
        }
    }
}

enum State {
    None,
    Some((u32, Range<u32>)),
    End,
}

impl State {
    pub fn get_indexes<const N: usize>(this: &[State; N]) -> [u32; N] {
        let mut arr = [0; N];
        for (i, item) in this.iter().enumerate() {
            unsafe { arr[i] = item.some_const().0 }
        }
        arr
    }

    unsafe fn some_const(&self) -> &(u32, Range<u32>) {
        match self {
            State::Some(range) => range,
            _ => unreachable!(),
        }
    }

    unsafe fn some(&mut self) -> &mut (u32, Range<u32>) {
        match self {
            State::Some(range) => range,
            _ => unreachable!(),
        }
    }
    fn try_some(&mut self) -> Option<&mut (u32, Range<u32>)> {
        match self {
            State::Some(range) => Some(range),
            _ => None,
        }
    }
}

#[test]
fn test_1() {
    let data = ([0..10, 10..20, 20..30], [5..10, 25..30]);
    let a: &mut dyn Iterator<Item = (/* index */ u32, Range<u32>)> =
        &mut data.0.iter().enumerate().map(|a| (a.0 as _, a.1.clone()));
    let b: &mut dyn Iterator<Item = (/* index */ u32, Range<u32>)> =
        &mut data.1.iter().enumerate().map(|a| (a.0 as _, a.1.clone()));
    let r = merge_ranges([a, b]);
    let v: Vec<_> = r.collect();
    println!("{v:?}");
}

#[test]
fn test_2() {
    let data = ([0..10, 10..20, 20..30], [0..5, 5..10, 10..30]);
    let a: &mut dyn Iterator<Item = (/* index */ u32, Range<u32>)> =
        &mut data.0.iter().enumerate().map(|a| (a.0 as _, a.1.clone()));
    let b: &mut dyn Iterator<Item = (/* index */ u32, Range<u32>)> =
        &mut data.1.iter().enumerate().map(|a| (a.0 as _, a.1.clone()));
    let r = merge_ranges([a, b]);
    let v: Vec<_> = r.collect();
    println!("{v:?}");
}
