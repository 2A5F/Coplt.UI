#pragma once

#include <format>

#include "Com.h"
#include "Error.h"

#if _DEBUG
#define COPLT_DEBUG_ASSERT(cond, ...) Coplt::DebugAssert(cond, "Assert Failure on \"" #cond "\"" __VA_OPT__(": ") __VA_ARGS__)
#else
#define COPLT_DEBUG_ASSERT(cond, ...)
#endif

namespace Coplt
{
    struct AssertException : Exception
    {
        explicit AssertException(
            std::string&& message,
            std::stacktrace&& stacktrace = std::stacktrace::current()
        ) : Exception(std::forward<std::string>(message), std::forward<std::stacktrace>(stacktrace))
        {
        }

        explicit AssertException(
            std::stacktrace stacktrace = std::stacktrace::current()
        ) : Exception(std::move(stacktrace))
        {
        }
    };

    template <usize E>
    COPLT_FORCE_INLINE void DebugAssert(const bool condition, const char (&message)[E])
    {
#if _DEBUG
        if (!condition) throw AssertException(message);
#endif
    }
}
