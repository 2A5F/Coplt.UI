#include "Text.h"

#include <format>

#include <icu.h>
#include <hb.h>

using namespace Coplt;

namespace Coplt::UnicodeUtils
{
    extern "C" usize coplt_ui_unicode_utils_script_to_locale(
        u32 script,
        Func<usize, u32> create
    );

    char16 const* LikelyLocale(const UScriptCode script)
    {
        return reinterpret_cast<char16 const*>(coplt_ui_unicode_utils_script_to_locale(
            static_cast<u32>(script), [](u32 sc)
            {
                const auto script = static_cast<UScriptCode>(sc);
                const auto src = std::format("und_{}", uscript_getShortName(script));
                auto dst = std::string(128, 0);
                UErrorCode e{};
                const auto len = uloc_addLikelySubtags(src.data(), dst.data(), dst.size(), &e);
                if (e > 0) [[unlikely]]
                {
                    throw Exception(std::format("LikelyLocale failed: {}", u_errorName(e)));
                }
                // ReSharper disable once CppDFAMemoryLeak
                const auto locale = new u16[len + 1];
                for (auto i = 0; i < len; ++i)
                {
                    locale[i] = static_cast<u16>(dst[i]);
                }
                locale[len] = 0;
                return reinterpret_cast<usize>(locale);
            }));
    }
}

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
                    .Locale = UnicodeUtils::LikelyLocale(cur_script),
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
            .Locale = UnicodeUtils::LikelyLocale(cur_script),
        });
    }
}
