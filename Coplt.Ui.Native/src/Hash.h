#pragma once

#include <concepts>
#include "Com.h"

namespace Coplt
{
    enum class InsertResult
    {
        None,
        AddNew,
        Overwrite,
    };

    template <class T>
    concept SelfHash = requires(const T& value)
    {
        { value.GetHashCode() } -> std::convertible_to<i32>;
    };

    template <class Self, class T>
    concept Hash = requires(const T& value)
    {
        { Self::GetHashCode(value) } -> std::convertible_to<i32>;
    };

    template <class Self, class A, class B = A>
    concept Eq = requires(const A& a, const B& b)
    {
        { Self::Equals(a, b) } -> std::convertible_to<bool>;
    };

    template <class T>
    struct DefaultHash
    {
        static i32 GetHashCode(const T& value)
        {
            if constexpr (SelfHash<T>)
            {
                return value.GetHashCode();
            }
            else if constexpr (std::is_pointer_v<T>)
            {
                return static_cast<i32>(reinterpret_cast<uintptr_t>(value));
            }
            else if constexpr (std::is_integral_v<T>)
            {
                return static_cast<i32>(value);
            }
            else
            {
                // Fallback to std::hash
                return static_cast<i32>(std::hash<T>{}(value));
            }
        }
    };

    template <class A, class B = A>
    struct DefaultEq
    {
        static bool Equals(const A& a, const B& b)
        {
            if constexpr (requires { a == b; })
            {
                return a == b;
            }
            else if constexpr (requires { std::equal_to<>{}(a, b); })
            {
                return std::equal_to<>{}(a, b);
            }
            else
            {
                static_assert(false, "Not implemented equals");
                return false;
            }
        }
    };
}

namespace Coplt::HashHelpers
{
    constexpr u32 HashCollisionThreshold = 100;
    // This is the maximum prime smaller than Array.MaxLength.
    constexpr i32 MaxPrimeArrayLength = 0x7FFFFFC3;

    constexpr i32 HashPrime = 101;

    constexpr i32 Primes[] = {
        3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
        1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
        17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
        187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
        1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369
    };

    constexpr bool IsPrime(const i32 candidate)
    {
        if ((candidate & 1) != 0)
        {
            i32 limit = static_cast<i32>(std::sqrt(candidate));
            for (i32 divisor = 3; divisor <= limit; divisor += 2)
            {
                if ((candidate % divisor) == 0)
                    return false;
            }
            return true;
        }
        return candidate == 2;
    }

    constexpr i32 GetPrime(const i32 min)
    {
        if (min < 0) throw Exception();

        for (i32 i = 0; i < std::size(Primes); ++i)
        {
            if (const auto prime = Primes[i]; prime >= min)
                return prime;
        }

        // Outside of our predefined table. Compute the hard way.
        for (i32 i = (min | 1); i < std::numeric_limits<i32>::max(); i += 2)
        {
            if (IsPrime(i) && ((i - 1) % HashPrime != 0))
                return i;
        }
        return min;
    }

    /// <summary>Returns approximate reciprocal of the divisor: ceil(2**64 / divisor).</summary>
    /// <remarks>This should only be used on 64-bit.</remarks>
    constexpr u64 GetFastModMultiplier(const u32 divisor)
    {
        return std::numeric_limits<u64>::max() / divisor + 1;
    }

    COPLT_FORCE_INLINE
    constexpr u32 FastMod(const u32 value, const u32 divisor, const u64 multiplier)
    {
        return static_cast<u32>(((((multiplier * value) >> 32) + 1) * divisor) >> 32);
    }

    // Returns size of hashtable to grow to.
    constexpr i32 ExpandPrime(const i32 oldSize)
    {
        const i32 newSize = 2 * oldSize;

        // Allow the hashtables to grow to maximum possible size (~2G elements) before encountering capacity overflow.
        // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
        if (static_cast<u32>(newSize) > MaxPrimeArrayLength && MaxPrimeArrayLength > oldSize)
        {
            return MaxPrimeArrayLength;
        }

        return GetPrime(newSize);
    }
}
