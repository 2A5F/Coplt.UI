namespace Coplt.UI.Native.Collections;

public unsafe struct FFIOrderedSet
{
    public int* m_buckets;
    public void* m_nodes;
    public ulong m_fast_mode_multiplier;
    public int m_cap;
    public int m_first;
    public int m_last;
    public int m_count;
    public int m_free_list;
    public int m_free_count;
}
