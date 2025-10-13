using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Dropping;

namespace Coplt.UI.Collections;

[Dropping]
internal unsafe partial struct PinChunkList<T>
{
    #region Const

    internal const int DefaultCapacity = 16;

    #endregion

    #region Fields

    [Drop]
    internal NativeList<NChunk> m_n_chunks;
    internal EmbedList<Chunk> m_chunks;
    internal int m_chunk_index;

    #region Chunk

    public struct Chunk(int cap)
    {
        public readonly T[] m_data = RuntimeHelpers.IsReferenceOrContainsReferences<T>()
            ? GC.AllocateArray<T>(cap, true)
            : GC.AllocateUninitializedArray<T>(cap, true);
    }

    public struct NChunk(T* data, int cap)
    {
        public T* m_data = data;
        public int m_cap = cap;
        public int m_len;
    }

    public ref struct ChunkRef(ref Chunk chunk, ref NChunk n_chunk)
    {
        public ref Chunk chunk = ref chunk;
        public ref NChunk n_chunk = ref n_chunk;
    }

    #endregion

    #endregion

    #region Props

    [UnscopedRef]
    public ref readonly NativeList<NChunk> RawNChunks => ref m_n_chunks;

    [UnscopedRef]
    public ref readonly EmbedList<Chunk> RawChunks => ref m_chunks;

    [UnscopedRef]
    public ref readonly int RawChunkIndex => ref m_chunk_index;

    #endregion

    #region Add

    [UnscopedRef]
    private ChunkRef AddGetChunk()
    {
        if (m_chunks.Count == 0)
        {
            ref var chunk = ref m_chunks.UnsafeAdd();
            chunk = new Chunk(DefaultCapacity);
            ref var n_chunk = ref m_n_chunks.UnsafeAdd();
            n_chunk = new((T*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(chunk.m_data)), chunk.m_data.Length);
            return new(ref chunk, ref n_chunk);
        }
        else
        {
            ref var chunk = ref m_chunks[m_chunk_index];
            ref var n_chunk = ref m_n_chunks[m_chunk_index];
            if (n_chunk.m_len >= n_chunk.m_cap)
            {
                m_chunk_index++;
                if (m_chunk_index >= m_chunks.Count)
                {
                    chunk = ref m_chunks.UnsafeAdd();
                    n_chunk = ref m_n_chunks.UnsafeAdd();
                    chunk = new Chunk(chunk.m_data.Length * 2);
                    n_chunk = new((T*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(chunk.m_data)), chunk.m_data.Length);
                }
            }
            return new(ref chunk, ref n_chunk);
        }
    }

    [UnscopedRef]
    public ref T UnsafeAdd()
    {
        var chunk = AddGetChunk();
        var index = chunk.n_chunk.m_len++;
        return ref chunk.n_chunk.m_data[index];
    }

    [UnscopedRef]
    public void Add(T item) => UnsafeAdd() = item;

    #endregion

    #region Get

    [UnscopedRef]
    public readonly ChunkRef ChunkAt(int chunk_index) => new(ref m_chunks[chunk_index], ref m_n_chunks[chunk_index]);

    [UnscopedRef]
    public ref T this[int chunk, int index] => ref m_n_chunks.Raw[chunk].m_data[index];

    #endregion
}
