use std::ffi::c_void;

use crate::com::{LayoutCache, LayoutCacheFlags};
use crate::utf16::Utf16Indices;
use crate::{layout::*, *};
use concat_idents::concat_idents;
use harfrust::UnicodeBuffer;

pub fn cache_get(
    data: &LayoutCache,
    known_dimensions: taffy::Size<Option<f32>>,
    available_space: taffy::Size<taffy::AvailableSpace>,
    run_mode: taffy::RunMode,
) -> Option<taffy::LayoutOutput> {
    match run_mode {
        taffy::RunMode::PerformLayout => {
            if data.Flags.contains(com::LayoutCacheFlags::Final) {
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
                    return Some(taffy::LayoutOutput::from_outer_size(cached_size));
                }
                None
            };
            macro_rules! check {
                    ( $n:tt ) => {
                        concat_idents!(has_name = Measure, $n {
                            if data.Flags.contains(com::LayoutCacheFlags::has_name) {
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

pub fn cache_store(
    data: &mut LayoutCache,
    known_dimensions: taffy::Size<Option<f32>>,
    available_space: taffy::Size<taffy::AvailableSpace>,
    run_mode: taffy::RunMode,
    layout_output: taffy::LayoutOutput,
) {
    match run_mode {
        taffy::RunMode::PerformLayout => {
            data.Flags |= com::LayoutCacheFlags::Final;
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
            let i = taffy::Cache::compute_cache_slot(known_dimensions, available_space);
            let items =
                unsafe { std::slice::from_raw_parts_mut(&mut data.MeasureEntries0 as *mut _, 9) };
            data.Flags |= LayoutCacheFlags::from(1u16 << (i + 1));
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

#[inline(always)]
pub fn cache_clear(data: &mut LayoutCache) {
    data.Flags = com::LayoutCacheFlags::Empty;
}

#[inline(always)]
pub fn compute_cached_layout<ComputeFunction>(
    cache: &mut LayoutCache,
    inputs: taffy::LayoutInput,
    mut compute_uncached: ComputeFunction,
) -> taffy::LayoutOutput
where
    ComputeFunction: FnMut(taffy::LayoutInput) -> taffy::LayoutOutput,
{
    let taffy::LayoutInput {
        known_dimensions,
        available_space,
        run_mode,
        ..
    } = inputs;

    // First we check if we have a cached result for the given input
    let cache_entry = cache_get(cache, known_dimensions, available_space, run_mode);
    if let Some(cached_size_and_baselines) = cache_entry {
        return cached_size_and_baselines;
    }

    let computed_size_and_baselines = compute_uncached(inputs);

    // Cache result
    cache_store(
        cache,
        known_dimensions,
        available_space,
        run_mode,
        computed_size_and_baselines,
    );

    computed_size_and_baselines
}

#[derive(Debug)]
pub struct ManagedHandle(
    pub *mut c_void,
    pub Option<unsafe extern "C" fn(*mut core::ffi::c_void) -> ()>,
);

impl ManagedHandle {
    pub fn new(h: *mut c_void, f: unsafe extern "C" fn(*mut core::ffi::c_void) -> ()) -> Self {
        if (f as usize) == 0 {
            Self(h, None)
        } else {
            Self(h, Some(f))
        }
    }
}

impl Default for ManagedHandle {
    fn default() -> Self {
        Self(Default::default(), Default::default())
    }
}

impl Drop for ManagedHandle {
    fn drop(&mut self) {
        unsafe {
            if let Some(f) = self.1 {
                f(self.0)
            }
        }
    }
}

pub trait UnicodeBufferPushUtf16 {
    fn push_utf16(&mut self, str: &[u16]);
}

impl UnicodeBufferPushUtf16 for UnicodeBuffer {
    fn push_utf16(&mut self, str: &[u16]) {
        self.reserve(str.len());
        for (index, c) in Utf16Indices::new(str) {
            self.add(unsafe { char::from_u32_unchecked(c) }, index as u32);
        }
    }
}
