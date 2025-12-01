#include "Backend.h"

#include "../Error.h"
#include "SystemFontCollection.h"
#include "SystemFontFallback.h"

using namespace Coplt;

TextBackend::TextBackend(Rc<IDWriteFactory7>&& dw_factory)
    : m_dw_factory(std::forward<Rc<IDWriteFactory7>>(dw_factory))
{
    m_font_manager = Rc(new DWriteFontManager());
}

Rc<TextBackend> TextBackend::Create(void* dw_factory)
{
    return Rc(new TextBackend(Rc(static_cast<IDWriteFactory7*>(dw_factory))));
}

Rc<DWriteFontManager> TextBackend::CreateFontManager() const
{
    return Rc(new DWriteFontManager());
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
