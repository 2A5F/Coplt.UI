#include "Error.h"

using namespace Coplt;

namespace
{
    thread_local std::string s_cur_err_msg{};
}

void Coplt::SetCurrentErrorMessage(std::string&& err)
{
    s_cur_err_msg = std::forward<std::string>(err);
}

const std::string& Coplt::GetCurrentErrorMessage()
{
    return s_cur_err_msg;
}
