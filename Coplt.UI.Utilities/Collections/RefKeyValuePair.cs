namespace Coplt.UI.Collections;

public readonly ref struct RefKeyValuePair<TKey, TValue>(ref TKey Key, ref TValue Value)
{
    public readonly ref TKey Key = ref Key;
    public readonly ref TValue Value = ref Value;

    public void Deconstruct(out TKey Key, out TValue Value)
    {
        Key = this.Key;
        Value = this.Value;
    }
}
