#pragma once

#include "Com.h"
#include "Hash.h"

namespace Coplt
{
    template <class TKey, class TValue, Hash<TKey> THash = DefaultHash<TKey>, Eq<TKey> TEq = DefaultEq<TKey>>
    struct Map : FFIMap
    {
        const i32 StartOfFreeList = -3;

        struct Entry
        {
            i32 HashCode;
            /// <summary>
            /// 0-based index of next entry in chain: -1 means end of chain
            /// also encodes whether this entry _itself_ is part of the free list by changing sign and subtracting 3,
            /// so -2 means end of free list, -3 means index 0 but on free list, -4 means index 1 but on free list, etc.
            /// </summary>
            i32 Next;
            TKey Key; // Key of entry
            TValue Value; // Value of entry
        };

    private:
        Entry* get_m_entries() const
        {
            return static_cast<Entry*>(m_entries);
        }

    public:
        TKey* UnsafeKeyAt(i32 index) const
        {
            return std::addressof(get_m_entries()[index].Key);
        }

        TKey* UnsafeKeyAt(u32 index) const
        {
            return std::addressof(get_m_entries()[index].Key);
        }

        TValue* UnsafeAt(i32 index) const
        {
            return std::addressof(get_m_entries()[index].Value);
        }

        TValue* UnsafeAt(u32 index) const
        {
            return std::addressof(get_m_entries()[index].Value);
        }

        i32 Count() const { return m_count - m_free_count; }
        i32 Capacity() const { return m_cap; }

        Map() = default;

        explicit Map(const i32 capacity)
        {
            if (capacity < 0) throw Exception();
            Initialize(capacity);
        }

    private:
        i32 Initialize(const i32 capacity)
        {
            const auto size = HashHelpers::GetPrime(capacity);

            m_free_list = -1;
            m_buckets = static_cast<i32*>(mi_zalloc_aligned(size * sizeof(i32), alignof(i32)));
            m_entries = static_cast<Entry*>(mi_malloc_aligned(size * sizeof(Entry), alignof(Entry)));
            m_fast_mode_multiplier = HashHelpers::GetFastModMultiplier(static_cast<u32>(size));
            m_cap = size;

            return size;
        }

        i32* GetBucket(const i32 hash_code) const
        {
            const auto buckets = m_buckets;
            return &buckets[HashHelpers::FastMod(static_cast<u32>(hash_code), static_cast<u32>(m_cap),
                                                 m_fast_mode_multiplier)];
        }

        void Resize()
        {
            Resize(HashHelpers::ExpandPrime(m_count));
        }

        void Resize(const i32 new_size)
        {
            m_entries = static_cast<Entry*>(mi_realloc_aligned(m_entries, new_size * sizeof(Entry), alignof(Entry)));

            mi_free(m_buckets);
            m_buckets = static_cast<i32*>(mi_zalloc_aligned(new_size * sizeof(i32), alignof(i32)));

            const auto count = m_count;
            m_fast_mode_multiplier = HashHelpers::GetFastModMultiplier(static_cast<u32>(new_size));
            for (i32 i = 0; i < count; i++)
            {
                Entry& entry = get_m_entries()[i];
                if (entry.Next >= -1)
                {
                    i32* bucket = GetBucket(entry.HashCode);
                    entry.Next = bucket - 1; // Value in _buckets is 1-based
                    *bucket = i + 1;
                }
            }

            m_cap = new_size;
        }

        InsertResult TryInsert(TKey&& key, TValue&& value, bool overwrite)
        {
            if (m_buckets == nullptr) Initialize(0);

            auto entries = get_m_entries();

            auto hash_code = THash::GetHashCode(key);

            u32 collision_count = 0;
            auto bucket = GetBucket(hash_code);
            auto i = *bucket - 1; // Value in _buckets is 1-based

            while (static_cast<u32>(i) < static_cast<u32>(m_cap))
            {
                auto& entry = entries[i];
                if (entry.HashCode == hash_code && TEq::Equals(key, entry.Key))
                {
                    if (overwrite)
                    {
                        entry.Value = std::forward<TValue>(value);
                        return InsertResult::Overwrite;
                    }

                    return InsertResult::None;
                }

                i = entry.Next;

                collision_count++;
                if (collision_count > static_cast<u32>(m_cap))
                {
                    throw Exception("Concurrent operations are not supported");
                }
            }

            i32 index;
            if (m_free_count > 0)
            {
                index = m_free_list;
                m_free_list = StartOfFreeList - entries[m_free_list].Next;
                m_free_count--;
            }
            else
            {
                auto count = m_count;
                if (count == m_cap)
                {
                    Resize();
                    bucket = GetBucket(hash_code);
                }
                index = count;
                m_count = count + 1;
                entries = get_m_entries();
            }

            {
                auto& entry = entries[index];
                entry.HashCode = hash_code;
                entry.Next = bucket - 1;
                new(std::addressof(entry.Key)) TKey(std::forward<TKey>(key));
                new(std::addressof(entry.Value)) TKey(std::forward<TValue>(value));
                *bucket = index + 1; // Value in _buckets is 1-based
            }
            return InsertResult::AddNew;
        }

        Entry* FindValue(const TKey& key) const
        {
            return FindValue<TKey, THash, TEq>(key);
        }

        template <class Q, Hash<Q> QHash = DefaultHash<Q>, Eq<Q> QEq = DefaultEq<Q, TKey>>
        Entry* FindValue(const Q& key) const
        {
            if (!m_buckets) return nullptr;

            auto entries = get_m_entries();

            auto hash_code = QHash::GetHashCode(key);
            auto i = *GetBucket(hash_code);
            u32 collision_count = 0;

            --i;
            do
            {
                // Test in if to drop range check for following array access
                if (static_cast<u32>(i) >= static_cast<u32>(m_cap)) return nullptr;

                auto& entry = entries[i];
                if (entry.HashCode == hash_code && QEq::Equals(key, entry.Key))
                    return &entry;

                i = entry.Next;

                collision_count++;
            }
            while (collision_count <= static_cast<u32>(m_cap));

            throw Exception("Concurrent operations are not supported");
        }

    public:
        bool TryAdd(TKey&& key, TValue&& value)
        {
            return TryInsert(std::forward<TKey>(key), std::forward<TValue>(value), false) == InsertResult::AddNew;
        }

        bool Set(TKey&& key, TValue&& value)
        {
            return TryInsert(std::forward<TKey>(key), std::forward<TValue>(value), true) == InsertResult::AddNew;
        }

        bool Contains(const TKey& key)
        {
            return FindValue(key) != nullptr;
        }

        template <class Q, Hash<Q> QHash = DefaultHash<Q>, Eq<Q> QEq = DefaultEq<Q, TKey>>
        bool Contains(const Q& key)
        {
            return FindValue<Q, QHash, QEq>(key) != nullptr;
        }

        std::optional<std::pair<TKey*, TValue*>> TryGet(const TKey& key)
        {
            const auto entry = FindValue(key);
            if (entry == nullptr) return std::nullopt;
            return std::make_pair(std::addressof(entry->Key), std::addressof(entry->Value));
        }

        template <class Q, Hash<Q> QHash = DefaultHash<Q>, Eq<Q> QEq = DefaultEq<Q, TKey>>
        std::optional<std::pair<TKey*, TValue*>> TryGet(const Q& key)
        {
            const auto entry = FindValue<Q, QHash, QEq>(key);
            if (entry == nullptr) return std::nullopt;
            return std::make_pair(std::addressof(entry->Key), std::addressof(entry->Value));
        }

        std::optional<std::pair<TKey, TValue>> Remove(const TKey& key)
        {
            return Remove<TKey, THash, TEq>(key);
        }

        template <class Q, Hash<Q> QHash = DefaultHash<Q>, Eq<Q> QEq = DefaultEq<Q, TKey>>
        std::optional<std::pair<TKey, TValue>> Remove(const Q& key)
        {
            if (!m_buckets) return std::nullopt;
            auto entries = get_m_entries();

            u32 collision_count = 0;
            auto hash_code = QHash::GetHashCode(key);

            auto bucket = GetBucket(hash_code);
            i32 last = -1;
            auto i = *bucket - 1; // Value in _buckets is 1-based
            while (i >= 0)
            {
                auto& entry = entries[i];

                if (entry.HashCode == hash_code && QEq::Equals(key, entry.Key))
                {
                    if (last < 0)
                    {
                        *bucket = entry.Next + 1;
                    }
                    else
                    {
                        entries[last].Next = entry.Next;
                    }

                    entry.Next = StartOfFreeList - m_free_list;

                    const auto r = std::make_pair(TKey(std::move(entry.Key)), TValue(std::move(entry.Value)));
                    entry.Key.~TKey();
                    entry.Value.~TValue();

                    m_free_list = i;
                    m_free_count++;

                    return r;
                }

                last = i;
                i = entry.Next;

                collision_count++;
                if (collision_count > static_cast<u32>(m_cap))
                {
                    throw Exception("Concurrent operations are not supported");
                }
            }

            return std::nullopt;
        }

        struct Enumerator
        {
        private:
            const Map* self;
            Entry* cur;
            i32 index;

        public:
            explicit Enumerator(const Map* self) : self(self), cur(nullptr), index(0)
            {
            }

            bool MoveNext()
            {
                while (static_cast<u32>(index) < static_cast<u32>(self->m_count))
                {
                    auto& entry = self->get_m_entries()[index++];

                    if (entry.Next >= -1)
                    {
                        cur = &entry;
                        return true;
                    }
                }

                index = self->m_count + 1;
                cur = nullptr;
                return false;
            }

            std::pair<TKey*, TValue*> Current()
            {
                return std::make_pair(std::addressof(cur->Key), std::addressof(cur->Value));
            }
        };

        Enumerator GetEnumerator() const
        {
            return Enumerator(this);
        }

        void Clear()
        {
            auto count = m_count;
            if (count <= 0) return;

            if constexpr (!std::is_trivially_destructible_v<TKey> || !std::is_trivially_destructible_v<TValue>)
            {
                auto e = GetEnumerator();
                while (e.MoveNext())
                {
                    auto [key, value] = e.Current();
                    key->~TKey();
                    value->~TValue();
                }
            }

            std::fill_n(m_buckets, m_cap, 0);
            m_count = 0;
            m_free_list = -1;
            m_free_count = 0;
            std::fill_n(get_m_entries(), count, 0);
        }

        ~Map()
        {
            if (!m_buckets) return;
            if constexpr (!std::is_trivially_destructible_v<TKey> || !std::is_trivially_destructible_v<TValue>)
            {
                auto e = GetEnumerator();
                while (e.MoveNext())
                {
                    auto [key, value] = e.Current();
                    key->~TKey();
                    value->~TValue();
                }
            }
            mi_free(m_buckets);
            mi_free(m_entries);
            m_buckets = nullptr;
            m_entries = nullptr;
            m_fast_mode_multiplier = 0;
            m_cap = 0;
            m_count = 0;
            m_free_list = 0;
            m_free_count = 0;
        }

        Map& swap(Map& other) noexcept
        {
            std::swap(m_buckets, other.m_buckets);
            std::swap(m_entries, other.m_entries);
            std::swap(m_fast_mode_multiplier, other.m_fast_mode_multiplier);
            std::swap(m_cap, other.m_cap);
            std::swap(m_count, other.m_count);
            std::swap(m_free_list, other.m_free_list);
            std::swap(m_free_count, other.m_free_count);
            return *this;
        }

        Map(const Map& other) = delete;
        Map& operator=(const Map& other) = delete;

        Map(Map&& other) noexcept
        {
            m_buckets = std::exchange(other.m_buckets, nullptr);
            m_entries = std::exchange(other.m_entries, nullptr);
            m_fast_mode_multiplier = std::exchange(other.m_fast_mode_multiplier, 0);
            m_cap = std::exchange(other.m_cap, 0);
            m_count = std::exchange(other.m_count, 0);
            m_free_list = std::exchange(other.m_free_list, 0);
            m_free_count = std::exchange(other.m_free_count, 0);
        }

        Map& operator=(Map&& other) noexcept
        {
            Map(std::forward<Map>(other)).swap(*this);
            return *this;
        }
    };

    template <class TKey, class TValue>
    Map<TKey, TValue>* ffi_map(FFIMap* list)
    {
        return reinterpret_cast<Map<TKey, TValue>*>(list);
    }

    template <class TKey, class TValue>
    const Map<TKey, TValue>* ffi_map(const FFIMap* list)
    {
        return reinterpret_cast<const Map<TKey, TValue>*>(list);
    }
}
