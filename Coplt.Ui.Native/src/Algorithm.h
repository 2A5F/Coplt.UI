#pragma once

namespace Coplt::Algorithm
{
    template <class T, class K, Fn<i32, const T&, const K&> F>
    constexpr i32 BinarySearch(T* Start, const i32 Index, const i32 Length, const K& Value, const F& Comparer)
    {
        i32 lo = Index;
        i32 hi = Index + Length - 1;
        while (lo <= hi)
        {
            const i32 i = lo + ((hi - lo) >> 1);
            const i32 order = Comparer(Start[i], Value);

            if (order == 0) return i;
            if (order < 0)
            {
                lo = i + 1;
            }
            else
            {
                hi = i - 1;
            }
        }

        return ~lo;
    }

    template <class T, class K, Fn<i32, const T&, const K&> F>
    constexpr i32 BinarySearch(T* Start, const i32 Length, const K& Value, const F& Comparer)
    {
        return BinarySearch(Start, 0, Length, Value, Comparer);
    }
}
