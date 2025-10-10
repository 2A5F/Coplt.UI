#include "Backend.h"

#include "SystemFontCollection.h"

Coplt::TextBackend::TextBackend(Rc<IDWriteFactory7>&& m_dw_factory)
    : m_dw_factory(std::forward<Rc<IDWriteFactory7>>(m_dw_factory))
{
}

Coplt::HResult Coplt::TextBackend::Create(Rc<TextBackend>& out)
{
    Rc<IDWriteFactory7> dw_factory{};
    HRESULT hr = DWriteCreateFactory(
        DWRITE_FACTORY_TYPE_SHARED,
        __uuidof(IDWriteFactory7),
        dw_factory.put<::IUnknown>()
    );
    if (FAILED(hr)) return hr;
    out = Rc(new TextBackend(std::move(dw_factory)));
    return HResultE::Ok;
}

Coplt::HResult Coplt::TextBackend::GetSystemFontCollection(Rc<IFontCollection>& out) const
{
    Rc<SystemFontCollection> sfc = SystemFontCollection::Create(this);
    out = std::move(sfc);
    return HResultE::Ok;
}
