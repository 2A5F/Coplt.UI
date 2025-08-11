using System.Buffers;

namespace Coplt.SoftGraphics.Utilities;

public unsafe struct Collector<T>(int InitCap) : IDisposable
{
    #region Fields

    public const int ChunkSize = 1024;

    internal T[]?[] m_chunks = ArrayPool<T[]?>.Shared.Rent(InitCap);
    internal int[] m_offset = ArrayPool<int>.Shared.Rent(InitCap);
    internal int m_current;
    internal int m_count;
    internal int m_cap = InitCap;

    #endregion

    #region Ctor

    public Collector() : this(ChunkSize) { }

    #endregion

    #region Dispose

    public void Dispose()
    {
        for (var i = 0; i <= m_current; i++)
        {
            ref var chunk = ref m_chunks[i];
            ArrayPool<T>.Shared.Return(chunk!);
            chunk = null;
        }
        ArrayPool<int>.Shared.Return(m_offset);
        m_offset = null!;
        ArrayPool<T[]?>.Shared.Return(m_chunks);
        m_chunks = null!;
    }

    #endregion

    #region Grow

    private void Grow()
    {
        m_cap *= 2;
        var old_chunks = m_chunks;
        var old_offset = m_offset;
        try
        {
            var new_chunks = ArrayPool<T[]?>.Shared.Rent(m_cap);
            var new_offset = ArrayPool<int>.Shared.Rent(m_cap);
            Array.Copy(old_chunks, new_chunks, 0);
            Array.Copy(old_offset, new_offset, 0);
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
            ArrayPool<T[]?>.Shared.Return(old_chunks);
            ArrayPool<int>.Shared.Return(old_offset);
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
        if (index >= ChunkSize)
        {
            m_current++;
            if (m_current >= m_cap) Grow();
        }
        m_count++;
        return ref slot;
    }

    #endregion
}
