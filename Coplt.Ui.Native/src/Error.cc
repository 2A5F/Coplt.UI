#include "Error.h"

#include "Com.h"

using namespace Coplt;

namespace
{
    enum class MessageSource
    {
        None,
        CppString,
        RustString,
    };

    struct RustString
    {
        const u8* p_rust_string_data;
        usize rust_string_len;
    };

    struct Message
    {
        union
        {
            std::string cpp_string;
            RustString rust_string;
        };

        MessageSource type;

        // ReSharper disable once CppPossiblyUninitializedMember
        Message()
            : type(MessageSource::None)
        {
        }

        // ReSharper disable once CppPossiblyUninitializedMember
        explicit Message(std::string&& string)
            : type(MessageSource::CppString), cpp_string(std::forward<std::string>(string))
        {
        }

        // ReSharper disable once CppPossiblyUninitializedMember
        explicit Message(const std::string& string)
            : type(MessageSource::CppString), cpp_string(string)
        {
        }

        explicit Message(RustString string)
            : type(MessageSource::RustString), rust_string(string)
        {
        }

        ~Message()
        {
            switch (type)
            {
            case MessageSource::None:
                break;
            case MessageSource::CppString:
                cpp_string.~basic_string();
                break;
            case MessageSource::RustString:
                // Since uses the same allocator, it can be free directly.
                mi_free(const_cast<void*>(static_cast<const void*>(rust_string.p_rust_string_data)));
                break;
            }
        }

        Message(const Message&) = delete;
        Message& operator=(const Message&) = delete;

        void swap(Message& other) noexcept
        {
            std::swap_ranges(
                reinterpret_cast<usize*>(this),
                reinterpret_cast<usize*>(this) + sizeof(Message) / sizeof(usize),
                reinterpret_cast<usize*>(&other)
            );
        }

        // ReSharper disable once CppPossiblyUninitializedMember
        Message(Message&& other) noexcept
            : type(std::exchange(other.type, MessageSource::None))
        {
            switch (type)
            {
            case MessageSource::None:
                break;
            case MessageSource::CppString:
                new(&cpp_string) std::string(std::move(other.cpp_string));
                break;
            case MessageSource::RustString:
                new(&rust_string) RustString(std::move(other.rust_string));
                break;
            }
        }

        Message& operator=(Message&& other) noexcept
        {
            Message(std::forward<Message>(other)).swap(*this);
            return *this;
        }

        Message& operator=(std::string&& other) noexcept
        {
            Message(std::forward<std::string>(other)).swap(*this);
            return *this;
        }

        Message& operator=(const std::string& other) noexcept
        {
            Message(other).swap(*this);
            return *this;
        }

        Message& operator=(RustString&& other) noexcept
        {
            Message(std::forward<RustString>(other)).swap(*this);
            return *this;
        }

        Message& operator=(const RustString& other) noexcept
        {
            Message(other).swap(*this);
            return *this;
        }

        Str8 GetStr8() const
        {
            switch (type)
            {
            case MessageSource::None:
                return Str8();
            case MessageSource::CppString:
                return Str8(reinterpret_cast<const u8*>(cpp_string.data()), cpp_string.size());
            case MessageSource::RustString:
                return Str8(rust_string.p_rust_string_data, rust_string.rust_string_len);
            }
            std::unreachable();
        }
    };

    thread_local Message s_cur_err_msg{};
}

void Coplt::SetCurrentErrorMessage(std::string&& err)
{
    s_cur_err_msg = std::forward<std::string>(err);
}

Str8 Coplt::GetCurrentErrorMessage()
{
    return s_cur_err_msg.GetStr8();
}

extern "C" void coplt_ui_set_current_error_message(const RustString* msg)
{
    s_cur_err_msg = *msg;
}
