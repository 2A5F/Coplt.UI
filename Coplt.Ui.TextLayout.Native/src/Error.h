#pragma once

#include <string>

namespace Coplt {
    void SetCurrentErrorMessage(std::string&& err);
    const std::string& GetCurrentErrorMessage();
}
