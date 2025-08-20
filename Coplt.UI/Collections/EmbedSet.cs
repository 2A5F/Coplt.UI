using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Coplt.UI.Collections;

public struct EmbedSet<T> : ICollection<T>
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
        public T Value;
    }

    #endregion

    #region Fields

    private int[]? m_buckets;
    private Node[]? m_nodes;
    private ulong m_fast_mode_multiplier;
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

    public EmbedSet(int capacity)
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

    private bool AddIfNotPresent(T value, out int location)
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
        AddIfNotPresent(item, out location);
        return ref m_nodes![location];
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

    public bool Add(T item) => AddIfNotPresent(item, out _);

    public ref T AddOrGet(T item) => ref AddOrGetReturnNode(item, out _).Value;

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
        m_free_list = -1;
    }

    #endregion

    #region ICollection

    void ICollection<T>.Add(T item) => Add(item);

    #endregion

    #region Enumerator

    [UnscopedRef]
    public Enumerator GetEnumerator() => new(ref this);

    public ref struct Enumerator(ref EmbedSet<T> self)
    {
        private ref EmbedSet<T> self = ref self;
        private int index;
        private ref Node cur;

        public bool MoveNext()
        {
            while ((uint)index < (uint)self.m_count)
            {
                ref var node = ref self.m_nodes![index++];
                if (node.Next >= -1)
                {
                    cur = ref node;
                    return true;
                }
            }

            index = self.m_count + 1;
            cur = ref Unsafe.NullRef<Node>();
            return false;
        }

        public ref T Current => ref cur.Value;
    }

    #endregion

    #region Enumerator Class

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new ClassEnumerator(ref this);
    IEnumerator IEnumerable.GetEnumerator() => new ClassEnumerator(ref this);

    public sealed class ClassEnumerator(scoped ref EmbedSet<T> self) : IEnumerator<T>
    {
        private int count = self.m_count;
        private Node[] nodes = self.m_nodes!;
        private int index;
        private int cur = -1;

        public bool MoveNext()
        {
            while ((uint)index < (uint)count)
            {
                var i = index++;
                ref var node = ref nodes[i];
                if (node.Next >= -1)
                {
                    cur = i;
                    return true;
                }
            }

            index = count + 1;
            cur = -1;
            return false;
        }

        public T Current => nodes[cur].Value;

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
}
