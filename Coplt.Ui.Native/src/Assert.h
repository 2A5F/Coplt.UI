#pragma once

#include "Com.h"
#include "Error.h"

#if _DEBUG
#define COPLT_DEBUG_ASSERT(cond, msg) Coplt::DebugAssert(cond, msg)
#else
#define COPLT_DEBUG_ASSERT(cond, msg)
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

    template <usize N>
    void DebugAssert(const bool condition, const char (&message)[N])
    {
#if _DEBUG
        if (!condition) throw AssertException(message);
#endif
    }
}
