#include "Text.h"

#include <format>

#include <icu.h>
#include <hb.h>

using namespace Coplt;

void Coplt::SplitTexts(List<TextRange>& out, const char16* str, const i32 len)
{
    if (len == 0) return;

    auto cur_script = USCRIPT_INVALID_CODE;
    auto cur_category = U_UNASSIGNED;
    i32 cur_i = 0;
    i32 i = 0;
    UChar32 c;
    while (i < len)
    {
        const auto li = i;
        U16_NEXT(str, i, len, c);
        UErrorCode e{};
        const auto script = uscript_getScript(c, &e);
        if (e > 0) [[unlikely]]
        {
            throw Exception(std::format("GetScript failed: {}", u_errorName(e)));
        }
        const auto category = static_cast<UCharCategory>(u_charType(c));
        if (script != cur_script || category != cur_category)
        {
            if (li != 0)
            {
                out.Add(TextRange{
                    .Start = cur_i,
                    .Length = li - cur_i,
                    .Script = static_cast<ScriptCode>(cur_script),
                    .Category = static_cast<CharCategory>(cur_category),
                    .ScriptIsRtl = static_cast<bool>(uscript_isRightToLeft(cur_script)),
                });
                cur_i = li;
            }
            cur_script = script;
            cur_category = category;
        }
    }
    {
        out.Add(TextRange{
            .Start = cur_i,
            .Length = len - cur_i,
            .Script = static_cast<ScriptCode>(cur_script),
            .Category = static_cast<CharCategory>(cur_category),
            .ScriptIsRtl = static_cast<bool>(uscript_isRightToLeft(cur_script)),
        });
    }
}
