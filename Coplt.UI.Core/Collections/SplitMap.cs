using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Dropping;
using Coplt.UI.Native;
using Coplt.Union;

namespace Coplt.UI.Collections;

using static SplitMap;

public static class SplitMap
{
    #region Consts

    internal const int StartOfFreeList = -3;

    #endregion
}

[Union2]
public partial struct CtrlOp
{
    [UnionTemplate]
    private interface Template
    {
        public void None();
        public void Init(int size);
        public void Resize(int new_size);
    }
}

[Dropping]
public unsafe partial struct NSplitMapCtrl<K>
{
    #region Static Check

    static NSplitMapCtrl()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<K>())
            throw new NotSupportedException("Only unmanaged types are supported.");
    }

    #endregion

    #region Ctrl

    public record struct Ctrl
    {
        public int HashCode;
        /// <summary>
        /// 0-based index of next entry in chain: -1 means end of chain
        /// also encodes whether this entry _itself_ is part of the free list by changing sign and subtracting 3,
        /// so -2 means end of free list, -3 means index 0 but on free list, -4 means index 1 but on free list, etc.
        /// </summary>
        public int Next;
        public K Key;
    }

    #endregion

    #region Fields

    internal int* m_buckets;
    [Drop]
    internal NSplitMapData<Ctrl> m_ctrls;
    internal ulong m_fast_mode_multiplier;
    internal int m_cap;
    internal int m_count;
    internal int m_free_list;
    internal int m_free_count;

    #endregion

    #region Props

    [UnscopedRef]
    public readonly int Count => m_count - m_free_count;

    [UnscopedRef]
    public readonly int Capacity => m_cap;

    [UnscopedRef]
    public readonly int* RawBuckets => m_buckets;

    [UnscopedRef]
    public readonly ref readonly NSplitMapData<Ctrl> RawCtrls => ref m_ctrls;

    [UnscopedRef]
    internal Span<int> Buckets => new(m_buckets, m_cap);

    #endregion

    #region Private

    #region Free

    [Drop]
    private void Free()
    {
        if (m_buckets == null) return;
        NativeLib.Free(m_buckets);
        m_buckets = null;
    }

    #endregion

    #region Initialize

    [UnscopedRef]
    private int Initialize(int capacity)
    {
        var size = HashHelpers.GetPrime(capacity);
        var buckets = NativeLib.ZAlloc<int>(size);
        var ctrls = new NSplitMapData<Ctrl>(size);

        // Assign member variables after both arrays are allocated to guard against corruption from OOM if second fails.
        m_free_list = -1;
        m_cap = size;
        m_buckets = buckets;
        m_ctrls = ctrls;
        m_fast_mode_multiplier = HashHelpers.GetFastModMultiplier((uint)size);

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
    private int Resize()
    {
        var size = HashHelpers.ExpandPrime(m_count);
        Resize(size);
        return size;
    }

    [UnscopedRef]
    private void Resize(int new_size)
    {
        Debug.Assert(m_buckets != null, "m_buckets should be non-null");
        Debug.Assert(new_size >= m_cap);

        m_ctrls.Resize(new_size);

        NativeLib.Free(m_buckets);
        m_buckets = NativeLib.ZAlloc<int>(new_size);
        if (m_buckets != null) throw new OutOfMemoryException();
        var count = m_count;
        m_fast_mode_multiplier = HashHelpers.GetFastModMultiplier((uint)new_size);
        for (var i = 0; i < count; i++)
        {
            ref var entry = ref m_ctrls.UnsafeAt(i);
            if (entry.Next >= -1)
            {
                ref int bucket = ref GetBucket(entry.HashCode);
                entry.Next = bucket - 1; // Value in _buckets is 1-based
                bucket = i + 1;
            }
        }
    }

    #endregion

    #endregion

    #region FindValue

    [UnscopedRef]
    public readonly int FindValue(K key)
    {
        if (m_buckets == null) goto ReturnNotFound;

        var hash_code = key == null ? 0 : EqualityComparer<K>.Default.GetHashCode(key);
        var i = GetBucket(hash_code);
        uint collision_count = 0;

        i--;
        do
        {
            // Test in if to drop range check for following array access
            if ((uint)i >= (uint)m_cap) goto ReturnNotFound;

            ref var entry = ref m_ctrls.UnsafeAt(i);
            if (entry.HashCode == hash_code && EqualityComparer<K>.Default.Equals(key, entry.Key))
                return i;

            i = entry.Next;

            collision_count++;
        } while (collision_count <= (uint)m_cap);

        // The chain of entries forms a loop; which means a concurrent update has happened.
        // Break out of the loop and throw, rather than looping forever.
        throw new InvalidOperationException("Concurrent operations are not supported");

        ReturnNotFound:
        return -1;
    }

    #endregion

    #region TryInsert

    [UnscopedRef]
    public InsertResult TryInsert(K key, bool overwrite, ref CtrlOp op, out int idx)
    {
        if (m_buckets == null)
        {
            var size = Initialize(0);
            op = CtrlOp.Init(size);
        }
        Debug.Assert(m_buckets != null);

        var hash_code = key == null ? 0 : EqualityComparer<K>.Default.GetHashCode(key);

        uint collision_count = 0;
        ref var bucket = ref GetBucket(hash_code);
        var i = bucket - 1; // Value in _buckets is 1-based

        while ((uint)i < (uint)m_cap)
        {
            ref var entry = ref m_ctrls.UnsafeAt(i);
            if (entry.HashCode == hash_code && EqualityComparer<K>.Default.Equals(key, entry.Key))
            {
                idx = i;
                return overwrite ? InsertResult.Overwrite : InsertResult.None;
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
            Debug.Assert((StartOfFreeList - m_ctrls.UnsafeAt(m_free_list).Next) >= -1, "shouldn't overflow because `next` cannot underflow");
            m_free_list = StartOfFreeList - m_ctrls.UnsafeAt(m_free_list).Next;
            m_free_count--;
        }
        else
        {
            var count = m_count;
            if (count == m_cap)
            {
                var new_size = Resize();
                op = CtrlOp.Resize(new_size);
                bucket = ref GetBucket(hash_code);
            }
            index = count;
            m_count = count + 1;
        }

        {
            idx = index;
            ref var entry = ref m_ctrls.UnsafeAt(index);
            entry.HashCode = hash_code;
            entry.Next = bucket - 1; // Value in _buckets is 1-based
            entry.Key = key;
            bucket = index + 1; // Value in _buckets is 1-based
        }

        return InsertResult.AddNew;
    }

    #endregion

    #region Remove

    [UnscopedRef]
    public int Remove(K key)
    {
        if (m_buckets == null) return -1;

        uint collision_count = 0;
        var hash_code = key == null ? 0 : EqualityComparer<K>.Default.GetHashCode(key);

        ref var bucket = ref GetBucket(hash_code);
        var last = -1;
        var i = bucket - 1; // Value in buckets is 1-based
        while (i >= 0)
        {
            ref var entry = ref m_ctrls.UnsafeAt(i);

            if (entry.HashCode == hash_code && EqualityComparer<K>.Default.Equals(key, entry.Key))
            {
                if (last < 0)
                {
                    bucket = entry.Next + 1; // Value in buckets is 1-based
                }
                else
                {
                    m_ctrls.UnsafeAt(last).Next = entry.Next;
                }

                Debug.Assert((StartOfFreeList - m_free_list) < 0,
                    "shouldn't underflow because max hashtable length is MaxPrimeArrayLength = 0x7FEFFFFD(2146435069) _freelist underflow threshold 2147483646");
                entry.Next = StartOfFreeList - m_free_list;

                m_free_list = i;
                m_free_count++;
                return i;
            }

            last = i;
            i = entry.Next;

            collision_count++;
            if (collision_count > (uint)m_cap)
            {
                throw new InvalidOperationException("Concurrent operations are not supported");
            }
        }

        return -1;
    }

    #endregion

    #region Clear

    [UnscopedRef]
    public int Clear()
    {
        int count = m_count;
        if (count > 0)
        {
            Debug.Assert(m_buckets != null, "_buckets should be non-null");

            new Span<int>(m_buckets, m_cap).Clear();

            m_count = 0;
            m_free_list = -1;
            m_free_count = 0;
        }
        return count;
    }

    #endregion

    #region Enumerator

    [UnscopedRef]
    public readonly Enumerator GetEnumerator() => new(in this);

    public ref struct Enumerator(ref readonly NSplitMapCtrl<K> self)
    {
        private readonly ref readonly NSplitMapCtrl<K> self = ref self;
        private int cur;
        private int index;

        public bool MoveNext()
        {
            while ((uint)index < (uint)self.m_count)
            {
                var i = index++;
                ref var entry = ref self.m_ctrls.UnsafeAt(i);

                if (entry.Next >= -1)
                {
                    cur = i;
                    return true;
                }
            }

            index = self.m_count + 1;
            cur = -1;
            return false;
        }

        public int Current => cur;
    }

    public struct EnumeratorCopy(NSplitMapCtrl<K> self)
    {
        private readonly NSplitMapCtrl<K> self = self;
        private int cur;
        private int index;

        public bool MoveNext()
        {
            while ((uint)index < (uint)self.m_count)
            {
                var i = index++;
                ref var entry = ref self.m_ctrls.UnsafeAt(i);

                if (entry.Next >= -1)
                {
                    cur = i;
                    return true;
                }
            }

            index = self.m_count + 1;
            cur = -1;
            return false;
        }

        public int Current => cur;
    }

    #endregion
}

[Dropping]
public unsafe partial struct NSplitMapData<T>
{
    #region Static Check

    static NSplitMapData()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            throw new NotSupportedException("Only unmanaged types are supported.");
    }

    #endregion

    #region Fields

    internal T* m_items;
    internal int m_cap;

    #endregion

    #region Props

    public Span<T> AsSpan => new(m_items, m_cap);
    public int Capacity => m_cap;
    public T* RawPtr => m_items;

    #endregion

    #region Private

    #region Free

    [Drop]
    private void Free()
    {
        if (m_items == null) return;
        NativeLib.Free(m_items);
        m_items = null;
    }

    #endregion

    #endregion

    #region Ctor

    public NSplitMapData(int capacity)
    {
        m_cap = capacity;
        m_items = NativeLib.Alloc<T>(capacity);
        if (m_items == null) throw new OutOfMemoryException();
    }

    #endregion

    #region Get

    [UnscopedRef]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref T UnsafeAt(int index) => ref m_items[index];

    #endregion

    #region Resize

    public void Resize(int new_size)
    {
        Debug.Assert(m_items != null, "m_items should be non-null");
        Debug.Assert(new_size >= m_cap);
        if (new_size == m_cap) return;
        m_cap = new_size;
        m_items = NativeLib.ReAlloc(m_items, m_cap);
        if (m_items == null) throw new OutOfMemoryException();
    }

    #endregion

    #region Clear

    [UnscopedRef]
    public void Clear(int count)
    {
        AsSpan[..count].Clear();
    }

    #endregion

    #region ApplyOp

    [UnscopedRef]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ApplyOp(CtrlOp op)
    {
        switch (op.Tag)
        {
            case CtrlOp.Tags.None: break;
            case CtrlOp.Tags.Init:
                Dispose();
                this = new(op.Init.size);
                break;
            case CtrlOp.Tags.Resize:
                Resize(op.Resize.new_size);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion
}

[Dropping]
public unsafe partial struct NSplitMap<K, V> : IEnumerable<KeyValuePair<K, V>>
{
    #region Fields

    [Drop]
    internal NSplitMapCtrl<K> m_ctrl;
    [Drop]
    internal NSplitMapData<V> m_data;

    #endregion

    #region Props

    public int Count => m_ctrl.Count;

    public int Capacity => m_ctrl.Capacity;

    [UnscopedRef]
    internal Span<int> AllBuckets => m_ctrl.Buckets;
    [UnscopedRef]
    internal Span<NSplitMapCtrl<K>.Ctrl> AllCtrls => m_ctrl.m_ctrls.AsSpan;
    [UnscopedRef]
    internal Span<V> AllValues => m_data.AsSpan;

    #endregion

    #region Index

    [UnscopedRef]
    public V this[K key]
    {
        readonly get => TryGet(key, out var value) ? value : throw new KeyNotFoundException();
        set => Set(key, value);
    }

    #endregion

    #region TryAdd

    [UnscopedRef]
    public bool TryAdd(K key, V value)
    {
        var op = CtrlOp.None;
        var r = m_ctrl.TryInsert(key, false, ref op, out var index);
        Debug.Assert(r != InsertResult.Overwrite);
        if (r != InsertResult.AddNew) return false;
        m_data.ApplyOp(op);
        m_data.UnsafeAt(index) = value;
        return true;
    }

    #endregion

    #region TryAdd

    [UnscopedRef]
    public bool Set(K key, V value)
    {
        var op = CtrlOp.None;
        var r = m_ctrl.TryInsert(key, true, ref op, out var index);
        if (r == InsertResult.None) return false;
        m_data.ApplyOp(op);
        m_data.UnsafeAt(index) = value;
        return r == InsertResult.AddNew;
    }

    #endregion

    #region Contains

    [UnscopedRef]
    public readonly bool Contains(K key) => m_ctrl.FindValue(key) != -1;

    #endregion

    #region TryGet

    [UnscopedRef]
    public readonly bool TryGet(K key, out V value)
    {
        var index = m_ctrl.FindValue(key);
        if (index != -1)
        {
            value = m_data.UnsafeAt(index);
            return true;
        }
        else
        {
            Unsafe.SkipInit(out value);
            return false;
        }
    }
    [UnscopedRef]
    public readonly ref V TryGetRef(K key)
    {
        var index = m_ctrl.FindValue(key);
        if (index != -1)
        {
            return ref m_data.UnsafeAt(index);
        }
        else
        {
            return ref Unsafe.NullRef<V>();
        }
    }

    #endregion

    #region Remove

    [UnscopedRef]
    public bool Remove(K key)
    {
        var index = m_ctrl.Remove(key);
        return index >= 0;
    }

    [UnscopedRef]
    public bool Remove(K key, [MaybeNullWhen(false)] out V value)
    {
        var index = m_ctrl.Remove(key);
        if (index < 0)
        {
            Unsafe.SkipInit(out value);
            return false;
        }
        value = m_data.UnsafeAt(index);
        return true;
    }

    #endregion

    #region Clear

    public void Clear()
    {
        m_ctrl.Clear();
    }

    #endregion

    #region Enumerator

    [UnscopedRef]
    public readonly Enumerator GetEnumerator() => new(m_ctrl.GetEnumerator(), m_ctrl.m_ctrls.m_items, m_data.m_items);

    public ref struct Enumerator(NSplitMapCtrl<K>.Enumerator ctrl, NSplitMapCtrl<K>.Ctrl* ctrls, V* items)
    {
        private NSplitMapCtrl<K>.Enumerator m_ctrl = ctrl;

        public bool MoveNext() => m_ctrl.MoveNext();

        public RefKeyValuePair<K, V> Current => new(ref ctrls[m_ctrl.Current].Key, ref items[m_ctrl.Current]);
    }

    IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() =>
        new EnumeratorCopy(new(m_ctrl), m_ctrl.m_ctrls.m_items, m_data.m_items);
    IEnumerator IEnumerable.GetEnumerator() =>
        new EnumeratorCopy(new(m_ctrl), m_ctrl.m_ctrls.m_items, m_data.m_items);

    public struct EnumeratorCopy(NSplitMapCtrl<K>.EnumeratorCopy ctrl, NSplitMapCtrl<K>.Ctrl* ctrls, V* items) : IEnumerator<KeyValuePair<K, V>>
    {
        private NSplitMapCtrl<K>.EnumeratorCopy m_ctrl = ctrl;

        public bool MoveNext() => m_ctrl.MoveNext();
        public KeyValuePair<K, V> Current => new(ctrls[m_ctrl.Current].Key, items[m_ctrl.Current]);

        object IEnumerator.Current => Current;

        void IDisposable.Dispose() { }

        void IEnumerator.Reset() => throw new NotSupportedException();
    }

    #endregion
}

public struct SplitMapCtrl<K>
{
    #region Ctrl

    public record struct Ctrl
    {
        public int HashCode;
        /// <summary>
        /// 0-based index of next entry in chain: -1 means end of chain
        /// also encodes whether this entry _itself_ is part of the free list by changing sign and subtracting 3,
        /// so -2 means end of free list, -3 means index 0 but on free list, -4 means index 1 but on free list, etc.
        /// </summary>
        public int Next;
        public K Key;
    }

    #endregion

    #region Fields

    internal int[]? m_buckets;
    internal SplitMapData<Ctrl> m_ctrls;
    private ulong m_fast_mode_multiplier;
    private int m_count;
    private int m_free_list;
    private int m_free_count;

    #endregion

    #region Props

    [UnscopedRef]
    public readonly int Count => m_count - m_free_count;

    [UnscopedRef]
    public readonly int Capacity => m_buckets?.Length ?? 0;

    [UnscopedRef]
    public readonly int[]? RawBuckets => m_buckets;

    [UnscopedRef]
    public readonly ref readonly SplitMapData<Ctrl> RawCtrls => ref m_ctrls;

    [UnscopedRef]
    internal Span<int> Buckets => m_buckets;

    #endregion

    #region Private

    #region Initialize

    [UnscopedRef]
    private int Initialize(int capacity)
    {
        var size = HashHelpers.GetPrime(capacity);
        var buckets = new int[size];
        var ctrls = new SplitMapData<Ctrl>(size);

        // Assign member variables after both arrays are allocated to guard against corruption from OOM if second fails.
        m_free_list = -1;
        m_buckets = buckets;
        m_ctrls = ctrls;
        m_fast_mode_multiplier = HashHelpers.GetFastModMultiplier((uint)size);

        return size;
    }

    #endregion

    #region GetBucket

    [UnscopedRef]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly ref int GetBucket(int hash_code)
    {
        var buckets = m_buckets!;
        return ref buckets[HashHelpers.FastMod((uint)hash_code, (uint)buckets.Length, m_fast_mode_multiplier)];
    }

    #endregion

    #region Resize

    [UnscopedRef]
    private int Resize()
    {
        var size = HashHelpers.ExpandPrime(m_count);
        Resize(size, force_new_hash_codes: false);
        return size;
    }

    [UnscopedRef]
    private void Resize(int new_size, bool  force_new_hash_codes)
    {
        Debug.Assert(m_buckets != null, "m_buckets should be non-null");
        Debug.Assert(new_size >= m_buckets.Length);

        m_ctrls.Resize(new_size);
        
        var count = m_count;
        if (!typeof(K).IsValueType && force_new_hash_codes)
        {
            for (var i = 0; i < count; i++)
            {
                ref var ctrl = ref m_ctrls.UnsafeAt(i);
                if (ctrl.Next >= -1)
                {
                    ctrl.HashCode = ctrl.Key != null ? EqualityComparer<K>.Default.GetHashCode(ctrl.Key) : 0;
                }
            }
        }

        m_buckets = new int[new_size];
        m_fast_mode_multiplier = HashHelpers.GetFastModMultiplier((uint)new_size);
        for (var i = 0; i < count; i++)
        {
            ref var entry = ref m_ctrls.UnsafeAt(i);
            if (entry.Next >= -1)
            {
                ref int bucket = ref GetBucket(entry.HashCode);
                entry.Next = bucket - 1; // Value in _buckets is 1-based
                bucket = i + 1;
            }
        }
    }

    #endregion

    #endregion

    #region FindValue

    [UnscopedRef]
    public readonly int FindValue(K key)
    {
        if (m_buckets == null) goto ReturnNotFound;

        var hash_code = key == null ? 0 : EqualityComparer<K>.Default.GetHashCode(key);
        var i = GetBucket(hash_code);
        uint collision_count = 0;

        i--;
        do
        {
            // Test in if to drop range check for following array access
            if ((uint)i >= (uint)m_buckets.Length) goto ReturnNotFound;

            ref var entry = ref m_ctrls.UnsafeAt(i);
            if (entry.HashCode == hash_code && EqualityComparer<K>.Default.Equals(key, entry.Key))
                return i;

            i = entry.Next;

            collision_count++;
        } while (collision_count <= (uint)m_buckets.Length);

        // The chain of entries forms a loop; which means a concurrent update has happened.
        // Break out of the loop and throw, rather than looping forever.
        throw new InvalidOperationException("Concurrent operations are not supported");

        ReturnNotFound:
        return -1;
    }

    #endregion

    #region TryInsert

    [UnscopedRef]
    public InsertResult TryInsert(K key, bool overwrite, ref CtrlOp op, out int idx)
    {
        if (m_buckets == null)
        {
            var size = Initialize(0);
            op = CtrlOp.Init(size);
        }
        Debug.Assert(m_buckets != null);

        var hash_code = key == null ? 0 : EqualityComparer<K>.Default.GetHashCode(key);

        uint collision_count = 0;
        ref var bucket = ref GetBucket(hash_code);
        var i = bucket - 1; // Value in _buckets is 1-based

        while ((uint)i < (uint)m_buckets.Length)
        {
            ref var entry = ref m_ctrls.UnsafeAt(i);
            if (entry.HashCode == hash_code && EqualityComparer<K>.Default.Equals(key, entry.Key))
            {
                idx = i;
                return overwrite ? InsertResult.Overwrite : InsertResult.None;
            }

            i = entry.Next;

            collision_count++;
            if (collision_count > (uint)m_buckets.Length)
            {
                throw new InvalidOperationException("Concurrent operations are not supported");
            }
        }

        int index;
        if (m_free_count > 0)
        {
            index = m_free_list;
            Debug.Assert((StartOfFreeList - m_ctrls.UnsafeAt(m_free_list).Next) >= -1, "shouldn't overflow because `next` cannot underflow");
            m_free_list = StartOfFreeList - m_ctrls.UnsafeAt(m_free_list).Next;
            m_free_count--;
        }
        else
        {
            var count = m_count;
            if (count == m_buckets.Length)
            {
                var new_size = Resize();
                op = CtrlOp.Resize(new_size);
                bucket = ref GetBucket(hash_code);
            }
            index = count;
            m_count = count + 1;
        }

        {
            idx = index;
            ref var entry = ref m_ctrls.UnsafeAt(index);
            entry.HashCode = hash_code;
            entry.Next = bucket - 1; // Value in _buckets is 1-based
            entry.Key = key;
            bucket = index + 1; // Value in _buckets is 1-based
        }

        return InsertResult.AddNew;
    }

    #endregion

    #region Remove

    [UnscopedRef]
    public int Remove(K key)
    {
        if (m_buckets == null) return -1;

        uint collision_count = 0;
        var hash_code = key == null ? 0 : EqualityComparer<K>.Default.GetHashCode(key);

        ref var bucket = ref GetBucket(hash_code);
        var last = -1;
        var i = bucket - 1; // Value in buckets is 1-based
        while (i >= 0)
        {
            ref var entry = ref m_ctrls.UnsafeAt(i);

            if (entry.HashCode == hash_code && EqualityComparer<K>.Default.Equals(key, entry.Key))
            {
                if (last < 0)
                {
                    bucket = entry.Next + 1; // Value in buckets is 1-based
                }
                else
                {
                    m_ctrls.UnsafeAt(last).Next = entry.Next;
                }

                Debug.Assert((StartOfFreeList - m_free_list) < 0,
                    "shouldn't underflow because max hashtable length is MaxPrimeArrayLength = 0x7FEFFFFD(2146435069) _freelist underflow threshold 2147483646");
                entry.Next = StartOfFreeList - m_free_list;

                if (RuntimeHelpers.IsReferenceOrContainsReferences<K>())
                {
                    entry.Key = default!;
                }

                m_free_list = i;
                m_free_count++;
                return i;
            }

            last = i;
            i = entry.Next;

            collision_count++;
            if (collision_count > (uint)m_buckets.Length)
            {
                throw new InvalidOperationException("Concurrent operations are not supported");
            }
        }

        return -1;
    }

    #endregion

    #region Clear

    [UnscopedRef]
    public int Clear()
    {
        int count = m_count;
        if (count > 0)
        {
            Debug.Assert(m_buckets != null, "_buckets should be non-null");

            m_buckets.AsSpan().Clear();

            m_count = 0;
            m_free_list = -1;
            m_free_count = 0;
            if (RuntimeHelpers.IsReferenceOrContainsReferences<K>())
            {
                m_ctrls.Clear(count);
            }
        }
        return count;
    }

    #endregion

    #region Enumerator

    [UnscopedRef]
    public readonly Enumerator GetEnumerator() => new(in this);

    public ref struct Enumerator(ref readonly SplitMapCtrl<K> self)
    {
        private readonly ref readonly SplitMapCtrl<K> self = ref self;
        private int cur;
        private int index;

        public bool MoveNext()
        {
            while ((uint)index < (uint)self.m_count)
            {
                var i = index++;
                ref var entry = ref self.m_ctrls.UnsafeAt(i);

                if (entry.Next >= -1)
                {
                    cur = i;
                    return true;
                }
            }

            index = self.m_count + 1;
            cur = -1;
            return false;
        }

        public int Current => cur;
    }

    public struct EnumeratorCopy(SplitMapCtrl<K> self)
    {
        private readonly SplitMapCtrl<K> self = self;
        private int cur;
        private int index;

        public bool MoveNext()
        {
            while ((uint)index < (uint)self.m_count)
            {
                var i = index++;
                ref var entry = ref self.m_ctrls.UnsafeAt(i);

                if (entry.Next >= -1)
                {
                    cur = i;
                    return true;
                }
            }

            index = self.m_count + 1;
            cur = -1;
            return false;
        }

        public int Current => cur;
    }

    #endregion
}

public struct SplitMapData<T>
{
    #region Fields

    internal T[]? m_items;

    #endregion

    #region Props

    public Span<T> AsSpan => m_items.AsSpan();
    public int Capacity => m_items?.Length ?? 0;
    public T[]? RawArr => m_items;

    #endregion

    #region Ctor

    public SplitMapData(int capacity)
    {
        m_items = GC.AllocateUninitializedArray<T>(capacity);
    }

    #endregion

    #region Get

    [UnscopedRef]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref T UnsafeAt(int index) => ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(m_items!), index);

    #endregion

    #region Resize

    public void Resize(int new_size)
    {
        Debug.Assert(m_items != null, "m_items should be non-null");
        Debug.Assert(new_size >= m_items.Length);
        if (new_size == m_items.Length) return;
        var new_items = GC.AllocateUninitializedArray<T>(new_size);
        m_items.CopyTo(new_items);
        m_items = new_items;
    }

    #endregion

    #region Clear

    [UnscopedRef]
    public void Clear(int count)
    {
        AsSpan[..count].Clear();
    }

    #endregion

    #region ApplyOp

    [UnscopedRef]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ApplyOp(CtrlOp op)
    {
        switch (op.Tag)
        {
            case CtrlOp.Tags.None: break;
            case CtrlOp.Tags.Init:
                this = new(op.Init.size);
                break;
            case CtrlOp.Tags.Resize:
                Resize(op.Resize.new_size);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion
}

public struct SplitMap<K, V> : IEnumerable<KeyValuePair<K, V>>
{
    #region Fields

    internal SplitMapCtrl<K> m_ctrl;
    internal SplitMapData<V> m_data;

    #endregion

    #region Props

    public int Count => m_ctrl.Count;

    public int Capacity => m_ctrl.Capacity;

    [UnscopedRef]
    internal Span<int> AllBuckets => m_ctrl.Buckets;
    [UnscopedRef]
    internal Span<SplitMapCtrl<K>.Ctrl> AllCtrls => m_ctrl.m_ctrls.AsSpan;
    [UnscopedRef]
    internal Span<V> AllValues => m_data.AsSpan;

    #endregion

    #region Index

    [UnscopedRef]
    public V this[K key]
    {
        readonly get => TryGet(key, out var value) ? value : throw new KeyNotFoundException();
        set => Set(key, value);
    }

    #endregion

    #region TryAdd

    [UnscopedRef]
    public bool TryAdd(K key, V value)
    {
        var op = CtrlOp.None;
        var r = m_ctrl.TryInsert(key, false, ref op, out var index);
        Debug.Assert(r != InsertResult.Overwrite);
        if (r != InsertResult.AddNew) return false;
        m_data.ApplyOp(op);
        m_data.UnsafeAt(index) = value;
        return true;
    }

    #endregion

    #region TryAdd

    [UnscopedRef]
    public bool Set(K key, V value)
    {
        var op = CtrlOp.None;
        var r = m_ctrl.TryInsert(key, true, ref op, out var index);
        if (r == InsertResult.None) return false;
        m_data.ApplyOp(op);
        m_data.UnsafeAt(index) = value;
        return r == InsertResult.AddNew;
    }

    #endregion

    #region Contains

    [UnscopedRef]
    public readonly bool Contains(K key) => m_ctrl.FindValue(key) != -1;

    #endregion

    #region TryGet

    [UnscopedRef]
    public readonly bool TryGet(K key, out V value)
    {
        var index = m_ctrl.FindValue(key);
        if (index != -1)
        {
            value = m_data.UnsafeAt(index);
            return true;
        }
        else
        {
            Unsafe.SkipInit(out value);
            return false;
        }
    }
    [UnscopedRef]
    public readonly ref V TryGetRef(K key)
    {
        var index = m_ctrl.FindValue(key);
        if (index != -1)
        {
            return ref m_data.UnsafeAt(index);
        }
        else
        {
            return ref Unsafe.NullRef<V>();
        }
    }

    #endregion

    #region Remove

    [UnscopedRef]
    public bool Remove(K key)
    {
        var index = m_ctrl.Remove(key);
        if (index < 0) return false;
        if (RuntimeHelpers.IsReferenceOrContainsReferences<V>())
        {
            m_data.UnsafeAt(index) = default!;
        }
        return true;
    }

    [UnscopedRef]
    public bool Remove(K key, [MaybeNullWhen(false)] out V value)
    {
        var index = m_ctrl.Remove(key);
        if (index < 0)
        {
            Unsafe.SkipInit(out value);
            return false;
        }
        value = m_data.UnsafeAt(index);
        if (RuntimeHelpers.IsReferenceOrContainsReferences<V>())
        {
            m_data.UnsafeAt(index) = default!;
        }
        return true;
    }

    #endregion

    #region Clear

    public void Clear()
    {
        var count = m_ctrl.Clear();
        if (RuntimeHelpers.IsReferenceOrContainsReferences<V>())
        {
            m_data.Clear(count);
        }
    }

    #endregion

    #region Enumerator

    [UnscopedRef]
    public readonly Enumerator GetEnumerator() => new(m_ctrl.GetEnumerator(), m_ctrl.m_ctrls.m_items!, m_data.m_items!);

    public ref struct Enumerator(SplitMapCtrl<K>.Enumerator ctrl, SplitMapCtrl<K>.Ctrl[] ctrls, V[] items)
    {
        private SplitMapCtrl<K>.Enumerator m_ctrl = ctrl;

        public bool MoveNext() => m_ctrl.MoveNext();

        public RefKeyValuePair<K, V> Current => new(ref ctrls[m_ctrl.Current].Key, ref items[m_ctrl.Current]);
    }

    IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() =>
        new EnumeratorCopy(new(m_ctrl), m_ctrl.m_ctrls.m_items!, m_data.m_items!);
    IEnumerator IEnumerable.GetEnumerator() =>
        new EnumeratorCopy(new(m_ctrl), m_ctrl.m_ctrls.m_items!, m_data.m_items!);

    public struct EnumeratorCopy(SplitMapCtrl<K>.EnumeratorCopy ctrl, SplitMapCtrl<K>.Ctrl[] ctrls, V[] items) : IEnumerator<KeyValuePair<K, V>>
    {
        private SplitMapCtrl<K>.EnumeratorCopy m_ctrl = ctrl;

        public bool MoveNext() => m_ctrl.MoveNext();
        public KeyValuePair<K, V> Current => new(ctrls[m_ctrl.Current].Key, items[m_ctrl.Current]);

        object IEnumerator.Current => Current;

        void IDisposable.Dispose() { }

        void IEnumerator.Reset() => throw new NotSupportedException();
    }

    #endregion
}
