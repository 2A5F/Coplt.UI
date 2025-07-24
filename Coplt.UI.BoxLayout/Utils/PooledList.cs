using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Coplt.UI.BoxLayout.Utilities;

public struct PooledList<T> : IDisposable
{
    private T[]? m_items;
    private int m_size;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PooledList(int capacity)
    {
        m_items = ArrayPool<T>.Shared.Rent(Math.Max(capacity, 4));
        m_size = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (m_items is null) return;
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) AsSpan.Clear();
        ArrayPool<T>.Shared.Return(m_items);
        m_items = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PooledList<T> Move()
    {
        var r = this;
        m_items = null;
        return r;
    }

    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => m_items?.Length ?? 0;
    }

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => m_size;
    }

    public Span<T> AsSpan
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => m_items is null ? [] : m_items.AsSpan(0, m_size);
    }

    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref AsSpan[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T UnsafeAdd()
    {
        if (Capacity <= m_size) EnsureCapacity(Math.Max(Capacity * 2, 4));
        return ref m_items![m_size++];
    }

    [UnscopedRef]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item) => UnsafeAdd() = item;

    [UnscopedRef]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) AsSpan.Clear();
        m_size = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int IndexOf(T item) => Array.IndexOf(m_items!, item, 0, m_size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item)
    {
        var index = IndexOf(item);
        if (index < 0) return false;
        RemoveAt(index);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T>.Enumerator GetEnumerator() => AsSpan.GetEnumerator();
}
