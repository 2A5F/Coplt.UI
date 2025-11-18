#pragma once

#include "Com.h"
#include "Assert.h"

namespace Coplt
{
    template <class T>
    struct StrideSpan
    {
        T* m_ptr;
        u32 m_stride;
        u32 m_count;

        StrideSpan() = default;

        explicit StrideSpan(T* ptr, const u32 stride, const u32 count)
            : m_ptr(ptr), m_stride(stride), m_count(count)
        {
        }

        T* ptr() const { return m_ptr; }
        u32 stride() const { return m_stride; }
        u32 count() const { return m_count; }

        T* at(const u32 index) const
        {
            COPLT_DEBUG_ASSERT(index < m_count);
            return reinterpret_cast<T*>(reinterpret_cast<usize>(m_ptr) + index * m_stride);
        }

        T& operator[](const u32 index) const
        {
            return *at(index);
        }
    };
}
