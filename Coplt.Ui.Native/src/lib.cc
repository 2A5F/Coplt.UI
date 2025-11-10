#include "mimalloc-new-delete.h"

#include "lib.h"
#include "Alloc.h"

#include <icu.h>

#include "Error.h"
#include "Text.h"

#if _WINDOWS
#include "dwrite/Layout.h"
#endif

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

LibUi::LibUi(LibLoadInfo* info)
{
    m_backend = TextBackend::Create(info->p_dwrite);
}

void LibUi::Impl_SetLogger(void* obj, Func<void, void*, LogLevel, i32, char16*>* logger, Func<void, void*>* drop)
{
    m_logger = LoggerData(obj, reinterpret_cast<Func<void, void*, LogLevel, i32, const char16*>*>(logger), drop);
}

Str8 LibUi::Impl_GetCurrentErrorMessage()
{
    const auto& msg = Coplt::GetCurrentErrorMessage();
    return Str8(reinterpret_cast<u8*>(const_cast<char*>(msg.data())), msg.size());
}

HResult LibUi::Impl_GetSystemFontCollection(IFontCollection** fc)
{
    return feb([&] -> HResult
    {
        auto out = m_backend->GetSystemFontCollection();
        *fc = out.leak();
        return HResultE::Ok;
    });
}

HResult LibUi::Impl_GetSystemFontFallback(IFontFallback** ff)
{
    return feb([&] -> HResult
    {
        auto out = m_backend->GetSystemFontFallback();
        *ff = out.leak();
        return HResultE::Ok;
    });
}

HResult LibUi::Impl_CreateLayout(ILayout** layout)
{
    return feb([&]
    {
        auto out = LayoutCalc::CreateLayout(CloneRc(this));
        *layout = out.leak();
        return HResultE::Ok;
    });
}

HResult LibUi::Impl_SplitTexts(NativeList<TextRange>* ranges, char16 const* chars, const i32 len)
{
    return feb([&]
    {
        Coplt::SplitTexts(*ffi_list(ranges), chars, len);
        return HResultE::Ok;
    });
}

HResultE Coplt::coplt_ui_create_lib(LibLoadInfo* info, ILib** lib)
{
    return feb([&]
    {
        *lib = new LibUi(info);
        return HResultE::Ok;
    });
}
