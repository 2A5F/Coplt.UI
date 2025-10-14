using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Dropping;
using Coplt.UI.Native;
using Coplt.UI.Utilities;

namespace Coplt.UI.Collections;

public static class Hive
{
    #region Const

    private const int BaseExp = 4;
    public const int InitCapacity = 1 << BaseExp;

    #endregion

    #region Utils

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ChunkSize(int chunk) => InitCapacity << chunk;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int chunk, int index) Locate(int index)
    {
        var scaled = (uint)index >> BaseExp;
        var chunk = BitOperations.Log2(scaled + 1);
        var bas = (1 << (BaseExp + chunk)) - InitCapacity;
        return (chunk,  index - bas);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Index(int chunk, int index)
    {
        var bas = (1 << (BaseExp + chunk)) - InitCapacity;
        return bas + index;
    }

    #endregion
}

public interface IHiveChunkMeta<out Self>
{
    public static abstract Self Create(int chunk, int size);
}

[Dropping]
public unsafe partial struct NativeHiveCtrl
{
    #region Fields

    [Drop]
    internal NativeHiveStorage<Ctrl, Meta> m_storage;
    internal int m_cur_chunk;
    internal int m_size;

    #endregion

    #region Structs

    public struct Meta(int cap, int chunk_index) : IHiveChunkMeta<Meta>
    {
        public int len;
        public readonly int cap = cap;
        public readonly int chunk_index = chunk_index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Meta Create(int chunk, int size) => new(size, chunk);
    }

    public struct Ctrl
    {
        public uint free_count;
    }

    #endregion

    #region Props

    public int Count => m_size;

    #endregion

    #region Add

    [UnscopedRef]
    private ref NativeHiveStorage<Ctrl, Meta>.Chunk LastChunk()
    {
        if (m_storage.Count == 0) return ref m_storage.EnsureChunk(0);
        ref var chunk = ref *m_storage.m_chunks.UnsafeAt(m_cur_chunk);
        if (chunk.m_meta.len >= chunk.m_meta.cap)
        {
            m_cur_chunk++;
            return ref m_storage.EnsureChunk(m_storage.m_chunks.Count);
        }
        return ref chunk;
    }

    [UnscopedRef]
    public (int chunk, int index) Add()
    {
        if (m_size >= Array.MaxLength) throw new OutOfMemoryException();
        ref var chunk = ref LastChunk();
        chunk.m_meta.len++;
        var index = m_size++;
        return (chunk.m_meta.chunk_index, index);
    }

    #endregion
}

public struct HiveStorage<T>
{
    #region Fields

    internal EmbedList<T[]> m_chunks;

    #endregion

    #region Props

    public int Count => m_chunks.Count;

    #endregion

    #region EnsureChunk

    [UnscopedRef]
    public T[] EnsureChunk(int chunk)
    {
        var len = m_chunks.Count;
        if (chunk < len)
        {
            return m_chunks.UnsafeAt(chunk);
        }
        for (var i = len; i <= chunk; i++)
        {
            var size = Hive.InitCapacity << i;
            m_chunks.Add(GC.AllocateUninitializedArray<T>(size));
        }
        return m_chunks.UnsafeAt(chunk);
    }

    #endregion

    #region EnsureAt

    [UnscopedRef]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T EnsureAt(int chunk, int index) => ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(EnsureChunk(chunk)), index);

    #endregion
}

[Dropping]
public unsafe partial struct NativeHiveStorage<T>
{
    #region Fields

    [Drop]
    internal NativeList<Chunk> m_chunks;

    #endregion

    #region Chunk

    public struct Chunk
    {
        public T* m_data;
    }

    #endregion

    #region Props

    public int Count => m_chunks.Count;

    #endregion

    #region EnsureChunk

    [UnscopedRef]
    public T* EnsureChunk(int chunk)
    {
        var len = m_chunks.Count;
        if (chunk < len)
        {
            return m_chunks.UnsafeAt(chunk)->m_data!;
        }
        for (var i = len; i <= chunk; i++)
        {
            var size = Hive.InitCapacity << i;
            m_chunks.Add(new()
            {
                m_data = (T*)NativeLib.Instance.Alloc(size, Utils.AlignOf<T>()),
            });
        }
        return m_chunks.UnsafeAt(chunk)->m_data!;
    }

    #endregion

    #region EnsureAt

    [UnscopedRef]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* EnsureAt(int chunk, int index) => &EnsureChunk(chunk)[index];

    #endregion

    #region Drop

    [Drop(Order = -1)]
    private void Drop()
    {
        var inst = NativeLib.Instance;
        foreach (ref var chunk in m_chunks)
        {
            inst.Free(chunk.m_data, Utils.AlignOf<T>());
        }
    }

    #endregion
}

[Dropping]
public unsafe partial struct NativeHiveStorage<T, C>
    where C : IHiveChunkMeta<C>
{
    #region Fields

    [Drop]
    internal NativeList<Chunk> m_chunks;

    #endregion

    #region Chunk

    public struct Chunk
    {
        public T* m_data;
        public C m_meta;
    }

    #endregion

    #region Props

    public int Count => m_chunks.Count;

    #endregion

    #region EnsureChunk

    [UnscopedRef]
    public ref Chunk EnsureChunk(int chunk)
    {
        var len = m_chunks.Count;
        if (chunk < len)
        {
            return ref *m_chunks.UnsafeAt(chunk);
        }
        for (var i = len; i <= chunk; i++)
        {
            var size = Hive.InitCapacity << i;
            m_chunks.Add(new()
            {
                m_data = (T*)NativeLib.Instance.Alloc(size, Utils.AlignOf<T>()),
                m_meta = C.Create(chunk, size),
            });
        }
        return ref *m_chunks.UnsafeAt(chunk);
    }

    #endregion

    #region Drop

    [Drop(Order = -1)]
    private void Drop()
    {
        var inst = NativeLib.Instance;
        foreach (ref var chunk in m_chunks)
        {
            inst.Free(chunk.m_data, Utils.AlignOf<T>());
        }
    }

    #endregion
}

[Dropping]
public unsafe partial struct NativeHive<T>
{
    #region Fields

    internal NativeHiveCtrl m_ctrl;
    internal NativeHiveStorage<T> m_storage;

    #endregion

    #region Add

    public ref T UnsafeAdd(out (int chunk, int index) place)
    {
        var (chunk, index) = m_ctrl.Add();
        place = (chunk, index);
        return ref *m_storage.EnsureAt(chunk, index);
    }

    public (int chunk, int index) Add(T item)
    {
        var (chunk, index) = m_ctrl.Add();
        *m_storage.EnsureAt(chunk, index) = item;
        return (chunk, index);
    }

    #endregion
}
