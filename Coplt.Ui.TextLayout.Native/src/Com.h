#pragma once

#ifdef _WINDOWS_
#include <windows.h>
#endif

#include <cpptrace/from_current.hpp>
#include <fmt/format.h>
#include <type_traits>

#include "CoCom.Interface.h"
#include "Error.h"

namespace Coplt
{
    inline void OnCatchException(const std::exception& e, const cpptrace::stacktrace& stacktrace)
    {
        auto msg = fmt::format("{} at\n{}", e.what(), stacktrace.to_string());
        SetCurrentErrorMessage(std::move(msg));
    }

    void OnCatchException(auto r, const std::exception& e, const cpptrace::stacktrace& stacktrace)
    {
        OnCatchException(e, stacktrace);

        if constexpr (std::is_same_v<decltype(r), HResult*>)
        {
            *r = HResult(HResultE::Fail);
        }
        else if constexpr (std::is_same_v<decltype(r), HResult**>)
        {
            **r = HResult(HResultE::Fail);
        }
    }
}

#define COPLT_COM_BEFORE_VIRTUAL_CALL(INTERFACE, METHOD, RET_TYPE)\
CPPTRACE_TRY {
#define COPLT_COM_AFTER_VIRTUAL_CALL(INTERFACE, METHOD, RET_TYPE)\
} CPPTRACE_CATCH(const std::exception& e) \
{  ::Coplt::OnCatchException(&r, e, ::cpptrace::from_current_exception()); }

#include "../api/Interface.h"
