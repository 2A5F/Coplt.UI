using System.Diagnostics.CodeAnalysis;
using Coplt.UI.Trees;

namespace Coplt.UI.Collections;

internal unsafe struct DenseSet
{
    #region Const

    internal const int DefaultCapacity = 16;

    #endregion

    #region Fields

    internal ulong m_id_inc;
    internal EmbedMap<ulong, (int chunk, int index)> m_id_map;
    internal EmbedList<Chunk> m_chunks;
    internal int m_chunk_index;
    internal int m_count;

    #endregion

    #region Chunk

    internal struct Chunk(int index, int cap)
    {
        public readonly ulong[] m_ids = GC.AllocateUninitializedArray<ulong>(cap);
        public readonly int m_chunk_index = index;
        public int m_len;
    }

    #endregion

    #region Add

    [UnscopedRef]
    private ref Chunk AddGetChunk()
    {
        if (m_chunks.Count == 0)
        {
            ref var chunk = ref m_chunks.UnsafeAdd();
            chunk = new Chunk(0, DefaultCapacity);
            return ref chunk;
        }
        else
        {
            ref var chunk = ref m_chunks[m_chunk_index];
            if (chunk.m_len >= chunk.m_ids.Length)
            {
                m_chunk_index++;
                if (m_chunk_index >= m_chunks.Count)
                {
                    chunk = ref m_chunks.UnsafeAdd();
                    chunk = new Chunk(m_chunk_index, chunk.m_ids.Length * 2);
                }
            }
            return ref chunk;
        }
    }

    [UnscopedRef]
    public NodeId Add(out int chunk, out int index)
    {
        var id_ = m_id_inc++;
        var chunk_ = AddGetChunk();
        var index_ = chunk_.m_len++;
        m_id_map[id_] = (chunk_.m_chunk_index, index_);
        chunk_.m_ids[index_] = id_;
        m_count++;
        chunk = chunk_.m_chunk_index;
        index = index_;
        return new(id_);
    }

    #endregion

    #region Remove

    public interface IRemoveHandler
    {
        public void Swap(int chunk, int index, int last_chunk, int last_index);
    }

    [UnscopedRef]
    public bool Remove<H>(H handler, NodeId id, out int chunk, out int index)
        where H : struct, IRemoveHandler
    {
        if (!m_id_map.Remove(id.Id, out var place))
        {
            chunk = 0;
            index = 0;
            return false;
        }
        chunk = place.chunk;
        index = place.index;
        m_count--;
        ref var target_chunk = ref m_chunks[place.chunk];
        ref var target_id = ref target_chunk.m_ids[place.index];
        ref var last_chunk = ref m_chunks[m_chunk_index];
        last_chunk.m_len--;
        ref var last_id = ref last_chunk.m_ids[last_chunk.m_len];
        if (last_id == id.Id) goto Remove;
        handler.Swap(place.chunk, place.index, m_chunk_index, last_chunk.m_len);
        target_id = last_id;
        Remove:
        if (last_chunk.m_len == 0 && m_chunk_index > 0) m_chunk_index--;
        return true;
    }

    #endregion
}
