#include "CustomFontFallback.h"

#include "Error.h"

using namespace Coplt;

CustomFontFallback::CustomFontFallback(Rc<IDWriteFactory7> dw_factory, Rc<IDWriteFontFallback1>& fallback)
    : BaseFontFallback(dw_factory, fallback)
{
}

Rc<CustomFontFallback> CustomFontFallback::Create(const Rc<IDWriteFactory7>& factory, IDWriteFontFallbackBuilder* builder)
{
    Rc<IDWriteFontFallback> fallback{};
    if (const auto hr = builder->CreateFontFallback(fallback.put()); FAILED(hr))
        throw ComException(hr, "Failed to create font fallback");

    Rc<IDWriteFontFallback1> fallback1{};
    if (const auto hr = fallback->QueryInterface(fallback1.put()); FAILED(hr))
        throw ComException(hr, "Failed to create font fallback");

    return Rc(
        new CustomFontFallback(
            factory, fallback1
        )
    );
}
