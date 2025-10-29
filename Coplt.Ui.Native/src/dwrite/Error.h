#pragma once

#include <exception>
#include <system_error>
#include <windows.h>
#include <fmt/format.h>
#include "../Error.h"

namespace Coplt
{
    class ComException : public Exception
    {
        HRESULT hr;

    public:
        explicit ComException(const HRESULT hr, const char* msg)
            : Exception(
                  fmt::format("{} (0x{:08X}: {})", msg, static_cast<uint32_t>(hr), std::system_category().message(hr))
              ),
              hr(hr)
        {
        }

        HRESULT HResult() const
        {
            return hr;
        }
    };
}
