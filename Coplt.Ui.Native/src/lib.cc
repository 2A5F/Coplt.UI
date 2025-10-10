#include "mimalloc-new-delete.h"
#include "lib.h"
#include "Error.h"

#include <hb.h>

using namespace Coplt;

extern "C" void* coplt_ui_malloc(const size_t size, const size_t align)
{
    return mi_malloc_aligned(size, align);
}

extern "C" void coplt_ui_free(void* ptr, const size_t align)
{
    mi_free_aligned(ptr, align);
}

extern "C" void* coplt_ui_zalloc(const size_t size, const size_t align)
{
    return mi_zalloc_aligned(size, align);
}

extern "C" void* coplt_ui_realloc(void* ptr, const size_t new_size, const size_t align)
{
    return mi_realloc_aligned(ptr, new_size, align);
}

void LibUi::Impl_SetLogger(void* obj, Func<void, LogLevel, i32, char16*>* logger, Func<void, void*>* drop)
{
    m_logger = LoggerData(obj, logger, drop);
}

Str8 LibUi::Impl_GetCurrentErrorMessage()
{
    const auto& msg = Coplt::GetCurrentErrorMessage();
    return Str8(reinterpret_cast<u8*>(const_cast<char*>(msg.data())), msg.size());
}

void* LibUi::Impl_Alloc(const i32 size, const i32 align) const
{
    return mi_malloc_aligned(size, align);
}

void LibUi::Impl_Free(void* ptr, const i32 align) const
{
    mi_free_aligned(ptr, align);
}

void* LibUi::Impl_ZAlloc(const i32 size, const i32 align) const
{
    return mi_zalloc_aligned(size, align);
}

void* LibUi::Impl_ReAlloc(void* ptr, const i32 size, const i32 align) const
{
    return mi_realloc_aligned(ptr, size, align);
}

HResult LibUi::Impl_GetSystemFontCollection(IFontCollection** fc)
{
    if (!m_backend)
        if (const auto hr = TextBackend::Create(m_backend); hr.IsError()) return hr;
    Rc<IFontCollection> out{};
    const auto hr = m_backend->GetSystemFontCollection(out);
    if (hr.IsSuccess()) *fc = out.leak();
    return hr;
}

ILib* Coplt::Coplt_CreateLibUi()
{
    hb_version_string();
    return new LibUi();
}
