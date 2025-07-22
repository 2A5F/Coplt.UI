namespace Coplt.UI.BoxLayout.Utilities;

public ref struct Ref<T>(ref T Value)
{
    public ref T Value = ref Value;
}

public ref struct RoRef<T>(in T Value)
{
    public ref readonly T Value = ref Value;
}

public static class Ref
{
    public static Ref<T> Of<T>(ref T value) => new(ref value);
    
    public static RoRef<T> In<T>(in T value) => new(in value);
}
