#pragma once
#include "Com.h"
#include "Defines.h"
#include "Backend.h"

namespace Coplt
{
    struct LoggerData
    {
        void* obj{};
        Func<void, LogLevel, i32, char16*>* logger{};
        Func<void, void*>* drop{};

        LoggerData() = default;

        explicit LoggerData(
            void* obj, Func<void, LogLevel, i32, char16*>* logger, Func<void, void*>* drop
        ) : obj(obj), logger(logger), drop(drop)
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
            std::swap(drop, other.drop);
            return *this;
        }
    };

    struct LibUi final : ComImpl<LibUi, ILib>
    {
        LoggerData m_logger{};
        Rc<TextBackend> m_backend{};

        HResult Backend();

        COPLT_IMPL_START

        COPLT_FORCE_INLINE
        void Impl_SetLogger(void* obj, Func<void, LogLevel, i32, char16*>* logger, Func<void, void*>* drop);

        COPLT_FORCE_INLINE
        Str8 Impl_GetCurrentErrorMessage();

        COPLT_FORCE_INLINE
        void* Impl_Alloc(i32 size, i32 align) const;

        COPLT_FORCE_INLINE
        void Impl_Free(void* ptr, i32 align) const;

        COPLT_FORCE_INLINE
        void* Impl_ZAlloc(i32 size, i32 align) const;

        COPLT_FORCE_INLINE
        void* Impl_ReAlloc(void* ptr, i32 size, i32 align) const;

        COPLT_FORCE_INLINE
        HResult Impl_GetSystemFontCollection(IFontCollection** fc);

        COPLT_FORCE_INLINE
        HResult Impl_GetSystemFontFallback(IFontFallback** ff);

        COPLT_FORCE_INLINE
        HResult Impl_CreateLayout(ILayout** layout);

        COPLT_IMPL_END
    };

    extern "C" COPLT_EXPORT ILib* Coplt_CreateLibUi();
}
