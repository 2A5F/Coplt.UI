#include "Layout.h"

#include "lib.h"

using namespace Coplt;

extern "C" int32_t coplt_ui_layout_calc(ILib* lib, NLayoutContext* ctx);

Layout::Layout(Rc<LibUi> lib) : m_lib(std::move(lib))
{
}

HResult Layout::Impl_Calc(NLayoutContext* ctx)
{
    return Internal::BitCast<HResult>(coplt_ui_layout_calc(m_lib.get(), ctx));
}
