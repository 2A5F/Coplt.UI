#pragma once

#include <mimalloc.h>

#include "Com.h"
#include "Hash.h"

namespace Coplt
{
    struct MapEntryDataOnly
    {
        i32 HashCode;
        /// <summary>
        /// 0-based index of next entry in chain: -1 means end of chain
        /// also encodes whether this entry _itself_ is part of the free list by changing sign and subtracting 3,
        /// so -2 means end of free list, -3 means index 0 but on free list, -4 means index 1 but on free list, etc.
        /// </summary>
        i32 Next;
    };

    template <class TKey>
    struct MapEntryKeyOnly : MapEntryDataOnly
    {
        TKey Key; // Key of entry
    };

    template <class TKey, class TValue>
    struct MapEntry : MapEntryKeyOnly<TKey>
    {
        TValue Value; // Value of entry
    };

    template <class TKey, class TValue>
    struct MapEntryOutput
    {
        MapEntry<TKey, TValue>* m_entry;
        i32 m_index;
        bool m_exists;
        bool m_exists_key;
        bool m_exists_value;

        i32 Index() const { return m_index; }

        bool Exists() const { return m_exists; }

        TKey& GetKey() const
        {
            if (!m_entry) throw Exception("Entry is null");
            if (!m_exists_key) throw Exception("Key is not exists");
            return m_entry->Key;
        }

        TValue& GetValue() const
        {
            if (!m_entry) throw Exception("Entry is null");
            if (!m_exists_value) throw Exception("Key is not exists");
            return m_entry->Value;
        }

        TKey* PtrKey() const
        {
            return std::addressof(m_entry->Key);
        }

        TValue* PtrValue() const
        {
            return std::addressof(m_entry->Value);
        }

        TKey& SetKey(const TKey& key)
        {
            if (!m_entry) throw Exception("Entry is null");
            if (m_exists_key) m_entry->Key = key;
            else
            {
                new(std::addressof(m_entry->Key)) TKey(key);
                m_exists_key = true;
            }
            return m_entry->Key;
        }

        TKey& SetKey(TKey&& key)
        {
            if (!m_entry) throw Exception("Entry is null");
            if (m_exists_key) m_entry->Key = std::forward<TKey>(key);
            else
            {
                new(std::addressof(m_entry->Key)) TKey(std::forward<TKey>(key));
                m_exists_key = true;
            }
            return m_entry->Key;
        }

        TValue& SetValue(const TValue& value)
        {
            if (!m_entry) throw Exception("Entry is null");
            if (m_exists_value) m_entry->Value = value;
            else
            {
                new(std::addressof(m_entry->Value)) TValue(value);
                m_exists_value = true;
            }
            return m_entry->Value;
        }

        TValue& SetValue(TValue&& value)
        {
            if (!m_entry) throw Exception("Entry is null");
            if (m_exists_value) m_entry->Value = std::forward<TValue>(value);
            else
            {
                new(std::addressof(m_entry->Value)) TValue(std::forward<TValue>(value));
                m_exists_value = true;
            }
            return m_entry->Value;
        }

        explicit operator bool() const { return m_entry && m_exists; }

        MapEntryOutput() = default;

        MapEntryOutput(MapEntry<TKey, TValue>* entry, const i32 index, const bool exists)
            : m_entry(entry), m_index(index), m_exists(exists), m_exists_key(exists), m_exists_value(exists)
        {
        }

        MapEntryOutput(MapEntry<TKey, TValue>* entry, const i32 index, const bool exists, const bool exists_key, const bool exists_value)
            : m_entry(entry), m_index(index), m_exists(exists), m_exists_key(exists_key), m_exists_value(exists_value)
        {
        }
    };

    template <class TKey, class TValue, Hash<TKey> THash = DefaultHash<TKey>, Eq<TKey> TEq = DefaultEq<TKey>>
    struct Map : FFIMap
    {
        using Entry = MapEntry<TKey, TValue>;

        const i32 StartOfFreeList = -3;

    private:
        Entry* get_m_entries() const
        {
            return static_cast<Entry*>(m_entries);
        }

    public:
        using EntryOutput = MapEntryOutput<TKey, TValue>;

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
            return &buckets[HashHelpers::FastMod(
                static_cast<u32>(hash_code), static_cast<u32>(m_cap),
                m_fast_mode_multiplier
            )];
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
                    entry.Next = *bucket - 1; // Value in _buckets is 1-based
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
                entry.Next = *bucket - 1;
                new(std::addressof(entry.Key)) TKey(std::forward<TKey>(key));
                new(std::addressof(entry.Value)) TValue(std::forward<TValue>(value));
                *bucket = index + 1; // Value in _buckets is 1-based
            }
            return InsertResult::AddNew;
        }

        EntryOutput FindValue(const TKey& key) const
        {
            return FindValue<TKey, THash, TEq>(key);
        }

        template <class Q, Hash<Q> QHash = DefaultHash<Q>, Eq<Q> QEq = DefaultEq<Q, TKey>>
        EntryOutput FindValue(const Q& key) const
        {
            if (!m_buckets) return EntryOutput();

            auto entries = get_m_entries();

            auto hash_code = QHash::GetHashCode(key);
            auto i = *GetBucket(hash_code);
            u32 collision_count = 0;

            --i;
            do
            {
                // Test in if to drop range check for following array access
                if (static_cast<u32>(i) >= static_cast<u32>(m_cap)) return EntryOutput();

                auto& entry = entries[i];
                if (entry.HashCode == hash_code && QEq::Equals(key, entry.Key))
                    return EntryOutput(&entry, i, true);

                i = entry.Next;

                collision_count++;
            }
            while (collision_count <= static_cast<u32>(m_cap));

            throw Exception("Concurrent operations are not supported");
        }

    public:
        EntryOutput GetValueRefOrUninitializedValue(TKey&& key)
        {
            auto r = GetValueRefOrUninitialized(key);
            r.SetKey(std::forward<TKey>(key));
            return r;
        }

        EntryOutput GetValueRefOrUninitializedValue(const TKey& key)
        {
            auto r = GetValueRefOrUninitialized(key);
            r.SetKey(key);
            return r;
        }

        EntryOutput GetValueRefOrUninitialized(const TKey& key)
        {
            return GetValueRefOrUninitialized<TKey, THash, TEq>(key);
        }

        template <class Q, Hash<Q> QHash = DefaultHash<Q>, Eq<Q> QEq = DefaultEq<Q, TKey>>
        EntryOutput GetValueRefOrUninitialized(const Q& key)
        {
            if (m_buckets == nullptr) Initialize(0);

            auto entries = get_m_entries();

            const auto hash_code = QHash::GetHashCode(key);

            u32 collision_count = 0;
            auto bucket = GetBucket(hash_code);
            auto i = *bucket - 1;

            while (static_cast<u32>(i) < static_cast<u32>(m_cap))
            {
                auto& entry = entries[i];
                if (entry.HashCode == hash_code && QEq::Equals(key, entry.Key))
                {
                    return EntryOutput(&entry, i, true);
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
                const auto count = m_count;
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
                entry.Next = *bucket - 1; // Value in _buckets is 1-based
                *bucket = index + 1; // Value in _buckets is 1-based
                return EntryOutput(&entry, index, false);
            }
        }

        bool TryAdd(const TKey& key, TValue&& value)
        {
            return TryAdd(TKey(key), std::forward<TValue>(value));
        }

        bool TryAdd(const TKey& key, const TValue& value)
        {
            return TryAdd(TKey(key), TValue(value));
        }

        bool TryAdd(TKey&& key, const TValue& value)
        {
            return TryInsert(std::forward<TKey>(key), TValue(value), false) == InsertResult::AddNew;
        }

        bool TryAdd(TKey&& key, TValue&& value)
        {
            return TryInsert(std::forward<TKey>(key), std::forward<TValue>(value), false) == InsertResult::AddNew;
        }

        bool Set(const TKey& key, TValue&& value)
        {
            return TryInsert(TKey(key), std::forward<TValue>(value), true) == InsertResult::AddNew;
        }

        bool Set(const TKey& key, const TValue& value)
        {
            return TryInsert(TKey(key), TValue(value), true) == InsertResult::AddNew;
        }

        bool Set(TKey&& key, const TValue& value)
        {
            return TryInsert(std::forward<TKey>(key), TValue(value), true) == InsertResult::AddNew;
        }

        bool Set(TKey&& key, TValue&& value)
        {
            return TryInsert(std::forward<TKey>(key), std::forward<TValue>(value), true) == InsertResult::AddNew;
        }

        bool Contains(const TKey& key)
        {
            return FindValue(key);
        }

        template <class Q, Hash<Q> QHash = DefaultHash<Q>, Eq<Q> QEq = DefaultEq<Q, TKey>>
        bool Contains(const Q& key)
        {
            return FindValue<Q, QHash, QEq>(key);
        }

        EntryOutput TryGet(const TKey& key)
        {
            return FindValue(key);
        }

        template <class Q, Hash<Q> QHash = DefaultHash<Q>, Eq<Q> QEq = DefaultEq<Q, TKey>>
        EntryOutput TryGet(const Q& key)
        {
            return FindValue<Q, QHash, QEq>(key);
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

                    std::pair<TKey, TValue> r = std::make_pair(TKey(std::move(entry.Key)), TValue(std::move(entry.Value)));
                    entry.Key.~TKey();
                    entry.Value.~TValue();

                    m_free_list = i;
                    m_free_count++;

                    return std::optional<std::pair<TKey, TValue>>{std::move(r)};
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
            explicit Enumerator(const Map* self)
                : self(self), cur(nullptr), index(0)
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
