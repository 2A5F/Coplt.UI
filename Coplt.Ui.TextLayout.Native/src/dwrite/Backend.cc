#include "Backend.h"

#include "SystemFontCollection.h"

Coplt::Backend::Backend(Rc<IDWriteFactory>&& m_dw_factory)
    : m_dw_factory(std::forward<Rc<IDWriteFactory>>(m_dw_factory))
{
}

Coplt::HResult Coplt::Backend::Create(Rc<Backend>& out)
{
    Rc<IDWriteFactory> dw_factory{};
    HRESULT hr = DWriteCreateFactory(
        DWRITE_FACTORY_TYPE_SHARED,
        __uuidof(IDWriteFactory),
        dw_factory.put<::IUnknown>()
    );
    if (FAILED(hr)) return hr;
    out = Rc(new Backend(std::move(dw_factory)));
    return HResultE::Ok;
}

Coplt::HResult Coplt::Backend::GetSystemFontCollection(Rc<IFontCollection>& out) const
{
    Rc<SystemFontCollection> sfc{};
    const auto hr = SystemFontCollection::Create(this, sfc);
    out = std::move(sfc);
    return hr;
}
