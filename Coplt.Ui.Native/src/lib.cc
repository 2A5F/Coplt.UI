#include "mimalloc-new-delete.h"

#include "lib.h"
#include "Alloc.h"

#include <icu.h>

#include "Error.h"
#include "Text.h"
#include "dwrite/FontFallbackBuilder.h"

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

void LibUi::Impl_SetLogger(void* obj, Func<void, void*, LogLevel, StrKind, i32, void*>* logger, Func<u8, void*, LogLevel>* is_enabled, Func<void, void*>* drop)
{
    m_logger = LoggerData(obj, logger, is_enabled, drop);
}

void LibUi::Impl_ClearLogger()
{
    m_logger = {};
}

Str8 LibUi::Impl_GetCurrentErrorMessage()
{
    const auto& msg = Coplt::GetCurrentErrorMessage();
    return Str8(reinterpret_cast<u8*>(const_cast<char*>(msg.data())), msg.size());
}

extern "C" void coplt_ui_new_atlas_allocator(
    AtlasAllocatorType t, i32 width, i32 height, IAtlasAllocator** output
);

HResult LibUi::Impl_CreateAtlasAllocator(AtlasAllocatorType Type, i32 Width, i32 Height, IAtlasAllocator** aa)
{
    return feb(
        [&]
        {
            coplt_ui_new_atlas_allocator(Type, Width, Height, aa);
            return HResultE::Ok;
        }
    );
}

HResult LibUi::Impl_CreateFrameSource(IFrameSource** fs)
{
    return feb(
        [&] -> HResult
        {
            *fs = new FrameSource();
            return HResultE::Ok;
        }
    );
}

extern "C" void coplt_ui_new_font_manager(
    /* move */ IFrameSource* frame_source, IFontManager** output
);

HResult LibUi::Impl_CreateFontManager(IFrameSource* fs, IFontManager** fm)
{
    return feb(
        [&] -> HResult
        {
            if (fs == nullptr || fm == nullptr) return HResultE::InvalidArg;
            fs->AddRef();
            coplt_ui_new_font_manager(fs, fm);
            return HResultE::Ok;
        }
    );
}

HResult LibUi::Impl_GetSystemFontCollection(IFontCollection** fc)
{
    return feb(
        [&] -> HResult
        {
            auto out = m_backend->GetSystemFontCollection();
            *fc = out.leak();
            return HResultE::Ok;
        }
    );
}

HResult LibUi::Impl_GetSystemFontFallback(IFontFallback** ff)
{
    return feb(
        [&] -> HResult
        {
            auto out = m_backend->GetSystemFontFallback();
            *ff = out.leak();
            return HResultE::Ok;
        }
    );
}

HResult LibUi::Impl_CreateFontFallbackBuilder(IFontFallbackBuilder** ffb, FontFallbackBuilderCreateInfo const* info)
{
    return feb(
        [&]
        {
            auto out = m_backend->CreateFontFallbackBuilder(*info);
            *ffb = out.leak();
            return HResultE::Ok;
        }
    );
}

HResult LibUi::Impl_CreateLayout(ILayout** layout)
{
    return feb(
        [&]
        {
            auto out = LayoutCalc::CreateLayout(CloneRc(this));
            *layout = out.leak();
            return HResultE::Ok;
        }
    );
}

HResult LibUi::Impl_SplitTexts(NativeList<TextRange>* ranges, char16 const* chars, const i32 len)
{
    return feb(
        [&]
        {
            Coplt::SplitTexts(*ffi_list(ranges), chars, len);
            return HResultE::Ok;
        }
    );
}

HResultE Coplt::coplt_ui_create_lib(LibLoadInfo* info, ILib** lib)
{
    return feb(
        [&]
        {
            *lib = new LibUi(info);
            return HResultE::Ok;
        }
    );
}
