namespace Coplt.UI.Native.Collections;

public unsafe struct FFIOrderedSet<T>
{
    public int* m_buckets;
    public FFIOrderedSetNode<T>* m_nodes;
    public ulong m_fast_mode_multiplier;
    public int m_cap;
    public int m_first;
    public int m_last;
    public int m_count;
    public int m_free_list;
    public int m_free_count;
}

public unsafe struct FFIOrderedSetNode<T>
{
    public int HashCode;
    public int Next;
    public int OrderNext;
    public int OrderPrev;
    public T Value;
}
