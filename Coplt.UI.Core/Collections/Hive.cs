using System.Diagnostics;
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

    public static readonly int MaxChunks = Locate(Array.MaxLength).chunk;

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
        return (chunk, index - bas);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Index(int chunk, int index)
    {
        var bas = (1 << (BaseExp + chunk)) - InitCapacity;
        return bas + index;
    }

    #endregion
}

[Dropping]
public unsafe partial struct NativeHiveCtrl
{
    #region Fields

    [Drop]
    internal NativeHiveStorage<Ctrl, Meta> m_storage;
    internal int m_free_chunk_head;
    internal int m_free_chunk_tail;
    internal int m_size;

    #endregion

    #region Structs

    public record struct Meta(int cap, int chunk_index) : NativeHiveStorage<Ctrl, Meta>.IMeta
    {
        public readonly int chunk_index = chunk_index;
        public readonly int cap = cap;
        public int free_head;
        public int next_free_chunk;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Meta Create(Ctrl* data, int chunk, int size) => new(size, chunk);
    }

    public record struct Ctrl(int skip_field, int next_free)
    {
        public int skip_field = skip_field;
        public int next_free = next_free;
    }

    #endregion

    #region Props

    public int Count => m_size;

    #endregion

    #region Exists

    public bool Exists(int chunk, int index)
    {
        if (chunk < 0 || index < 0) return false;
        ref var ch = ref m_storage.TryGetChunk(chunk, out var chunk_exist);
        if (!chunk_exist) return false;
        if (index >= ch.m_meta.cap) return false;
        ref var ctrl = ref ch.m_data[index];
        return ctrl.skip_field == 0;
    }

    #endregion

    #region Add

    [UnscopedRef]
    private (int chunk, int index) GrowAdd()
    {
        var chunk_index = m_storage.m_chunks.Count;
        if (chunk_index >= Hive.MaxChunks) throw new OutOfMemoryException();
        ref var chunk = ref m_storage.EnsureChunk(chunk_index);
        if (chunk_index != 0)
        {
            ref var last_chunk = ref m_storage.UnsafeGetChunk(m_free_chunk_tail);
            last_chunk.m_meta.next_free_chunk = chunk_index;
        }
        new Span<Ctrl>(chunk.m_data, chunk.m_meta.cap).Fill(new(-1, -1)); // aligned simd fill
        var size = chunk.m_meta.cap - 1;
        chunk.m_data[0] = new(0, -1);
        chunk.m_data[1] = new(size, -1);
        chunk.m_data[size] = new(size, -1);
        chunk.m_meta.free_head = 1;
        m_size++;
        return (chunk.m_meta.chunk_index, 0);
    }

    [UnscopedRef]
    public (int chunk, int index) Add()
    {
        if (m_free_chunk_head < 0 || m_storage.Count == 0) return GrowAdd();
        ref var chunk = ref m_storage.UnsafeGetChunk(m_free_chunk_head);
        Debug.Assert(chunk.m_meta.free_head >= 0);
        var index = chunk.m_meta.free_head;
        ref var slot = ref chunk.m_data[index];
        Debug.Assert(slot.skip_field > 0);
        var l = slot.skip_field - 1;
        slot.skip_field = 0;
        if (l == 0)
        {
            Debug.Assert(slot.next_free != 0);
            if (slot.next_free > 0) chunk.m_meta.free_head = slot.next_free;
            else
            {
                Debug.Assert(slot.next_free < 0);
                chunk.m_meta.free_head = -1;
                m_free_chunk_head = chunk.m_meta.next_free_chunk;
            }
        }
        else
        {
            chunk.m_data[index + 1] = slot with { skip_field = l };
            if (l > 1) chunk.m_data[index + slot.skip_field].skip_field = l;
        }
        return (chunk.m_meta.chunk_index, index);
    }

    #endregion

    #region Remove

    /// <returns>was existed</returns>
    public bool Remove(int chunk, int index)
    {
        if (chunk < 0 || index < 0) return false;
        ref var ch = ref m_storage.TryGetChunk(chunk, out var chunk_exist);
        if (!chunk_exist) return false;
        if (index >= ch.m_meta.cap) return false;
        ref var ctrl = ref ch.m_data[index];
        throw new NotImplementedException();
        // if (ctrl != 0) return false;
        // if (index == 0)
        // {
        //     ref var next = ref ch.m_data[1];
        //     if (next == 0)
        //     {
        //         ctrl = 1;
        //         ch.m_meta.free_head = 0;
        //         return true;
        //     }
        //     Debug.Assert(next > 0);
        //     var l = next;
        //     var l1 = l + 1;
        //     ctrl = l1;
        //     next = -1;
        //     ch.m_data[l] = l1;
        //     return true;
        // }
        // else if (index == ch.m_meta.cap - 1)
        // {
        //     ref var prev = ref ch.m_data[index - 1];
        //     if (prev == 0)
        //     {
        //         ctrl = 1;
        //         if (ch.m_meta.free_head > index) ch.m_meta.free_head = index;
        //         return true;
        //     }
        //     Debug.Assert(prev > 0);
        //     var l = prev + 1;
        //     ctrl = l;
        //     prev = -1;
        //     ch.m_data[index - prev] = l;
        //     return true;
        // }
        // else
        // {
        //     ref var prev = ref ch.m_data[index - 1];
        //     ref var next = ref ch.m_data[index + 1];
        //     switch (prev, next)
        //     {
        //         case (0, 0):
        //         {
        //             ctrl = 1;
        //             if (ch.m_meta.free_head > index) ch.m_meta.free_head = index;
        //             return true;
        //         }
        //         case (not 0, not 0):
        //         {
        //             Debug.Assert(prev > 0 && next > 0);
        //             ctrl = -1;
        //             var l = prev + next + 1;
        //             ch.m_data[index - prev] = l;
        //             ch.m_data[index + next] = l;
        //             next = -1;
        //             prev = -1;
        //             return true;
        //         }
        //         case (0, not 0):
        //         {
        //             Debug.Assert(next > 0);
        //             var l = next + 1;
        //             ctrl = l;
        //             ch.m_data[index + next] = l;
        //             if (ch.m_meta.free_head > index) ch.m_meta.free_head = index;
        //             next = -1;
        //             return true;
        //         }
        //         case (not 0, 0):
        //         {
        //             Debug.Assert(prev > 0);
        //             var l = prev + 1;
        //             ctrl = l;
        //             ch.m_data[index - prev] = l;
        //             prev = -1;
        //             return true;
        //         }
        //     }
        // }
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

    [UnscopedRef]
    public T[]? TryGetChunk(int chunk)
    {
        var len = m_chunks.Count;
        if (chunk >= len) return null;
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

    #region Chunk

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
                m_data = NativeLib.Instance.Alloc<T>(size),
            });
        }
        return m_chunks.UnsafeAt(chunk)->m_data!;
    }

    [UnscopedRef]
    public T* TryGetChunk(int chunk, out bool exists)
    {
        var len = m_chunks.Count;
        if (chunk >= len)
        {
            exists = false;
            return null;
        }
        exists = true;
        return m_chunks.UnsafeAt(chunk)->m_data!;
    }

    [UnscopedRef]
    public T* UnsafeGetChunk(int chunk) => m_chunks.UnsafeAt(chunk)->m_data!;

    #endregion

    #region At

    [UnscopedRef]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* EnsureAt(int chunk, int index) => &EnsureChunk(chunk)[index];

    [UnscopedRef]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* TryAt(int chunk, int index) => &TryGetChunk(chunk, out _)[index];

    [UnscopedRef]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* UnsafeAt(int chunk, int index) => &UnsafeGetChunk(chunk)[index];

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
    where C : NativeHiveStorage<T, C>.IMeta
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

    public interface IMeta
    {
        public static abstract C Create(T* data, int chunk, int size);
    }

    #endregion

    #region Props

    public int Count => m_chunks.Count;

    #endregion

    #region Chunk

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
            var data = NativeLib.Instance.Alloc<T>(size);
            m_chunks.Add(new()
            {
                m_data = data,
                m_meta = C.Create(data, chunk, size),
            });
        }
        return ref *m_chunks.UnsafeAt(chunk);
    }

    [UnscopedRef]
    public ref Chunk TryGetChunk(int chunk, out bool exists)
    {
        var len = m_chunks.Count;
        if (chunk >= len)
        {
            exists = false;
            return ref Unsafe.NullRef<Chunk>();
        }
        exists = true;
        return ref *m_chunks.UnsafeAt(chunk);
    }

    [UnscopedRef]
    public ref Chunk UnsafeGetChunk(int chunk) => ref *m_chunks.UnsafeAt(chunk);

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
    #region Static Check

    static NativeHive()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            throw new NotSupportedException("Only unmanaged types are supported.");
    }

    #endregion

    #region Fields

    internal NativeHiveCtrl m_ctrl;
    internal NativeHiveStorage<T> m_storage;

    #endregion

    #region Exists

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Exists(int chunk, int index) => m_ctrl.Exists(chunk, index);

    #endregion

    #region At

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* TryAt(int chunk, int index)
    {
        if (!Exists(chunk, index)) return null;
        return m_storage.UnsafeAt(chunk, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* UnsafeAt(int chunk, int index) => m_storage.UnsafeAt(chunk, index);

    #endregion

    #region Add

    public T* UnsafeAdd(out (int chunk, int index) place)
    {
        var (chunk, index) = m_ctrl.Add();
        place = (chunk, index);
        return m_storage.EnsureAt(chunk, index);
    }

    public (int chunk, int index) Add(T item)
    {
        var (chunk, index) = m_ctrl.Add();
        *m_storage.EnsureAt(chunk, index) = item;
        return (chunk, index);
    }

    #endregion

    #region Remove

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(int chunk, int index) => m_ctrl.Remove(chunk, index);

    #endregion
}
