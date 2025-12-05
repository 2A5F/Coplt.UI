#pragma once

#ifdef _WINDOWS_
#include <windows.h>
#endif

#include <cpptrace/from_current.hpp>
#include <fmt/format.h>
#include <type_traits>

#include "CoCom.Interface.h"
#include "Error.h"

namespace Coplt
{
    COPLT_NO_INLINE
    inline void OnCatchException(const std::exception& e)
    {
        SetCurrentErrorMessage(std::string(e.what()));
    }

    COPLT_NO_INLINE
    inline void OnCatchException(const NullPointerError& e)
    {
        SetCurrentErrorMessage(e.build_message());
    }

    COPLT_NO_INLINE
    inline void OnCatchException(const Exception& e)
    {
        SetCurrentErrorMessage(e.ToString());
    }

    template <class T>
    COPLT_FORCE_INLINE auto DefaultReturnOnError()
    {
        if constexpr (std::is_same_v<T, void>)
        {
        }
        else if constexpr (std::is_same_v<T, HResult>)
        {
            return HResult(HResultE::Fail);
        }
        else if constexpr (std::is_same_v<T, HResultE>)
        {
            return HResultE::Fail;
        }
        else if constexpr (std::is_same_v<T, HRESULT>)
        {
            return E_FAIL;
        }
        else
        {
            T t;
            return t;
        }
    }

    template <std::invocable F>
    auto feb(F&& f) noexcept
    {
        using return_type = std::invoke_result_t<F>;
        try
        {
            return std::invoke(std::forward<F>(f));
        }
        catch (const Exception& e)
        {
            OnCatchException(e);
            return DefaultReturnOnError<return_type>();
        }
        catch (const NullPointerError& e)
        {
            OnCatchException(e);
            return DefaultReturnOnError<return_type>();
        }
        catch (const std::exception& e)
        {
            OnCatchException(e);
            return DefaultReturnOnError<return_type>();
        }
        catch (const HResultE& e)
        {
            if constexpr (std::is_same_v<return_type, HResult>)
            {
                return HResult(e);
            }
            else if constexpr (std::is_same_v<return_type, HResultE>)
            {
                return e;
            }
            return DefaultReturnOnError<return_type>();
        }
        catch (const HResult& e)
        {
            if constexpr (std::is_same_v<return_type, HResult>)
            {
                return e;
            }
            else if constexpr (std::is_same_v<return_type, HResultE>)
            {
                return static_cast<HResultE>(e);
            }
            return DefaultReturnOnError<return_type>();
        }
    }
}

#include "../api/Interface.h"
