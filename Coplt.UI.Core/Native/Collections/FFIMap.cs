namespace Coplt.UI.Native.Collections;

public unsafe struct FFIMap<K, V>
{
    public int* m_buckets;
    public FFIMapEntry<K, V>* m_entries;
    public ulong m_fast_mode_multiplier;
    public int m_cap;
    public int m_count;
    public int m_free_list;
    public int m_free_count;
}

public struct FFIMapEntry<K, V>
{
    public int HashCode;
    public int Next;
    public K Key;
    public V Value;
}
