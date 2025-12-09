#pragma once

#include <stacktrace>
#include <format>
#include <string>

namespace Coplt
{
    struct Str8;

    void SetCurrentErrorMessage(std::string&& err);
    Str8 GetCurrentErrorMessage();

    class Exception
    {
        std::string m_message;
        std::stacktrace m_stacktrace;

    public:
        explicit Exception(
            std::string&& message,
            std::stacktrace&& stacktrace = std::stacktrace::current()
        )
            : m_message(std::forward<std::string>(message)), m_stacktrace(std::forward<std::stacktrace>(stacktrace))
        {
        }

        explicit Exception(
            std::stacktrace stacktrace = std::stacktrace::current()
        )
            : m_stacktrace(std::move(stacktrace))
        {
        }

        const std::string& Message() const
        {
            return m_message;
        }

        const std::stacktrace& Stacktrace() const
        {
            return m_stacktrace;
        }

        std::string ToString() const
        {
            return std::format("{}\n{}", m_message, m_stacktrace);
        }
    };
}
