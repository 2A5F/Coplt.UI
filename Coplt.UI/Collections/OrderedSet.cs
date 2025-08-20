﻿using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Coplt.UI.BoxLayouts.Utilities;

namespace Coplt.UI.Collections;

public struct OrderedSet<T> : ICollection<T>
{
    #region Consts

    private const int StartOfFreeList = -3;

    #endregion

    #region Node

    private record struct Node
    {
        public int HashCode;
        /// <summary>
        /// 0-based index of next entry in chain: -1 means end of chain
        /// also encodes whether this entry _itself_ is part of the free list by changing sign and subtracting 3,
        /// so -2 means end of free list, -3 means index 0 but on free list, -4 means index 1 but on free list, etc.
        /// </summary>
        public int Next;
        public int OrderNext;
        public int OrderPrev;
        public T Value;
    }

    #endregion

    #region Fields

    private int[]? m_buckets;
    private Node[]? m_nodes;
    private ulong m_fast_mode_multiplier;
    private int m_first;
    private int m_last;
    private int m_count;
    private int m_free_list;
    private int m_free_count;

    #endregion

    #region Props

    public int Count => m_count - m_free_count;

    public int Capacity => m_nodes?.Length ?? 0;

    bool ICollection<T>.IsReadOnly => false;

    #endregion

    #region Constructors

    public OrderedSet(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException();
        Initialize(capacity);
    }

    #endregion

    #region Private

    #region Initialize

    private int Initialize(int capacity)
    {
        var size = HashHelpers.GetPrime(capacity);
        var buckets = new int[size];
        var nodes = new Node[size];

        // Assign member variables after both arrays are allocated to guard against corruption from OOM if second fails.
        m_first = -1;
        m_last = -1;
        m_free_list = -1;
        m_buckets = buckets;
        m_nodes = nodes;
        m_fast_mode_multiplier = HashHelpers.GetFastModMultiplier((uint)size);

        return size;
    }

    #endregion

    #region GetBucketRef

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref int GetBucketRef(int hash_code)
    {
        var buckets = m_buckets!;
        return ref buckets[HashHelpers.FastMod((uint)hash_code, (uint)buckets.Length, m_fast_mode_multiplier)];
    }

    #endregion

    #region Resize

    private void Resize() => Resize(HashHelpers.ExpandPrime(m_count), force_new_hash_codes: false);
    private void Resize(int new_size, bool force_new_hash_codes)
    {
        // Value types never rehash
        Debug.Assert(!force_new_hash_codes || !typeof(T).IsValueType);
        Debug.Assert(m_nodes != null, "m_nodes should be non-null");
        Debug.Assert(new_size >= m_nodes.Length);

        var nodes = new Node[new_size];
        var count = m_count;
        m_nodes.AsSpan(0, count).CopyTo(nodes.AsSpan(0, count));

        if (!typeof(T).IsValueType && force_new_hash_codes)
        {
            for (var i = 0; i < count; i++)
            {
                ref var node = ref nodes[i];
                if (node.Next >= -1)
                {
                    node.HashCode = node.Value != null ? EqualityComparer<T>.Default.GetHashCode(node.Value) : 0;
                }
            }
        }

        m_buckets = new int[new_size];
        m_fast_mode_multiplier = HashHelpers.GetFastModMultiplier((uint)new_size);
        for (var i = 0; i < count; i++)
        {
            ref var node = ref nodes[i];
            if (node.Next >= -1)
            {
                ref int bucket = ref GetBucketRef(node.HashCode);
                node.Next = bucket - 1; // Value in _buckets is 1-based
                bucket = i + 1;
            }
        }

        m_nodes = nodes;
    }

    #endregion

    #region FindItemIndex

    private int FindItemIndex(T item)
    {
        var buckets = m_buckets;
        if (buckets == null) return -1;
        var nodes = m_nodes;
        Debug.Assert(nodes != null, "Expected m_nodes to be initialized");

        uint collision_count = 0;
        {
            var hash_code = item == null ? 0 : EqualityComparer<T>.Default.GetHashCode(item);
            var i = GetBucketRef(hash_code) - 1; // Value in _buckets is 1-based
            while (i >= 0)
            {
                ref var node = ref nodes[i];
                if (node.HashCode == hash_code && EqualityComparer<T>.Default.Equals(node.Value, item))
                {
                    return i; // Found
                }
                i = node.Next;

                collision_count++;
                if (collision_count > (uint)nodes.Length)
                {
                    throw new InvalidOperationException("Concurrent operations are not supported");
                }
            }
        }

        return -1;
    }

    #endregion

    #region AddIfNotPresent

    private bool AddIfNotPresent(T value, out int location, bool insert_first)
    {
        if (m_buckets == null) Initialize(0);
        Debug.Assert(m_buckets != null);

        var nodes = m_nodes;
        Debug.Assert(nodes != null);

        int hash_code;
        uint collision_count = 0;

        ref var bucket = ref Unsafe.NullRef<int>();

        {
            hash_code = value == null ? 0 : EqualityComparer<T>.Default.GetHashCode(value);
            bucket = ref GetBucketRef(hash_code);
            var i = bucket - 1; // Value in _buckets is 1-based

            while (i >= 0)
            {
                ref var node = ref nodes[i];
                if (node.HashCode == hash_code && EqualityComparer<T>.Default.Equals(node.Value, value))
                {
                    location = i;
                    return false;
                }
                i = node.Next;

                collision_count++;
                if (collision_count > (uint)nodes.Length)
                {
                    throw new InvalidOperationException("Concurrent operations are not supported");
                }
            }
        }

        int index;
        if (m_free_count > 0)
        {
            index = m_free_list;
            m_free_count--;
            Debug.Assert((StartOfFreeList - nodes[m_free_list].Next) >= -1, "shouldn't overflow because `next` cannot underflow");
            m_free_list = StartOfFreeList - nodes[m_free_list].Next;
        }
        else
        {
            var count = m_count;
            if (count == nodes.Length)
            {
                Resize();
                bucket = ref GetBucketRef(hash_code);
            }
            index = count;
            m_count = count + 1;
            nodes = m_nodes;
        }

        {
            ref var node = ref nodes![index];
            node.HashCode = hash_code;
            node.Next = bucket - 1;
            node.Value = value;
            bucket = index + 1;
            location = index;

            if (insert_first)
            {
                var first = m_first;
                node.OrderPrev = -1;
                node.OrderNext = first;
                if (m_last == -1) m_last = index;
                if (first != -1)
                {
                    ref var next = ref nodes[first];
                    next.OrderPrev = index;
                }
                m_first = index;
            }
            else
            {
                var last = m_last;
                node.OrderNext = -1;
                node.OrderPrev = last;
                if (m_first == -1) m_first = index;
                if (last != -1)
                {
                    ref var prev = ref nodes[last];
                    prev.OrderNext = index;
                }
                m_last = index;
            }
        }

        // Value types never rehash
        if (!typeof(T).IsValueType && collision_count > HashHelpers.HashCollisionThreshold)
        {
            // If we hit the collision threshold we'll need to switch to the comparer which is using randomized string hashing
            // i.e. EqualityComparer<string>.Default.
            Resize(nodes.Length, force_new_hash_codes: true);
            location = FindItemIndex(value);
            Debug.Assert(location >= 0);
        }

        return true;
    }

    #endregion

    #region AddOrGetExistingReturnNode

    private ref Node AddOrGetReturnNode(T item, out int location)
    {
        AddIfNotPresent(item, out location, false);
        return ref m_nodes![location];
    }

    #endregion

    #region RemoveOrderOnly

    private void RemoveOrderOnly(ref Node node)
    {
        var nodes = m_nodes;
        Debug.Assert(nodes != null);

        if (node.OrderPrev == -1)
        {
            m_first = node.OrderNext;
        }
        else
        {
            nodes[node.OrderPrev].OrderNext = node.OrderNext;
        }
        if (node.OrderNext == -1)
        {
            m_last = node.OrderPrev;
        }
        else
        {
            nodes[node.OrderNext].OrderPrev = node.OrderPrev;
        }
    }

    #endregion

    #endregion

    #region Contains

    public bool Contains(T item) => FindItemIndex(item) >= 0;

    #endregion

    #region TryGetValue

    public bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T actualValue)
    {
        if (m_buckets != null)
        {
            var index = FindItemIndex(equalValue);
            if (index >= 0)
            {
                actualValue = m_nodes![index].Value;
                return true;
            }
        }

        actualValue = default;
        return false;
    }

    #endregion

    #region Add

    public bool Add(T item) => AddIfNotPresent(item, out _, false);

    public ref T AddOrGet(T item) => ref AddOrGetReturnNode(item, out _).Value;

    public bool AddFirst(T item) => AddIfNotPresent(item, out _, true);

    #endregion

    #region Remove

    public bool Remove(T item)
    {
        if (m_buckets == null) return false;
        var nodes = m_nodes;
        Debug.Assert(nodes != null, "nodes should be non-null");

        uint collision_count = 0;
        var last = -1;

        var hash_code = item == null ? 0 : EqualityComparer<T>.Default.GetHashCode(item);

        ref var bucket = ref GetBucketRef(hash_code);
        var i = bucket - 1;

        while (i >= 0)
        {
            ref var node = ref nodes[i];

            if (node.HashCode == hash_code && EqualityComparer<T>.Default.Equals(node.Value, item))
            {
                if (last < 0)
                {
                    bucket = node.Next + 1; // Value in buckets is 1-based
                }
                else
                {
                    nodes[last].Next = node.Next;
                }

                Debug.Assert((StartOfFreeList - m_free_list) < 0,
                    "shouldn't underflow because max hashtable length is MaxPrimeArrayLength = 0x7FEFFFFD(2146435069) _freelist underflow threshold 2147483646");
                node.Next = StartOfFreeList - m_free_list;

                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                {
                    node.Value = default!;
                }

                m_free_list = i;
                m_free_count++;

                RemoveOrderOnly(ref node);

                return true;
            }

            last = i;
            i = node.Next;

            collision_count++;
            if (collision_count > (uint)nodes.Length)
            {
                throw new InvalidOperationException("Concurrent operations are not supported");
            }
        }

        return false;
    }

    #endregion

    #region Clear

    public void Clear()
    {
        var count = Count;
        if (count == 0) return;

        m_buckets.AsSpan().Clear();
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            m_nodes.AsSpan(0, m_count).Clear();
        }

        m_free_count = m_count = 0;
        m_free_list = m_first = m_last = -1;
    }

    #endregion

    #region ICollection

    void ICollection<T>.Add(T item) => Add(item);

    #endregion

    #region Enumerator

    [UnscopedRef]
    public Enumerator GetEnumerator() => new(ref this);

    public ref struct Enumerator(ref OrderedSet<T> self) : IIterator<T>
    {
        private ref OrderedSet<T> self = ref self;
        private ref Node cur = ref Unsafe.NullRef<Node>();

        public bool MoveNext()
        {
            if (self.m_nodes == null) return false;
            if (Unsafe.IsNullRef(ref cur))
            {
                if (self.m_first == -1) return false;
                cur = ref self.m_nodes[self.m_first];
            }
            else
            {
                if (cur.OrderNext == -1) return false;
                cur = ref self.m_nodes[cur.OrderNext];
            }
            return true;
        }

        public ref T Current => ref cur.Value;
        T IIterator<T>.Current => cur.Value;
    }

    #endregion

    #region Enumerator Class

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new ClassEnumerator(ref this);
    IEnumerator IEnumerable.GetEnumerator() => new ClassEnumerator(ref this);

    public sealed class ClassEnumerator(scoped ref OrderedSet<T> self) : IEnumerator<T>
    {
        private readonly int first = self.m_first;
        private Node[]? nodes = self.m_nodes!;
        private int cur = -1;

        public bool MoveNext()
        {
            if (nodes == null) return false;
            if (cur == -1)
            {
                if (first == -1) return false;
                cur = first;
            }
            else
            {
                ref var node = ref nodes[cur];
                if (node.OrderNext == -1) return false;
                cur = node.OrderNext;
            }
            return true;
        }

        public T Current => nodes![cur].Value;

        object? IEnumerator.Current => Current;

        public void Reset() => throw new NotSupportedException();

        void IDisposable.Dispose() { }
    }

    #endregion

    #region CopyTo

    public void CopyTo(Span<T> target)
    {
        if (target.IsEmpty) return;
        var inc = 0;
        foreach (ref var item in this)
        {
            var i = inc++;
            if (i >= target.Length) return;
            target[i] = item;
        }
    }

    void ICollection<T>.CopyTo(T[] array, int arrayIndex) => CopyTo(array.AsSpan(arrayIndex));

    #endregion

    #region TryGetFirst

    public bool TryGetFirst([MaybeNullWhen(false)] out T first)
    {
        if (m_first >= 0)
        {
            first = m_nodes![m_first].Value;
            return true;
        }
        first = default;
        return false;
    }

    #endregion

    #region TryGetLast

    public bool TryGetLast([MaybeNullWhen(false)] out T last)
    {
        if (m_last >= 0)
        {
            last = m_nodes![m_last].Value;
            return true;
        }
        last = default;
        return false;
    }

    #endregion

    #region TryGetPrev

    public bool TryGetPrev(T item, [MaybeNullWhen(false)] out T prev)
    {
        var index = FindItemIndex(item);
        if (index >= 0)
        {
            ref var node = ref m_nodes![index];
            if (node.OrderPrev >= 0)
            {
                prev = m_nodes[node.OrderPrev].Value;
                return true;
            }
        }
        prev = default;
        return false;
    }

    #endregion

    #region TryGetNext

    public bool TryGetNext(T item, [MaybeNullWhen(false)] out T next)
    {
        var index = FindItemIndex(item);
        if (index >= 0)
        {
            ref var node = ref m_nodes![index];
            if (node.OrderNext >= 0)
            {
                next = m_nodes[node.OrderNext].Value;
                return true;
            }
        }
        next = default;
        return false;
    }

    #endregion

    #region SetNext

    public void SetNext(T item, T next)
    {
        ref var this_node = ref AddOrGetReturnNode(item, out var this_index);
        ref var next_node = ref AddOrGetReturnNode(next, out var next_index);
        if (this_node.OrderNext == next_index) return; // already set
        RemoveOrderOnly(ref next_node);
        var old_next = this_node.OrderNext;
        this_node.OrderNext = next_index;
        next_node.OrderNext = old_next;
        if (old_next == -1)
        {
            m_last = next_index;
        }
        else
        {
            m_nodes![old_next].OrderPrev = next_index;
        }
        next_node.OrderPrev = this_index;
    }

    #endregion

    #region SetPrev

    public void SetPrev(T item, T prev)
    {
        ref var this_node = ref AddOrGetReturnNode(item, out var this_index);
        ref var prev_node = ref AddOrGetReturnNode(prev, out var prev_index);
        if (this_node.OrderPrev == prev_index) return; // already set
        RemoveOrderOnly(ref prev_node);
        var old_prev = this_node.OrderPrev;
        this_node.OrderPrev = prev_index;
        prev_node.OrderPrev = old_prev;
        if (old_prev == -1)
        {
            m_first = prev_index;
        }
        else
        {
            m_nodes![old_prev].OrderNext = prev_index;
        }
        prev_node.OrderNext = this_index;
    }

    #endregion
}
