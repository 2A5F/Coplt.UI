#include "Text.h"

#include <icu.h>
#include <hb.h>

using namespace Coplt;

void Coplt::SplitScripts(List<SimpleRange>& out, const char16* str, const i32 len)
{
    if (len == 0) return;

    auto cur_script = USCRIPT_INVALID_CODE;
    i32 cur_i = 0;
    i32 i = 0;
    UChar32 c;
    while (i < len)
    {
        U16_NEXT(str, i, len, c);
        UErrorCode e;
        if (const auto script = uscript_getScript(c, &e); script != cur_script)
        {
            if (cur_script != USCRIPT_INVALID_CODE && cur_i != 0)
            {
                out.Add(SimpleRange{.Start = cur_i, .Length = i - cur_i});
            }
            cur_i = i;
            cur_script = script;
        }
    }
    {
        out.Add(SimpleRange{.Start = cur_i, .Length = len - cur_i});
    }
}
