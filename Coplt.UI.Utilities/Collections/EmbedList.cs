using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Coplt.UI.Collections;

public struct EmbedList<T> : IList<T>, IReadOnlyList<T>
{
    #region Capacity

    private const int DefaultCapacity = 4;

    #endregion

    #region Fields

    private T[]? m_items;
    private int m_size;

    #endregion

    #region Ctor

    public EmbedList(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));

        m_items = capacity == 0 ? [] : new T[capacity];
    }

    #endregion

    #region Props

    public int Count => m_size;

    bool ICollection<T>.IsReadOnly => false;

    public T[] UnsafeInnerArray => m_items ?? [];

    public Memory<T> AsMemory => m_items.AsMemory(0, m_size);

    public Span<T> AsSpan => m_items.AsSpan(0, m_size);

    public ref T this[int index] => ref AsSpan[index];

    T IReadOnlyList<T>.this[int index] => this[index];

    T IList<T>.this[int index]
    {
        get => this[index];
        set => this[index] = value;
    }

    #endregion

    #region Capacity

    public int Capacity
    {
        get => m_items?.Length ?? 0;
        set
        {
            if (value < m_size) throw new ArgumentOutOfRangeException(nameof(value));

            if (m_items == null)
            {
                m_items = value == 0 ? [] : new T[value];
            }
            else if (value != m_items.Length)
            {
                var new_items = new T[value];
                if (m_size > 0)
                {
                    m_items.AsSpan(0, m_size).CopyTo(new_items.AsSpan(0, m_size));
                    m_items = new_items;
                }
                else
                {
                    m_items = [];
                }
            }
        }
    }

    #endregion

    #region Private

    private void Grow(int capacity)
    {
        Capacity = GetNewCapacity(capacity);
    }

    private void GrowForInsertion(int indexToInsert, int insertionCount = 1)
    {
        Debug.Assert(insertionCount > 0);

        var requiredCapacity = checked(m_size + insertionCount);
        var newCapacity = GetNewCapacity(requiredCapacity);

        // Inline and adapt logic from set_Capacity

        T[] newItems = new T[newCapacity];
        if (m_items != null)
        {
            if (indexToInsert != 0)
            {
                Array.Copy(m_items, newItems, length: indexToInsert);
            }

            if (m_size != indexToInsert)
            {
                Array.Copy(m_items, indexToInsert, newItems, indexToInsert + insertionCount, m_size - indexToInsert);
            }
        }

        m_items = newItems;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetNewCapacity(int capacity)
    {
        Debug.Assert(m_items == null || m_items.Length < capacity);

        var newCapacity = m_items == null || m_items.Length == 0 ? DefaultCapacity : 2 * m_items.Length;

        // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
        // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
        if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;

        // If the computed capacity is still less than specified, set to the original argument.
        // Capacities exceeding Array.MaxLength will be surfaced as OutOfMemoryException by Array.Resize.
        if (newCapacity < capacity) newCapacity = capacity;

        return newCapacity;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private ref T UnsafeAddWithResize()
    {
        Debug.Assert(m_items == null || m_size == m_items.Length);
        var size = m_size;
        Grow(size + 1);
        m_size = size + 1;
        return ref m_items![size];
    }

    #endregion

    #region Add

    public ref T UnsafeAdd()
    {
        var items = m_items;
        var size = m_size;
        if (items != null && (uint)size < (uint)items.Length)
        {
            m_size = size + 1;
            return ref items[size];
        }
        else
        {
            return ref UnsafeAddWithResize();
        }
    }

    public void Add(T item) => UnsafeAdd() = item;

    #endregion

    #region Clear

    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            var size = m_size;
            m_size = 0;
            if (size > 0)
            {
                m_items.AsSpan(0, size).Clear();
            }
        }
        else
        {
            m_size = 0;
        }
    }

    public void UnsafeClear()
    {
        m_size = 0;
    }

    #endregion

    #region Insert

    public void Insert(int index, T item)
    {
        if ((uint)index > (uint)m_size) throw new ArgumentOutOfRangeException();
        if (m_items == null || m_size == m_items.Length) GrowForInsertion(index, 1);
        else if (index < m_size) Array.Copy(m_items, index, m_items, index + 1, m_size - index);
        m_items![index] = item;
        m_size++;
    }

    #endregion

    #region IndexOf

    public int IndexOf(T item) => m_size == 0 ? -1 : Array.IndexOf(m_items!, item, 0, m_size);

    #endregion

    #region Contains

    public bool Contains(T item) => IndexOf(item) != -1;

    #endregion

    #region RemoveAt

    public void RemoveAt(int index)
    {
        if ((uint)index >= (uint)m_size) throw new IndexOutOfRangeException(nameof(index));
        m_size--;
        if (index < m_size)
        {
            Array.Copy(m_items!, index + 1, m_items!, index, m_size - index);
        }
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            m_items![m_size] = default!;
        }
    }

    #endregion

    #region Remove

    public bool Remove(T item)
    {
        var index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }

        return false;
    }

    #endregion

    #region CopyTo

    public void CopyTo(Span<T> target) => AsSpan.CopyTo(target);

    public void CopyTo(T[] array, int arrayIndex) => CopyTo(array.AsSpan(arrayIndex));

    #endregion

    #region Enumerator

    public Span<T>.Enumerator GetEnumerator() => AsSpan.GetEnumerator();

    #endregion

    #region EnumeratorClass

    private class ClassEnumerator(Memory<T> range) : IEnumerator<T>
    {
        private int _index = -1;

        public bool MoveNext()
        {
            int index = _index + 1;
            if (index < range.Length)
            {
                _index = index;
                return true;
            }

            return false;
        }

        public T Current => range.Span[_index];

        object? IEnumerator.Current => Current;

        public void Reset() => _index = -1;

        public void Dispose() { }
    }

    IEnumerator IEnumerable.GetEnumerator() => new ClassEnumerator(AsMemory);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new ClassEnumerator(AsMemory);

    #endregion

    #region GetPinnableReference

    public ref T GetPinnableReference() => ref AsSpan.GetPinnableReference();

    #endregion
}
