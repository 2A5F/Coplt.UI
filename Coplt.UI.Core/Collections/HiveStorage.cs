using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Coplt.Dropping;
using Coplt.UI.Native;
using Coplt.UI.Utilities;

namespace Coplt.UI.Collections;

public static class HiveStorage
{
    #region Const

    public const int InitCapacity = 16;

    #endregion
}

public struct HiveStorage<T>
{
    #region Fields

    internal EmbedList<T[]> m_chunks;

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
            var size = HiveStorage.InitCapacity << i;
            m_chunks.Add(GC.AllocateUninitializedArray<T>(size));
        }
        return m_chunks.UnsafeAt(chunk);
    }

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
            var size = HiveStorage.InitCapacity << i;
            m_chunks.Add(new()
            {
                m_data = (T*)NativeLib.Instance.Alloc(size, Utils.AlignOf<T>()),
            });
        }
        return m_chunks.UnsafeAt(chunk)->m_data!;
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
