#include "mimalloc-new-delete.h"
#include "lib.h"
#include "Error.h"

using namespace Coplt;

void LibTextLayout::Impl_SetLogger(void* obj, Func<void, LogLevel, i32, char16*>* logger, Func<void, void*>* drop)
{
    m_logger = LoggerData(obj, logger, drop);
}

const u8* LibTextLayout::Impl_get_CurrentErrorMessage()
{
    return reinterpret_cast<const u8*>(GetCurrentErrorMessage().c_str());
}

HResult LibTextLayout::Impl_GetSystemFontCollection(IFontCollection** fc)
{
    if (!m_backend)
        if (const auto hr = Backend::Create(m_backend); hr.IsError()) return hr;
    Rc<IFontCollection> out{};
    const auto hr = m_backend->GetSystemFontCollection(out);
    if (hr.IsSuccess()) *fc = out.leak();
    return hr;
}

ILibTextLayout* Coplt::Coplt_CreateLibTextLayout()
{
    return new LibTextLayout();
}
