#pragma once

#include "Com.h"

namespace Coplt
{
    template <class T>
    struct Arc
    {
        struct Inner
        {
            std::atomic_uint64_t m_count;
            T m_data;

            Inner* AddRef()
            {
                m_count.fetch_add(1, std::memory_order_relaxed);
                return this;
            }
        };

        Inner* m_ptr;

        ~Arc()
        {
            Clear();
        }

        Arc() = default;

        Arc(Arc&& other) noexcept : m_ptr(std::exchange(other.m_ptr, nullptr))
        {
        }

        Arc(const Arc& other) : m_ptr(other.m_ptr ? other.m_ptr->AddRef() : nullptr)
        {
        }

        Arc& operator=(Arc&& other) noexcept
        {
            if (&m_ptr != &other.m_ptr) Arc(std::forward<Arc>(other)).swap(*this);
            return *this;
        }

        Arc& operator=(Arc& other)
        {
            if (m_ptr != other.m_ptr) Arc(other).swap(*this);
            return *this;
        }

        Arc& swap(Arc& r) noexcept
        {
            std::swap(m_ptr, r.m_ptr);
            return *this;
        }

        void Clear()
        {
            if (!m_ptr)return;
            if (m_ptr->m_count.fetch_sub(1, std::memory_order_acq_rel) == 0)
            {
                delete m_ptr;
                m_ptr = nullptr;
            }
        }

        T& operator*() const
        {
            return m_ptr->m_data;
        }

        T* operator->() const
        {
            return &m_ptr->m_data;
        }

        operator bool() const
        {
            return m_ptr;
        }

        T* get() const
        {
            return m_ptr->m_data;
        }

        bool operator==(std::nullptr_t) const
        {
            return m_ptr == nullptr;
        }

        bool operator==(const Arc& other) const
        {
            return m_ptr == other.m_ptr;
        }
    };

    template <class T, class U = T>
    Arc<U>* ffi_arc(NativeArc<T>* arc)
    {
        return reinterpret_cast<Arc<U>*>(arc);
    }
}
