#pragma once

#include "Com.h"

namespace Coplt
{
    template <class T>
    struct List
    {
        // todo

        void Add(T&& value)
        {
            // todo
        }

        void Add(const T& value)
        {
            // todo
        }
    };

    template <class T, class U = T>
    List<U>* ffi_list(NativeList<T>* list)
    {
        return reinterpret_cast<List<U>*>(list);
    }
}
