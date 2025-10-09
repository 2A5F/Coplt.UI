#include "mimalloc-new-delete.h"
#include "lib.h"
#include "Error.h"

#include <hb.h>

using namespace Coplt;

void LibTextLayout::Impl_SetLogger(void* obj, Func<void, LogLevel, i32, char16*>* logger, Func<void, void*>* drop)
{
    m_logger = LoggerData(obj, logger, drop);
}

Str8 LibTextLayout::Impl_GetCurrentErrorMessage()
{
    const auto& msg = Coplt::GetCurrentErrorMessage();
    return Str8(reinterpret_cast<u8*>(const_cast<char*>(msg.data())), msg.size());
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
    hb_version_string();
    return new LibTextLayout();
}
