use std::sync::LazyLock;

use dashmap::DashMap;

static SCIPRT_TO_LOCALE: LazyLock<DashMap<u32, usize>> = LazyLock::new(|| DashMap::new());

#[unsafe(no_mangle)]
pub extern "C" fn coplt_ui_unicode_utils_script_to_locale(
    script: u32,
    create: unsafe extern "C" fn(u32) -> usize,
) -> usize {
    let map = &*SCIPRT_TO_LOCALE;
    match map.entry(script) {
        dashmap::Entry::Occupied(entry) => *entry.get(),
        dashmap::Entry::Vacant(entry) => *entry.insert(unsafe { create(script) }),
    }
}
