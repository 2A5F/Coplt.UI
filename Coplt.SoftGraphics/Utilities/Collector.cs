using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Coplt.SoftGraphics.Utilities;

internal struct LocalCollector<T>(int InitCap) : IDisposable
{
    #region Fields

    public const int ChunkSize = 1024;
    public const int InitCap = 4;

    internal T[]?[] m_chunks = ArrayPool<T[]?>.Shared.Rent(InitCap);
    internal int[] m_offset = ArrayPool<int>.Shared.Rent(InitCap);
    internal int m_current;
    internal int m_count;
    internal int m_cap = InitCap;

    #endregion

    #region Ctor

    public LocalCollector() : this(InitCap) { }

    #endregion

    #region Dispose

    public void Dispose()
    {
        if (m_offset == null) return;
        Drop(true);
    }

    internal void Drop(bool ret)
    {
        if (ret)
        {
            for (var i = 0; i <= m_current; i++)
            {
                ref var chunk = ref m_chunks[i];
                ArrayPool<T>.Shared.Return(chunk!, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
                chunk = null;
            }
        }
        ArrayPool<int>.Shared.Return(m_offset);
        ArrayPool<T[]?>.Shared.Return(m_chunks, !ret);
        m_offset = null!;
        m_chunks = null!;
        m_current = 0;
        m_count = 0;
        m_cap = 0;
    }

    internal void ClearKeepLastChunk()
    {
        m_chunks.AsSpan(0, m_current).Clear();
        m_offset.AsSpan(0, m_current).Clear();
        m_chunks[0] = m_chunks[m_current];
        m_offset[0] = m_offset[m_current];
        m_chunks[m_current] = null;
        m_offset[m_current] = 0;
        m_current = 0;
        m_count %= ChunkSize;
    }

    #endregion

    #region Move

    public LocalCollector<T> Move()
    {
        var r = this;
        m_offset = null!;
        m_chunks = null!;
        return r;
    }

    #endregion

    #region Grow

    private void Grow()
    {
        var cap = m_cap;
        m_cap *= 2;
        var old_chunks = m_chunks;
        var old_offset = m_offset;
        if (old_chunks.Length >= m_cap) return;
        try
        {
            var new_chunks = ArrayPool<T[]?>.Shared.Rent(m_cap);
            var new_offset = ArrayPool<int>.Shared.Rent(m_cap);
            old_chunks.AsSpan(0, cap).CopyTo(new_chunks.AsSpan(0, cap));
            old_offset.AsSpan(0, cap).CopyTo(new_offset.AsSpan(0, cap));
            m_chunks = new_chunks;
            m_offset = new_offset;
        }
        catch
        {
            m_chunks = null!;
            m_offset = null!;
            throw;
        }
        finally
        {
            ArrayPool<T[]?>.Shared.Return(old_chunks, true);
            ArrayPool<int>.Shared.Return(old_offset);
        }
    }

    #endregion

    #region Props

    public int Count => m_count;

    public ref T this[int i]
    {
        get
        {
            if ((uint)i >= (uint)m_count) throw new IndexOutOfRangeException();
            var (q, r) = Math.DivRem(i, ChunkSize);
            return ref m_chunks[q]![r];
        }
    }

    #endregion

    #region Add

    public ref T Add()
    {
        ref var chunk = ref m_chunks[m_current];
        ref var offset = ref m_offset[m_current];
        chunk ??= ArrayPool<T>.Shared.Rent(ChunkSize);
        var index = offset++;
        ref var slot = ref chunk[index];
        if (offset >= ChunkSize)
        {
            m_current++;
            if (m_current >= m_cap) Grow();
        }
        m_count++;
        return ref slot;
    }

    public void BatchAdd(ReadOnlySpan<T> items)
    {
        re:
        if (items.Length == 0) return;
        ref var chunk = ref m_chunks[m_current];
        ref var offset = ref m_offset[m_current];
        chunk ??= ArrayPool<T>.Shared.Rent(ChunkSize);
        var index = offset;
        var len = Math.Min(items.Length, ChunkSize - offset);
        Debug.Assert(len > 0);
        items[..len].CopyTo(chunk.AsSpan(index));
        m_count += len;
        items = items[len..];
        offset += len;
        if (offset >= ChunkSize)
        {
            m_current++;
            if (m_current >= m_cap) Grow();
        }
        goto re;
    }

    #endregion

    #region Enumerator

    [UnscopedRef]
    public Enumerator GetEnumerator() => new(ref this);

    public ref struct Enumerator(ref LocalCollector<T> self)
    {
        private readonly ref LocalCollector<T> m_self = ref self;
        private int m_index = -1;

        public bool MoveNext()
        {
            var index = m_index + 1;
            if (index >= m_self.m_count) return false;
            m_index = index;
            return true;
        }

        public ref T Current => ref m_self[m_index];
    }

    #endregion
}

internal struct Collector<T>() : IDisposable
{
    public const int ChunkSize = LocalCollector<T>.ChunkSize;

    private LocalCollector<T>[] m_collectors = ArrayPool<LocalCollector<T>>.Shared.Rent(Environment.ProcessorCount);
    private int m_len;

    public void Dispose()
    {
        if (m_collectors == null) return;
        for (var i = 0; i <= m_len; i++)
        {
            m_collectors[i].Dispose();
        }
        ArrayPool<LocalCollector<T>>.Shared.Return(m_collectors);
        m_collectors = null!;
    }

    public Collector<T> Move()
    {
        var r = this;
        m_collectors = null!;
        return r;
    }

    public int Alloc()
    {
        var len = m_len;
        if (len >= m_collectors.Length) throw new IndexOutOfRangeException();
        m_len++;
        m_collectors[len] = new();
        return len;
    }

    public ref LocalCollector<T> this[int i]
    {
        get
        {
            if ((uint)i >= (uint)m_len) throw new IndexOutOfRangeException();
            return ref m_collectors[i];
        }
    }

    public LocalCollector<T> ToCollected()
    {
        var sum_count = 0;
        for (var i = 0; i < m_len; i++)
        {
            ref var collector = ref m_collectors[i];
            if (collector.Count == 0) continue;
            sum_count += collector.Count;
        }
        var result = new LocalCollector<T>((sum_count + ChunkSize - 1) / ChunkSize);
        try
        {
            for (var i = 0; i < m_len; i++)
            {
                ref var old = ref m_collectors[i];
                if (old.Count == 0) continue;
                if (old.Count % ChunkSize == 0)
                {
                    var len = old.m_current + 1;
                    old.m_chunks.AsSpan(0, len).CopyTo(result.m_chunks.AsSpan(result.m_current, len));
                    result.m_current += len;
                    old.Drop(false);
                }
                else
                {
                    var len = old.m_current;
                    if (len == 0) continue;
                    old.m_chunks.AsSpan(0, len).CopyTo(result.m_chunks.AsSpan(result.m_current, len));
                    result.m_current += len;
                    old.ClearKeepLastChunk();
                }
            }
            for (var i = 0; i < m_len; i++)
            {
                ref var old = ref m_collectors[i];
                if (old.Count == 0) continue;
                result.BatchAdd(old.m_chunks[0].AsSpan(0, old.Count));
                old.Drop(false);
            }
            Dispose();
            result.m_count = sum_count;
            return result.Move();
        }
        finally
        {
            result.Dispose();
        }
    }
}
