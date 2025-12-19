#include "Text.h"

#include <format>

#include <icu.h>

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
                char dst[64];
                UErrorCode e{};
                uloc_addLikelySubtags(src.data(), dst, std::size(dst), &e);
                if (e > 0) [[unlikely]]
                    throw Exception(std::format("LikelyLocale failed: {}", u_errorName(e)));

                char lang[16];
                const auto lang_len = uloc_getLanguage(dst, lang, std::size(lang), &e);
                if (e > 0) [[unlikely]]
                    throw Exception(std::format("LikelyLocale failed: {}", u_errorName(e)));

                char country[16];
                const auto country_len = uloc_getCountry(dst, country, std::size(country), &e);
                if (e > 0) [[unlikely]]
                    throw Exception(std::format("LikelyLocale failed: {}", u_errorName(e)));

                // ReSharper disable once CppDFAMemoryLeak
                const auto locale = new u16[lang_len + country_len + 2];
                locale[lang_len + country_len + 1] = 0;
                locale[lang_len] = '_';
                for (auto i = 0u; i < lang_len; ++i)
                {
                    locale[i] = lang[i];
                }
                for (auto i = 0; i < country_len; ++i)
                {
                    locale[lang_len + 1 + i] = country[i];
                }
                return reinterpret_cast<usize>(locale);
            }
        ));
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
                out.Add(
                    TextRange{
                        .Start = cur_i,
                        .Length = li - cur_i,
                        .Script = static_cast<ScriptCode>(cur_script),
                        .Category = static_cast<CharCategory>(cur_category),
                        .ScriptIsRtl = static_cast<bool>(uscript_isRightToLeft(cur_script)),
                        .Locale = UnicodeUtils::LikelyLocale(cur_script),
                    }
                );
                cur_i = li;
            }
            cur_script = script;
            cur_category = category;
        }
    }
    {
        out.Add(
            TextRange{
                .Start = cur_i,
                .Length = len - cur_i,
                .Script = static_cast<ScriptCode>(cur_script),
                .Category = static_cast<CharCategory>(cur_category),
                .ScriptIsRtl = static_cast<bool>(uscript_isRightToLeft(cur_script)),
                .Locale = UnicodeUtils::LikelyLocale(cur_script),
            }
        );
    }
}

extern "C" const char* coplt_ui_get_user_ui_default_locale_impl(usize* len);

const char* Coplt::coplt_ui_get_user_ui_default_locale(usize* len)
{
    return coplt_ui_get_user_ui_default_locale_impl(len);
}
