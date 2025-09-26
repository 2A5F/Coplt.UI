#include "SystemFontCollection.h"

#include "Backend.h"
#include "CoCom.Types.h"

using namespace Coplt;

SystemFontCollection::SystemFontCollection(
    Rc<IDWriteFactory>&& dw_factory,
    Rc<IDWriteFontCollection>&& collection
) :
    m_dw_factory(std::forward<Rc<IDWriteFactory>>(dw_factory)),
    m_collection(std::forward<Rc<IDWriteFontCollection>>(collection))
{
}

HResult SystemFontCollection::Create(const Backend* backend, Rc<SystemFontCollection>& out)
{
    Rc<IDWriteFontCollection> collection{};
    if (const auto hr = backend->m_dw_factory->GetSystemFontCollection(collection.put()); FAILED(hr)) return hr;
    out = Rc(new SystemFontCollection(
        Rc(backend->m_dw_factory), std::move(collection)
    ));
    return HResultE::Ok;
}
