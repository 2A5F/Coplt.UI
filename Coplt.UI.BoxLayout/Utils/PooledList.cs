using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Coplt.UI.BoxLayout.Utilities;

public struct PooledList<T> : IDisposable
{
    private T[]? m_items;
    private int m_size;

    public PooledList(int capacity)
    {
        m_items = ArrayPool<T>.Shared.Rent(capacity);
        m_size = 0;
    }

    public void Dispose()
    {
        if (m_items is null) return;
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) AsSpan.Clear();
        ArrayPool<T>.Shared.Return(m_items);
        m_items = null;
    }

    public PooledList<T> Move()
    {
        var r = this;
        m_items = null;
        return r;
    }

    public int Capacity => m_items?.Length ?? 0;

    public int Count => m_size;

    public Span<T> AsSpan => m_items is null ? [] : m_items.AsSpan(0, m_size);

    public ref T this[int index] => ref AsSpan[index];

    public void EnsureCapacity(int capacity)
    {
        if (m_items is not null && m_items.Length >= capacity) return;
        var new_items = ArrayPool<T>.Shared.Rent(capacity);
        if (m_items is not null)
        {
            var span = AsSpan;
            span.CopyTo(new_items);
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) span.Clear();
            ArrayPool<T>.Shared.Return(m_items);
        }
        m_items = new_items;
    }

    [UnscopedRef]
    public ref T UnsafeAdd()
    {
        if (Capacity <= m_size) EnsureCapacity(Math.Max(Capacity * 2, 4));
        return ref m_items![m_size++];
    }

    [UnscopedRef]
    public void Add(T item) => UnsafeAdd() = item;

    [UnscopedRef]
    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) AsSpan.Clear();
        m_size = 0;
    }

    public int IndexOf(T item) => Array.IndexOf(m_items!, item, 0, m_size);

    public void RemoveAt(int index)
    {
        if (m_items is null || (uint)index >= (uint)m_size) throw new ArgumentOutOfRangeException();
        m_size--;
        if (index < m_size)
        {
            Array.Copy(m_items!, index + 1, m_items!, index, m_size - index);
        }
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            m_items[m_size] = default!;
        }
    }

    public bool Remove(T item)
    {
        var index = IndexOf(item);
        if (index < 0) return false;
        RemoveAt(index);
        return true;
    }

    public Span<T>.Enumerator GetEnumerator() => AsSpan.GetEnumerator();
}
