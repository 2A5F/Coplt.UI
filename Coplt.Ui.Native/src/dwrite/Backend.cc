#include "Backend.h"

#include "../Error.h"
#include "SystemFontCollection.h"
#include "SystemFontFallback.h"

using namespace Coplt;

TextBackend::TextBackend(Rc<IDWriteFactory7>&& dw_factory)
    : m_dw_factory(std::forward<Rc<IDWriteFactory7>>(dw_factory))
{
}

Rc<TextBackend> TextBackend::Create(void* dw_factory)
{
    return Rc(new TextBackend(Rc(static_cast<IDWriteFactory7*>(dw_factory))));
}

Rc<IFontCollection> TextBackend::GetSystemFontCollection() const
{
    return SystemFontCollection::Create(this);
}

Rc<IFontFallback> TextBackend::GetSystemFontFallback() const
{
    return SystemFontFallback::Create(this);
}
