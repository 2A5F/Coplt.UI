using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Native;
using Coplt.UI.Utilities;

namespace Coplt.UI.Collections;

[Dropping]
public unsafe partial struct NativeMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
{
    #region Static Check

    static NativeMap()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<TKey>() || RuntimeHelpers.IsReferenceOrContainsReferences<TValue>())
            throw new NotSupportedException("Only unmanaged types are supported");
    }

    #endregion

    #region Consts

    private const int StartOfFreeList = -3;

    #endregion

    #region Entry

    internal record struct Entry
    {
        public int HashCode;
        /// <summary>
        /// 0-based index of next entry in chain: -1 means end of chain
        /// also encodes whether this entry _itself_ is part of the free list by changing sign and subtracting 3,
        /// so -2 means end of free list, -3 means index 0 but on free list, -4 means index 1 but on free list, etc.
        /// </summary>
        public int Next;
        public TKey Key; // Key of entry
        public TValue Value; // Value of entry
    }

    #endregion

    #region Fields

    private int* m_buckets;
    private Entry* m_entries;
    private ulong m_fast_mode_multiplier;
    private int m_cap;
    private int m_count;
    private int m_free_list;
    private int m_free_count;

    #endregion

    #region Props

    public int Count => m_count - m_free_count;

    public int Capacity => m_cap;

    #endregion

    #region Ctor

    public NativeMap(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException();
        Initialize(capacity);
    }

    #endregion

    #region Private

    #region Initialize

    [UnscopedRef]
    private int Initialize(int capacity)
    {
        var size = HashHelpers.GetPrime(capacity);
        var lib = NativeLib.Instance;

        // Assign member variables after both arrays are allocated to guard against corruption from OOM if second fails.
        m_free_list = -1;
        m_buckets = lib.ZAlloc<int>(size);
        m_entries = lib.Alloc<Entry>(size);
        m_fast_mode_multiplier = HashHelpers.GetFastModMultiplier((uint)size);
        m_cap = size;

        return size;
    }

    #endregion

    #region GetBucket

    [UnscopedRef]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly ref int GetBucket(int hash_code)
    {
        var buckets = m_buckets!;
        return ref buckets[HashHelpers.FastMod((uint)hash_code, (uint)m_cap, m_fast_mode_multiplier)];
    }

    #endregion

    #region Resize

    [UnscopedRef]
    private void Resize() => Resize(HashHelpers.ExpandPrime(m_count));
    [UnscopedRef]
    private void Resize(int new_size)
    {
        Debug.Assert(m_entries != null, "m_nodes should be non-null");
        Debug.Assert(new_size >= m_cap);

        var lib = NativeLib.Instance;

        m_entries = lib.ReAlloc(m_entries, new_size);

        lib.Free(m_buckets);
        m_buckets = lib.ZAlloc<int>(new_size);

        var count = m_count;
        m_fast_mode_multiplier = HashHelpers.GetFastModMultiplier((uint)new_size);
        for (var i = 0; i < count; i++)
        {
            ref var entry = ref m_entries[i];
            if (entry.Next >= -1)
            {
                ref int bucket = ref GetBucket(entry.HashCode);
                entry.Next = bucket - 1; // Value in _buckets is 1-based
                bucket = i + 1;
            }
        }

        m_cap = new_size;
    }

    #endregion

    #region TryInsert

    [UnscopedRef]
    private InsertResult TryInsert(TKey key, TValue value, bool overwrite)
    {
        if (m_buckets == null) Initialize(0);
        Debug.Assert(m_buckets != null);

        var entries = m_entries;
        Debug.Assert(entries != null, "expected entries to be non-null");

        var hash_code = key == null ? 0 : EqualityComparer<TKey>.Default.GetHashCode(key);

        uint collision_count = 0;
        ref var bucket = ref GetBucket(hash_code);
        var i = bucket - 1; // Value in _buckets is 1-based

        while ((uint)i < (uint)m_cap)
        {
            ref var entry = ref entries[i];
            if (entry.HashCode == hash_code && EqualityComparer<TKey>.Default.Equals(key, entry.Key))
            {
                if (overwrite)
                {
                    DisposeProxy.TryDispose(ref entry.Value);
                    entry.Value = value;
                    return InsertResult.Overwrite;
                }

                return InsertResult.None;
            }

            i = entry.Next;

            collision_count++;
            if (collision_count > (uint)m_cap)
            {
                throw new InvalidOperationException("Concurrent operations are not supported");
            }
        }

        int index;
        if (m_free_count > 0)
        {
            index = m_free_list;
            Debug.Assert((StartOfFreeList - entries[m_free_list].Next) >= -1, "shouldn't overflow because `next` cannot underflow");
            m_free_list = StartOfFreeList - entries[m_free_list].Next;
            m_free_count--;
        }
        else
        {
            var count = m_count;
            if (count == m_cap)
            {
                Resize();
                bucket = ref GetBucket(hash_code);
            }
            index = count;
            m_count = count + 1;
            entries = m_entries;
        }

        {
            ref var entry = ref entries![index];
            entry.HashCode = hash_code;
            entry.Next = bucket - 1; // Value in _buckets is 1-based
            entry.Key = key;
            entry.Value = value;
            bucket = index + 1; // Value in _buckets is 1-based
        }

        return InsertResult.AddNew;
    }

    #endregion

    #region FindValue

    [UnscopedRef]
    internal readonly ref TValue FindValue(TKey key)
    {
        if (m_buckets == null) goto ReturnNotFound;

        var entries = m_entries;
        Debug.Assert(entries != null, "expected entries to be != null");

        var hash_code = key == null ? 0 : EqualityComparer<TKey>.Default.GetHashCode(key);
        var i = GetBucket(hash_code);
        uint collision_count = 0;

        i--;
        do
        {
            // Test in if to drop range check for following array access
            if ((uint)i >= (uint)m_cap) goto ReturnNotFound;

            ref var entry = ref entries[i];
            if (entry.HashCode == hash_code && EqualityComparer<TKey>.Default.Equals(key, entry.Key))
                return ref entry.Value;

            i = entry.Next;

            collision_count++;
        } while (collision_count <= (uint)m_cap);

        // The chain of entries forms a loop; which means a concurrent update has happened.
        // Break out of the loop and throw, rather than looping forever.
        throw new InvalidOperationException("Concurrent operations are not supported");

        ReturnNotFound:
        return ref Unsafe.NullRef<TValue>();
    }

    #endregion

    #endregion

    #region GetValueRefOrNullRef

    [UnscopedRef]
    public readonly ref TValue GetValueRefOrNullRef(TKey key, out bool exists)
    {
        ref var result = ref GetValueRefOrNullRef(key);
        exists = !Unsafe.IsNullRef(ref result);
        return ref result;
    }

    [UnscopedRef]
    public readonly ref TValue GetValueRefOrNullRef(TKey key) => ref FindValue(key);

    #endregion

    #region GetValueRefOrAddDefault

    [UnscopedRef]
    public ref TValue? GetValueRefOrAddDefault(TKey key, out bool exists)
    {
        if (m_buckets == null) Initialize(0);
        Debug.Assert(m_buckets != null);

        var entries = m_entries;
        Debug.Assert(entries != null, "expected entries to be non-null");

        var hash_code = key == null ? 0 : EqualityComparer<TKey>.Default.GetHashCode(key);

        uint collision_count = 0;
        ref var bucket = ref GetBucket(hash_code);
        var i = bucket - 1; // Value in _buckets is 1-based

        while ((uint)i < (uint)m_cap)
        {
            ref var entry = ref entries[i];
            if (entry.HashCode == hash_code && EqualityComparer<TKey>.Default.Equals(key, entry.Key))
            {
                exists = true;
                return ref entry.Value!;
            }

            i = entry.Next;

            collision_count++;
            if (collision_count > (uint)m_cap)
            {
                throw new InvalidOperationException("Concurrent operations are not supported");
            }
        }

        int index;
        if (m_free_count > 0)
        {
            index = m_free_list;
            Debug.Assert((StartOfFreeList - entries[m_free_list].Next) >= -1, "shouldn't overflow because `next` cannot underflow");
            m_free_list = StartOfFreeList - entries[m_free_list].Next;
            m_free_count--;
        }
        else
        {
            var count = m_count;
            if (count == m_cap)
            {
                Resize();
                bucket = ref GetBucket(hash_code);
            }
            index = count;
            m_count = count + 1;
            entries = m_entries;
        }

        {
            ref var entry = ref entries![index];
            entry.HashCode = hash_code;
            entry.Next = bucket - 1; // Value in _buckets is 1-based
            entry.Key = key;
            entry.Value = default!;
            bucket = index + 1; // Value in _buckets is 1-based

            exists = false;
            return ref entry.Value!;
        }
    }

    #endregion

    #region GetValueRefOrUninitialized

    [UnscopedRef]
    public ref TValue? GetValueRefOrUninitialized(TKey key, out bool exists)
    {
        if (m_buckets == null) Initialize(0);
        Debug.Assert(m_buckets != null);

        var entries = m_entries;
        Debug.Assert(entries != null, "expected entries to be non-null");

        var hash_code = key == null ? 0 : EqualityComparer<TKey>.Default.GetHashCode(key);

        uint collision_count = 0;
        ref var bucket = ref GetBucket(hash_code);
        var i = bucket - 1; // Value in _buckets is 1-based

        while ((uint)i < (uint)m_cap)
        {
            ref var entry = ref entries[i];
            if (entry.HashCode == hash_code && EqualityComparer<TKey>.Default.Equals(key, entry.Key))
            {
                exists = true;
                return ref entry.Value!;
            }

            i = entry.Next;

            collision_count++;
            if (collision_count > (uint)m_cap)
            {
                throw new InvalidOperationException("Concurrent operations are not supported");
            }
        }

        int index;
        if (m_free_count > 0)
        {
            index = m_free_list;
            Debug.Assert((StartOfFreeList - entries[m_free_list].Next) >= -1, "shouldn't overflow because `next` cannot underflow");
            m_free_list = StartOfFreeList - entries[m_free_list].Next;
            m_free_count--;
        }
        else
        {
            var count = m_count;
            if (count == m_cap)
            {
                Resize();
                bucket = ref GetBucket(hash_code);
            }
            index = count;
            m_count = count + 1;
            entries = m_entries;
        }

        {
            ref var entry = ref entries![index];
            entry.HashCode = hash_code;
            entry.Next = bucket - 1; // Value in _buckets is 1-based
            entry.Key = key;
            // entry.Value = default!; // uninit
            bucket = index + 1; // Value in _buckets is 1-based

            exists = false;
            return ref entry.Value!;
        }
    }

    #endregion

    #region TryAdd

    [UnscopedRef]
    public bool TryAdd(TKey key, TValue value) => TryInsert(key, value, false) == InsertResult.AddNew;

    #endregion

    #region Set

    [UnscopedRef]
    public bool Set(TKey key, TValue value) => TryInsert(key, value, true) == InsertResult.AddNew;

    #endregion

    #region Contains

    [UnscopedRef]
    public readonly bool Contains(TKey key) => !Unsafe.IsNullRef(ref FindValue(key));

    #endregion

    #region TryGet

    [UnscopedRef]
    public readonly bool TryGet(TKey key, out TValue value)
    {
        ref var value_ref = ref FindValue(key);
        if (!Unsafe.IsNullRef(ref value_ref))
        {
            value = value_ref;
            return true;
        }
        else
        {
            value = default!;
            return false;
        }
    }

    #endregion

    #region Remove

    [UnscopedRef]
    public bool Remove(TKey key)
    {
        if (m_buckets == null) return false;
        var entries = m_entries;
        Debug.Assert(entries != null, "entries should be non-null");

        uint collision_count = 0;
        var hash_code = key == null ? 0 : EqualityComparer<TKey>.Default.GetHashCode(key);

        ref var bucket = ref GetBucket(hash_code);
        var last = -1;
        var i = bucket - 1; // Value in buckets is 1-based
        while (i >= 0)
        {
            ref var entry = ref entries[i];

            if (entry.HashCode == hash_code && EqualityComparer<TKey>.Default.Equals(key, entry.Key))
            {
                if (last < 0)
                {
                    bucket = entry.Next + 1; // Value in buckets is 1-based
                }
                else
                {
                    entries[last].Next = entry.Next;
                }

                Debug.Assert((StartOfFreeList - m_free_list) < 0,
                    "shouldn't underflow because max hashtable length is MaxPrimeArrayLength = 0x7FEFFFFD(2146435069) _freelist underflow threshold 2147483646");
                entry.Next = StartOfFreeList - m_free_list;

                DisposeProxy.TryDispose(ref entry.Key);
                DisposeProxy.TryDispose(ref entry.Value);

                m_free_list = i;
                m_free_count++;
                return true;
            }

            last = i;
            i = entry.Next;

            collision_count++;
            if (collision_count > (uint)m_cap)
            {
                throw new InvalidOperationException("Concurrent operations are not supported");
            }
        }

        return false;
    }

    [UnscopedRef]
    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (m_buckets == null) goto None;
        var entries = m_entries;
        Debug.Assert(entries != null, "entries should be non-null");

        uint collision_count = 0;
        var hash_code = key == null ? 0 : EqualityComparer<TKey>.Default.GetHashCode(key);

        ref var bucket = ref GetBucket(hash_code);
        var last = -1;
        var i = bucket - 1; // Value in buckets is 1-based
        while (i >= 0)
        {
            ref var entry = ref entries[i];

            if (entry.HashCode == hash_code && EqualityComparer<TKey>.Default.Equals(key, entry.Key))
            {
                if (last < 0)
                {
                    bucket = entry.Next + 1; // Value in buckets is 1-based
                }
                else
                {
                    entries[last].Next = entry.Next;
                }

                value = entry.Value;

                Debug.Assert((StartOfFreeList - m_free_list) < 0,
                    "shouldn't underflow because max hashtable length is MaxPrimeArrayLength = 0x7FEFFFFD(2146435069) _freelist underflow threshold 2147483646");
                entry.Next = StartOfFreeList - m_free_list;

                DisposeProxy.TryDispose(ref entry.Key);

                m_free_list = i;
                m_free_count++;
                return true;
            }

            last = i;
            i = entry.Next;

            collision_count++;
            if (collision_count > (uint)m_cap)
            {
                throw new InvalidOperationException("Concurrent operations are not supported");
            }
        }

        None:
        value = default!;
        return false;
    }

    #endregion

    #region Clear

    [UnscopedRef]
    public void Clear()
    {
        int count = m_count;
        if (count > 0)
        {
            Debug.Assert(m_buckets != null, "_buckets should be non-null");
            Debug.Assert(m_entries != null, "_entries should be non-null");

            if (DisposeProxy<TKey>.IsDisposable || DisposeProxy<TValue>.IsDisposable)
            {
                foreach (var kv in this)
                {
                    DisposeProxy.TryDispose(ref kv.Key);
                    DisposeProxy.TryDispose(ref kv.Value);
                }
            }

            new Span<int>(m_buckets, m_cap).Clear();
            m_count = 0;
            m_free_list = -1;
            m_free_count = 0;
            new Span<Entry>(m_entries, count).Clear();
        }
    }

    #endregion

    #region Enumerator

    [UnscopedRef]
    public Enumerator GetEnumerator() => new(ref this);

    public ref struct Enumerator(ref NativeMap<TKey, TValue> self)
    {
        private ref NativeMap<TKey, TValue> self = ref self;
        private ref Entry cur = ref Unsafe.NullRef<Entry>();
        private int index;

        public bool MoveNext()
        {
            while ((uint)index < (uint)self.m_count)
            {
                ref var entry = ref self.m_entries![index++];

                if (entry.Next >= -1)
                {
                    cur = ref entry;
                    return true;
                }
            }

            index = self.m_count + 1;
            cur = ref Unsafe.NullRef<Entry>();
            return false;
        }

        public RefKeyValuePair<TKey, TValue> Current => new(ref cur.Key, ref cur.Value);
    }

    #endregion

    #region Enumerator Class

    public class ClassEnumerator(scoped ref NativeMap<TKey, TValue> self) : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private int count = self.m_count;
        private Entry* entries = self.m_entries!;
        private int index;
        private int cur = -1;

        public bool MoveNext()
        {
            while ((uint)index < (uint)count)
            {
                var i = index++;
                ref var entry = ref entries[i];
                if (entry.Next >= -1)
                {
                    cur = i;
                    return true;
                }
            }

            index = count + 1;
            cur = -1;
            return false;
        }

        public KeyValuePair<TKey, TValue> Current => new(entries[cur].Key, entries[cur].Value);
        object? IEnumerator.Current => Current;

        public void Dispose() { }
        public void Reset() => throw new NotSupportedException();
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => new ClassEnumerator(ref this);

    IEnumerator IEnumerable.GetEnumerator() => new ClassEnumerator(ref this);

    #endregion
}
