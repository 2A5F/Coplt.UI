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

Rc<IFontFallbackBuilder> TextBackend::CreateFontFallbackBuilder(const FontFallbackBuilderCreateInfo& info) const
{
    return Rc(new FontFallbackBuilder(this, info));
}

extern "C" HResultE coplt_ui_dwrite_create_layout(const Rc<IDWriteFactory7>* factory, ILayout** out);

HResultE TextBackend::CreateLayout(ILayout** out) const
{
    return coplt_ui_dwrite_create_layout(&m_dw_factory, out);
}
