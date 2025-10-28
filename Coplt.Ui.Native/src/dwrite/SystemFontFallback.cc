#include "SystemFontFallback.h"

#include "Error.h"

using namespace Coplt;

SystemFontFallback::SystemFontFallback(Rc<IDWriteFactory7> dw_factory, Rc<IDWriteFontFallback>& fallback)
    : m_dw_factory(std::move(dw_factory)), m_fallback(std::move(fallback))
{
}

Rc<IFontFallback> SystemFontFallback::Create(const TextBackend* backend)
{
    Rc<IDWriteFontFallback> fallback{};
    if (const auto hr = backend->m_dw_factory->GetSystemFontFallback(fallback.put()); FAILED(hr))
        throw ComException(hr, "Failed to get system font fallback");
    return Rc(new SystemFontFallback(
        Rc(backend->m_dw_factory), fallback
    ));
}
