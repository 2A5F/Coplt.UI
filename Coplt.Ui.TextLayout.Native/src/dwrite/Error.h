#pragma once

#include <exception>
#include <system_error>
#include <windows.h>
#include <fmt/format.h>

namespace Coplt
{
    struct ComException : std::exception
    {
        HRESULT hr;
        std::string msg;

        explicit ComException(const HRESULT hr, const char* msg)
            : hr(hr), msg(fmt::format("{} (0x{:08X}: {})", msg, static_cast<uint32_t>(hr), std::system_category().message(hr)))
        {
        }

        ~ComException() noexcept override = default;

        const char* what() const noexcept override
        {
            return msg.c_str();
        }
    };
}
