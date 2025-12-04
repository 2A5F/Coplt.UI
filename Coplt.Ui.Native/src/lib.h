#pragma once

#include "Com.h"
#include "Defines.h"
#include "Backend.h"
#include "FrameSource.h"

namespace Coplt
{
    struct LoggerData
    {
        void* obj{};
        Func<void, void*, LogLevel, StrKind, i32, void*>* logger{};
        Func<u8, void*, LogLevel>* is_enabled{};
        Func<void, void*>* drop{};

        LoggerData() = default;

        explicit LoggerData(
            void* obj, Func<void, void*, LogLevel, StrKind, i32, void*>* logger, Func<u8, void*, LogLevel>* is_enabled, Func<void, void*>* drop
        )
            : obj(obj), logger(logger), is_enabled(is_enabled), drop(drop)
        {
        }

        ~LoggerData()
        {
            if (drop == nullptr) return;
            drop(obj);
        }

        LoggerData(LoggerData& other) = delete;
        LoggerData& operator=(LoggerData& other) = delete;

        LoggerData(LoggerData&& other) noexcept
        {
            swap(other);
        }

        LoggerData& operator=(LoggerData&& other) noexcept
        {
            LoggerData(std::move(other)).swap(*this);
            return *this;
        }

        LoggerData& swap(LoggerData& other) noexcept
        {
            std::swap(obj, other.obj);
            std::swap(logger, other.logger);
            std::swap(is_enabled, other.is_enabled);
            std::swap(drop, other.drop);
            return *this;
        }

        bool IsEnabled(const LogLevel level) const
        {
            if (logger == nullptr) return false;
            if (is_enabled == nullptr) return true;
            return is_enabled(obj, level);
        }

        void Log(const LogLevel level, const i32 size, const char8* msg) const
        {
            if (logger == nullptr) return;
            logger(obj, level, StrKind::Str8, size, const_cast<void*>(static_cast<const void*>(msg)));
        }

        void Log(const LogLevel level, const i32 size, const char16* msg) const
        {
            if (logger == nullptr) return;
            logger(obj, level, StrKind::Str16, size, const_cast<void*>(static_cast<const void*>(msg)));
        }

        template <i32 N>
        void Log(const LogLevel level, const char8 (&msg)[N]) const
        {
            Log(obj, level, N - 1, msg);
        }

        template <i32 N>
        void Log(const LogLevel level, const char16 (&msg)[N]) const
        {
            Log(obj, level, N - 1, msg);
        }

        template <class S> requires requires(S s) { { s.size() } -> std::convertible_to<i32>; { s.data() } -> std::convertible_to<const char8*>; }
        void Log(const LogLevel level, const S& msg) const
        {
            Log(level, msg.size(), msg.data());
        }

        template <class S> requires requires(S s) { { s.size() } -> std::convertible_to<i32>; { s.data() } -> std::convertible_to<const char16*>; }
        void Log(const LogLevel level, const S& msg) const
        {
            Log(level, msg.size(), msg.data());
        }

        template <class F> requires requires(F f) { { f().size() } -> std::convertible_to<i32>; { f().data() } -> std::convertible_to<const char8*>; }
        void Log(const LogLevel level, F f) const
        {
            if (logger == nullptr) return;
            if (!IsEnabled(level)) return;
            const auto msg = f();
            Log(level, msg);
        }

        template <class F> requires requires(F f) { { f().size() } -> std::convertible_to<i32>; { f().data() } -> std::convertible_to<const char16*>; }
        void Log(const LogLevel level, F f) const
        {
            if (logger == nullptr) return;
            if (!IsEnabled(level)) return;
            const auto msg = f();
            Log(level, msg);
        }
    };

    struct LibLoadInfo
    {
        void* p_dwrite;
    };

    struct LibUi final : ComImpl<LibUi, ILib>
    {
        LoggerData m_logger{};
        Rc<TextBackend> m_backend{};

        explicit LibUi(LibLoadInfo* info);

        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        void Impl_SetLogger(void* obj, Func<void, void*, LogLevel, StrKind, i32, void*>* logger, Func<u8, void*, LogLevel>* is_enabled, Func<void, void*>* drop);

        COPLT_FORCE_INLINE
        void Impl_ClearLogger();

        COPLT_FORCE_INLINE
        Str8 Impl_GetCurrentErrorMessage();

        COPLT_FORCE_INLINE
        HResult Impl_CreateAtlasAllocator(AtlasAllocatorType Type, i32 Width, i32 Height, IAtlasAllocator** aa);

        COPLT_FORCE_INLINE
        HResult Impl_CreateFrameSource(IFrameSource** fs);

        COPLT_FORCE_INLINE
        HResult Impl_CreateFontManager(IFrameSource* fs, IFontManager** fm);

        COPLT_FORCE_INLINE
        HResult Impl_GetSystemFontCollection(IFontCollection** fc);

        COPLT_FORCE_INLINE
        HResult Impl_GetSystemFontFallback(IFontFallback** ff);

        COPLT_FORCE_INLINE
        HResult Impl_CreateFontFallbackBuilder(IFontFallbackBuilder** ffb, FontFallbackBuilderCreateInfo const* info);

        COPLT_FORCE_INLINE
        HResult Impl_CreateLayout(ILayout** layout);

        COPLT_FORCE_INLINE
        HResult Impl_SplitTexts(NativeList<TextRange>* ranges, char16 const* chars, i32 len);

        COPLT_IMPL_END
    };

    extern "C" COPLT_EXPORT HResultE coplt_ui_create_lib(LibLoadInfo* info, ILib** lib);
}
