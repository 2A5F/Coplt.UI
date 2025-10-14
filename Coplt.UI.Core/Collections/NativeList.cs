using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Coplt.Com;
using Coplt.Dropping;
using Coplt.UI.Native;
using Coplt.UI.Utilities;

namespace Coplt.UI.Collections;

[Dropping]
public unsafe partial struct NativeList<T> : IList<T>, IReadOnlyList<T>
{
    #region Static Check

    static NativeList()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            throw new NotSupportedException("Only unmanaged types are supported");
    }

    #endregion
    
    #region Capacity

    private const int DefaultCapacity = 4;

    #endregion

    #region Fields

    private T* m_items;
    private int m_cap;
    private int m_size;

    #endregion

    #region Ctor

    public NativeList(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));

        m_cap = capacity;
        m_items = capacity == 0 ? null : Alloc(capacity);
    }

    #endregion

    #region Props

    public int Count => m_size;

    bool ICollection<T>.IsReadOnly => false;

    public T* Raw => m_items;

    public Span<T> AsSpan => new(m_items, m_size);
    public NSpan<T> AsNSpan => new(m_items, (nuint)m_size);

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
        get => m_cap;
        set
        {
            if (value < m_size) throw new ArgumentOutOfRangeException(nameof(value));

            if (m_items == null)
            {
                m_items = value == 0 ? null : Alloc(value);
            }
            else if (value != m_cap)
            {
                if (m_size > 0)
                {
                    m_items = ReAlloc(m_items, value);
                }
                else
                {
                    Free();
                }
            }
            m_cap = value;
        }
    }

    #endregion

    #region Private

    private static T* Alloc(int size) => (T*)NativeLib.Instance.Alloc(sizeof(T) * size, Utils.AlignOf<T>());

    private static T* ReAlloc(T* ptr, int size) => (T*)NativeLib.Instance.ReAlloc(ptr, sizeof(T) * size, Utils.AlignOf<T>());

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

        if (m_items != null)
        {
            var newItems = Alloc(newCapacity);
            if (indexToInsert != 0)
            {
                new Span<T>(m_items, indexToInsert).CopyTo(new Span<T>(newItems, indexToInsert));
            }

            if (m_size != indexToInsert)
            {
                new Span<T>(m_items, m_size)[indexToInsert..].CopyTo(new Span<T>(newItems, m_size + 1)[(indexToInsert + 1)..]);
            }
            Free();
            m_items = newItems;
        }
        else
        {
            m_items = Alloc(newCapacity);
        }
        m_cap = newCapacity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetNewCapacity(int capacity)
    {
        Debug.Assert(m_items == null || m_cap < capacity);

        var newCapacity = m_items == null || m_cap == 0 ? DefaultCapacity : 2 * m_cap;

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
        Debug.Assert(m_items == null || m_size == m_cap);
        var size = m_size;
        Grow(size + 1);
        m_size = size + 1;
        return ref m_items![size];
    }

    #endregion

    #region Dispose

    [Drop]
    private void Free()
    {
        if (m_items == null) return;
        NativeLib.Instance.Free(m_items, Utils.AlignOf<T>());
        m_items = null;
    }

    #endregion

    #region Add

    public ref T UnsafeAdd()
    {
        var items = m_items;
        var size = m_size;
        if (items != null && (uint)size < (uint)m_cap)
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

    [Drop(Order = -1)]
    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            var size = m_size;
            m_size = 0;
            if (size > 0)
            {
                AsSpan.Clear();
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
        if (m_items == null || m_size == m_cap) GrowForInsertion(index, 1);
        else if (index < m_size)
        {
            AsSpan.Slice(index, m_size - 1).CopyTo(AsSpan[(index + 1)..]);
        }
        m_items![index] = item;
        m_size++;
    }

    #endregion

    #region IndexOf

    public int IndexOf(T item) => m_size == 0 ? -1 : AsSpan.IndexOf(item);

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
            AsSpan[(index + 1)..].CopyTo(AsSpan.Slice(index, m_size - 1));
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

    private class ClassEnumerator(NSpan<T> range) : IEnumerator<T>
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

        public T Current => range[_index];

        object? IEnumerator.Current => Current;

        public void Reset() => _index = -1;

        public void Dispose() { }
    }

    IEnumerator IEnumerable.GetEnumerator() => new ClassEnumerator(AsNSpan);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new ClassEnumerator(AsNSpan);

    #endregion

    #region GetPinnableReference

    public ref T GetPinnableReference() => ref *m_items;

    #endregion
}
