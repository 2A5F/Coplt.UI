#include "Backend.h"

#include "../Error.h"
#include "SystemFontCollection.h"

using namespace Coplt;

TextBackend::TextBackend(Rc<IDWriteFactory7>&& m_dw_factory)
    : m_dw_factory(std::forward<Rc<IDWriteFactory7>>(m_dw_factory))
{
}

HResult TextBackend::Create(Rc<TextBackend>& out)
{
    return feb([&] -> HResult
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
    });
}

HResult TextBackend::GetSystemFontCollection(Rc<IFontCollection>& out) const
{
    return feb([&]
    {
        Rc<SystemFontCollection> sfc = SystemFontCollection::Create(this);
        out = std::move(sfc);
        return HResultE::Ok;
    });
}
