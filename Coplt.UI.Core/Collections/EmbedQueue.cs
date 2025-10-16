using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Coplt.UI.Collections;

public struct EmbedQueue<T> : IReadOnlyCollection<T>
{
    #region Fields

    internal T[]? m_array;
    internal int m_head;
    internal int m_tail;
    internal int m_size;

    #endregion

    #region Ctor

    public EmbedQueue()
    {
        m_array = [];
    }

    public EmbedQueue(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        m_array = new T[capacity];
    }

    #endregion

    #region Props

    [UnscopedRef]
    public int Count => m_size;

    int IReadOnlyCollection<T>.Count => Count;

    [UnscopedRef]
    public int Capacity => m_array?.Length ?? 0;

    #endregion

    #region Private

    [UnscopedRef]
    private void SetCapacity(int capacity)
    {
        Debug.Assert(capacity >= m_size);
        var new_array = new T[capacity];
        if (m_size > 0)
        {
            if (m_head < m_tail)
            {
                Array.Copy(m_array!, m_head, new_array, 0, m_size);
            }
            else
            {
                Array.Copy(m_array!, m_head, new_array, 0, m_array!.Length - m_head);
                Array.Copy(m_array, 0, new_array, m_array.Length - m_head, m_tail);
            }
        }

        m_array = new_array;
        m_head = 0;
        m_tail = m_size == capacity ? 0 : m_size;
    }

    [UnscopedRef]
    private void Grow(int capacity)
    {
        Debug.Assert(m_array is null || m_array.Length < capacity);

        const int GrowFactor = 2;
        const int MinimumGrow = 4;

        var old_capacity = m_array?.Length ?? 0;
        var new_capacity = GrowFactor * old_capacity;

        if ((uint)new_capacity > Array.MaxLength) new_capacity = Array.MaxLength;
        new_capacity = Math.Max(new_capacity, old_capacity + MinimumGrow);
        if (new_capacity < capacity) new_capacity = capacity;

        SetCapacity(new_capacity);
    }

    [UnscopedRef]
    private void MoveNext(ref int index)
    {
        var tmp = index + 1;
        if (tmp == m_array!.Length)
        {
            tmp = 0;
        }
        index = tmp;
    }

    #endregion

    #region Enqueue

    [UnscopedRef]
    public ref T UnsafeEnqueue()
    {
        if (m_array == null || m_size == m_array.Length)
        {
            Grow(m_size + 1);
        }
        ref var slot = ref m_array![m_tail];
        MoveNext(ref m_tail);
        m_size++;
        return ref slot!;
    }

    [UnscopedRef]
    public void Enqueue(T item) => UnsafeEnqueue() = item;

    #endregion

    #region Dequque

    [UnscopedRef]
    public T Dequeue()
    {
        var head = m_head;
        var array = m_array!;

        if (m_size == 0) throw new InvalidOperationException("Queue is empty");

        var removed = array[head];
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            array[head] = default!;

        MoveNext(ref m_head);
        m_size--;
        return removed;
    }

    [UnscopedRef]
    public bool TryDequeue([MaybeNullWhen(false)] out T result)
    {
        var head = m_head;
        var array = m_array!;

        if (m_size == 0)
        {
            result = default;
            return false;
        }

        result = array[head];
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            array[head] = default!;
        }
        MoveNext(ref m_head);
        m_size--;
        return true;
    }

    #endregion

    #region Peek

    [UnscopedRef]
    public readonly ref T Peek()
    {
        if (m_size == 0) throw new InvalidOperationException("Queue is empty");

        return ref m_array![m_head];
    }

    [UnscopedRef]
    public readonly bool TryPeek([MaybeNullWhen(false)] out T result)
    {
        if (m_size == 0)
        {
            result = default;
            return false;
        }

        result = m_array![m_head];
        return true;
    }

    [UnscopedRef]
    public readonly ref T TryPeek(out bool exist)
    {
        if (m_size == 0)
        {
            exist = false;
            return ref Unsafe.NullRef<T>();
        }

        exist = true;
        return ref m_array![m_head];
    }

    #endregion

    #region Clear

    [UnscopedRef]
    public void Clear()
    {
        if (m_size != 0)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                if (m_head < m_tail)
                {
                    Array.Clear(m_array!, m_head, m_size);
                }
                else
                {
                    Array.Clear(m_array!, m_head, m_array!.Length - m_head);
                    Array.Clear(m_array, 0, m_tail);
                }
            }

            m_size = 0;
        }

        m_head = 0;
        m_tail = 0;
    }

    #endregion

    #region Contains

    [UnscopedRef]
    public readonly bool Contains(T item)
    {
        if (m_size == 0) return false;

        if (m_head < m_tail)
        {
            return Array.IndexOf(m_array!, item, m_head, m_size) >= 0;
        }

        return
            Array.IndexOf(m_array!, item, m_head, m_array!.Length - m_head) >= 0 ||
            Array.IndexOf(m_array, item, 0, m_tail) >= 0;
    }

    #endregion

    #region ToArray

    [UnscopedRef]
    public readonly T[] ToArray()
    {
        if (m_size == 0) return [];

        var arr = new T[m_size];

        if (m_head < m_tail)
        {
            Array.Copy(m_array!, m_head, arr, 0, m_size);
        }
        else
        {
            Array.Copy(m_array!, m_head, arr, 0, m_array!.Length - m_head);
            Array.Copy(m_array, 0, arr, m_array.Length - m_head, m_tail);
        }

        return arr;
    }

    #endregion

    #region CopyTo

    [UnscopedRef]
    public readonly void CopyTo(Span<T> span)
    {
        if (span.Length < m_size) throw new ArgumentException("span is too small", nameof(span));

        var numToCopy = m_size;
        if (numToCopy == 0) return;

        var firstPart = Math.Min(m_array!.Length - m_head, numToCopy);
        m_array.AsSpan(m_head, firstPart).CopyTo(span);
        numToCopy -= firstPart;
        if (numToCopy > 0)
        {
            m_array.AsSpan(0, numToCopy).CopyTo(span.Slice(m_array.Length - m_head, numToCopy));
        }
    }

    #endregion

    #region Enumerator

    [UnscopedRef]
    public readonly Enumerator GetEnumerator() => new(in this);

    public ref struct Enumerator(ref readonly EmbedQueue<T> self)
    {
        private readonly ref readonly EmbedQueue<T> m_self = ref self;
        private int m_i;
        private ref T m_cur = ref Unsafe.NullRef<T>();

        public bool MoveNext()
        {
            var size = m_self.m_size;

            var offset = m_i + 1;
            if ((uint)offset < (uint)size)
            {
                m_i = offset;
                var array = m_self.m_array!;
                var index = m_self.m_head + offset;
                if ((uint)index >= (uint)array.Length)
                {
                    index -= array.Length;
                }
                m_cur = ref array[index];

                return true;
            }

            m_i = -2;
            m_cur = ref Unsafe.NullRef<T>();
            return false;
        }

        public ref T Current => ref m_cur;
    }

    private sealed class EnumeratorClass(EmbedQueue<T> self) : IEnumerator<T>
    {
        private readonly EmbedQueue<T> m_self = self;
        private int m_i;
        private T? m_cur;

        public bool MoveNext()
        {
            var size = m_self.m_size;

            var offset = m_i + 1;
            if ((uint)offset < (uint)size)
            {
                m_i = offset;
                var array = m_self.m_array!;
                var index = m_self.m_head + offset;
                if ((uint)index >= (uint)array.Length)
                {
                    index -= array.Length;
                }
                m_cur = array[index];

                return true;
            }

            m_i = -2;
            m_cur = default!;
            return false;
        }

        public T Current => m_cur!;

        object? IEnumerator.Current => m_cur;

        public void Dispose()
        {
            m_i = -2;
            m_cur = default;
        }

        public void Reset()
        {
            m_i = -1;
            m_cur = default;
        }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new EnumeratorClass(this);
    IEnumerator IEnumerable.GetEnumerator() => new EnumeratorClass(this);

    #endregion
}
