#pragma once

#include <mimalloc.h>

#include "Com.h"

namespace Coplt
{
    template <class T>
    struct List
    {
        static constexpr i32 DefaultCapacity = 4;

        T* m_items{};
        i32 m_cap{};
        i32 m_size{};

        ~List()
        {
            if (!m_items) return;
            Clear();
            mi_free(m_items);
        }

        List()
        {
        }

        explicit List(i32 capacity)
        {
            if (capacity <= 0) capacity = DefaultCapacity;

            m_cap = capacity;
            if (capacity) m_items = mi_malloc_aligned(sizeof(T) * capacity, alignof(T));
        }

        List(const List& other) = delete;

        List(List&& other) noexcept
            : m_items(std::exchange(other.m_items, nullptr)),
              m_cap(std::exchange(other.m_cap, 0)),
              m_size(std::exchange(other.m_size, 0))
        {
        }

        List& swap(List& other) noexcept
        {
            std::swap(m_items, other.m_items);
            std::swap(m_cap, other.m_cap);
            std::swap(m_size, other.m_size);
            return *this;
        }

        List& operator=(const List& other) = delete;

        List& operator=(List&& other) noexcept
        {
            List(std::forward<List>(other)).swap(*this);
            return *this;
        }

        T& operator[](i32 index)
        {
            if (m_items == nullptr || index < 0 || index >= m_size) throw Exception("Index out of range");
            return m_items[index];
        }

        const T& operator[](i32 index) const
        {
            if (m_items == nullptr || index < 0 || index >= m_size) throw Exception("Index out of range");
            return m_items[index];
        }

        T* At(i32 index)
        {
            if (m_items == nullptr || index < 0 || index >= m_size) throw Exception("Index out of range");
            return m_items[index];
        }

        const T* At(i32 index) const
        {
            if (m_items == nullptr || index < 0 || index >= m_size) throw Exception("Index out of range");
            return m_items[index];
        }

        T* TryAt(i32 index)
        {
            if (m_items == nullptr || index < 0 || index >= m_size) return nullptr;
            return m_items[index];
        }

        const T* TryAt(i32 index) const
        {
            if (m_items == nullptr || index < 0 || index >= m_size) return nullptr;
            return m_items[index];
        }

        T* data()
        {
            return m_items;
        }

        const T* data() const
        {
            return m_items;
        }

        usize size() const
        {
            return m_size;
        }

        i32 Count() const
        {
            return m_size;
        }

        i32 Capacity() const
        {
            return m_cap;
        }

        void SetCapacity(const i32 value)
        {
            if (value < m_size) throw Exception("Argument out of range");

            if (m_items == nullptr)
            {
                if (value)
                {
                    m_items = static_cast<T*>(mi_malloc_aligned(sizeof(T) * value, alignof(T)));
                }
            }
            else if (value != m_cap)
            {
                if (m_size > 0)
                {
                    m_items = static_cast<T*>(mi_realloc_aligned(m_items, sizeof(T) * value, alignof(T)));
                }
                else
                {
                    mi_free(m_items);
                    m_items = nullptr;
                }
            }

            m_cap = value;
        }

        i32 GetNewCapacity(i32 capacity)
        {
            auto newCapacity = m_items == nullptr || m_cap == 0 ? DefaultCapacity : 2 * m_cap;
            if (static_cast<u32>(newCapacity) > static_cast<u32>(std::numeric_limits<i32>::max()))
                newCapacity = std::numeric_limits<i32>::max();
            if (newCapacity < capacity) newCapacity = capacity;
            return newCapacity;
        }

        void Grow(const i32 capacity)
        {
            SetCapacity(GetNewCapacity(capacity));
        }

        COPLT_NO_INLINE
        T* UnsafeAddWithResize()
        {
            const auto size = m_size;
            Grow(size + 1);
            m_size = size + 1;
            return &m_items[size];
        }

        T* UnsafeAdd()
        {
            const auto items = m_items;
            const auto size = m_size;
            if (items != nullptr && static_cast<u32>(size) < static_cast<u32>(m_cap))
            {
                m_size = size + 1;
                return &items[size];
            }
            else
            {
                return UnsafeAddWithResize();
            }
        }

        void Add(T&& value)
        {
            new(UnsafeAdd()) T(std::forward<T>(value));
        }

        void Add(const T& value)
        {
            new(UnsafeAdd()) T(value);
        }

        void Clear()
        {
            if (!m_size || !m_items) return;
            if constexpr (!std::is_trivially_destructible_v<T>)
            {
                for (T* item : this)
                {
                    item->~T();
                }
            }
            m_size = 0;
        }

        T RemoveAt(const i32 index)
        {
            if (static_cast<u32>(index) >= static_cast<u32>(m_size)) throw Exception("Index out of range");
            m_size--;
            const auto item = &m_items[index];
            T value = std::move(*item);
            if constexpr (!std::is_trivially_destructible_v<T>) item->~T();
            if (index < m_size)
            {
                std::memmove(m_items + index, m_items + index + 1, (m_size - index) * sizeof(T));
            }
            return value;
        }

        T* begin() const
        {
            return m_items;
        }

        T* end() const
        {
            return m_items + m_size;
        }
    };

    template <class T, class U = T>
    List<U>* ffi_list(NativeList<T>* list)
    {
        return reinterpret_cast<List<U>*>(list);
    }
}
