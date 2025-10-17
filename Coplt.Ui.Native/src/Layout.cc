#include "Layout.h"

using namespace Coplt;

extern "C" int32_t coplt_ui_layout_calc(NLayoutContext* ctx);

HResult Layout::Impl_Calc(NLayoutContext* ctx)
{
    return Internal::BitCast<HResult>(coplt_ui_layout_calc(ctx));
}
