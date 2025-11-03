namespace Coplt.UI.Native.Collections;

public unsafe struct FFIMap
{
    public int* m_buckets;
    public void* m_entries;
    public ulong m_fast_mode_multiplier;
    public int m_cap;
    public int m_count;
    public int m_free_list;
    public int m_free_count;
}
