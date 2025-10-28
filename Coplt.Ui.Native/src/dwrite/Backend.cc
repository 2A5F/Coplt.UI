#include "Backend.h"

#include "../Error.h"
#include "SystemFontCollection.h"
#include "SystemFontFallback.h"

using namespace Coplt;

TextBackend::TextBackend(Rc<IDWriteFactory7>&& m_dw_factory)
    : m_dw_factory(std::forward<Rc<IDWriteFactory7>>(m_dw_factory))
{
}

HResult TextBackend::Create(Rc<TextBackend>& out)
{
    Rc<IDWriteFactory7> dw_factory{};
    HRESULT hr = DWriteCreateFactory(
        DWRITE_FACTORY_TYPE_SHARED,
        __uuidof(IDWriteFactory7),
        dw_factory.put<::IUnknown>()
    );
    if (FAILED(hr)) return hr;
    return feb([&] -> HResult
    {
        out = Rc(new TextBackend(std::move(dw_factory)));
        return HResultE::Ok;
    });
}

Rc<IFontCollection> TextBackend::GetSystemFontCollection() const
{
    return SystemFontCollection::Create(this);
}

Rc<IFontFallback> TextBackend::GetSystemFontFallback() const
{
    return SystemFontFallback::Create(this);
}
