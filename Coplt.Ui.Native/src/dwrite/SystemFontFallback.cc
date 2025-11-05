#include "SystemFontFallback.h"

#include "Error.h"

using namespace Coplt;

SystemFontFallback::SystemFontFallback(Rc<IDWriteFactory7> dw_factory, Rc<IDWriteFontFallback1>& fallback)
    : BaseFontFallback(dw_factory, fallback)
{
}

Rc<IFontFallback> SystemFontFallback::Create(const TextBackend* backend)
{
    Rc<IDWriteFontFallback> fallback{};
    if (const auto hr = backend->m_dw_factory->GetSystemFontFallback(fallback.put()); FAILED(hr))
        throw ComException(hr, "Failed to get system font fallback");

    Rc<IDWriteFontFallback1> fallback1{};
    if (const auto hr = fallback->QueryInterface(fallback1.put()); FAILED(hr))
        throw ComException(hr, "Failed to get system font fallback");

    return Rc(new SystemFontFallback(
        Rc(backend->m_dw_factory), fallback1
    ));
}
